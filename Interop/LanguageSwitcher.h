#pragma once

#include <map>
#include <vector>
#include "PerLanguageMethods.hpp"

using namespace System;

namespace FruitToolbox {
namespace Interop {
namespace Unmanaged {
typedef void(__stdcall* onLanguageChangeCallback)(int lcid, bool imeMode);

class LanguageSwitcher {
private:
    inline static LCID hklToLcid(HKL hkl) {
        return (UINT64(hkl) & 0xffff);
    }

    static map<LCID, Language> languageList;
    static LCID activeLanguages[2];
    static bool inImeMode;
    static onLanguageChangeCallback categorySwapHandler;
    static onLanguageChangeCallback newLanguageHandler;

    static void resetFields();
    static void buildLanguageList();

    static void applyInputLanguage();
    static void fixImeConversionMode(HWND hWnd);

    // Windows hook related
    static vector<HWINEVENTHOOK> hooks;
    static void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
    static void updateWindowInputLanguage(HWND hwnd);

    static inline bool isLockOn(int vkCode) {
        return (GetKeyState(vkCode) & 0x1);
    }

public:
    static bool start(
        onLanguageChangeCallback _categorySwapHandler, 
        onLanguageChangeCallback _newLanguageHandler);
    static void stop();

    static void updateInputLanguage();
    static bool swapCategory();
    static void setCurrentLanguage(LCID lcid); // returns true if lcid is in the list, false otherwise
    static void onRaltUp();

    static inline LCID getCurrentLanguage() {
        return activeLanguages[inImeMode];
    }

    static inline bool getCategory() {
        return inImeMode;
    }
};
}
}
}
