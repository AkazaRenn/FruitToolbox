#include "pch.h"
#include "LanguageSwitcher.h"
#include "PerLanguageMethods.h"

#pragma comment(lib, "imm32")

using namespace FruitLanguageSwitcher;

constexpr size_t             REG_LANGUAGE_MULTI_SZ_MAX_LENGTH = 1024;
constexpr LPCWSTR            REG_LANGUAGES_DIR = L"Control Panel\\International\\User Profile";
constexpr LPCWSTR            REG_LANGUAGES_KEY = L"Languages";

constexpr UINT               MAX_TRY_TIMES = 2;
constexpr UINT               RETRY_WAIT_MS = 50;

inline LCID hklToLcid(HKL hkl) {
    return (UINT64(hkl) & 0xffff);
}

void LanguageSwitcher::applyInputLanguage() {
    if(activeLanguages[inImeMode]) {
        auto hwnd = GetForegroundWindow();

        SendMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, 0, activeLanguages[inImeMode]);
        fixImeConversionMode(hwnd);
    }
}

void LanguageSwitcher::updateInputLanguage() {
    updateInputLanguage(GetForegroundWindow());
}

void LanguageSwitcher::updateInputLanguage(HWND hwnd) {
    setCurrentLanguage(hklToLcid(GetKeyboardLayout(GetWindowThreadProcessId(hwnd, nullptr))));
    fixImeConversionMode(hwnd);
}

bool LanguageSwitcher::swapCategory() {
    inImeMode = !inImeMode;
    applyInputLanguage();

    languageChangeHandler(0x040c);
    return inImeMode;
}

bool LanguageSwitcher::getCategory() {
    return inImeMode;
}

LCID LanguageSwitcher::getCurrentLanguage() {
    return activeLanguages[inImeMode];
}

void LanguageSwitcher::setCurrentLanguage(LCID lcid) {
    if(languageList.find(lcid) == languageList.end()) {
        languageList[lcid] = Language(lcid);
    }
    else if(getCurrentLanguage() != lcid) {
        inImeMode = languageList[lcid].isImeLanguage();
        activeLanguages[inImeMode] = languageList[lcid].getLocaleId();
    }
    else {
        return;
    }

    // if we didn't return then it's an updated condition, call handler
    languageChangeHandler(0x0409);
}

//[TODO] handle focused box change within the same app (like Edge webpages)
// not sure how to achieve, need help
//[TODO] put in a thread so it can do non-block retries
void LanguageSwitcher::fixImeConversionMode(HWND hWnd, LCID language) {
    auto retryCount = 0;
    auto perLangMethods = getPerLanguageMethods(language);
    while((!perLangMethods.inConversionMode(hWnd)) && (retryCount++ <= MAX_TRY_TIMES)) {
        perLangMethods.fixConversionMode(hWnd);
        Sleep(RETRY_WAIT_MS);
    }
}

void LanguageSwitcher::fixImeConversionMode(HWND hWnd) {
    if(languageList[activeLanguages[inImeMode]].isImeLanguage()) {
        fixImeConversionMode(hWnd, activeLanguages[inImeMode]);
    }
}

void LanguageSwitcher::onRaltUp() {
    getPerLanguageMethods(activeLanguages[inImeMode]).onRaltUp();
}

#pragma managed(push, off)
LanguageSwitcher::LanguageSwitcher(onLanguageChangeCallback handler) {
    instance = this;
    if(instance != this) {
        return;
    }

    languageChangeHandler = handler;

    WCHAR buffer[REG_LANGUAGE_MULTI_SZ_MAX_LENGTH];
    DWORD dwLen = sizeof(buffer);
    RegGetValue(HKEY_CURRENT_USER, REG_LANGUAGES_DIR, REG_LANGUAGES_KEY, RRF_RT_REG_MULTI_SZ, NULL, buffer, &dwLen);

    for(size_t i = 0; (buffer[i] != L'\0' && i < REG_LANGUAGE_MULTI_SZ_MAX_LENGTH); i++) {
        auto newLang = Language(buffer + i);

        if((activeLanguages[false] == 0) && (activeLanguages[true] == 0)) {
            inImeMode = newLang.isImeLanguage();
            activeLanguages[inImeMode] = newLang.getLocaleId();
        }
        else if((activeLanguages[!inImeMode] == 0) && (inImeMode != newLang.isImeLanguage())) {
            activeLanguages[!inImeMode] = newLang.getLocaleId();
        }

        languageList[newLang.getLocaleId()] = newLang;

        i += wcslen(buffer + i);
    }

    applyInputLanguage();

    windowChangeEvent = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, NULL, LanguageSwitcher::onActiveWindowChange, 0, 0, WINEVENT_OUTOFCONTEXT);
}
#pragma managed(pop)

LanguageSwitcher::~LanguageSwitcher() {
    UnhookWinEvent(windowChangeEvent);
}

bool LanguageSwitcher::ready() {
    return (activeLanguages[false] != 0 && activeLanguages[true] != 0);
}

LanguageSwitcher* LanguageSwitcher::instance;
void CALLBACK LanguageSwitcher::onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if(instance && hwnd == GetForegroundWindow()) {
        instance->updateInputLanguage(hwnd);
    }
}
