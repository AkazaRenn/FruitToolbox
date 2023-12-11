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

    const int WindowsAnimationMs = 200;

    static readonly Dictionary<Guid, nint> DesktopWindowMap = [];
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
            VirtualDesktop.DestroyBegin += OnDesktopDestroyBegin;
        } else {
            ReorderDesktopTimer.Elapsed -= OnReorderDesktopTimer;
            VirtualDesktop.CurrentChanged -= OnDesktopSwitch;
            VirtualDesktop.Created -= OnDesktopCreate;
            VirtualDesktop.DestroyBegin -= OnDesktopDestroyBegin;
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

    private static void OnDesktopCreate(object sender, VirtualDesktop e) =>
        SafeVirtualDesktop.Move(e.Id, 1);

    private static void OnDesktopDestroyBegin(object _, VirtualDesktopDestroyEventArgs e) {
        if (e.Destroyed.Id == HomeDesktopId) {
            SafeVirtualDesktop newDesktop = SafeVirtualDesktop.Create();
            HomeDesktopId = newDesktop.Id;
            newDesktop.Name = e.Destroyed.Name;

            SafeVirtualDesktop.Move(HomeDesktopId, 0);
        } else if (DesktopWindowMap.TryGetValue(e.Destroyed.Id, out nint hwnd)) {
            Interop.Utils.CloseWindow(hwnd);
            //SafeVirtualDesktop.MoveToDesktop(hwnd, HomeDesktopId);
            DesktopWindowMap.Remove(e.Destroyed.Id);
        }
    }

    private static void OnReorderDesktopTimer(object _, EventArgs e) {
        ReorderDesktopTimer.Stop();
        if (CurrentDesktopId != HomeDesktopId) {
            SafeVirtualDesktop.Current.Move(1);
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
        DesktopWindowMap.Add(desktop.Id, e.HWnd);

        desktop.Move(1);
    }

    private static void OnUnmax(object _, WindowEvent e) {
        if (SafeVirtualDesktop.TryFromHwnd(e.HWnd, out Guid desktopId)) {
            SafeVirtualDesktop.PinWindow(e.HWnd);
            RemoveDesktop(desktopId);
        }
    }

    private static void OnMin(object _, WindowEvent e) {
        if (SafeVirtualDesktop.TryFromHwnd(e.HWnd, out Guid desktopId) &&
            CurrentDesktopId == desktopId) {
            SafeVirtualDesktop.Switch(HomeDesktopId);

            Thread.Sleep(100);
            Interop.Utils.UnminimizeInBackground(e.HWnd);
        }
    }

    private static void OnClose(object _, WindowEvent e) {
        if (SafeVirtualDesktop.TryFromHwnd(e.HWnd, out Guid desktopId)) {
            RemoveDesktop(desktopId);
        }
    }

    private static void OnWindowTitleChange(object _, WindowEvent e) {
        if (SafeVirtualDesktop.TryFromHwnd(e.HWnd, out Guid id)) {
            SafeVirtualDesktop.Rename(id, e.HWnd);
        }
    }

    private static void InitializeDesktops() {
        ClearAutoDesktops();

        var curr = SafeVirtualDesktop.Current;

        while (curr.Left != null) {
            curr = curr.Left;
        }
        HomeDesktopId = curr.Id;
    }

    private static void ClearAutoDesktops() {
        foreach (var d in SafeVirtualDesktop.Desktops) {
            if (d.IsAutoCreated) {
                Thread.Sleep(50);
                d.Remove();
            }
        }
    }

    private static void RemoveDesktop(Guid desktopId) {
        if (CurrentDesktopId == desktopId) {
            //SafeVirtualDesktop.Switch(CurrentDesktopId);
            // Logically should switch to CurrentDesktopId
            // but the there's no animation in that case
            SafeVirtualDesktop.Switch(HomeDesktopId);
        }
        // remove in advance, so destroy event handler
        // doesn't close the window
        DesktopWindowMap.Remove(desktopId);
        SafeVirtualDesktop.Remove(desktopId);
    }
}
