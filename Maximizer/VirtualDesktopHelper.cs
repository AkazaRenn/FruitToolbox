using WindowsDesktop;

namespace FruitToolbox.Maximizer
{
    internal static class VirtualDesktopHelper
    {
        public static bool IsPinnedWindow(nint window)
        {
            try
            { 
                return VirtualDesktop.IsPinnedWindow(window); 
            }
            catch 
            { 
                return false; 
            }
        }

        public static void TryPinWindow(nint window)
        {
            try
            {
                VirtualDesktop.PinWindow(window);
            } catch { }
        }

        public static void PinWindow(nint window)
        {
            do
            {
                try
                {
                    VirtualDesktop.PinWindow(window);
                    Thread.Sleep(200);
                } catch { }
            } while(!VirtualDesktop.IsPinnedWindow(window));
        }

        public static void UnpinWindow(nint window)
        {
            do
            {
                try
                {
                    VirtualDesktop.UnpinWindow(window);
                    Thread.Sleep(200);
                } catch { }
            } while(VirtualDesktop.IsPinnedWindow(window));
        }

        public static void MoveWindow(nint window, VirtualDesktop desktop)
        {
            do
            {
                try
                {
                    VirtualDesktop.MoveToDesktop(window, desktop);
                    Thread.Sleep(200);
                } catch { }
            } while(VirtualDesktop.FromHwnd(window) == desktop);
        }

        public static void Switch(VirtualDesktop desktop)
        {
            do
            {
                try
                {
                    desktop.Switch();
                    Thread.Sleep(200);
                } catch { }
            } while(VirtualDesktop.Current.Id != desktop.Id);
        }
    }       
}
