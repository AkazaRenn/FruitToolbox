using System;
using System.Runtime.InteropServices;
using System.Threading;

using AutoHotkey.Interop;

namespace FruitLanguageSwitcher.Core
{
    internal class Hotkey {
        public const int onCapsLockMessage = 1;
        public const int onLanguageChangeMessage = 2;
        public const int onRaltDownMessage = 3;
        public const int onRaltUpMessage = 4;

        public const int windowActivateWaitMs = 500;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void AHKDelegate(int s);

        private readonly AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
        private static Action onCapsLock;
        private static Action onLanguageChange;
        private static Action onRaltDown;
        private static Action onRaltUp;

        public Hotkey(Action _onCapsLock, Action _onLanguageChange, Action _onRaltDown, Action _onRaltUp) {
            onCapsLock = _onCapsLock;
            onLanguageChange = _onLanguageChange;
            onRaltDown = _onRaltDown;
            onRaltUp = _onRaltUp;

            SetUpAhk();
        }

        public void Reload() {
            ahk.Reset();
            SetUpAhk();
        }

        private void SetUpAhk()
        {
            IntPtr ptr = Marshal.GetFunctionPointerForDelegate((AHKDelegate)ipcHandler);
            ahk.SetVar("ptr", ptr.ToInt64().ToString());

            ahk.SetVar("onCapsLock", onCapsLockMessage.ToString());
            ahk.SetVar("onLanguageChange", onLanguageChangeMessage.ToString());
            ahk.SetVar("onRaltDown", onRaltDownMessage.ToString());
            ahk.SetVar("onRaltUp", onRaltUpMessage.ToString());

            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.CapsLock));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.LanguageChangeMonitor));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.WinKeyToPTRun));
            ahk.ExecRaw(System.Text.Encoding.Default.GetString(Properties.Resources.RAltModifier));
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
            case onRaltDownMessage:
                onRaltDown();
                break;
            case onRaltUpMessage:
                onRaltUp();
                break;
            }
        }
    }
}
