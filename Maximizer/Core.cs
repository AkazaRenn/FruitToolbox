using WindowsDesktop;

using static FruitToolbox.Constants;

namespace FruitToolbox.Maximizer;

internal static class Core {
    const int UserCreatedDesktopCount = 1;
    const int WindowAnimationWaitMs = 300;

    static readonly Dictionary<nint, Guid> HwndDesktopMap = [];
    static readonly System.Timers.Timer ReorderDesktopTimer = new(5000);
    static Guid HomeDesktopId;
    static Guid MostRecentDesktopId;

    public static bool Start() {
        InitializeDesktops();

        WindowTracker.NewFloatWindowEvent += OnFloatWindow;
        WindowTracker.MaxWindowEvent += OnMax;
        WindowTracker.UnmaxWindowEvent += OnUnmax;
        WindowTracker.MinWindowEvent += OnMinOrClose;
        WindowTracker.CloseWindowEvent += OnMinOrClose;
        WindowTracker.WindowTitleChangeEvent += OnWindowTitleChange;

        Hotkey.Core.HomeEvent += OnHome;

        bool rc = WindowTracker.Start();
        GoHome();

        ReorderDesktopTimer.Elapsed += OnReorderDesktopTimer;
        VirtualDesktop.CurrentChanged += OnDesktopSwitched;
        VirtualDesktop.Destroyed += OnDesktopDestroy;

        return rc;
    }

    public static void Stop() {
        ClearAutoDesktops();

        WindowTracker.Stop();
    }

    private static void OnHome(object _, EventArgs e) {

        if (SafeVirtualDesktop.Current.Id == HomeDesktopId) {
            SafeVirtualDesktop.CurrentRight.Switch();
        } else {
            GoHome();
        }
    }

    private static void OnDesktopDestroy(object _, VirtualDesktopDestroyEventArgs e) {
        if (e.Destroyed.Id == MostRecentDesktopId) {
            MostRecentDesktopId = SafeVirtualDesktop.CurrentRight.Id;
        }
    }

    private static void OnReorderDesktopTimer(object _, EventArgs e) {
        ReorderDesktopTimer.Stop();
        if (SafeVirtualDesktop.Current.Id != HomeDesktopId) {
            SafeVirtualDesktop.Current.Move(UserCreatedDesktopCount);
        }
    }

    private static void OnDesktopSwitched(object _, VirtualDesktopChangedEventArgs e) {
        MostRecentDesktopId = e.NewDesktop.Id;

        if (MostRecentDesktopId == HomeDesktopId) {
            ReorderDesktopTimer.Stop();
            Thread.Sleep(100);
            // No effect if OldDesktop no longer exists
            SafeVirtualDesktop.Move(e.OldDesktop.Id, UserCreatedDesktopCount);
        } else {
            ReorderDesktopTimer.Start();
        }
    }

    public static void GoHome() =>
        SafeVirtualDesktop.Switch(HomeDesktopId);

    public static void OnFloatWindow(object _, WindowEvent e) =>
        SafeVirtualDesktop.PinWindow(e.HWnd);

    public static void OnMax(object _, WindowEvent e) {
        var desktop = SafeVirtualDesktop.Create();
        desktop.Rename(e.HWnd);
        Thread.Sleep(WindowAnimationWaitMs);

        SafeVirtualDesktop.UnpinWindow(e.HWnd);

        desktop.MoveWindow(e.HWnd);
        desktop.Switch();

        HwndDesktopMap[e.HWnd] = desktop.Id;
    }

    public static void OnUnmax(object _, WindowEvent e) {
        Thread.Sleep(WindowAnimationWaitMs);

        SafeVirtualDesktop.PinWindow(e.HWnd);
        OnMinOrClose(_, e);
    }

    public static void OnMinOrClose(object _, WindowEvent e) {
        if (HwndDesktopMap.TryGetValue(e.HWnd, out Guid desktopId) &&
            SafeVirtualDesktop.Current.Id == desktopId) {
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
