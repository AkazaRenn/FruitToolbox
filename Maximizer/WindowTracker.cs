namespace FruitToolbox.Maximizer;

internal static partial class WindowTracker
{
    public const int WindowAnimationWaitMs = 500;

    private delegate void WindowChangeCallbackDelegate(nint hwnd);

    public static event EventHandler<Constants.WindowEvent> NewFloatWindowEvent;
    private static void InvokeNewFloatWindowEvent(nint hwnd)
    {
        NewFloatWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
    }

    public static event EventHandler<Constants.WindowEvent> MaxWindowEvent;
    private static void InvokeMaxWindowEvent(nint hwnd)
    {
        MaxWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
    }

    public static event EventHandler<Constants.WindowEvent> UnmaxWindowEvent;
    private static void InvokeUnmaxWindowEvent(nint hwnd)
    {
        UnmaxWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
    }

    public static event EventHandler<Constants.WindowEvent> MinWindowEvent;
    private static void InvokeMinWindowEvent(nint hwnd)
    {
        MinWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
    }

    public static event EventHandler<Constants.WindowEvent> CloseWindowEvent;
    private static void InvokeCloseWindowEvent(nint hwnd)
    {
        CloseWindowEvent?.Invoke(null, new Constants.WindowEvent(hwnd));
    }

    public static bool Start() => Interop.WindowTracker.Start(
        InvokeNewFloatWindowEvent, 
        InvokeMaxWindowEvent, 
        InvokeUnmaxWindowEvent, 
        InvokeMinWindowEvent, 
        InvokeCloseWindowEvent);

    public static void Stop() => Interop.WindowTracker.Stop();
}
