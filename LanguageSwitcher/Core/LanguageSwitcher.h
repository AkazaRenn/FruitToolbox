#pragma once
#include "Language.h"
#include <map>

using namespace std;

namespace FruitToolbox {
    typedef void(__stdcall* onLanguageChangeCallback)(int lcid);

    class LanguageSwitcher {
    private:
        inline static LCID hklToLcid(HKL hkl) {
            return (UINT64(hkl) & 0xffff);
        }

        static map<LCID, Language> languageList;
        static LCID activeLanguages[2];
        static bool inImeMode;
        static onLanguageChangeCallback languageChangeHandler;

        static void resetFields();

        static void applyInputLanguage();
        static void fixImeConversionMode(HWND hWnd);
        static void fixImeConversionMode(HWND hWnd, LCID language);

        // Windows hook related
        static HWINEVENTHOOK windowChangeHook;
        static void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        static void updateInputLanguage(HWND hwnd, bool doCallback = true);

    public:
        static bool start(onLanguageChangeCallback handler);
        static void stop();

        static void updateInputLanguage(bool doCallback = true);
        static bool swapCategory();
        static bool getCategory();
        static void setCurrentLanguage(LCID lcid, bool doCallback = true); // returns true if lcid is in the list, false otherwise
        static void onRaltUp();

        static inline LCID getCurrentLanguage() {
            return activeLanguages[inImeMode];
        }
    };
}

