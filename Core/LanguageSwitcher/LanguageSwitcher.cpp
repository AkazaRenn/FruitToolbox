#include "pch.h"
#include "LanguageSwitcher.h"
#include "PerLanguageMethods.h"
#include <vector>
#include <thread>

#pragma comment(lib, "imm32")

using namespace FruitLanguageSwitcher;

constexpr size_t             REG_LANGUAGE_MULTI_SZ_MAX_LENGTH = 1024;
constexpr LPCWSTR            REG_LANGUAGES_DIR = L"Control Panel\\International\\User Profile";
constexpr LPCWSTR            REG_LANGUAGES_KEY = L"Languages";

constexpr UINT               MAX_RETRY_TIMES = 1;

constexpr UINT               CAPSLOCK_WAIT_TIME_MS = 500;

inline constexpr LCID hklToLcid(HKL hkl) {
    return (long(hkl) & 0xffff);
}

void LanguageSwitcher::applyInputLanguage() {
    auto hwnd = GetForegroundWindow();
    SendMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, 0, activeLanguages[inImeMode].getLocaleId());

    fixImeConversionMode(hwnd);
}

void LanguageSwitcher::updateInputLanguage() {
    activeWindowChangeHandler(GetForegroundWindow());
}

bool LanguageSwitcher::swapCategory() {
    inImeMode = !inImeMode;
    applyInputLanguage();
    return inImeMode;
}

bool LanguageSwitcher::getCategory() {
    return inImeMode;
}

LCID LanguageSwitcher::getCurrentLanguage() {
    return activeLanguages[inImeMode].getLocaleId();
}

bool LanguageSwitcher::setCurrentLanguage(LCID lcid) {
    if(languageList.find(lcid) == languageList.end()) {
        return false;
    }
    else if(getCurrentLanguage() != lcid) {
        inImeMode = languageList[lcid].isImeLanguage();
        activeLanguages[inImeMode] = languageList[lcid];
    }

    return true;
}

//[TODO] handle focused box change within the same app (like Edge webpages)
// not sure how to achieve, need help
//[TODO] put in a thread so it can do non-block retries
void LanguageSwitcher::fixImeConversionMode(HWND hWnd, LCID language) {
    auto retryCount = 0;
    auto perLangMethods = getPerLanguageMethods(language);
    while((!perLangMethods.inConversionMode(hWnd)) && (retryCount++ <= MAX_RETRY_TIMES)) {
        perLangMethods.fixConversionMode(hWnd);
        Sleep(50);
    }
}

void LanguageSwitcher::fixImeConversionMode(HWND hWnd) {
    if(activeLanguages[inImeMode].isImeLanguage()) {
        fixImeConversionMode(hWnd, activeLanguages[inImeMode].getLocaleId());
    }
}

void LanguageSwitcher::activeWindowChangeHandler(HWND hwnd) {
    setCurrentLanguage(hklToLcid(GetKeyboardLayout(GetWindowThreadProcessId(hwnd, nullptr))));
    fixImeConversionMode(hwnd);
}

LanguageSwitcher::LanguageSwitcher() {
    instance = this;
    if(instance != this) {
        return;
    }

    WCHAR buffer[REG_LANGUAGE_MULTI_SZ_MAX_LENGTH];
    DWORD dwLen = sizeof(buffer);
    RegGetValue(HKEY_CURRENT_USER, REG_LANGUAGES_DIR, REG_LANGUAGES_KEY, RRF_RT_REG_MULTI_SZ, NULL, buffer, &dwLen);

    for(size_t i = 0; (buffer[i] != L'\0' && i < REG_LANGUAGE_MULTI_SZ_MAX_LENGTH); i++) {
        auto newLang = Language(buffer + i);

        if(activeLanguages.size() == 0) {
            activeLanguages.push_back(newLang);
            inImeMode = newLang.isImeLanguage();
        }
        else if((activeLanguages.size() == 1)&&(inImeMode != newLang.isImeLanguage())) {
            activeLanguages.push_back(newLang);
        }

        languageList[newLang.getLocaleId()] = newLang;

        i += wcslen(buffer + i);
    }


    applyInputLanguage();

    windowChangeEvent = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, NULL, LanguageSwitcher::onActiveWindowChange, 0, 0, WINEVENT_OUTOFCONTEXT);
}

LanguageSwitcher::~LanguageSwitcher() {
    UnhookWinEvent(windowChangeEvent);
}


LanguageSwitcher* LanguageSwitcher::instance;
void CALLBACK LanguageSwitcher::onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if(instance && hwnd == GetForegroundWindow()) {
        instance->activeWindowChangeHandler(hwnd);
    }
}
