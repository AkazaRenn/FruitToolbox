using System.Runtime.InteropServices;

using FruitToolbox.Utils;

namespace FruitToolbox.LanguageSwitcher;

internal static partial class Core {
    public const int WindowActivateWaitMs = 500;

    private static Flyout NewLangFlyout = null;
    private delegate void LanguageChangedCallbackDelegate(int lcid);
    public static event EventHandler<Constants.LanguageEvent> NewLanguageEvent;
    private static void InvokeNewLanguageEvent(int lcid)
    {
        NewLanguageEvent?.Invoke(null, new Constants.LanguageEvent(lcid));
    }

    [LibraryImport("LanguageSwitcher")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LanguageSwitcher_start(LanguageChangedCallbackDelegate fn);
    public static bool Start()
    { 
        if(NewLangFlyout != null)
        {
            Stop();
        }

        ToggleFlyoutEnabled(Settings.Core.FlyoutEnabled);
        Settings.Core.SettingsChangedEventHandler += SettingsUpdateHandler;
        return LanguageSwitcher_start(InvokeNewLanguageEvent);
    }

    [LibraryImport("LanguageSwitcher")]
    private static partial void LanguageSwitcher_stop();
    public static void Stop()
    {
        Settings.Core.SettingsChangedEventHandler -= SettingsUpdateHandler;
        ToggleFlyoutEnabled(false);
        LanguageSwitcher_stop();
    }

    public static void SettingsUpdateHandler(object sender, EventArgs e)
    {
        ToggleFlyoutEnabled(Settings.Core.FlyoutEnabled);
    }

    private static void ToggleFlyoutEnabled(bool enabled)
    {
        if(enabled)
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

    [LibraryImport("LanguageSwitcher")]
    private static partial void LanguageSwitcher_updateInputLanguage([MarshalAs(UnmanagedType.Bool)] bool doCallback);
    public static void UpdateInputLanguage() => LanguageSwitcher_updateInputLanguage(true);
    public static void UpdateInputLanguageByKeyboard(object _, EventArgs e)
    {
        Thread.Sleep(WindowActivateWaitMs);
        LanguageSwitcher_updateInputLanguage(false);
    }

    [LibraryImport("LanguageSwitcher")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LanguageSwitcher_swapCategory();
    public static bool SwapCategory() => LanguageSwitcher_swapCategory();
    public static void SwapCategoryNoReturn(object _, EventArgs e) => LanguageSwitcher_swapCategory();

    [LibraryImport("LanguageSwitcher")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LanguageSwitcher_getCategory();
    public static bool GetCategory() => LanguageSwitcher_getCategory();

    [LibraryImport("LanguageSwitcher")]
    private static partial uint LanguageSwitcher_getCurrentLanguage();
    public static uint GetCurrentLanguage() => LanguageSwitcher_getCurrentLanguage();

    [LibraryImport("LanguageSwitcher")]
    private static partial void LanguageSwitcher_setCurrentLanguage(uint newLanguage);
    public static void SetCurrentLanguage(uint newLanguage) => LanguageSwitcher_setCurrentLanguage(newLanguage);

    [LibraryImport("LanguageSwitcher")]
    private static partial uint LanguageSwitcher_getLanguageList([MarshalAs(UnmanagedType.Bool)] bool isImeLanguageList, uint[] list);
    public static uint GetLanguageList(bool isImeLanguageList, uint[] list) => LanguageSwitcher_getLanguageList(isImeLanguageList, list);

    [LibraryImport("LanguageSwitcher")]
    private static partial void LanguageSwitcher_onRaltUp();
    public static void OnRaltUp(object _, EventArgs e) => LanguageSwitcher_onRaltUp();
}
