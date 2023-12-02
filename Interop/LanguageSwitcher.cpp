#include "pch.h"

#include "LanguageSwitcher.h"

#pragma comment(lib, "imm32")

using namespace FruitToolbox::Interop::Unmanaged;

constexpr size_t  REG_LANGUAGE_MULTI_SZ_MAX_LENGTH = 1024;
constexpr LPCWSTR REG_LANGUAGES_DIR = L"Control Panel\\International\\User Profile";
constexpr LPCWSTR REG_LANGUAGES_KEY = L"Languages";

constexpr UINT    MAX_TRY_TIMES = 2;
constexpr UINT    RETRY_WAIT_MS = 50;

void LanguageSwitcher::applyInputLanguage() {
    HWND hwnd = GetForegroundWindow();

    SendMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, 0, getCurrentLanguage());
    fixImeConversionMode(hwnd);
}

void LanguageSwitcher::updateInputLanguage() {
    updateWindowInputLanguage(GetForegroundWindow());
}

void LanguageSwitcher::updateWindowInputLanguage(HWND hwnd) {
    setCurrentLanguage(hklToLcid(GetKeyboardLayout(GetWindowThreadProcessId(hwnd, nullptr))));
    fixImeConversionMode(hwnd);
    newLanguageHandler(getCurrentLanguage(), inImeMode);
}

bool LanguageSwitcher::swapCategory() {
    inImeMode = !inImeMode;
    applyInputLanguage();

    categorySwapHandler(getCurrentLanguage(), inImeMode);
    return inImeMode;
}

void LanguageSwitcher::setCurrentLanguage(LCID lcid) {
    if (languageList.find(lcid) == languageList.end()) {
        languageList[lcid] = Language(lcid);
    } else if (getCurrentLanguage() != lcid) {
        inImeMode = languageList[lcid].isImeLanguage();
        activeLanguages[inImeMode] = languageList[lcid].getLocaleId();
    } else {
        return;
    }
}

//[TODO] handle focused box change within the same app (like Edge webpages)
// not sure how to achieve, need help
//[TODO] put in a thread so it can do non-block retries
void LanguageSwitcher::fixImeConversionMode(HWND hWnd) {
    if (languageList[getCurrentLanguage()].isImeLanguage()) {
        auto retryCount = 0;
        auto perLangMethods = getPerLanguageMethods(getCurrentLanguage());
        while ((!perLangMethods.inConversionMode(hWnd)) && (retryCount++ <= MAX_TRY_TIMES)) {
            perLangMethods.fixConversionMode(hWnd);
            Sleep(RETRY_WAIT_MS);
        }
    }
}

void LanguageSwitcher::onRaltUp() {
    getPerLanguageMethods(getCurrentLanguage()).onRaltUp();
}

// Reset all values in resetFields()
map<LCID, Language> LanguageSwitcher::languageList = {};
LCID LanguageSwitcher::activeLanguages[2] = {};
bool LanguageSwitcher::inImeMode = false;

vector<HWINEVENTHOOK> LanguageSwitcher::hooks = {};
onLanguageChangeCallback LanguageSwitcher::categorySwapHandler = nullptr;
onLanguageChangeCallback LanguageSwitcher::newLanguageHandler = nullptr;
void LanguageSwitcher::resetFields() {
    languageList = {};
    fill_n(activeLanguages, sizeof(activeLanguages), 0);
    inImeMode = false;

    hooks = {};
    categorySwapHandler = nullptr;
    newLanguageHandler = nullptr;
}

void LanguageSwitcher::buildLanguageList() {
    WCHAR buffer[REG_LANGUAGE_MULTI_SZ_MAX_LENGTH] = {};
    DWORD dwLen = sizeof(buffer);
    RegGetValue(HKEY_CURRENT_USER, REG_LANGUAGES_DIR, REG_LANGUAGES_KEY, RRF_RT_REG_MULTI_SZ, NULL, buffer, &dwLen);

    for (size_t i = 0; (buffer[i] != L'\0' && i < REG_LANGUAGE_MULTI_SZ_MAX_LENGTH); i++) {
        auto newLang = Language(buffer + i);

        if ((activeLanguages[false] == 0) && (activeLanguages[true] == 0)) {
            inImeMode = newLang.isImeLanguage();
            activeLanguages[inImeMode] = newLang.getLocaleId();
        } else if ((activeLanguages[!inImeMode] == 0) && (inImeMode != newLang.isImeLanguage())) {
            activeLanguages[!inImeMode] = newLang.getLocaleId();
        }

        languageList[newLang.getLocaleId()] = newLang;

        i += wcslen(buffer + i);
    }
}

bool LanguageSwitcher::start(
    onLanguageChangeCallback _categorySwapHandler, 
    onLanguageChangeCallback _newLanguageHandler) {
    // Initialized before, don't do it again
    if (!hooks.empty()) return false;

    if ((categorySwapHandler = _categorySwapHandler) == nullptr ||
        (newLanguageHandler = _newLanguageHandler) == nullptr) {
        stop();
        return false;
    }

    buildLanguageList();
    if ((activeLanguages[false] == 0) || (activeLanguages[true] == 0)) {
        stop();
        return false;
    }
    updateWindowInputLanguage(GetForegroundWindow());

    hooks.push_back(SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, NULL, onActiveWindowChange, 0, 0, WINEVENT_OUTOFCONTEXT));
    for (const auto hook : hooks) {
        if (hook == nullptr) {
            stop();
            return false;
        }
    }

    return true;
}

void LanguageSwitcher::stop() {
    for (const auto hook : hooks) {
        UnhookWinEvent(hook);
    }

    resetFields();
}

void CALLBACK LanguageSwitcher::onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (hwnd == GetForegroundWindow()) {
        thread(updateWindowInputLanguage, hwnd).detach();
    }
}
