// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "LanguageSwitcher.h"

using namespace FruitLanguageSwitcher;

#define DLLEXPORT __declspec(dllexport)

extern "C" {
    DLLEXPORT void* LanguageSwitcher_new(onLanguageChangeCallback handler) {
        return (void*)new LanguageSwitcher(handler);
    }

    DLLEXPORT void LanguageSwitcher_delete(LanguageSwitcher* s) {
        delete s;
    }

    DLLEXPORT bool LanguageSwitcher_ready(LanguageSwitcher* s) {
        return s->ready();
    }

    DLLEXPORT void LanguageSwitcher_updateInputLanguage(LanguageSwitcher* s) {
        s->updateInputLanguage();
    }

    DLLEXPORT bool LanguageSwitcher_swapCategory(LanguageSwitcher* s) {
        return s->swapCategory();
    }

    DLLEXPORT bool LanguageSwitcher_getCategory(LanguageSwitcher* s) {
        return s->getCategory();
    }

    DLLEXPORT DWORD LanguageSwitcher_getCurrentLanguage(LanguageSwitcher* s) {
        return s->getCurrentLanguage();
    }

    DLLEXPORT void LanguageSwitcher_setCurrentLanguage(LanguageSwitcher* s,
                                                       DWORD newLanguage) {
        s->setCurrentLanguage(newLanguage);
    }

    DLLEXPORT void LanguageSwitcher_onRaltUp(LanguageSwitcher* s) {
        s->onRaltUp();
    }
}
