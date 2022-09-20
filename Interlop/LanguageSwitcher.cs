using System.Runtime.InteropServices;
using System;

namespace FruitLanguageSwitcher.Interlop
{
    internal class LanguageSwitcher
    {
        private readonly IntPtr wrappedObject;

        [DllImport("LanguageSwitcher")]
        private static extern IntPtr LanguageSwitcher_new(bool defaultImeMode, uint[] imeLanguageOrder, uint n);
        public LanguageSwitcher(bool defaultImeMode, uint[] imeLanguageOrder, uint n) => wrappedObject = LanguageSwitcher_new(defaultImeMode, imeLanguageOrder, n);

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
        private static extern void LanguageSwitcher_orderLanguageList(IntPtr obj, bool isImeLanguageList, uint[] list);
        public void orderLanguageList(bool isImeLanguageList, uint[] list) => LanguageSwitcher_orderLanguageList(wrappedObject, isImeLanguageList, list);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnLanguageChange(bool inImeMode, uint index);
        [DllImport("LanguageSwitcher")]
        private static extern void LanguageSwitcher_setOnLanguageChange(IntPtr obj, OnLanguageChange handler);
        public void setOnLanguageChange(OnLanguageChange handler) => LanguageSwitcher_setOnLanguageChange(wrappedObject, handler);
    }
}
