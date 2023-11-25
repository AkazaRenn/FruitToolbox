// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "LanguageSwitcher.h"

using namespace FruitToolbox;

#define DLLEXPORT __declspec(dllexport)

extern "C" {
    DLLEXPORT bool LanguageSwitcher_start(onLanguageChangeCallback handler) {
        return LanguageSwitcher::start(handler);
    }

    DLLEXPORT void LanguageSwitcher_stop() {
        LanguageSwitcher::stop();
    }

    DLLEXPORT void LanguageSwitcher_updateInputLanguage(bool doCallback) {
        LanguageSwitcher::updateInputLanguage(doCallback);
    }

    DLLEXPORT bool LanguageSwitcher_swapCategory() {
        return LanguageSwitcher::swapCategory();
    }

    DLLEXPORT bool LanguageSwitcher_getCategory() {
        return LanguageSwitcher::getCategory();
    }

    DLLEXPORT DWORD LanguageSwitcher_getCurrentLanguage() {
        return LanguageSwitcher::getCurrentLanguage();
    }

    DLLEXPORT void LanguageSwitcher_setCurrentLanguage(DWORD newLanguage) {
        LanguageSwitcher::setCurrentLanguage(newLanguage);
    }

    DLLEXPORT void LanguageSwitcher_onRaltUp() {
        LanguageSwitcher::onRaltUp();
    }
}
