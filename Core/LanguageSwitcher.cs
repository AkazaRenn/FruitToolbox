using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace FruitLanguageSwitcher.Core {
    internal partial class LanguageSwitcher {
        public const int WindowActivateWaitMs = 500;

        private static IntPtr WrappedObject;
        private delegate void LanguageChangedCallbackDelegate(int lcid);

        public static event EventHandler<Constants.LanguageEvent> NewLanguageEvent;
        private static void InvokeNewLanguageEvent(int lcid)
        {
            NewLanguageEvent?.Invoke(null, new Constants.LanguageEvent(lcid));
        }

        [LibraryImport("LanguageSwitcher")]
        private static partial IntPtr LanguageSwitcher_new(LanguageChangedCallbackDelegate fn);
        public LanguageSwitcher() => WrappedObject = LanguageSwitcher_new(InvokeNewLanguageEvent);

        [LibraryImport("LanguageSwitcher")]
        private static partial void LanguageSwitcher_delete(IntPtr obj);
        ~LanguageSwitcher() => LanguageSwitcher_delete(WrappedObject);

        public void Reload() {
            LanguageSwitcher_delete(WrappedObject);
            WrappedObject = LanguageSwitcher_new(InvokeNewLanguageEvent);
        }

        [LibraryImport("LanguageSwitcher")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool LanguageSwitcher_ready(IntPtr obj);
        public bool Ready() => LanguageSwitcher_ready(WrappedObject);

        [LibraryImport("LanguageSwitcher")]
        private static partial void LanguageSwitcher_updateInputLanguage(IntPtr obj, [MarshalAs(UnmanagedType.Bool)] bool doCallback);
        public void UpdateInputLanguage() {
            LanguageSwitcher_updateInputLanguage(WrappedObject, true);
        }
        public void UpdateInputLanguageByKeyboard() {
            Thread.Sleep(WindowActivateWaitMs);
            LanguageSwitcher_updateInputLanguage(WrappedObject, false);
        }

        [LibraryImport("LanguageSwitcher")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool LanguageSwitcher_swapCategory(IntPtr obj);
        public bool SwapCategory() => LanguageSwitcher_swapCategory(WrappedObject);
        public void SwapCategoryNoReturn() {
            LanguageSwitcher_swapCategory(WrappedObject);
        }

        [LibraryImport("LanguageSwitcher")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool LanguageSwitcher_getCategory(IntPtr obj);
        public bool GetCategory() => LanguageSwitcher_getCategory(WrappedObject);

        [LibraryImport("LanguageSwitcher")]
        private static partial uint LanguageSwitcher_getCurrentLanguage(IntPtr obj);
        public uint GetCurrentLanguage() => LanguageSwitcher_getCurrentLanguage(WrappedObject);

        [LibraryImport("LanguageSwitcher")]
        private static partial void LanguageSwitcher_setCurrentLanguage(IntPtr obj, uint newLanguage);
        public void SetCurrentLanguage(uint newLanguage) => LanguageSwitcher_setCurrentLanguage(WrappedObject, newLanguage);

        [LibraryImport("LanguageSwitcher")]
        private static partial uint LanguageSwitcher_getLanguageList(IntPtr obj, [MarshalAs(UnmanagedType.Bool)] bool isImeLanguageList, uint[] list);
        public uint GetLanguageList(bool isImeLanguageList, uint[] list) => LanguageSwitcher_getLanguageList(WrappedObject, isImeLanguageList, list);

        [LibraryImport("LanguageSwitcher")]
        private static partial void LanguageSwitcher_onRaltUp(IntPtr obj);
        public void OnRaltUp() => LanguageSwitcher_onRaltUp(WrappedObject);
    }
}
