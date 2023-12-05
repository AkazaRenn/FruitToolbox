using Microsoft.Toolkit.Uwp.Notifications;

using WindowsDesktop;

using static FruitToolbox.Utils;

namespace FruitToolbox.Maximizer;

internal class Core : IDisposable {
    private static bool Started = false;
    static Core _Instance = null;
    public static Core Instance {
        get {
            _Instance ??= new();
            return _Instance;
        }
    }

    static readonly BidirectionalDictionary<nint, Guid> HwndDesktopMap = new();
    static readonly System.Timers.Timer ReorderDesktopTimer = new(Settings.Core.ReorgnizeDesktopIntervalMs);

    static int UserCreatedDesktopCount = 1;
    static Guid HomeDesktopId;
    static Guid CurrentDesktopId;

    static bool EnableSwitchingDesktop = false;
    static bool CanSwitchDesktop {
        get {
            return EnableSwitchingDesktop &&
                !(Settings.Core.DisableSwapInFullscreen &&
                Interop.Utils.InFullScreen());
        }
    }


    private Core() {
        if (Started) {
            return;
        }

        ToggleStartedState(true);
        Settings.Core.SettingsChangedEventHandler += OnSettingsUpdate;
        Settings.Core.MaximizerEnabled = Started;
    }

    ~Core() {
        Dispose();
    }

    public void Dispose() {
        if (Started) {
            ToggleStartedState(false);
        }
        Settings.Core.SettingsChangedEventHandler -= OnSettingsUpdate;
        _Instance = null;
        GC.SuppressFinalize(this);
    }

    private static void ToggleStartedState(bool enable) {
        if (enable && !Started) {
            if (Settings.Core.MaximizerEnabled) {
                EnableSwitchingDesktop = false;

                InitializeDesktops();
                ToggleWindowTrackerHooks(true);
                Started = WindowTracker.Start();
                ToggleInternalHooks(true);

                EnableSwitchingDesktop = true;

                if (!Started) {
                    new ToastContentBuilder()
                        .AddText("Unable to enable maximizer")
                        .Show();
                } else {
                    ToggleExternalHooks(true);
                }
            }
        } else if (!enable && Started) {
            ToggleExternalHooks(false);
            ToggleWindowTrackerHooks(false);
            ToggleInternalHooks(false);
            ClearAutoDesktops();
            ReorderDesktopTimer.Stop();
            WindowTracker.Stop();
            Started = false;
        }
    }

    private static void ToggleWindowTrackerHooks(bool enable) {
        if (enable) {
            WindowTracker.NewFloatWindowEvent += OnFloatWindow;
            WindowTracker.MaxWindowEvent += OnMax;
            WindowTracker.UnmaxWindowEvent += OnUnmax;
            WindowTracker.MinWindowEvent += OnMin;
            WindowTracker.CloseWindowEvent += OnClose;
            WindowTracker.WindowTitleChangeEvent += OnWindowTitleChange;
        } else {
            WindowTracker.NewFloatWindowEvent -= OnFloatWindow;
            WindowTracker.MaxWindowEvent -= OnMax;
            WindowTracker.UnmaxWindowEvent -= OnUnmax;
            WindowTracker.MinWindowEvent -= OnMin;
            WindowTracker.CloseWindowEvent -= OnClose;
            WindowTracker.WindowTitleChangeEvent -= OnWindowTitleChange;
        }
    }

    private static void ToggleExternalHooks(bool enable) {
        if (enable) {
            Hotkey.Core.GuiUpEvent += OnTaskView;
        } else {
            Hotkey.Core.GuiUpEvent -= OnTaskView;
        }
    }

    private static void ToggleInternalHooks(bool enable) {
        if (enable) {
            ReorderDesktopTimer.Elapsed += OnReorderDesktopTimer;
            VirtualDesktop.CurrentChanged += OnDesktopSwitch;
            VirtualDesktop.Created += OnDesktopCreate;
            VirtualDesktop.Destroyed += OnDesktopDestroy;
        } else {
            ReorderDesktopTimer.Elapsed -= OnReorderDesktopTimer;
            VirtualDesktop.CurrentChanged -= OnDesktopSwitch;
            VirtualDesktop.Created -= OnDesktopCreate;
            VirtualDesktop.Destroyed -= OnDesktopDestroy;
        }
    }

    private static void OnSettingsUpdate(object sender, EventArgs e) {
        if (Started) {
            ReorderDesktopTimer.Interval = Settings.Core.ReorgnizeDesktopIntervalMs;
        }

        if (Started != Settings.Core.MaximizerEnabled) {
            ToggleStartedState(Settings.Core.MaximizerEnabled);
            Settings.Core.MaximizerEnabled = Started;
        }
    }

    private static void OnTaskView(object _, EventArgs e) {
        ReorderDesktopTimer.Stop();
        TaskView();
    }

    private static void OnDesktopCreate(object _, VirtualDesktop e) {
        Thread.Sleep(200);
        if (!new SafeVirtualDesktop(e).IsAutoCreated) {
            UserCreatedDesktopCount++;
        }
    }

    private static void OnDesktopDestroy(object _, VirtualDesktopDestroyEventArgs e) {
        if (e.Destroyed.Id == CurrentDesktopId) {
            CurrentDesktopId = SafeVirtualDesktop.CurrentRight.Id;
        }

        if (new SafeVirtualDesktop(e.Destroyed.Id).IsAutoCreated) {
            HwndDesktopMap.RemoveValue(e.Destroyed.Id);
        } else {
            UserCreatedDesktopCount--;
            if (e.Destroyed.Id == HomeDesktopId) {
                SafeVirtualDesktop curr = SafeVirtualDesktop.RightMost;

            }
        }
    }

    private static void OnReorderDesktopTimer(object _, EventArgs e) {
        ReorderDesktopTimer.Stop();
        if (SafeVirtualDesktop.Current.Id != HomeDesktopId) {
            SafeVirtualDesktop.Current.Move(UserCreatedDesktopCount);
        }
    }

    private static void OnDesktopSwitch(object _, VirtualDesktopChangedEventArgs e) {
        CurrentDesktopId = e.NewDesktop.Id;

        if (CurrentDesktopId == HomeDesktopId) {
            ReorderDesktopTimer.Stop();
        } else {
            ReorderDesktopTimer.Start();
        }
    }

    public static void OnFloatWindow(object _, WindowEvent e) =>
        SafeVirtualDesktop.PinWindow(e.HWnd);

    public static void OnMax(object _, WindowEvent e) {
        var desktop = SafeVirtualDesktop.AutoCreate(e.HWnd);

        if (CanSwitchDesktop) {
            desktop.Switch();
        }
        desktop.MoveWindow(e.HWnd);
        HwndDesktopMap.Add(e.HWnd, desktop.Id);

        SafeVirtualDesktop.UnpinWindow(e.HWnd);
    }

    public static void OnUnmax(object _, WindowEvent e) {
        SafeVirtualDesktop.PinWindow(e.HWnd);

        OnClose(_, e);
    }

    public static void OnMin(object _, WindowEvent e) {
        if (HwndDesktopMap.TryGetValue(e.HWnd, out Guid desktopId) &&
            SafeVirtualDesktop.Current.Id == desktopId) {
            SafeVirtualDesktop.Switch(HomeDesktopId);

            Thread.Sleep(100);
            Interop.Utils.UnminimizeInBackground(e.HWnd);
        }
    }

    public static void OnClose(object _, WindowEvent e) {
        if (HwndDesktopMap.TryGetValue(e.HWnd, out Guid desktopId) &&
            SafeVirtualDesktop.Current.Id == desktopId) {
            //SafeVirtualDesktop.Switch(CurrentDesktopId);
            // Logically should switch to CurrentDesktopId
            // but the there's no animation in that case
            SafeVirtualDesktop.Switch(HomeDesktopId);
        }

        SafeVirtualDesktop.Remove(desktopId);
        HwndDesktopMap.RemoveKey(e.HWnd);
    }

    public static void OnWindowTitleChange(object _, WindowEvent e) {
        if (HwndDesktopMap.TryGetValue(e.HWnd, out Guid id)) {
            new SafeVirtualDesktop(id).Rename(e.HWnd);
        }
    }

    public static void InitializeDesktops() {
        ClearAutoDesktops();

        var curr = SafeVirtualDesktop.RightMost;
        HomeDesktopId = curr.Id;

        while (curr.Left != null) {
            //curr.Right.Remove();
            curr = curr.Left;
            UserCreatedDesktopCount++;
        }
    }

    public static void ClearAutoDesktops() {
        foreach (var d in SafeVirtualDesktop.Desktops) {
            if (d.IsAutoCreated) {
                Thread.Sleep(50);
                d.Remove();
            }
        }
    }
}
