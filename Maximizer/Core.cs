using Microsoft.Toolkit.Uwp.Notifications;

using WindowsDesktop;

using static FruitToolbox.Constants;

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

    const int UserCreatedDesktopCount = 1;

    static readonly Dictionary<nint, Guid> HwndDesktopMap = [];
    static readonly System.Timers.Timer ReorderDesktopTimer = new(Settings.Core.ReorgnizeDesktopIntervalMs);

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
            Hotkey.Core.GuiDownEvent += OnHome;
            Hotkey.Core.GuiUpEvent += OnTaskView;
        } else {
            Hotkey.Core.GuiDownEvent -= OnHome;
            Hotkey.Core.GuiUpEvent -= OnTaskView;
        }
    }

    private static void ToggleInternalHooks(bool enable) {
        if (enable) {
            ReorderDesktopTimer.Elapsed += OnReorderDesktopTimer;
            VirtualDesktop.CurrentChanged += OnDesktopSwitched;
            VirtualDesktop.Destroyed += OnDesktopDestroy;
        } else {
            ReorderDesktopTimer.Elapsed -= OnReorderDesktopTimer;
            VirtualDesktop.CurrentChanged -= OnDesktopSwitched;
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

    private static void OnHome(object _, EventArgs e) {

        if (SafeVirtualDesktop.Current.Id == HomeDesktopId) {
            SafeVirtualDesktop.SwitchRight();
        } else {
            GoHome();
        }
    }

    private static void OnTaskView(object _, EventArgs e) {
        ReorderDesktopTimer.Stop();
        Utils.TaskView();
    }

    private static void OnDesktopDestroy(object _, VirtualDesktopDestroyEventArgs e) {
        if (e.Destroyed.Id == CurrentDesktopId) {
            CurrentDesktopId = SafeVirtualDesktop.CurrentRight.Id;
        }
    }

    private static void OnReorderDesktopTimer(object _, EventArgs e) {
        ReorderDesktopTimer.Stop();
        if (SafeVirtualDesktop.Current.Id != HomeDesktopId) {
            SafeVirtualDesktop.Current.Move(UserCreatedDesktopCount);
        }
    }

    private static void OnDesktopSwitched(object _, VirtualDesktopChangedEventArgs e) {
        CurrentDesktopId = e.NewDesktop.Id;

        if (CurrentDesktopId == HomeDesktopId) {
            ReorderDesktopTimer.Stop();
        } else {
            ReorderDesktopTimer.Start();
        }
    }

    public static void GoHome() {
        SafeVirtualDesktop.Current.Move(UserCreatedDesktopCount);
        SafeVirtualDesktop.SwitchLeft();
        Interop.Utils.Unfocus();
    }

    public static void OnFloatWindow(object _, WindowEvent e) =>
        SafeVirtualDesktop.PinWindow(e.HWnd);

    public static void OnMax(object _, WindowEvent e) {
        var desktop = SafeVirtualDesktop.Create();
        desktop.Rename(e.HWnd);

        if (CanSwitchDesktop) {
            desktop.Switch();
        }
        desktop.MoveWindow(e.HWnd);
        HwndDesktopMap[e.HWnd] = desktop.Id;

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
        HwndDesktopMap.Remove(e.HWnd);
    }

    public static void OnWindowTitleChange(object _, WindowEvent e) {
        if (HwndDesktopMap.TryGetValue(e.HWnd, out Guid id)) {
            new SafeVirtualDesktop(id).Rename(e.HWnd);
        }
    }

    public static void InitializeDesktops() {
        ClearAutoDesktops();

        var curr = SafeVirtualDesktop.Current;

        while (curr.Left != null) {
            curr = curr.Left;
        }
        HomeDesktopId = curr.Id;

        while (curr.Right != null) {
            curr.Right.Remove();
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
