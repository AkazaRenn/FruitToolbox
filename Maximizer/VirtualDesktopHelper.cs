using System.Runtime.InteropServices;

using Castle.Components.DictionaryAdapter.Xml;

using WindowsDesktop;

namespace FruitToolbox.Maximizer
{
    internal static class VirtualDesktopHelper
    {
        const int DefaultRetry = 10;
        const int DefaultWait = 50;

        public static void TryPinWindow(nint window, int maxRetry = DefaultRetry, int wait = DefaultWait)
        {
            int retry = 0;
            do
            {
                try
                {
                    VirtualDesktop.PinWindow(window);
                    return;
                } catch
                {
                    Thread.Sleep(wait);
                }
            } while(retry++ < maxRetry);
        }

        public static void TryUnpinWindow(nint window, int maxRetry = DefaultRetry, int wait = DefaultWait)
        {
            int retry = 0;
            do
            {
                try
                {
                    VirtualDesktop.UnpinWindow(window);
                    return;
                } catch
                {
                    Thread.Sleep(wait);
                }
            } while(retry++ < maxRetry);
        }

        public static void TryMoveToDesktop(nint window, VirtualDesktop desktop, int maxRetry = DefaultRetry, int wait = DefaultWait)
        {
            int retry = 0;
            do
            {
                try
                {
                    VirtualDesktop.MoveToDesktop(window, desktop);
                    return;
                } catch
                {
                    Thread.Sleep(wait);
                }
            } while(retry++ < maxRetry);
        }

        public static void TrySwitch(VirtualDesktop desktop, int maxRetry = DefaultRetry, int wait = DefaultWait)
        {
            int retry = 0;
            do
            {
                try
                {
                    desktop.Switch();
                    return;
                } catch
                {
                    Thread.Sleep(wait);
                }
            } while(retry++ < maxRetry);
        }

        public static void TryMove(VirtualDesktop desktop, int index, int maxRetry = DefaultRetry, int wait = DefaultWait)
        {
            int retry = 0;
            do
            {
                try
                {
                    desktop.Move(index);
                    return;
                } catch  
                {
                    Thread.Sleep(wait);
                }
            } while(retry++ < maxRetry);
        }
    }       
}
