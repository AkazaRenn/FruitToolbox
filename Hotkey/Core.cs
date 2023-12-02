using System.Runtime.InteropServices;

using AutoHotkey.Interop;

namespace FruitToolbox.Hotkey;

internal static class Core {
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void AHKDelegate();
    public static event EventHandler<EventArgs> CapsLockSwitchLanguageEvent;
    public static event EventHandler<EventArgs> CapsLockOnEvent;
    public static event EventHandler<EventArgs> CapsLockOffEvent;
    public static event EventHandler<EventArgs> LanguageChangeEvent;
    public static event EventHandler<EventArgs> RAltUpEvent;
    public static event EventHandler<EventArgs> HomeEvent;
    public static event EventHandler<EventArgs> GuiUpEvent;
    public static event EventHandler<EventArgs> GuiDownEvent;
    private static readonly AutoHotkeyEngine AHKEngine = AutoHotkeyEngine.Instance;
    private static bool Started = false;

    public static void Start() {
        if (Started) {
            return;
        }

        InitializeHandlers();
        SetVarOnSettings();

        Settings.Core.SettingsChangedEventHandler += SettingsUpdateHandler;
        Started = true;
    }

    private static void InitializeHandlers() {
        AHKEngine.SetVar("onCapsLockLanguageSwitchPtr", GetActionDelegateStr(OnCapsLockSwitchLanguage));
        AHKEngine.SetVar("onCapsLockOnPtr", GetActionDelegateStr(OnCapsLockOn));
        AHKEngine.SetVar("onCapsLockOffPtr", GetActionDelegateStr(OnCapsLockOffEvent));
        AHKEngine.SetVar("onLanguageChangePtr", GetActionDelegateStr(OnLanguageChange));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.LanguageSwitcher));

        AHKEngine.SetVar("onRaltUpPtr", GetActionDelegateStr(OnRAltUp));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.RAltModifier));

        AHKEngine.SetVar("onDesktopPtr", GetActionDelegateStr(OnDesktop));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.DesktopToHome));

        AHKEngine.SetVar("onGuiUpPtr", GetActionDelegateStr(OnGuiUp));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.SwapVirtualDesktopHotkeys));

        AHKEngine.SetVar("onGuiDownPtr", GetActionDelegateStr(OnGuiDown));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.SwapVirtualDesktopHotkeys));

        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.LGuiRemap));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.ReverseMouseWheel));
    }

    private static void SetVarOnSettings() {
        AHKEngine.SetVar("LanguageSwitcherEnabled", GetBoolStr(Settings.Core.LanguageSwitcherEnabled));
        AHKEngine.SetVar("RAltModifierEnabled", GetBoolStr(Settings.Core.RAltModifierEnabled));
        AHKEngine.SetVar("LGuiRemapEnabled", GetBoolStr(Settings.Core.LGuiRemapEnabled));
        AHKEngine.SetVar("ReverseMouseWheelEnabled", GetBoolStr(Settings.Core.ReverseMouseWheelEnabled));
        AHKEngine.SetVar("DesktopToHomeEnabled", GetBoolStr(Settings.Core.DesktopToHomeEnabled));
        AHKEngine.SetVar("SwapVirtualDesktopHotkeysEnabled", GetBoolStr(Settings.Core.SwapVirtualDesktopHotkeysEnabled));
    }

    private static void OnCapsLockSwitchLanguage() {
        CapsLockSwitchLanguageEvent?.Invoke(null, null);
    }

    private static void OnCapsLockOn() {
        CapsLockOnEvent?.Invoke(null, null);
    }

    private static void OnCapsLockOffEvent() {
        CapsLockOffEvent?.Invoke(null, null);
    }

    private static void OnLanguageChange() {
        LanguageChangeEvent?.Invoke(null, null);
    }

    private static void OnRAltUp() {
        RAltUpEvent?.Invoke(null, null);
    }

    private static void OnDesktop() {
        HomeEvent?.Invoke(null, null);
    }

    private static void OnGuiUp() {
        GuiUpEvent?.Invoke(null, null);
    }

    private static void OnGuiDown() {
        GuiDownEvent?.Invoke(null, null);
    }

    private static void SettingsUpdateHandler(object sender, EventArgs e) {
        SetVarOnSettings();
    }

    public static string GetActionDelegateStr(AHKDelegate act)
        => Marshal.GetFunctionPointerForDelegate(act).ToInt64().ToString();
    public static string GetBoolStr(bool input)
        => Convert.ToUInt16(input).ToString();
}
