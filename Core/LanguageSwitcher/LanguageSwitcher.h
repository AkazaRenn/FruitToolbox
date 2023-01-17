#pragma once
#pragma unmanaged
#include <vector>
#include "Language.h"
#include <map>

using namespace std;

namespace FruitLanguageSwitcher {
    class LanguageSwitcher {
    private:
        map<LCID, Language> languageList;
        atomic<LCID> activeLanguages[2];
        atomic<bool> inImeMode;

        void applyInputLanguage();
        void fixImeConversionMode(HWND hWnd);
        void fixImeConversionMode(HWND hWnd, LCID language);

        // Windows hook related
        static LanguageSwitcher* instance;

        HWINEVENTHOOK windowChangeEvent;
        static void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        void updateInputLanguage(HWND hwnd);

    public:
        explicit LanguageSwitcher();
        ~LanguageSwitcher();

        void updateInputLanguage();
        bool swapCategory();
        bool getCategory();
        LCID getCurrentLanguage();
        bool setCurrentLanguage(LCID lcid); // returns true if lcid is in the list, false otherwise
    };
}

