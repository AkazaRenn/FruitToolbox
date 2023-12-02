namespace FruitToolbox.LanguageSwitcher;

internal static partial class Core {
    public const int WindowActivateWaitMs = 500;

    private static Flyout NewLangFlyout = null;
    public static event EventHandler<Constants.LanguageEvent> NewLanguageEvent;
    private static void InvokeNewLanguageEvent(int lcid) =>
        NewLanguageEvent?.Invoke(null, new Constants.LanguageEvent(lcid));

    public static bool Start() {
        if (NewLangFlyout != null) {
            Interop.LanguageSwitcher.Stop();
        }

        ToggleFlyoutEnabled(Settings.Core.FlyoutEnabled);
        ToggleExternalHooks(true);
        return Interop.LanguageSwitcher.Start(InvokeNewLanguageEvent);
    }

    public static void Stop() {
        ToggleExternalHooks(false);
        ToggleFlyoutEnabled(false);
        Interop.LanguageSwitcher.Stop();
    }

    public static void SettingsUpdateHandler(object sender, EventArgs e) =>
        ToggleFlyoutEnabled(Settings.Core.FlyoutEnabled);

    private static void ToggleExternalHooks(bool enable) {
        if (enable) {
            Settings.Core.SettingsChangedEventHandler += SettingsUpdateHandler;
            Hotkey.Core.CapsLockSwitchLanguageEvent += SwapCategory;
            Hotkey.Core.LanguageChangeEvent += UpdateInputLanguageByKeyboard;
            Hotkey.Core.RAltUpEvent += OnRaltUp;
        } else {
            Settings.Core.SettingsChangedEventHandler -= SettingsUpdateHandler;
            Hotkey.Core.CapsLockSwitchLanguageEvent -= SwapCategory;
            Hotkey.Core.LanguageChangeEvent -= UpdateInputLanguageByKeyboard;
            Hotkey.Core.RAltUpEvent -= OnRaltUp;
        }
    }

    private static void ToggleFlyoutEnabled(bool enable) {
        if (enable) {
            if (NewLangFlyout == null) {
                NewLangFlyout = new();
                NewLangFlyout.Activate();
            }
        } else if (NewLangFlyout != null) {
            NewLangFlyout.Dispose();
            NewLangFlyout = null;
        }
    }

    public static void UpdateInputLanguageByKeyboard(object _, EventArgs e) {
        Thread.Sleep(WindowActivateWaitMs);
        Interop.LanguageSwitcher.UpdateInputLanguage();
    }

    public static void SwapCategory(object _, EventArgs e) =>
        Interop.LanguageSwitcher.SwapCategory();

    public static void OnRaltUp(object _, EventArgs e) =>
        Interop.LanguageSwitcher.OnRaltUp();
}
