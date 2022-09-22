// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "LanguageSwitcher.h"

using namespace FruitLanguageSwitcher;

#define DLLEXPORT __declspec(dllexport)

extern "C" {
    DLLEXPORT void* LanguageSwitcher_new(bool defaultImeMode) {
        return (void*)new LanguageSwitcher(defaultImeMode);
    }

    DLLEXPORT void LanguageSwitcher_delete(LanguageSwitcher* s) {
        delete s;
    }

    DLLEXPORT bool LanguageSwitcher_swapCategory(LanguageSwitcher* s) {
        return s->swapCategory();
    }

    DLLEXPORT bool LanguageSwitcher_getCategory(LanguageSwitcher* s) {
        return s->getCategory();
    }

    DLLEXPORT unsigned int LanguageSwitcher_nextLanguage(LanguageSwitcher* s) {
        return s->nextLanguage();
    }

    DLLEXPORT unsigned int LanguageSwitcher_lastLanguage(LanguageSwitcher* s) {
        return s->lastLanguage();
    }

    DLLEXPORT DWORD LanguageSwitcher_getCurrentLanguage(LanguageSwitcher* s) {
        return s->getCurrentLanguage();
    }

    DLLEXPORT bool LanguageSwitcher_setCurrentLanguage(LanguageSwitcher* s,
                                                       DWORD newLanguage) {
        return s->setCurrentLanguage(newLanguage);
    }

    DLLEXPORT unsigned int LanguageSwitcher_getLanguageList(LanguageSwitcher* s,
                                                            bool isImeLanguageList,
                                                            DWORD* list) {
        auto langVec = s->getLanguageList(isImeLanguageList);
        list = &(langVec[0]);
        return langVec.size();
    }

    DLLEXPORT void LanguageSwitcher_orderLanguageList(LanguageSwitcher* s,
                                                      bool isImeLanguageList,
                                                      DWORD* list,
                                                      unsigned int n) {
        s->orderLanguageList(isImeLanguageList, vector<LCID>(list, list + n));
    }

    DLLEXPORT void LanguageSwitcher_setOnLanguageChange(LanguageSwitcher* s,
                                                        OnLanguageChange handler) {
        s->setOnLanguageChange(handler);
    }
}