using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace FruitLanguageSwitcher.Core {
    internal class LanguageSwitcher {
        public const int WindowActivateWaitMs = 500;

        private IntPtr WrappedObject;
        private delegate void LanguageChangedCallbackDelegate(int lcid);

        public delegate void EventHandler(object sender, LanguageEvent e);
        public static event EventHandler<LanguageEvent> NewLanguageEvent;

        public class LanguageEvent: EventArgs
        {
            public LanguageEvent(int lcid)
            {
                LCID = lcid;
            }

            public int LCID { get; }
        }
        private static void InvokeNewLanguageEvent(int lcid)
        {
            NewLanguageEvent(null, new LanguageEvent(lcid));
        }

        [DllImport("LanguageSwitcher")]
        private static extern IntPtr LanguageSwitcher_new(LanguageChangedCallbackDelegate fn);
        public LanguageSwitcher() => WrappedObject = LanguageSwitcher_new(InvokeNewLanguageEvent);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_delete(IntPtr obj);
        ~LanguageSwitcher() => LanguageSwitcher_delete(WrappedObject);

        public void Reload() {
            LanguageSwitcher_delete(WrappedObject);
            WrappedObject = LanguageSwitcher_new(InvokeNewLanguageEvent);
        }

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_ready(IntPtr obj);
        public bool Ready() => LanguageSwitcher_ready(WrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_updateInputLanguage(IntPtr obj, bool doCallback);
        public void UpdateInputLanguage() {
            LanguageSwitcher_updateInputLanguage(WrappedObject, true);
        }
        public void UpdateInputLanguageByKeyboard() {
            Thread.Sleep(WindowActivateWaitMs);
            LanguageSwitcher_updateInputLanguage(WrappedObject, false);
        }

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_swapCategory(IntPtr obj);
        public bool SwapCategory() => LanguageSwitcher_swapCategory(WrappedObject);
        public void SwapCategoryNoReturn() {
            LanguageSwitcher_swapCategory(WrappedObject);
        }

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_getCategory(IntPtr obj);
        public bool GetCategory() => LanguageSwitcher_getCategory(WrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern uint LanguageSwitcher_getCurrentLanguage(IntPtr obj);
        public uint GetCurrentLanguage() => LanguageSwitcher_getCurrentLanguage(WrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_setCurrentLanguage(IntPtr obj, uint newLanguage);
        public void SetCurrentLanguage(uint newLanguage) => LanguageSwitcher_setCurrentLanguage(WrappedObject, newLanguage);

        [DllImport("LanguageSwitcher")]
        private static extern uint LanguageSwitcher_getLanguageList(IntPtr obj, bool isImeLanguageList, uint[] list);
        public uint GetLanguageList(bool isImeLanguageList, uint[] list) => LanguageSwitcher_getLanguageList(WrappedObject, isImeLanguageList, list);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_onRaltUp(IntPtr obj);
        public void OnRaltUp() => LanguageSwitcher_onRaltUp(WrappedObject);
    }
}
