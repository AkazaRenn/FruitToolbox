using System.Runtime.InteropServices;
using System;

namespace FruitLanguageSwitcher.Core {
    internal class LanguageSwitcher {
        private IntPtr wrappedObject;

        [DllImport("LanguageSwitcher")]
        private static extern IntPtr LanguageSwitcher_new(bool defaultImeMode);
        public LanguageSwitcher(bool defaultImeMode) => wrappedObject = LanguageSwitcher_new(defaultImeMode);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_delete(IntPtr obj);
        ~LanguageSwitcher() => LanguageSwitcher_delete(wrappedObject);

        public void reload() {
            LanguageSwitcher_delete(wrappedObject);
            wrappedObject = LanguageSwitcher_new(false);
        }

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_swapCategory(IntPtr obj);
        public bool swapCategory() => LanguageSwitcher_swapCategory(wrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_getCategory(IntPtr obj);
        public bool getCategory() => LanguageSwitcher_getCategory(wrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern uint LanguageSwitcher_nextLanguage(IntPtr obj);
        public uint nextLanguage() => LanguageSwitcher_nextLanguage(wrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern uint LanguageSwitcher_lastLanguage(IntPtr obj);
        public uint lastLanguage() => LanguageSwitcher_lastLanguage(wrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern uint LanguageSwitcher_getCurrentLanguage(IntPtr obj);
        public uint getCurrentLanguage() => LanguageSwitcher_getCurrentLanguage(wrappedObject);

        [DllImport("LanguageSwitcher")]
        private static extern bool LanguageSwitcher_setCurrentLanguage(IntPtr obj, uint newLanguage);
        public bool setCurrentLanguage(uint newLanguage) => LanguageSwitcher_setCurrentLanguage(wrappedObject, newLanguage);

        [DllImport("LanguageSwitcher")]
        private static extern uint LanguageSwitcher_getLanguageList(IntPtr obj, bool isImeLanguageList, uint[] list);
        public uint getLanguageList(bool isImeLanguageList, uint[] list) => LanguageSwitcher_getLanguageList(wrappedObject, isImeLanguageList, list);

        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_orderLanguageList(IntPtr obj, bool isImeLanguageList, uint[] list, uint n);
        public void orderLanguageList(bool isImeLanguageList, uint[] list) => LanguageSwitcher_orderLanguageList(wrappedObject, isImeLanguageList, list, (uint)list.Length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnLanguageChange(bool inImeMode, uint index);
        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_setOnLanguageChange(IntPtr obj, OnLanguageChange handler);
        public void setOnLanguageChange(OnLanguageChange handler) => LanguageSwitcher_setOnLanguageChange(wrappedObject, handler);
    }
}
