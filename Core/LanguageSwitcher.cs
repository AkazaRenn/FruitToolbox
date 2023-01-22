using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace FruitLanguageSwitcher.Core {
    internal class LanguageSwitcher {
        public const int windowActivateWaitMs = 500;

        private IntPtr wrappedObject;

        [DllImport("LanguageSwitcher")]
        private static extern IntPtr LanguageSwitcher_new();
        public LanguageSwitcher() => wrappedObject = LanguageSwitcher_new();

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_delete(IntPtr obj);
        ~LanguageSwitcher() => LanguageSwitcher_delete(wrappedObject);

        public void Reload() {
            LanguageSwitcher_delete(wrappedObject);
            wrappedObject = LanguageSwitcher_new();
        }

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_ready(IntPtr obj);
        public bool Ready() => LanguageSwitcher_ready(wrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_updateInputLanguage(IntPtr obj);
        public void UpdateInputLanguage() {
            LanguageSwitcher_updateInputLanguage(wrappedObject);
        }
        public void UpdateInputLanguageByKeyboard() {
            Thread.Sleep(windowActivateWaitMs);
            LanguageSwitcher_updateInputLanguage(wrappedObject);
        }

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_swapCategory(IntPtr obj);
        public bool SwapCategory() => LanguageSwitcher_swapCategory(wrappedObject);
        public void SwapCategoryNoReturn() {
            LanguageSwitcher_swapCategory(wrappedObject);
        }

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_getCategory(IntPtr obj);
        public bool GetCategory() => LanguageSwitcher_getCategory(wrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern uint LanguageSwitcher_getCurrentLanguage(IntPtr obj);
        public uint GetCurrentLanguage() => LanguageSwitcher_getCurrentLanguage(wrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_setCurrentLanguage(IntPtr obj, uint newLanguage);
        public void SetCurrentLanguage(uint newLanguage) => LanguageSwitcher_setCurrentLanguage(wrappedObject, newLanguage);

        [DllImport("LanguageSwitcher")]
        private static extern uint LanguageSwitcher_getLanguageList(IntPtr obj, bool isImeLanguageList, uint[] list);
        public uint GetLanguageList(bool isImeLanguageList, uint[] list) => LanguageSwitcher_getLanguageList(wrappedObject, isImeLanguageList, list);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_onRaltUp(IntPtr obj);
        public void OnRaltUp() => LanguageSwitcher_onRaltUp(wrappedObject);
    }
}
