using AutoHotkey.Interop;

using WindowsDesktop;

namespace FruitToolbox.Maximizer;

internal class SafeVirtualDesktop {
    const int DefaultRetry = 10;
    const int DefaultWait = 25;
    const string CreatedDesktopNamePostfix = " "; //"​";
    const string UnnamableWindowName = "Administrative Window";

    private static readonly AutoHotkeyEngine AHKEngine = AutoHotkeyEngine.Instance;
    private readonly Guid WrappedDesktopId;

    public SafeVirtualDesktop(Guid id) {
        WrappedDesktopId = id;
    }

    public SafeVirtualDesktop(VirtualDesktop desktop) {
        WrappedDesktopId = desktop.Id;
    }

    public Guid Id {
        get {
            return WrappedDesktopId;
        }
    }

    public string Name {
        get {
            return Try(() => VirtualDesktop.FromId(WrappedDesktopId)?.Name);
        }
        set {
            Try(() => VirtualDesktop.FromId(WrappedDesktopId).Name = value);
        }
    }

    public bool IsAutoCreated {
        get {
            return Name.EndsWith(CreatedDesktopNamePostfix);
        }
    }

    public static SafeVirtualDesktop Current {
        get {
            return Try(() => new SafeVirtualDesktop(VirtualDesktop.Current));
        }
    }

    public static SafeVirtualDesktop CurrentLeft {
        get {
            return Current.Left;
        }
    }

    public static SafeVirtualDesktop CurrentRight {
        get {
            return Current.Right;
        }
    }

    public static SafeVirtualDesktop LeftMost {
        get {
            SafeVirtualDesktop curr = Current;
            while (curr.Left != null) {
                curr = curr.Left;
            }
            return curr;
        }
    }

    public static SafeVirtualDesktop RightMost {
        get {
            SafeVirtualDesktop curr = Current;
            while (curr.Right != null) {
                curr = curr.Right;
            }
            return curr;
        }
    }

    public static SafeVirtualDesktop[] Desktops {
        get {
            return Try(() => VirtualDesktop.GetDesktops().Select(desktop => new SafeVirtualDesktop(desktop)).ToArray());
        }
    }

    public static SafeVirtualDesktop Create() {
        return Try(() => new SafeVirtualDesktop(VirtualDesktop.Create()));
    }

    public static SafeVirtualDesktop AutoCreate(nint hwnd) => Try(() => {
        SafeVirtualDesktop d = Create();
        d.Rename(hwnd);
        return d;
    });

    public SafeVirtualDesktop Left {
        get {
            var desktop = Try(() => VirtualDesktop.FromId(WrappedDesktopId)?.GetLeft());
            if (desktop == null) {
                return null;
            }
            return new SafeVirtualDesktop(desktop);
        }
    }

    public SafeVirtualDesktop Right {
        get {
            var desktop = Try(() => VirtualDesktop.FromId(WrappedDesktopId)?.GetRight());
            if (desktop == null) {
                return null;
            }
            return new SafeVirtualDesktop(desktop);
        }
    }

    public void Remove(Guid fallbackId = default) {
        if (fallbackId == default) {
            Try(() => VirtualDesktop.FromId(WrappedDesktopId)?.Remove());
        } else {
            Try(() => VirtualDesktop.FromId(WrappedDesktopId)?.Remove(VirtualDesktop.FromId(fallbackId)));
        }
    }

    public static void Remove(Guid targetId, Guid fallbackId = default) {
        if (fallbackId == default) {
            Try(() => VirtualDesktop.FromId(targetId).Remove());
        } else {
            Try(() => VirtualDesktop.FromId(targetId).Remove(VirtualDesktop.FromId(fallbackId)));
        }
    }

    public static SafeVirtualDesktop FromId(Guid id) {
        return Try(() => new SafeVirtualDesktop(VirtualDesktop.FromId(id)));
    }

    public static void PinWindow(nint window) {
        Try(() => VirtualDesktop.PinWindow(window));
    }

    public static void UnpinWindow(nint window) {
        Try(() => VirtualDesktop.UnpinWindow(window));
    }

    public void MoveWindow(nint window) {
        Try(() => VirtualDesktop.MoveToDesktop(window, VirtualDesktop.FromId(WrappedDesktopId)));
    }

    public static void MoveToDesktop(nint window, Guid id) {
        Try(() => VirtualDesktop.MoveToDesktop(window, VirtualDesktop.FromId(id)));
    }

    public void Switch() {
        Try(() => VirtualDesktop.FromId(WrappedDesktopId)?.Switch());
    }

    public static void Switch(Guid id) {
        new SafeVirtualDesktop(id).Switch();
    }

    public void Move(int index) {
        Try(() => VirtualDesktop.FromId(WrappedDesktopId)?.Move(index));
    }

    public static void Move(Guid id, int index) {
        new SafeVirtualDesktop(id).Move(index);
    }

    public void Rename(nint hwnd) =>
        Name = GetWindowTitle(hwnd) + CreatedDesktopNamePostfix;

    public static void Rename(Guid id, nint hwnd) =>
        new SafeVirtualDesktop(id).Rename(hwnd);

    private static string GetWindowTitle(nint hwnd) {
        try {
            return Interop.Utils.GetWindowTitle(hwnd);
        } catch {
            return UnnamableWindowName;
        }
    }

    public static T Try<T>(Func<T> function, int maxRetry = DefaultRetry, int wait = DefaultWait) {
        int retry = 0;
        do {
            try {
                return function();
            } catch {
                Thread.Sleep(wait);
            }
        } while (retry++ < maxRetry);

        return default;
    }

    public static void Try(Action function, int maxRetry = DefaultRetry, int wait = DefaultWait) {
        int retry = 0;
        do {
            try {
                function();
                return;
            } catch {
                Thread.Sleep(wait);
            }
        } while (retry++ < maxRetry);
    }
}
