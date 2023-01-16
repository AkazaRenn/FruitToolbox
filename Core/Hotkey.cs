using System;
using System.IO;
using System.Security.Cryptography;

using AutoHotkey.Interop;

using FruitLanguageSwitcher.Properties;

namespace FruitLanguageSwitcher.Core
{
    internal class Hotkey {
        public const string onCapsLockMessage = "a";
        public const string onLanguageChangeMessage = "b";

        private readonly AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
        private Action onCapsLock;
        private Action onLanguageChange;

        public Hotkey(Action onCapsLock, Action onLanguageChange) {
            this.onCapsLock = onCapsLock;
            this.onLanguageChange = onLanguageChange;

            ahk.InitalizePipesModule(ipcHandler);

            ahk.SetVar("OnCapsLock", onCapsLockMessage);
            ahk.SetVar("onLanguageChange", onLanguageChangeMessage);

            ahk.ExecRaw(System.Text.Encoding.Default.GetString(FruitLanguageSwitcher.Properties.Resources.CapsLock));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(FruitLanguageSwitcher.Properties.Resources.LanguageChangeMonitor));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(FruitLanguageSwitcher.Properties.Resources.WinKeyToPTRun));
        }

        private string ipcHandler(string fromAhk) {
            switch(fromAhk) {
            case onCapsLockMessage:
                onCapsLock();
                break;
            case onLanguageChangeMessage:
                onLanguageChange();
                break;
            }

            return ".NET: I LIKE PIE!";
        }
    }
}
