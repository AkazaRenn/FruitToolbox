using Microsoft.Toolkit.Uwp.Notifications;

namespace FruitToolbox.LanguageSwitcher;

internal static partial class Core {
    private static bool Started = false;

    public const int WindowActivateWaitMs = 500;

    private static Flyout NewLangFlyout = null;
    public static event EventHandler<Constants.LanguageEvent> NewLanguageEvent;
    public static event EventHandler<Constants.LanguageEvent> SwapCategoryEvent;

    public static bool Start() {
        if (Started) {
            return false;
        }

        ToggleExternalHooks(true);
        ToggleStartedState(true);
        Settings.Core.LanguageSwitcherEnabled = Started;
        return Started;
    }

    public static void Stop() {
        if (Started) {
            ToggleExternalHooks(false);
            ToggleStartedState(false);
        }
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
                }
            }
        } else if (!enable && Started) {
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
