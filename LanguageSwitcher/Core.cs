using FruitToolbox.Utils;

namespace FruitToolbox.LanguageSwitcher;

internal static partial class Core {
    public const int WindowActivateWaitMs = 500;

    private static Flyout NewLangFlyout = null;
    public static event EventHandler<Constants.LanguageEvent> NewLanguageEvent;
    private static void InvokeNewLanguageEvent(int lcid)
    {
        NewLanguageEvent?.Invoke(null, new Constants.LanguageEvent(lcid));
    }

    public static bool Start()
    { 
        if(NewLangFlyout != null)
        {
            Interop.stop();
        }

        ToggleFlyoutEnabled(Settings.Core.FlyoutEnabled);
        ToggleExternalHooks(true);
        return Interop.start(InvokeNewLanguageEvent);
    }

    public static void Stop()
    {
        ToggleExternalHooks(false);
        ToggleFlyoutEnabled(false);
        Interop.stop();
    }

    public static void SettingsUpdateHandler(object sender, EventArgs e)
    {
        ToggleFlyoutEnabled(Settings.Core.FlyoutEnabled);
    }

    private static void ToggleExternalHooks(bool enable)
    {
        if(enable)
        {
            Settings.Core.SettingsChangedEventHandler += SettingsUpdateHandler;
        } else
        {
            Settings.Core.SettingsChangedEventHandler -= SettingsUpdateHandler;
        }
    }

    private static void ToggleFlyoutEnabled(bool enable)
    {
        if(enable)
        {
            if(NewLangFlyout == null)
            {
                NewLangFlyout = new();
                NewLangFlyout.Activate();
            }
        } else if(NewLangFlyout != null)
        {
            NewLangFlyout.Dispose();
            NewLangFlyout = null;
        }
    }

    public static void UpdateInputLanguage() => Interop.updateInputLanguage(true);
    public static void UpdateInputLanguageByKeyboard(object _, EventArgs e)
    {
        Thread.Sleep(WindowActivateWaitMs);
        Interop.updateInputLanguage(false);
    }

    public static bool SwapCategory() => Interop.swapCategory();

    public static void SwapCategoryNoReturn(object _, EventArgs e) => Interop.swapCategory();

    public static bool GetCategory() => Interop.getCategory();

    public static void OnRaltUp(object _, EventArgs e) => Interop.onRaltUp();
}
