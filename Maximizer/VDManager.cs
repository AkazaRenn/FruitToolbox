using System.Diagnostics;
using System.Runtime.InteropServices;

using FruitToolbox.Interop;

using WindowsDesktop;

using YamlDotNet.Serialization.NodeTypeResolvers;

using static FruitToolbox.Constants;

namespace FruitToolbox.Maximizer
{
    internal class VDManager
    {
        const string CreatedDesktopNameFixes = " "; //"​";
        const string UnnamableWindowName = "Administrative Window";
        const int WindowAnimationWaitMs = 300;

        public static Guid HomeDesktopId { get; private set; }

        static readonly Dictionary<nint, Guid> HwndDesktopMap = [];

        public static VirtualDesktop Home
        {
            get
            {
                return VDHelper.FromId(HomeDesktopId);
            }
        }

        public static void GoHome()
        {
            VDHelper.Switch(Home);
        }

        public static bool AutoCreated(VirtualDesktop desktop) =>
            desktop.Name.StartsWith(CreatedDesktopNameFixes);

        public static string GetWindowDescription(nint hwnd)
        {
            try
            {
                var process = Process.GetProcessById(Utils.GetProcessId(hwnd));
                if(process.MainWindowTitle.Length > 0 && process.MainWindowTitle.Length <= 30)
                {
                    return process.MainWindowTitle;
                }

                string fileDescription = process.MainModule.FileVersionInfo.FileDescription;
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

        private static string GetAutoDesktopName(nint hwnd) =>
            CreatedDesktopNameFixes + GetWindowDescription(hwnd) + CreatedDesktopNameFixes;

        public static void OnFloatWindow(object _, WindowEvent e) =>
            VDHelper.PinWindow(e.HWnd);

        public static void OnMax(object _, WindowEvent e)
        {
            var desktop = VDHelper.Create();
            desktop.Name = GetAutoDesktopName(e.HWnd);
            Thread.Sleep(WindowAnimationWaitMs);

            VDHelper.UnpinWindow(e.HWnd);

            VDHelper.MoveToDesktop(e.HWnd, desktop);
            VDHelper.Switch(desktop);

            HwndDesktopMap[e.HWnd] = desktop.Id;
        }

        public static void OnUnmax(object _, WindowEvent e)
        {
            Thread.Sleep(WindowAnimationWaitMs);

            VDHelper.PinWindow(e.HWnd);
            OnMinOrClose(_, e);
        }

        public static void OnMinOrClose(object _, WindowEvent e)
        {
            Guid desktopId = HwndDesktopMap[e.HWnd];
            if(VDHelper.Current.Id == desktopId)
            {
                VDHelper.Switch(HomeDesktopId);
            }

            VDHelper.Remove(desktopId);
            HwndDesktopMap.Remove(e.HWnd);
        }

        public static void InitializeDesktops()
        {
            ClearAutoDesktops();

            var curr = VDHelper.Current;

            while(curr.GetLeft() != null)
            {
                curr = curr.GetLeft();
            }
            HomeDesktopId = curr.Id;

            while(curr.GetRight() != null)
            {
                VDHelper.Remove(curr.GetRight());
            }
        }

        public static void ClearAutoDesktops()
        {
            foreach(var d in VDHelper.Desktops)
            {
                if(AutoCreated(d))
                {
                    Thread.Sleep(50);
                    VDHelper.Remove(d);
                }
            }
        }
    }
}
