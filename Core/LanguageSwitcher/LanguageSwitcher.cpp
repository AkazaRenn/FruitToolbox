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

#define SEND_MOCK_KEY() {                           \
    keybd_event(0x9F, 0, 0, 0);                     \
    keybd_event(0x9F, 0, KEYEVENTF_KEYUP, 0);       \
}

#define SEND_PT_RUN_HOTKEYS() {                     \
    keybd_event(VK_CONTROL, 0, 0, 0);               \
    keybd_event(VK_SHIFT, 0, 0, 0);                 \
    keybd_event(VK_MENU, 0, 0, 0);                  \
    keybd_event('S', 0, 0, 0);                      \
    keybd_event('S', 0, KEYEVENTF_KEYUP, 0);        \
    keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0);    \
    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);   \
    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0); \
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

static inline bool GET_CAPS_LOCK() {
    return ((GetKeyState(VK_CAPITAL) & 0x0001) != 0);
}

static inline void SET_CAPS_LOCK(bool on) {
    if(GET_CAPS_LOCK() != on) {
        keybd_event(VK_CAPITAL, 0, KEYEVENTF_EXTENDEDKEY, 0);
        keybd_event(VK_CAPITAL, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
    }
}

LRESULT LanguageSwitcher::keyPressHandler(int nCode, WPARAM wParam, LPARAM lParam) {
    auto data = (PKBDLLHOOKSTRUCT)lParam;
    switch(wParam) {
    case WM_KEYDOWN:
        switch(data->vkCode) {
        case VK_LWIN:
        {
            if(!winDown) {
                winAsModifier = false;
                winDown = true;
            }
            return CallNextHookEx(NULL, nCode, wParam, lParam);
        }
        case VK_CAPITAL:
        {
            if(!data->scanCode) { // sent by software
                return CallNextHookEx(NULL, nCode, wParam, lParam);
            }
            thread t([&] () {
                if(!capslockDown) {
                    capslockDown = true;
                    if(GET_CAPS_LOCK()) {
                        SET_CAPS_LOCK(false);
                    } else {
                        capsLockTimer.setTimeout(CAPSLOCK_WAIT_TIME_MS, [&] () { SET_CAPS_LOCK(true); });
                    }
                }
                     });
            t.detach();
            return 1;
        }
        default:
        {
            if(winDown) {
                winAsModifier = true;
            }
        }
        }
        return CallNextHookEx(NULL, nCode, wParam, lParam);
    case WM_KEYUP:
        switch(data->vkCode) {
        case VK_LWIN:
        {
            winDown = false;
            if(!winAsModifier) {
                SEND_PT_RUN_HOTKEYS();
            }
            return CallNextHookEx(NULL, nCode, wParam, lParam);
        }
        case VK_CAPITAL:
        {
            if(!data->scanCode) { // sent by software
                return CallNextHookEx(NULL, nCode, wParam, lParam);
            }
            thread t([&] () {
                capslockDown = false;
                capsLockTimer.stop([&] () { swapCategory(); });
                     });
            t.detach();
            return 1;
        }
        case VK_RMENU:
        {
            if(!getPerLanguageMethods(getCurrentLanguage()).onRaltUp()) {
                return 1;
            }
        }
        }
        return CallNextHookEx(NULL, nCode, wParam, lParam);
    case WM_SYSKEYDOWN: // yes they use different events for Alt up and down
    {
        if(data->vkCode == VK_RMENU) {
            if(!getPerLanguageMethods(getCurrentLanguage()).onRaltDown()) {
                return 1;
            }
        }
        return CallNextHookEx(NULL, nCode, wParam, lParam);
    }
    }
    return CallNextHookEx(NULL, nCode, wParam, lParam);
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
    //keyboardEvent = SetWindowsHookEx(WH_KEYBOARD_LL, onKeyPress, 0, 0);
}

LanguageSwitcher::~LanguageSwitcher() {
    UnhookWinEvent(windowChangeEvent);
    //UnhookWindowsHookEx(keyboardEvent);
}


LanguageSwitcher* LanguageSwitcher::instance;
void CALLBACK LanguageSwitcher::onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if(instance) {
        instance->activeWindowChangeHandler(hwnd);
    }
}

LRESULT CALLBACK LanguageSwitcher::onKeyPress(int nCode, WPARAM wParam, LPARAM lParam) {
    if(nCode == HC_ACTION && instance) {
        return instance->keyPressHandler(nCode, wParam, lParam);
    }

    return CallNextHookEx(NULL, nCode, wParam, lParam);
}
