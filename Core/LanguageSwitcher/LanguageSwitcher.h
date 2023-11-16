#pragma once
#include <vector>
#include "Language.h"
#include <map>

using namespace std;

namespace FruitLanguageSwitcher {
    typedef void(__stdcall* onLanguageChangeCallback)(int lcid);

    class LanguageSwitcher {
    private:
        map<LCID, Language> languageList;
        LCID activeLanguages[2];
        bool inImeMode;
        onLanguageChangeCallback languageChangeHandler = nullptr;

        void applyInputLanguage();
        void fixImeConversionMode(HWND hWnd);
        void fixImeConversionMode(HWND hWnd, LCID language);


        // Windows hook related
        static LanguageSwitcher* instance;
        HWINEVENTHOOK windowChangeEvent;
        static void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        void updateInputLanguage(HWND hwnd);

    public:
        explicit LanguageSwitcher(onLanguageChangeCallback handler);
        ~LanguageSwitcher();
        bool ready();

        void updateInputLanguage();
        bool swapCategory();
        bool getCategory();
        LCID getCurrentLanguage();
        void setCurrentLanguage(LCID lcid); // returns true if lcid is in the list, false otherwise
        void onRaltUp();
    };
}

