#pragma once

#include <vector>
#include "Language.h"
#include <map>
//#include <chrono>

using namespace std;

typedef void(__stdcall* OnLanguageChange)(bool inImeMode, unsigned int languageIndex);

namespace FruitLanguageSwitcher {
    struct LanguageCategory {
        vector<Language> langs;
        unsigned int index = 0;
    };

    const static map<long, vector<long>> imeConversionModeCodeMap{
        {0x404,  {1}}, // zh-TW, Chinese (Traditional, Taiwan)
        {0x411,  {9, 11, 27}}, // ja-JP, Japanese (Japan)
        //{0x412,  {}}, // ko-KR, Korean (Korea)
        //{0x45E,  {}}, // am-ET, Amharic (Ethiopia)
        //{0x473,  {}}, // ti-ET, Tigrinya (Ethiopia)
        {0x804,  {1}}, // zh-CN, Chinese (Simplified, PRC)
        //{0x873,  {}}, // ti-ER, Tigrinya (Eritrea)
        {0xC04,  {1}}, // zh-HK, Chinese (Traditional, Hong Kong S.A.R.)
        {0x1004, {1}}, // zh-SG, Chinese (Simplified, Singapore)
        {0x1404, {1}}, // zh-MO, Chinese (Traditional, Macao S.A.R.)
    };

    enum WinKeyUsage {
        NONE = 0x0,
        KEY = 0x01,
        MODIFIER_INTERNAL = 0x10,
        MODIFIER_EXTERNAL = 0x11,
    };

    class LanguageSwitcher {
    private:
        LanguageCategory categories[2];
        bool inImeMode;
        HWINEVENTHOOK windowChangeEvent;
        HHOOK keyboardEvent;
        //void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        OnLanguageChange onLanguageChange = nullptr;

        // keyboard status
        bool winDown = false;
        WinKeyUsage winKeyUsage = NONE;
        //bool capslockDown = false;
        //chrono::system_clock::time_point capslockDownTime;


        void buildLanguageList();
        void updateInputLanguage();
        void fixImeConversionMode(HWND hWnd);
        void fixImeConversionMode(HWND hWnd, LCID language);

        static LanguageSwitcher* instance;

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
        void activeWindowChangeHandler(HWND hwnd);
        LRESULT keyPressHandler(int nCode, WPARAM wParam, LPARAM lParam);

        static bool registerHotkeys();
        static void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        static LRESULT CALLBACK onKeyPress(int nCode, WPARAM wParam, LPARAM lParam);
    };
}

