#pragma once

#include "pch.h"

namespace FruitToolbox {
    namespace LanguageSwitcher {
        public ref class Interop {
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
    }
}

