using System.Diagnostics;

using FruitToolbox.Interop;

using WindowsDesktop;

using static FruitToolbox.Constants;

namespace FruitToolbox.Maximizer;

internal static class Core
{
    const string CreatedDesktopNameFixes = " "; //"​";
    const string UnnamableWindowName = "Administrative Window";
    const int WindowAnimationWaitMs = 300;

    static Guid HomeDesktopId;
    static Guid TempCurrDesktopId;
    static readonly Dictionary<nint, Guid> HwndDesktopMap = [];
    static readonly System.Timers.Timer ReorderDesktopTimer = new(5000);

    public static string GetWindowDescription(nint hwnd)
    {
        try
        {
            int proceddId = Utils.GetProcessId(hwnd);
            if(proceddId <= 0)
            {
                return UnnamableWindowName;
            }

            var process = Process.GetProcessById(proceddId);
            if(process.MainWindowTitle.Length > 0 && process.MainWindowTitle.Length <= 30)
            {
                return process.MainWindowTitle;
            }

            var mainModule = process.MainModule;
            if(mainModule == null)
            {
                return UnnamableWindowName;
            }

            string fileDescription = mainModule.FileVersionInfo.FileDescription;
            if("Application Frame Host".Equals(fileDescription) || fileDescription == null || fileDescription.Length == 0)
            {
                return new DirectoryInfo(process.ProcessName).Name;
            }

            return fileDescription;
        } catch
        {
            return UnnamableWindowName;
        }
    }

    static void OnFloatWindow(object _, WindowEvent e) =>
        VirtualDesktopHelper.TryPinWindow(e.HWnd);

    static void OnMax(object _, WindowEvent e)
    {
        var desktop = VirtualDesktop.Create();
        desktop.Name = CreatedDesktopNameFixes + GetWindowDescription(e.HWnd) + CreatedDesktopNameFixes;
        Thread.Sleep(WindowAnimationWaitMs);

        VirtualDesktopHelper.TryUnpinWindow(e.HWnd);

        VirtualDesktopHelper.TryMoveToDesktop(e.HWnd, desktop);
        desktop.Switch();

        HwndDesktopMap[e.HWnd] = desktop.Id;
    }

    static void OnUnmax(object _, WindowEvent e)
    {
        Thread.Sleep(WindowAnimationWaitMs);

        VirtualDesktopHelper.TryPinWindow(e.HWnd);

        VirtualDesktop hwndDesktop = VirtualDesktop.FromId(HwndDesktopMap[e.HWnd]);
        if(VirtualDesktop.Current == hwndDesktop)
        {
            VirtualDesktopHelper.TrySwitch(VirtualDesktop.FromId(HomeDesktopId));
        }

        hwndDesktop?.Remove();
        HwndDesktopMap.Remove(e.HWnd);
    }

    static void OnMinOrClose(object _, WindowEvent e)
    {
        //Thread.Sleep(WindowAnimationWaitMs);

        VirtualDesktop hwndDesktop = VirtualDesktop.FromId(HwndDesktopMap[e.HWnd]);
        if(VirtualDesktop.Current == hwndDesktop)
        {
            VirtualDesktopHelper.TrySwitch(VirtualDesktop.FromId(HomeDesktopId));
        }

        hwndDesktop?.Remove();
        HwndDesktopMap.Remove(e.HWnd);
    }

    static void InitializeDesktops()
    {
        ClearDesktops();

        var curr = VirtualDesktop.Current;
        while (curr.GetRight() != null)
        {
            curr = curr.GetRight();
        }
        HomeDesktopId = curr.Id;
    }

    private static void ClearDesktops()
    {
        foreach(var d in VirtualDesktop.GetDesktops())
        {
            if(d.Name.StartsWith(CreatedDesktopNameFixes))
            {
                Thread.Sleep(50);
                d.Remove();
            }
        }
    }

    private static void OnHome(object _, EventArgs e)
    {

        if(VirtualDesktop.Current.Id != HomeDesktopId)
        {
            TempCurrDesktopId = VirtualDesktop.Current.Id;
            VirtualDesktopHelper.TrySwitch(VirtualDesktop.FromId(HomeDesktopId));
        } else
        {
            VirtualDesktop.FromId(TempCurrDesktopId)?.Switch();
        }
    }

    private static void OnReorderDesktopTimer(object _, EventArgs e)
    {
        ReorderDesktopTimer.Stop();
        if(VirtualDesktop.Current.Id != HomeDesktopId)
        {
            //VirtualDesktopHelper.TryMove(VirtualDesktop.FromId(HomeDesktop), 0);
            VirtualDesktopHelper.TryMove(VirtualDesktop.Current, 1); 
        }
    }

    public static bool Start()
    {
        InitializeDesktops();

        WindowTracker.NewFloatWindowEvent += OnFloatWindow;
        WindowTracker.MaxWindowEvent += OnMax;
        WindowTracker.UnmaxWindowEvent += OnUnmax;
        WindowTracker.MinWindowEvent += OnMinOrClose;
        WindowTracker.CloseWindowEvent += OnMinOrClose;

        Hotkey.Core.HomeEvent += OnHome;

        bool rc = WindowTracker.Start();
        VirtualDesktopHelper.TrySwitch(VirtualDesktop.FromId(HomeDesktopId));
        ReorderDesktopTimer.Elapsed += OnReorderDesktopTimer;
        VirtualDesktop.CurrentChanged += (_, _) => ReorderDesktopTimer.Start();

        return rc;
    }

    public static void Stop()
    {
        ClearDesktops();

        WindowTracker.Stop();
        Hotkey.Core.HomeEvent -= OnHome;
    }
}
