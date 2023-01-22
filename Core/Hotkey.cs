using System;
using System.Runtime.InteropServices;

using AutoHotkey.Interop;

namespace FruitLanguageSwitcher.Core {
    internal class Hotkey {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void AHKDelegate();
        private readonly AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
        private readonly Action onCapsLock;
        private readonly Action onLanguageChange;
        private readonly Action onRaltUp;

        public Hotkey(Action _onCapsLock, Action _onLanguageChange, Action _onRaltUp) {
            SetVarOnSettings();

            onCapsLock = _onCapsLock;
            ahk.SetVar("onCapsLockPtr", GetActionDelegateStr(onCapsLock));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.CapsLock));

            onLanguageChange = _onLanguageChange;
            ahk.SetVar("onLanguageChangePtr", GetActionDelegateStr(onLanguageChange));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.LanguageChangeMonitor));

            onRaltUp = _onRaltUp;
            ahk.SetVar("onRaltUpPtr", GetActionDelegateStr(onRaltUp));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.RAltModifier));

            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.WinKeyToPTRun));
        }

        public void SettingsUpdateHandler(object sender, EventArgs e) {
            SetVarOnSettings();
        }

        public void SetVarOnSettings() {
            ahk.SetVar("LanguageSwitcherEnabled", GetBoolStr(App.Settings.LanguageSwitcherEnabled));
            ahk.SetVar("RAltModifierEnabled", GetBoolStr(App.Settings.RAltModifierEnabled));
            ahk.SetVar("LWinRemapEnabled", GetBoolStr(App.Settings.LWinRemapEnabled));
        }

        static private string GetActionDelegateStr(Action act)
            => Marshal.GetFunctionPointerForDelegate((AHKDelegate)act.Invoke).ToInt64().ToString();
        static private string GetBoolStr(bool input)
            => Convert.ToUInt16(input).ToString();
    }
}
