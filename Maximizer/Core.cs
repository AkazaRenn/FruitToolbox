using System.Diagnostics;

using FruitToolbox.Interop;

using WindowsDesktop;

using static FruitToolbox.Constants;

namespace FruitToolbox.Maximizer;

internal static class Core
{
    const string CreatedDesktopNameFixes = " "; //"​";
    const string UnnamableWindowName = "Administrative Window";

    static VirtualDesktop HomeDesktop;
    static Guid TempCurrDesktopId;
    static readonly Dictionary<nint, Guid> HwndDesktopMap = [];
    static readonly System.Timers.Timer ReorderDesktopTimer = new(5000);

    public static string GetWindowDescription(nint hwnd)
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
    }

    static void OnFloatWindow(object _, WindowEvent e) =>
        VirtualDesktopHelper.TryPinWindow(e.HWnd);

    static void OnMax(object _, WindowEvent e)
    {
        var desktop = VirtualDesktop.Create();
        desktop.Name = CreatedDesktopNameFixes + GetWindowDescription(e.HWnd) + CreatedDesktopNameFixes;
        Thread.Sleep(300);

        VirtualDesktop.UnpinWindow(e.HWnd);

        VirtualDesktop.MoveToDesktop(e.HWnd, desktop);
        desktop.Switch();

        HwndDesktopMap[e.HWnd] = desktop.Id;
    }

    static void OnUnmax(object _, WindowEvent e)
    {
        Thread.Sleep(300);

        VirtualDesktop.PinWindow(e.HWnd);
        if(VirtualDesktop.Current != HomeDesktop)
        {
            HomeDesktop.Switch();
        }

        VirtualDesktop.FromId(HwndDesktopMap[e.HWnd])?.Remove();
        HwndDesktopMap.Remove(e.HWnd);
    }

    static void OnMinOrClose(object _, WindowEvent e)
    {
        Thread.Sleep(300);

        if(VirtualDesktop.Current != HomeDesktop)
        {
            HomeDesktop.Switch();
        }

        VirtualDesktop.FromId(HwndDesktopMap[e.HWnd])?.Remove();
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
        HomeDesktop = curr;
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
        if(VirtualDesktop.Current != HomeDesktop)
        {
            TempCurrDesktopId = VirtualDesktop.Current.Id;
            HomeDesktop.Switch();
        } else
        {
            VirtualDesktop.FromId(TempCurrDesktopId)?.Switch();
        }
    }

    private static void OnReorderDesktopTimer(object _, EventArgs e)
    {
        ReorderDesktopTimer.Stop();
        VirtualDesktop.Current.Move(1);
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
        HomeDesktop.Switch();
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
