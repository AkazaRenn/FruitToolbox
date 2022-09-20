#include "pch.h"
#include "LanguageSwitcher.h"
#include <vector>

#pragma comment(lib, "imm32")

using namespace FruitLanguageSwitcher;

constexpr UINT               IMC_GETCONVERSIONMODE = 0x1;
constexpr UINT               IMC_SETCONVERSIONMODE = 0x2;

constexpr size_t             REG_LANGUAGE_MULTI_SZ_MAX_LENGTH = 1024;
constexpr LPCWSTR            REG_LANGUAGES_DIR                = L"Control Panel\\International\\User Profile";
constexpr LPCWSTR            REG_LANGUAGES_KEY                = L"Languages";

constexpr LPCWSTR            REG_LANGUAGE_PER_WINDOW_DIR      = L"HKEY_CURRENT_USER\\Control Panel\\Desktop";
constexpr LPCWSTR            REG_LANGUAGE_PER_WINDOW_KEY      = L"UserPreferencesMask";
constexpr unsigned int       REG_LANGUAGE_PER_WINDOW_OFFSET   = 31;
constexpr unsigned long long REG_LANGUAGE_PER_WINDOW_MASK     = (0x1 << REG_LANGUAGE_PER_WINDOW_OFFSET);

constexpr UINT               MAX_RETRY_TIMES = 2;

constexpr LCID HKL_TO_LCID(HKL hkl) { return (long(hkl) & 0xffff); }

#define isNotInVector(l, v) (std::find(l.begin(), l.end(),v) == l.end())

void LanguageSwitcher::buildLanguageList() {
    WCHAR buffer[REG_LANGUAGE_MULTI_SZ_MAX_LENGTH];
    DWORD dwLen = sizeof(buffer);
    RegGetValue(HKEY_CURRENT_USER, REG_LANGUAGES_DIR, REG_LANGUAGES_KEY, RRF_RT_REG_MULTI_SZ, NULL, buffer, &dwLen);

    for (size_t i = 0; (buffer[i] != L'\0' && i < REG_LANGUAGE_MULTI_SZ_MAX_LENGTH); i++) {
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

    if (onLanguageChange) {
        onLanguageChange(inImeMode, categories[inImeMode].index);
    }
    return inImeMode;
}

unsigned int LanguageSwitcher::nextLanguage() {
    categories[inImeMode].index++;
    if (categories[inImeMode].index >= categories[inImeMode].langs.size() || categories[inImeMode].index < 0) {
        categories[inImeMode].index = 0;
    }
    updateInputLanguage();

    if (onLanguageChange) {
        onLanguageChange(inImeMode, categories[inImeMode].index);
    }

    return categories[inImeMode].index;
}

unsigned int LanguageSwitcher::lastLanguage() {
    categories[inImeMode].index--;
    if (categories[inImeMode].index >= categories[inImeMode].langs.size() || categories[inImeMode].index < 0) {
        categories[inImeMode].index = categories[inImeMode].langs.size() - 1;
    }
    updateInputLanguage();

    if (onLanguageChange) {
        onLanguageChange(inImeMode, categories[inImeMode].index);
    }

    return categories[inImeMode].index;
}

bool LanguageSwitcher::getCategory() {
    return inImeMode;
}

LCID LanguageSwitcher::getCurrentLanguage() {
    return categories[inImeMode].langs[categories[inImeMode].index].getLocaleId();
}

bool LanguageSwitcher::setCurrentLanguage(LCID lcid) {
    if (getCurrentLanguage() != lcid) {
        for (unsigned int i = 0; i < (sizeof(categories) / sizeof(categories[0])); i++) {
            for (unsigned int j = 0; j < categories[i].langs.size(); j++) {
                if (categories[i].langs[j].getLocaleId() == lcid) {
                    inImeMode = i;
                    categories[i].index = j;
                    if (onLanguageChange) {
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
void LanguageSwitcher::fixImeConversionMode(HWND hWnd, LCID language) {
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

    auto imeHwnd = ImmGetDefaultIMEWnd(hWnd);
    LRESULT dwConversion = SendMessage(imeHwnd, WM_IME_CONTROL, IMC_GETCONVERSIONMODE, 0);
    if (imeConversionModeCodeMap.count(language) == 0 || imeConversionModeCodeMap.at(language).empty()) {
        return;
    }
    auto retryCount = 0;
    //wcout << "Current: " << dwConversion << " new: " << imeConversionModeCodeMap.at(language)[0] << endl;
    while (isNotInVector(imeConversionModeCodeMap.at(language), dwConversion) && retryCount++ < MAX_RETRY_TIMES) {
        SendMessage(imeHwnd, WM_IME_CONTROL, IMC_SETCONVERSIONMODE, imeConversionModeCodeMap.at(language)[0]);
        //Sleep(50);
        //wcout << "Now: " << SendMessage(imeHwnd, WM_IME_CONTROL, IMC_GETCONVERSIONMODE, 0) << endl;
    }
}

void LanguageSwitcher::fixImeConversionMode(HWND hWnd) {
    if (categories[inImeMode].langs[categories[inImeMode].index].isImeLanguage()) {
        fixImeConversionMode(hWnd, categories[inImeMode].langs[categories[inImeMode].index].getLocaleId());
    }
}


vector<LCID> LanguageSwitcher::getLanguageList(bool getImeLanguageList) {
    vector<LCID> languageList;

    for (auto &lang : categories[getImeLanguageList].langs) {
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

LanguageSwitcher* LanguageSwitcher::instance;
void CALLBACK LanguageSwitcher::onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {    
    instance->setCurrentLanguage(HKL_TO_LCID(GetKeyboardLayout(GetWindowThreadProcessId(hwnd, nullptr))));
    //wcout << L"Active window change, new language: " << localeMap.at(switcher.getCurrentLanguage()).desc << endl;
    instance->fixImeConversionMode(hwnd);
}


LanguageSwitcher::LanguageSwitcher() : LanguageSwitcher(false, vector<LCID>()) {}

LanguageSwitcher::LanguageSwitcher(bool defaultImeMode, vector<LCID> imeLanguageOrder) {
    instance = this;
    if (instance != this) {
        return;
    }

    inImeMode = defaultImeMode;
    buildLanguageList();

    for (auto& cate : categories) {
        if (cate.langs.empty()) {
            exit(0); // you don't need it
        }
    }

    for (int i = imeLanguageOrder.size() - 1; i >= 0; i--) {
        auto currentLanguageList = getLanguageList(true);
        auto it = find(currentLanguageList.begin(), currentLanguageList.end(), imeLanguageOrder[i]);
        if (it != currentLanguageList.end())
        {
            auto actualIterator = categories[true].langs.begin() + (it - currentLanguageList.begin());
            auto temp = *actualIterator;
            categories[true].langs.erase(actualIterator);
            categories[true].langs.insert(categories[true].langs.begin(), temp);
        }
    }

    updateInputLanguage();

    windowChangeEvent = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, NULL, LanguageSwitcher::onActiveWindowChange, 0, 0, WINEVENT_OUTOFCONTEXT);
}

LanguageSwitcher::~LanguageSwitcher() {
    UnhookWinEvent(windowChangeEvent);
}