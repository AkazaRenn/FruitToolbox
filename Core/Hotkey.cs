using System;
using System.Runtime.InteropServices;

using AutoHotkey.Interop;

namespace FruitLanguageSwitcher.Core {

    internal class Hotkey {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void AHKDelegate();
        enum AHKEvent: ushort {
            onCapsLock = 0,
            onLanguageChange,
            onRaltUp,
        }

        private readonly AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
        private readonly Action onCapsLockHandler;
        private readonly Action onLanguageChangeHandler;
        private readonly Action onRaltUpHandler;

        public Hotkey(
            Action _onCapsLockHandler, 
            Action _onLanguageChangeHandler, 
            Action _onRaltUpHandler) {
            SetVarOnSettings();

            onCapsLockHandler = _onCapsLockHandler;
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.CapsLock));

            onLanguageChangeHandler = _onLanguageChangeHandler;
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.LanguageChangeMonitor));

            onRaltUpHandler = _onRaltUpHandler;
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.RAltModifier));

            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.WinKeyToPTRun));

            ahk.SetVar("AHKEventHandler", GetActionDelegateStr(AHKEventHandler));
        }

        private void AHKEventHandler() {
            switch()
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
