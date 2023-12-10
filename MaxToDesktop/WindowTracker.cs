using static FruitToolbox.Utils.Constants;

namespace FruitToolbox.MaxToDesktop;

internal class WindowTracker {
    public const int WindowAnimationWaitMs = 500;

    private delegate void WindowChangeCallbackDelegate(nint hwnd);

    public static event EventHandler<WindowEvent> NewFloatWindowEvent;
    private static void InvokeNewFloatWindowEvent(nint hwnd) {
        NewFloatWindowEvent?.Invoke(null, new WindowEvent(hwnd));
    }

    public static event EventHandler<WindowEvent> TaskViewEvent;
    private static void InvokeTaskViewEvent(nint hwnd) {
        TaskViewEvent?.Invoke(null, new WindowEvent(hwnd));
    }

    public static event EventHandler<WindowEvent> MaxWindowEvent;
    private static void InvokeMaxWindowEvent(nint hwnd) {
        MaxWindowEvent?.Invoke(null, new WindowEvent(hwnd));
    }

    public static event EventHandler<WindowEvent> UnmaxWindowEvent;
    private static void InvokeUnmaxWindowEvent(nint hwnd) {
        UnmaxWindowEvent?.Invoke(null, new WindowEvent(hwnd));
    }

    public static event EventHandler<WindowEvent> MinWindowEvent;
    private static void InvokeMinWindowEvent(nint hwnd) {
        MinWindowEvent?.Invoke(null, new WindowEvent(hwnd));
    }

    public static event EventHandler<WindowEvent> CloseWindowEvent;
    private static void InvokeCloseWindowEvent(nint hwnd) {
        CloseWindowEvent?.Invoke(null, new WindowEvent(hwnd));
    }

    public static event EventHandler<WindowEvent> WindowTitleChangeEvent;
    private static void InvokeWindowTitleChangeEvent(nint hwnd) {
        WindowTitleChangeEvent?.Invoke(null, new WindowEvent(hwnd));
    }

    public static bool Start() => Interop.WindowTracker.Start(
        InvokeNewFloatWindowEvent,
        InvokeTaskViewEvent,
        InvokeMaxWindowEvent,
        InvokeUnmaxWindowEvent,
        InvokeMinWindowEvent,
        InvokeCloseWindowEvent,
        InvokeWindowTitleChangeEvent);

    public static void Stop() => Interop.WindowTracker.Stop();
}
