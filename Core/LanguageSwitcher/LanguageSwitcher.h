#pragma once
#pragma unmanaged
#include <vector>
#include "Language.h"
#include <map>
#include "Timer.h"

using namespace std;

typedef void(__stdcall* OnLanguageChange)(bool inImeMode, unsigned int languageIndex);

namespace FruitLanguageSwitcher {
    struct LanguageCategory {
        vector<Language> langs;
        unsigned int index = 0;
    };

    class LanguageSwitcher {
    private:
        LanguageCategory categories[2];
        bool inImeMode;

        HWINEVENTHOOK windowChangeEvent;
        HHOOK keyboardEvent;
        OnLanguageChange onLanguageChange = nullptr;

        void buildLanguageList();
        void updateInputLanguage();
        void fixImeConversionMode(HWND hWnd);
        void fixImeConversionMode(HWND hWnd, LCID language);

        // keyboard status
        bool winDown = false;
        bool winAsModifier = false;
        bool capslockDown = false;
        Timer capsLockTimer;

        // Windows hook related
        static LanguageSwitcher* instance;
        static bool registerHotkeys();
        static void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        static LRESULT CALLBACK onKeyPress(int nCode, WPARAM wParam, LPARAM lParam);

        void activeWindowChangeHandler(HWND hwnd);
        LRESULT keyPressHandler(int nCode, WPARAM wParam, LPARAM lParam);

    public:
        explicit LanguageSwitcher(bool defaultImeMode);
        explicit LanguageSwitcher();
        ~LanguageSwitcher();

        bool swapCategory();
        bool getCategory();
        unsigned int nextLanguage();
        unsigned int lastLanguage();
        LCID getCurrentLanguage();
        bool setCurrentLanguage(LCID lcid); // returns true if lcid is in the list, false otherwise
        vector<LCID> getLanguageList(bool getImeLanguageList);
        void setOnLanguageChange(OnLanguageChange handler);
        void orderLanguageList(bool isImeLanguageList,
                               vector<LCID> list);
    };
}

