using System.Runtime.InteropServices;

using AutoHotkey.Interop;

namespace FruitToolbox.Hotkey;

internal class Core : IDisposable {
    private static bool Started = false;
    static Core _Instance = null;
    public static Core Instance {
        get {
            _Instance ??= new();
            return _Instance;
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void AHKDelegate();
    private static readonly AutoHotkeyEngine AHKEngine = AutoHotkeyEngine.Instance;

    public static event EventHandler<EventArgs> CapsLockSwitchLanguageEvent;
    private static void OnCapsLockSwitchLanguage() => CapsLockSwitchLanguageEvent?.Invoke(null, null);

    public static event EventHandler<EventArgs> CapsLockOnEvent;
    private static void OnCapsLockOn() => CapsLockOnEvent?.Invoke(null, null);

    public static event EventHandler<EventArgs> CapsLockOffEvent;
    private static void OnCapsLockOffEvent() => CapsLockOffEvent?.Invoke(null, null);

    public static event EventHandler<EventArgs> LanguageChangeEvent;
    private static void OnLanguageChange() => LanguageChangeEvent?.Invoke(null, null);

    public static event EventHandler<EventArgs> RAltUpEvent;
    private static void OnRAltUp() => RAltUpEvent?.Invoke(null, null);

    private Core() {
        if (Started) {
            return;
        }

        InitializeHandlers();
        SetVarOnSettings();

        Settings.Core.SettingsChangedEventHandler += OnSettingsUpdate;
        Started = true;
    }

    ~Core() {
        Dispose();
    }

    public void Dispose() {
        _Instance = null;
        Settings.Core.SettingsChangedEventHandler -= OnSettingsUpdate;
        AHKEngine.Reset();
        Started = false;
        GC.SuppressFinalize(this);
    }

    private static void InitializeHandlers() {
        AHKEngine.SetVar("onCapsLockLanguageSwitchPtr", GetActionDelegateStr(OnCapsLockSwitchLanguage));
        AHKEngine.SetVar("onCapsLockOnPtr", GetActionDelegateStr(OnCapsLockOn));
        AHKEngine.SetVar("onCapsLockOffPtr", GetActionDelegateStr(OnCapsLockOffEvent));
        AHKEngine.SetVar("onLanguageChangePtr", GetActionDelegateStr(OnLanguageChange));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.LanguageSwitcher));

        AHKEngine.SetVar("onRaltUpPtr", GetActionDelegateStr(OnRAltUp));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.RAltModifier));

        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.SwapVirtualDesktopHotkeys));

        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.LGuiRemap));
        AHKEngine.LoadScript(System.Text.Encoding.Default.GetString(Properties.Resources.ReverseMouseWheel));
    }

    private static void SetVarOnSettings() {
        AHKEngine.SetVar("LanguageSwitcherEnabled", GetBoolStr(Settings.Core.LanguageSwitcherEnabled));
        AHKEngine.SetVar("RAltModifierEnabled", GetBoolStr(Settings.Core.RAltModifierEnabled));
        AHKEngine.SetVar("LGuiRemapEnabled", GetBoolStr(Settings.Core.LGuiRemapEnabled));
        AHKEngine.SetVar("ReverseMouseWheelEnabled", GetBoolStr(Settings.Core.ReverseMouseWheelEnabled));
        AHKEngine.SetVar("SwapVirtualDesktopHotkeysEnabled", GetBoolStr(Settings.Core.SwapVirtualDesktopHotkeysEnabled));
    }

    private static void OnSettingsUpdate(object sender, EventArgs e) {
        SetVarOnSettings();
    }

    public static string GetActionDelegateStr(AHKDelegate act)
        => Marshal.GetFunctionPointerForDelegate(act).ToInt64().ToString();
    public static string GetBoolStr(bool input)
        => Convert.ToUInt16(input).ToString();
}
