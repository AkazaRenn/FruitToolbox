using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using AutoHotkey.Interop;

namespace FruitLanguageSwitcher.Core {
    internal class Hotkey {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void AHKDelegate();
        private readonly AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
        private readonly List<GCHandle> handles;

        public Hotkey(AHKDelegate _onCapsLock, AHKDelegate _onLanguageChange, AHKDelegate _onRaltUp) {
            SetVarOnSettings();

            handles.Add(GCHandle.Alloc(_onCapsLock));
            ahk.SetVar("onCapsLockPtr", GetActionDelegateStr(_onCapsLock));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.CapsLock));

            handles.Add(GCHandle.Alloc(_onLanguageChange));
            ahk.SetVar("onLanguageChangePtr", GetActionDelegateStr(_onLanguageChange));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.LanguageChangeMonitor));

            handles.Add(GCHandle.Alloc(_onRaltUp));
            ahk.SetVar("onRaltUpPtr", GetActionDelegateStr(_onRaltUp));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.RAltModifier));

            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.WinKeyToPTRun));
        }

        ~Hotkey() {
            foreach(var handle in handles) {
                handle.Free();
            }

        }

        public void SettingsUpdateHandler(object sender, EventArgs e) {
            SetVarOnSettings();
        }

        public void SetVarOnSettings() {
            ahk.SetVar("LanguageSwitcherEnabled", GetBoolStr(App.Settings.LanguageSwitcherEnabled));
            ahk.SetVar("RAltModifierEnabled", GetBoolStr(App.Settings.RAltModifierEnabled));
            ahk.SetVar("LWinRemapEnabled", GetBoolStr(App.Settings.LWinRemapEnabled));
        }

        static private string GetActionDelegateStr(AHKDelegate act)
            => Marshal.GetFunctionPointerForDelegate(act).ToInt64().ToString();
        static private string GetBoolStr(bool input)
            => Convert.ToUInt16(input).ToString();
    }
}
