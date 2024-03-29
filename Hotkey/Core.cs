﻿using System.Runtime.InteropServices;

using AutoHotkey.Interop;

using FruitToolbox.Utils;


namespace FruitToolbox.Hotkey;

internal static class Core
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void AHKDelegate();
    public static event EventHandler<EventArgs> CapsLockSwitchLanguageEvent;
    public static event EventHandler<EventArgs> CapsLockOnEvent;
    public static event EventHandler<EventArgs> CapsLockOffEvent;
    public static event EventHandler<EventArgs> LanguageChangeEvent;
    public static event EventHandler<EventArgs> RAltUpEvent;
    public static event EventHandler<EventArgs> HomeEvent;
    private static readonly AutoHotkeyEngine AHKEngine = AutoHotkeyEngine.Instance;
    private static bool Initialized = false;

    public static void Start()
    {
        if (Initialized)
        {
            return;
        }

        InitializeHandlers();
        SetVarOnSettings();

        Settings.Core.SettingsChangedEventHandler += SettingsUpdateHandler;
        Initialized = true;
    }

    private static void InitializeHandlers() 
    { 
        AHKEngine.SetVar("onCapsLockLanguageSwitchPtr", GetActionDelegateStr(OnCapsLockSwitchLanguage));
        AHKEngine.SetVar("onCapsLockOnPtr", GetActionDelegateStr(OnCapsLockOn));
        AHKEngine.SetVar("onCapsLockOffPtr", GetActionDelegateStr(OnCapsLockOffEvent));
        AHKEngine.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.CapsLock));

        AHKEngine.SetVar("onLanguageChangePtr", GetActionDelegateStr(OnLanguageChange));
        AHKEngine.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.LanguageChangeMonitor));

        AHKEngine.SetVar("onRaltUpPtr", GetActionDelegateStr(OnRAltUp));
        AHKEngine.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.RAltModifier));

        AHKEngine.SetVar("onHomePtr", GetActionDelegateStr(OnHome));
        AHKEngine.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.DesktopToHome));

        AHKEngine.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.RGuiToPTRun));
        AHKEngine.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.ReverseMouseWheel));
    }

    private static void SetVarOnSettings()
    {
        AHKEngine.SetVar("LanguageSwitcherEnabled", GetBoolStr(Settings.Core.LanguageSwitcherEnabled));
        AHKEngine.SetVar("RAltModifierEnabled", GetBoolStr(Settings.Core.RAltModifierEnabled));
        AHKEngine.SetVar("LGuiRemapEnabled", GetBoolStr(Settings.Core.LGuiRemapEnabled));
        AHKEngine.SetVar("ReverseMouseWheelEnabled", GetBoolStr(Settings.Core.ReverseMouseWheelEnabled));
        AHKEngine.SetVar("DesktopToHomeEnabled", GetBoolStr(Settings.Core.DesktopToHomeEnabled));
    }

    private static void OnCapsLockSwitchLanguage()
    {
        CapsLockSwitchLanguageEvent?.Invoke(null, null);
    }

    private static void OnCapsLockOn()
    {
        CapsLockOnEvent?.Invoke(null, null);
    }

    private static void OnCapsLockOffEvent()
    {
        CapsLockOffEvent?.Invoke(null, null);
    }

    private static void OnLanguageChange()
    {
        LanguageChangeEvent?.Invoke(null, null);
    }

    private static void OnRAltUp()
    {
        RAltUpEvent?.Invoke(null, null);
    }

    private static void OnHome()
    {
        HomeEvent?.Invoke(null, null);
    }

    private static void SettingsUpdateHandler(object sender, EventArgs e)
    {
        SetVarOnSettings();
    }

    public static string GetActionDelegateStr(AHKDelegate act)
        => Marshal.GetFunctionPointerForDelegate(act).ToInt64().ToString();
    public static string GetBoolStr(bool input)
        => Convert.ToUInt16(input).ToString();
}
