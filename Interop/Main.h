#pragma once

#include "pch.h"

namespace FruitToolbox {
    namespace Interop {
        public ref class LanguageSwitcher {
        public:
            delegate void onLanguageChangeCallbackDelegate(int lcid);

            static bool start(onLanguageChangeCallbackDelegate^ handler);
            static void stop();
            static void updateInputLanguage(bool doCallback);
            static bool swapCategory();
            static bool getCategory();
            static void setCurrentLanguage(int lcid);
            static void onRaltUp();
        };

        public ref class WindowTracker {
        public:
            delegate void windowChangedCallbackDelegate(HWND hwnd);

            static bool start(windowChangedCallbackDelegate^ _onNewFloatWindowHandler,
                windowChangedCallbackDelegate^ _onMaxWindowHandler,
                windowChangedCallbackDelegate^ _onUnmaxWindowHandler,
                windowChangedCallbackDelegate^ _onMinWindowHandler,
                windowChangedCallbackDelegate^ _onCloseWindowHandler);
            static void stop();
        };
    }
}

