﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using AutoHotkey.Interop;

namespace FruitToolbox.Hotkey;

internal class Core
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void AHKDelegate();
    private readonly AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
    private static readonly List<AHKDelegate> handlers = new();

    public Core(AHKDelegate _onCapsLock, AHKDelegate _onLanguageChange, AHKDelegate _onRaltUp)
    {
        SetVarOnSettings();

        handlers.Add(_onCapsLock);
        ahk.SetVar("onCapsLockPtr", GetActionDelegateStr(_onCapsLock));
        ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.CapsLock));

        handlers.Add(_onLanguageChange);
        ahk.SetVar("onLanguageChangePtr", GetActionDelegateStr(_onLanguageChange));
        ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.LanguageChangeMonitor));

        handlers.Add(_onRaltUp);
        ahk.SetVar("onRaltUpPtr", GetActionDelegateStr(_onRaltUp));
        ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.RAltModifier));

        ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.WinKeyToPTRun));
        ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.ReverseMouseWheel));
    }

    public void SettingsUpdateHandler(object sender, EventArgs e)
    {
        SetVarOnSettings();
    }

    public void SetVarOnSettings()
    {
        ahk.SetVar("LanguageSwitcherEnabled", GetBoolStr(Settings.Core.LanguageSwitcherEnabled));
        ahk.SetVar("RAltModifierEnabled", GetBoolStr(Settings.Core.RAltModifierEnabled));
        ahk.SetVar("LWinRemapEnabled", GetBoolStr(Settings.Core.LGuiRemapEnabled));
        ahk.SetVar("ReverseMouseWheelEnabled", GetBoolStr(Settings.Core.ReverseMouseWheelEnabled));
    }

    static private string GetActionDelegateStr(AHKDelegate act)
        => Marshal.GetFunctionPointerForDelegate(act).ToInt64().ToString();
    static private string GetBoolStr(bool input)
        => Convert.ToUInt16(input).ToString();
}
