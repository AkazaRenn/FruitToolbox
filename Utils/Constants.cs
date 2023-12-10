using AutoHotkey.Interop;

namespace FruitToolbox.Utils;

internal class Constants {
    public const string AppID = "AkazaRenn.82975CBC0BB1_fhf2jh1qk9hx4!App";
    public const string AppName = "FruitToolbox";

    static readonly AutoHotkeyEngine AutoHotkey = AutoHotkeyEngine.Instance;

    public static void SetScrollLock(bool enable) {
        AutoHotkey.ExecRaw($"SetScrollLockState, {(enable ? "On" : "Off")}");
    }

    public static void DisableCapsLock() {
        AutoHotkey.ExecRaw("SetCapsLockState, Off");
    }

    public class LanguageEvent(int lcid, bool imeLanguage): EventArgs {
        public int LCID { get; } = lcid;
        public bool IMELanguage { get; } = imeLanguage;
    }

    public class WindowEvent(nint hWnd): EventArgs {
        public nint HWnd { get; } = hWnd;
    }
}
