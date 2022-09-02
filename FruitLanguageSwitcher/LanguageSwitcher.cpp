#include "LanguageSwitcher.h"
#include <vector>

constexpr UINT               IMC_GETCONVERSIONMODE = 0x1;
constexpr UINT               IMC_SETCONVERSIONMODE = 0x2;

constexpr size_t             REG_LANGUAGE_MULTI_SZ_MAX_LENGTH = 1024;
constexpr LPCWSTR            REG_LANGUAGES_DIR                = L"Control Panel\\International\\User Profile";
constexpr LPCWSTR            REG_LANGUAGES_KEY                = L"Languages";

constexpr LPCWSTR            REG_LANGUAGE_PER_WINDOW_DIR      = L"HKEY_CURRENT_USER\\Control Panel\\Desktop";
constexpr LPCWSTR            REG_LANGUAGE_PER_WINDOW_KEY      = L"UserPreferencesMask";
constexpr unsigned int       REG_LANGUAGE_PER_WINDOW_OFFSET   = 31;
constexpr unsigned long long REG_LANGUAGE_PER_WINDOW_MASK     = (0x1 << REG_LANGUAGE_PER_WINDOW_OFFSET);

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

void LanguageSwitcher::swapCategory() {
    inImeMode = !inImeMode;
    updateInputLanguage();
}

void LanguageSwitcher::nextLanguage() {
    categories[inImeMode].index++;
    if (categories[inImeMode].index >= categories[inImeMode].langs.size() || categories[inImeMode].index < 0) {
        categories[inImeMode].index = 0;
    }
    updateInputLanguage();
}

void LanguageSwitcher::lastLanguage() {
    categories[inImeMode].index--;
    if (categories[inImeMode].index >= categories[inImeMode].langs.size() || categories[inImeMode].index < 0) {
        categories[inImeMode].index = categories[inImeMode].langs.size() - 1;
    }
    updateInputLanguage();
}

bool LanguageSwitcher::isInImeMode() {
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
                    //updateInputLanguage();
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
        {0x411,  {9, 27}}, // ja-JP, Japanese (Japan)
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
    wcout << "Current: " << dwConversion << " new: " << imeConversionModeCodeMap.at(language)[0] << endl;
    if (isNotInVector(imeConversionModeCodeMap.at(language), dwConversion)) {
        SendMessage(imeHwnd, WM_IME_CONTROL, IMC_SETCONVERSIONMODE, imeConversionModeCodeMap.at(language)[0]);
        wcout << "Now: " << SendMessage(imeHwnd, WM_IME_CONTROL, IMC_GETCONVERSIONMODE, 0) << endl;
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


LanguageSwitcher::LanguageSwitcher() : LanguageSwitcher(false) {}

LanguageSwitcher::LanguageSwitcher(bool defaultImeMode) {
    inImeMode = defaultImeMode;
    buildLanguageList();

    for (auto &cate : categories) {
        if (cate.langs.empty()) {
            exit(0); // you don't need it
        }
    }

    updateInputLanguage();
}
