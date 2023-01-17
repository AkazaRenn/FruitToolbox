using System;
using System.Runtime.InteropServices;
using System.Threading;

using AutoHotkey.Interop;

namespace FruitLanguageSwitcher.Core
{
    internal class Hotkey {
        public const int onCapsLockMessage = 1;
        public const int onLanguageChangeMessage = 2;
                    
        public const int windowActivateWaitMs = 100;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void AHKDelegate(int s);

        private readonly AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
        private static Action onCapsLock;
        private static Action onLanguageChange;

        public Hotkey(Action _onCapsLock, Action _onLanguageChange) {
            onCapsLock = _onCapsLock;
            onLanguageChange = _onLanguageChange;

            IntPtr ptr = Marshal.GetFunctionPointerForDelegate((AHKDelegate)ipcHandler);
            ahk.SetVar("ptr", ptr.ToInt64().ToString());

            ahk.SetVar("onCapsLock", onCapsLockMessage.ToString());
            ahk.SetVar("onLanguageChange", onLanguageChangeMessage.ToString());

            ahk.ExecRaw(System.Text.Encoding.Default.GetString(FruitLanguageSwitcher.Properties.Resources.CapsLock));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(FruitLanguageSwitcher.Properties.Resources.LanguageChangeMonitor));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(FruitLanguageSwitcher.Properties.Resources.WinKeyToPTRun));
        }

        static private void ipcHandler(int fromAhk) {
            switch(fromAhk) {
            case onCapsLockMessage:
                onCapsLock();
                break;
            case onLanguageChangeMessage:
                // wait for the window to actually go back active
                Thread.Sleep(windowActivateWaitMs);
                onLanguageChange();
                break;
            }
        }
    }
}
