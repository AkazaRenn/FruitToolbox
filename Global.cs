using AutoHotkey.Interop;

namespace FruitToolbox;

internal class Utils {
    public const string AppID = "AkazaRenn.82975CBC0BB1_fhf2jh1qk9hx4!App";

    public class LanguageEvent(int lcid, bool imeLanguage): EventArgs {
        public int LCID { get; } = lcid;
        public bool IMELanguage { get; } = imeLanguage;
    }

    public class WindowEvent(nint hWnd): EventArgs {
        public nint HWnd { get; } = hWnd;
    }

    static readonly AutoHotkeyEngine AutoHotkey = AutoHotkeyEngine.Instance;

    public static void TaskView() {
        AutoHotkey.ExecRaw("Send #{Tab}");
    }

    public static void SetScrollLock(bool enable) {
        AutoHotkey.ExecRaw($"SetScrollLockState, {(enable ? "On" : "Off")}");
    }

    public static void DisableCapsLock() {
        AutoHotkey.ExecRaw("SetCapsLockState, Off");
    }

    public class BidirectionalDictionary<TKey, TValue> {
        private readonly Dictionary<TKey, TValue> _forward = [];
        private readonly Dictionary<TValue, TKey> _backward = [];

        public void Add(TKey key, TValue value) {
            _forward.Add(key, value);
            _backward.Add(value, key);
        }

        public bool TryGetValue(TKey key, out TValue value) => _forward.TryGetValue(key, out value);
        public bool GetKey(TValue value, out TKey key) => _backward.TryGetValue(value, out key);

        public bool ContainsKey(TKey key) => _forward.ContainsKey(key);
        public bool ContainsValue(TValue value) => _backward.ContainsKey(value);

        public bool RemoveKey(TKey key) =>
            _backward.Remove(_forward[key]) && _forward.Remove(key);
        public bool RemoveValue(TValue value) =>
            _forward.Remove(_backward[value]) && _backward.Remove(value);
    }

}
