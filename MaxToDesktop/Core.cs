using FruitToolbox.Utils;

using Microsoft.Toolkit.Uwp.Notifications;

using WindowsDesktop;

using static FruitToolbox.Utils.Constants;

namespace FruitToolbox.MaxToDesktop;

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
    const int WindowsAnimationMs = 200;

    static readonly BidirectionalDictionary<nint, Guid> HwndDesktopMap = new();
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
        Settings.Core.MaxToDesktopEnabled = Started;
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
        if (enable != Started) {
            if (Started == false) {
                if (Settings.Core.MaxToDesktopEnabled) {
                    EnableSwitchingDesktop = false;

                    InitializeDesktops();
                    ToggleWindowTrackerHooks(true);
                    Started = WindowTracker.Start();
                    ToggleInternalHooks(true);

                    EnableSwitchingDesktop = true;

                    if (!Started) {
                        new ToastContentBuilder()
                            .AddText("Unable to enable Max To Desktop")
                            .Show();
                    }
                }
            } else {
                ToggleWindowTrackerHooks(false);
                ToggleInternalHooks(false);
                ClearAutoDesktops();
                ReorderDesktopTimer.Stop();
                WindowTracker.Stop();
                Started = false;
            }
        }
    }

    private static void ToggleWindowTrackerHooks(bool enable) {
        if (enable) {
            WindowTracker.NewFloatWindowEvent += OnFloatWindow;
            WindowTracker.TaskViewEvent += OnTaskView;
            WindowTracker.MaxWindowEvent += OnMax;
            WindowTracker.UnmaxWindowEvent += OnUnmax;
            WindowTracker.MinWindowEvent += OnMin;
            WindowTracker.CloseWindowEvent += OnClose;
            WindowTracker.WindowTitleChangeEvent += OnWindowTitleChange;
        } else {
            WindowTracker.NewFloatWindowEvent -= OnFloatWindow;
            WindowTracker.TaskViewEvent -= OnTaskView;
            WindowTracker.MaxWindowEvent -= OnMax;
            WindowTracker.UnmaxWindowEvent -= OnUnmax;
            WindowTracker.MinWindowEvent -= OnMin;
            WindowTracker.CloseWindowEvent -= OnClose;
            WindowTracker.WindowTitleChangeEvent -= OnWindowTitleChange;
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

        if (Started != Settings.Core.MaxToDesktopEnabled) {
            ToggleStartedState(Settings.Core.MaxToDesktopEnabled);
            Settings.Core.MaxToDesktopEnabled = Started;
        }
    }

    private static void OnDesktopCreate(object _, VirtualDesktop e) {
        Thread.Sleep(200);
        if (e.Id != HomeDesktopId &&
            !new SafeVirtualDesktop(e).IsAutoCreated) {
            SafeVirtualDesktop.Remove(e.Id, HomeDesktopId);
        }
    }

    private static void OnDesktopDestroy(object _, VirtualDesktopDestroyEventArgs e) {
        if (e.Destroyed.Id == CurrentDesktopId) {
            CurrentDesktopId = SafeVirtualDesktop.CurrentRight.Id;
        }

        if (e.Destroyed.Id == HomeDesktopId) {
            if (SafeVirtualDesktop.Current.IsAutoCreated) {
                SafeVirtualDesktop newDesktop = SafeVirtualDesktop.Create();
                HomeDesktopId = newDesktop.Id;
                newDesktop.Name = e.Destroyed.Name;
            } else {
                HomeDesktopId = SafeVirtualDesktop.Current.Id;
            }
            SafeVirtualDesktop.Move(HomeDesktopId, UserCreatedDesktopCount - 1);
        }

        if (HwndDesktopMap.TryGet(e.Destroyed.Id, out nint hwnd)) {
            HwndDesktopMap.Remove(hwnd);
            SafeVirtualDesktop.MoveToDesktop(hwnd, HomeDesktopId);
        }
    }

    private static void OnReorderDesktopTimer(object _, EventArgs e) {
        ReorderDesktopTimer.Stop();
        if (CurrentDesktopId != HomeDesktopId) {
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

    private static void OnFloatWindow(object _, WindowEvent e) =>
        SafeVirtualDesktop.PinWindow(e.HWnd);

    private static void OnTaskView(object _, WindowEvent e) =>
        ReorderDesktopTimer.Stop();

    private static void OnMax(object _, WindowEvent e) {
        SafeVirtualDesktop.PinWindow(e.HWnd);
        var desktop = SafeVirtualDesktop.Create(e.HWnd);

        if (CanSwitchDesktop) {
            Thread.Sleep(WindowsAnimationMs);
            desktop.Switch();
        }
        desktop.MoveWindow(e.HWnd);
        HwndDesktopMap[e.HWnd] = desktop.Id;

        SafeVirtualDesktop.UnpinWindow(e.HWnd);
        desktop.Move(UserCreatedDesktopCount);
    }

    private static void OnUnmax(object _, WindowEvent e) {
        SafeVirtualDesktop.PinWindow(e.HWnd);

        OnClose(_, e);
    }

    private static void OnMin(object _, WindowEvent e) {
        if (HwndDesktopMap.TryGet(e.HWnd, out Guid desktopId) &&
            CurrentDesktopId == desktopId) {
            SafeVirtualDesktop.Switch(HomeDesktopId);

            Thread.Sleep(100);
            Interop.Utils.UnminimizeInBackground(e.HWnd);
        }
    }

    private static void OnClose(object _, WindowEvent e) {
        if (HwndDesktopMap.TryGet(e.HWnd, out Guid desktopId) &&
            CurrentDesktopId == desktopId) {
            //SafeVirtualDesktop.Switch(CurrentDesktopId);
            // Logically should switch to CurrentDesktopId
            // but the there's no animation in that case
            SafeVirtualDesktop.Switch(HomeDesktopId);
        }

        SafeVirtualDesktop.Remove(desktopId);
    }

    private static void OnWindowTitleChange(object _, WindowEvent e) {
        if (HwndDesktopMap.TryGet(e.HWnd, out Guid id)) {
            new SafeVirtualDesktop(id).Rename(e.HWnd);
        }
    }

    private static void InitializeDesktops() {
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

    private static void ClearAutoDesktops() {
        foreach (var d in SafeVirtualDesktop.Desktops) {
            if (d.IsAutoCreated) {
                Thread.Sleep(50);
                d.Remove();
            }
        }
    }
}
