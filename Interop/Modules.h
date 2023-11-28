#pragma once

#include "pch.h"

namespace FruitToolbox {
    namespace Interop {
        public ref class LanguageSwitcher {
        public:
            delegate void OnLanguageChangeCallbackDelegate(int lcid);

            static bool Start(OnLanguageChangeCallbackDelegate^ handler);
            static void Stop();
            static void UpdateInputLanguage(bool doCallback);
            static bool SwapCategory();
            static bool GetCategory();
            static void SetCurrentLanguage(int lcid);
            static void OnRaltUp();
        };

        public ref class WindowTracker {
        public:
            delegate void WindowChangedCallbackDelegate(System::IntPtr hwnd);

            static bool Start(
                WindowChangedCallbackDelegate^ _onNewFloatWindowHandler,
                WindowChangedCallbackDelegate^ _onMaxWindowHandler,
                WindowChangedCallbackDelegate^ _onUnmaxWindowHandler,
                WindowChangedCallbackDelegate^ _onMinWindowHandler,
                WindowChangedCallbackDelegate^ _onCloseWindowHandler);
            static void Stop();
        };
    }
}

