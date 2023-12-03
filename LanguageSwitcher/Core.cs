using Microsoft.Toolkit.Uwp.Notifications;

namespace FruitToolbox.LanguageSwitcher;

internal class Core : IDisposable {
    static bool Started = false;
    static Core _Instance = null;
    public static Core Instance {
        get {
            _Instance ??= new();
            return _Instance;
        }
    }

    public const int WindowActivateWaitMs = 500;

    static Flyout NewLangFlyout = null;
    public static event EventHandler<Constants.LanguageEvent> NewLanguageEvent;
    public static event EventHandler<Constants.LanguageEvent> SwapCategoryEvent;

    private Core() {
        if (Started) {
            return;
        }

        ToggleStartedState(true);
        Settings.Core.SettingsChangedEventHandler += OnSettingsUpdate;
        Settings.Core.LanguageSwitcherEnabled = Started;
    }

    ~Core() {
        Dispose();
    }

    public void Dispose() {
        if (Started) {
            ToggleStartedState(false);
        }
        Settings.Core.SettingsChangedEventHandler -= OnSettingsUpdate;
        _Instance = null;
        GC.SuppressFinalize(this);
    }

    private static void ToggleStartedState(bool enable) {
        if (enable && !Started) {
            if (Settings.Core.LanguageSwitcherEnabled) {
                Started = Interop.LanguageSwitcher.Start(InvokeSwapCategoryEvent, InvokeNewLanguageEvent);

                if (!Started) {
                    new ToastContentBuilder()
                        .AddText("Unable to enable language switcher")
                        .AddText("Please make sure you have both keyboard languages and IME languages installed")
                        .Show();
                } else {
                    ToggleExternalHooks(true);
                }
            }
        } else if (!enable && Started) {
            ToggleExternalHooks(false);
            ToggleFlyoutEnabled(false);
            Interop.LanguageSwitcher.Stop();
            Started = false;
        }
    }

    private static void ToggleExternalHooks(bool enable) {
        if (enable) {
            Hotkey.Core.CapsLockSwitchLanguageEvent += OnCapsLockSwitch;
            Hotkey.Core.LanguageChangeEvent += OnLanguageChange;
            Hotkey.Core.RAltUpEvent += OnRaltUp;
        } else {
            Hotkey.Core.CapsLockSwitchLanguageEvent -= OnCapsLockSwitch;
            Hotkey.Core.LanguageChangeEvent -= OnLanguageChange;
            Hotkey.Core.RAltUpEvent -= OnRaltUp;
        }
    }

    private static void OnSettingsUpdate(object sender, EventArgs e) {
        if (Started) {
            ToggleFlyoutEnabled(Settings.Core.FlyoutEnabled);
        }

        if (Started != Settings.Core.LanguageSwitcherEnabled) {
            ToggleStartedState(Settings.Core.LanguageSwitcherEnabled);
            Settings.Core.LanguageSwitcherEnabled = Started;
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
    private static void InvokeSwapCategoryEvent(int lcid, bool imeMode) {
        SetScrollLock(imeMode);
        SwapCategoryEvent?.Invoke(null, new Constants.LanguageEvent(lcid, imeMode));
    }

    private static void InvokeNewLanguageEvent(int lcid, bool imeMode) {
        SetScrollLock(imeMode);
        NewLanguageEvent?.Invoke(null, new Constants.LanguageEvent(lcid, imeMode));
        if(Settings.Core.DisableCapsLockOnLanguageChange) {
            Utils.DisableCapsLock();
        }
    }

    private static void SetScrollLock(bool enable) {
        if (Settings.Core.ScrollLockForImeLanguage) {
            Utils.SetScrollLock(enable);
        }
    }

    public static void OnLanguageChange(object _, EventArgs e) {
        Thread.Sleep(WindowActivateWaitMs);
        Interop.LanguageSwitcher.UpdateInputLanguage();
    }

    public static void OnCapsLockSwitch(object _, EventArgs e) =>
        Interop.LanguageSwitcher.SwapCategory();

    public static void OnRaltUp(object _, EventArgs e) =>
        Interop.LanguageSwitcher.OnRaltUp();
}
