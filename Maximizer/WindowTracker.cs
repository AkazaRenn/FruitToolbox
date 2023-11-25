using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using FruitToolbox.Utils;

namespace FruitToolbox.Maximizer
{
    internal static partial class WindowTracker
    {
        public const int WindowAnimationWaitMs = 500;

        private delegate void WindowChangeCallbackDelegate(IntPtr hwnd);

        public static event EventHandler<Constants.WindowEvent> NewFloatWindowEvent;
        private static void InvokeNewFloatWindowEvent(IntPtr hwnd)
        {
            NewFloatWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
        }

        public static event EventHandler<Constants.WindowEvent> MaxWindowEvent;
        private static void InvokeMaxWindowEvent(IntPtr hwnd)
        {
            MaxWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
        }

        public static event EventHandler<Constants.WindowEvent> UnmaxWindowEvent;
        private static void InvokeUnmaxWindowEvent(IntPtr hwnd)
        {
            UnmaxWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
        }

        public static event EventHandler<Constants.WindowEvent> MinWindowEvent;
        private static void InvokeMinWindowEvent(IntPtr hwnd)
        {
            MinWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
        }

        public static event EventHandler<Constants.WindowEvent> CloseWindowEvent;
        private static void InvokeCloseWindowEvent(IntPtr hwnd)
        {
            CloseWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
        }

        [LibraryImport("WindowTracker")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool WindowTracker_start(
            WindowChangeCallbackDelegate newFloatWindowHandler,
            WindowChangeCallbackDelegate maxWindowHandler,
            WindowChangeCallbackDelegate unmaxWindowHandler,
            WindowChangeCallbackDelegate minWindowHandler,
            WindowChangeCallbackDelegate closeWindowHandler);
        public static bool Start() => 
            WindowTracker_start(
                InvokeNewFloatWindowEvent, 
                InvokeMaxWindowEvent, 
                InvokeUnmaxWindowEvent, 
                InvokeMinWindowEvent, 
                InvokeCloseWindowEvent);


        [LibraryImport("WindowTracker")]
        private static partial void WindowTracker_stop();
        public static void Stop() => WindowTracker_stop();
    }
}
