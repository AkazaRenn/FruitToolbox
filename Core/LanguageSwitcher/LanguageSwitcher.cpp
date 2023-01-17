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

void LanguageSwitcher::buildLanguageList() {
    WCHAR buffer[REG_LANGUAGE_MULTI_SZ_MAX_LENGTH];
    DWORD dwLen = sizeof(buffer);
    RegGetValue(HKEY_CURRENT_USER, REG_LANGUAGES_DIR, REG_LANGUAGES_KEY, RRF_RT_REG_MULTI_SZ, NULL, buffer, &dwLen);

    for(size_t i = 0; (buffer[i] != L'\0' && i < REG_LANGUAGE_MULTI_SZ_MAX_LENGTH); i++) {
        auto newLang = Language(buffer + i);
        newLang.isImeLanguage() ? categories[1].langs.push_back(newLang) : categories[0].langs.push_back(newLang);

        i += wcslen(buffer + i);
    }
}

void LanguageSwitcher::updateInputLanguage() {
    auto newLanguage = categories[inImeMode].langs[categories[inImeMode].index];
    auto hwnd = GetForegroundWindow();
    SendMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, 0, newLanguage.getLocaleId());

    fixImeConversionMode(hwnd);
}

bool LanguageSwitcher::swapCategory() {
    inImeMode = !inImeMode;
    updateInputLanguage();

    if(onLanguageChange) {
        onLanguageChange(inImeMode, categories[inImeMode].index);
    }
    return inImeMode;
}

bool LanguageSwitcher::getCategory() {
    return inImeMode;
}

LCID LanguageSwitcher::getCurrentLanguage() {
    return categories[inImeMode].langs[categories[inImeMode].index].getLocaleId();
}

bool LanguageSwitcher::setCurrentLanguage(LCID lcid) {
    if(getCurrentLanguage() != lcid) {
        for(unsigned int i = 0; i < (sizeof(categories) / sizeof(categories[0])); i++) {
            for(unsigned int j = 0; j < categories[i].langs.size(); j++) {
                if(categories[i].langs[j].getLocaleId() == lcid) {
                    inImeMode = i;
                    categories[i].index = j;
                    if(onLanguageChange) {
                        onLanguageChange(inImeMode, categories[inImeMode].index);
                    }
                    return true;
                }
            }
        }
    }

    return false;
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
    if(categories[inImeMode].langs[categories[inImeMode].index].isImeLanguage()) {
        fixImeConversionMode(hWnd, categories[inImeMode].langs[categories[inImeMode].index].getLocaleId());
    }
}


vector<LCID> LanguageSwitcher::getLanguageList(bool getImeLanguageList) {
    vector<LCID> languageList;

    for(auto& lang : categories[getImeLanguageList].langs) {
        languageList.push_back(lang.getLocaleId());
    }

    return languageList;
}

void LanguageSwitcher::setOnLanguageChange(OnLanguageChange handler) {
    onLanguageChange = handler;
}

bool LanguageSwitcher::registerHotkeys() {
    return false;
}

void LanguageSwitcher::orderLanguageList(bool isImeLanguageList,
                                         vector<LCID> list) {
    for(int i = list.size() - 1; i >= 0; i--) {
        auto currentLanguageList = getLanguageList(true);
        auto it = find(currentLanguageList.begin(), currentLanguageList.end(), list[i]);
        if(it != currentLanguageList.end()) {
            auto actualIterator = categories[true].langs.begin() + (it - currentLanguageList.begin());
            auto temp = *actualIterator;
            categories[true].langs.erase(actualIterator);
            categories[true].langs.insert(categories[true].langs.begin(), temp);
        }
    }
}

void LanguageSwitcher::activeWindowChangeHandler(HWND hwnd) {
    setCurrentLanguage(hklToLcid(GetKeyboardLayout(GetWindowThreadProcessId(hwnd, nullptr))));
    fixImeConversionMode(hwnd);
}

LanguageSwitcher::LanguageSwitcher() : LanguageSwitcher(false) {}

LanguageSwitcher::LanguageSwitcher(bool defaultImeMode) {
    instance = this;
    if(instance != this) {
        return;
    }

    inImeMode = defaultImeMode;
    buildLanguageList();

    for(auto& cate : categories) {
        if(cate.langs.empty()) {
            exit(0); // you don't need it
        }
    }

    updateInputLanguage();

    windowChangeEvent = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, NULL, LanguageSwitcher::onActiveWindowChange, 0, 0, WINEVENT_OUTOFCONTEXT);
}

LanguageSwitcher::~LanguageSwitcher() {
    UnhookWinEvent(windowChangeEvent);
}


LanguageSwitcher* LanguageSwitcher::instance;
void CALLBACK LanguageSwitcher::onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if(instance) {
        instance->activeWindowChangeHandler(hwnd);
    }
}
