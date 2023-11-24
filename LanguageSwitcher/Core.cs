﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using FruitToolbox.Utils;

namespace FruitToolbox.LanguageSwitcher;

internal static partial class Core {
    public const int WindowActivateWaitMs = 500;

    private delegate void LanguageChangedCallbackDelegate(int lcid);

    public static event EventHandler<Constants.LanguageEvent> NewLanguageEvent;
    private static void InvokeNewLanguageEvent(int lcid)
    {
        NewLanguageEvent?.Invoke(null, new Constants.LanguageEvent(lcid));
    }

    [LibraryImport("LanguageSwitcher")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LanguageSwitcher_start(LanguageChangedCallbackDelegate fn);
    public static bool Start() => LanguageSwitcher_start(InvokeNewLanguageEvent);

    [LibraryImport("LanguageSwitcher")]
    private static partial void LanguageSwitcher_stop();
    public static void Stop() => LanguageSwitcher_stop();

    [LibraryImport("LanguageSwitcher")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LanguageSwitcher_ready();
    public static bool Ready() => LanguageSwitcher_ready();

    [LibraryImport("LanguageSwitcher")]
    private static partial void LanguageSwitcher_updateInputLanguage([MarshalAs(UnmanagedType.Bool)] bool doCallback);
    public static void UpdateInputLanguage() => LanguageSwitcher_updateInputLanguage(true);
    public static void UpdateInputLanguageByKeyboard() {
        Thread.Sleep(WindowActivateWaitMs);
        LanguageSwitcher_updateInputLanguage(false);
    }

    [LibraryImport("LanguageSwitcher")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LanguageSwitcher_swapCategory();
    public static bool SwapCategory() => LanguageSwitcher_swapCategory();
    public static void SwapCategoryNoReturn() => LanguageSwitcher_swapCategory();

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
    public static void OnRaltUp() => LanguageSwitcher_onRaltUp();
}
