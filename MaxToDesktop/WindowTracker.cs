namespace FruitToolbox.MaxToDesktop;

internal class WindowTracker {
    public const int WindowAnimationWaitMs = 500;

    private delegate void WindowChangeCallbackDelegate(nint hwnd);

    public static event EventHandler<Utils.WindowEvent> NewFloatWindowEvent;
    private static void InvokeNewFloatWindowEvent(nint hwnd) {
        NewFloatWindowEvent?.Invoke(null, new Utils.WindowEvent(hwnd));
    }

    public static event EventHandler<Utils.WindowEvent> TaskViewEvent;
    private static void InvokeTaskViewEvent(nint hwnd) {
        TaskViewEvent?.Invoke(null, new Utils.WindowEvent(hwnd));
    }

    public static event EventHandler<Utils.WindowEvent> MaxWindowEvent;
    private static void InvokeMaxWindowEvent(nint hwnd) {
        MaxWindowEvent?.Invoke(null, new Utils.WindowEvent(hwnd));
    }

    public static event EventHandler<Utils.WindowEvent> UnmaxWindowEvent;
    private static void InvokeUnmaxWindowEvent(nint hwnd) {
        UnmaxWindowEvent?.Invoke(null, new Utils.WindowEvent(hwnd));
    }

    public static event EventHandler<Utils.WindowEvent> MinWindowEvent;
    private static void InvokeMinWindowEvent(nint hwnd) {
        MinWindowEvent?.Invoke(null, new Utils.WindowEvent(hwnd));
    }

    public static event EventHandler<Utils.WindowEvent> CloseWindowEvent;
    private static void InvokeCloseWindowEvent(nint hwnd) {
        CloseWindowEvent?.Invoke(null, new Utils.WindowEvent(hwnd));
    }

    public static event EventHandler<Utils.WindowEvent> WindowTitleChangeEvent;
    private static void InvokeWindowTitleChangeEvent(nint hwnd) {
        WindowTitleChangeEvent?.Invoke(null, new Utils.WindowEvent(hwnd));
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
