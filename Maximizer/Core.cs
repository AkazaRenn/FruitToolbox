using WindowsDesktop;

namespace FruitToolbox.Maximizer;

internal static class Core
{
    const int UserCreatedDesktopCount = 1;

    static readonly System.Timers.Timer ReorderDesktopTimer = new(5000);
    static Guid MostRecentDesktopId;

    public static bool Start()
    {
        VDManager.InitializeDesktops();

        WindowTracker.NewFloatWindowEvent += VDManager.OnFloatWindow;
        WindowTracker.MaxWindowEvent += VDManager.OnMax;
        WindowTracker.UnmaxWindowEvent += VDManager.OnUnmax;
        WindowTracker.MinWindowEvent += VDManager.OnMinOrClose;
        WindowTracker.CloseWindowEvent += VDManager.OnMinOrClose;

        Hotkey.Core.HomeEvent += OnHome;

        bool rc = WindowTracker.Start();
        VDManager.GoHome();

        ReorderDesktopTimer.Elapsed += OnReorderDesktopTimer;
        VirtualDesktop.CurrentChanged += OnDesktopSwitched;
        VirtualDesktop.Destroyed += OnDesktopDestroy;

        return rc;
    }

    public static void Stop()
    {
        VDManager.ClearAutoDesktops();

        WindowTracker.Stop();
    }

    private static void OnHome(object _, EventArgs e)
    {

        if(VDHelper.Current.Id != VDManager.HomeDesktopId)
        {
            MostRecentDesktopId = VDHelper.Current.Id;
            VDManager.GoHome();
            VDHelper.Move(MostRecentDesktopId, UserCreatedDesktopCount);
        } else
        {
            VDHelper.Switch(MostRecentDesktopId);
        }
    }

    private static void OnDesktopDestroy(object _, VirtualDesktopDestroyEventArgs e)
    {
        if(e.Destroyed.Id == MostRecentDesktopId)
        {
            MostRecentDesktopId = VDHelper.Right.Id;
        }
    }

    private static void OnReorderDesktopTimer(object _, EventArgs e)
    {
        ReorderDesktopTimer.Stop();
        if(VDHelper.Current.Id != VDManager.HomeDesktopId)
        {
            VDHelper.Move(VDHelper.Current, UserCreatedDesktopCount);
        }
    }

    private static void OnDesktopSwitched(object _, VirtualDesktopChangedEventArgs e)
    {
        if(e.NewDesktop.Id == VDManager.HomeDesktopId)
        {
            ReorderDesktopTimer.Stop();
        } else
        {
            ReorderDesktopTimer.Start();
            MostRecentDesktopId = e.NewDesktop.Id;
        }
    }
}
