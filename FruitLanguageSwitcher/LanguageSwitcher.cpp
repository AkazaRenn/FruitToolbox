#include "LanguageSwitcher.h"
#include <vector>

constexpr size_t             REG_LANGUAGE_MULTI_SZ_MAX_LENGTH = 1024;
constexpr LPCWSTR            REG_LANGUAGES_DIR                = L"Control Panel\\International\\User Profile";
constexpr LPCWSTR            REG_LANGUAGES_KEY                = L"Languages";

constexpr LPCWSTR            REG_LANGUAGE_PER_WINDOW_DIR      = L"HKEY_CURRENT_USER\\Control Panel\\Desktop";
constexpr LPCWSTR            REG_LANGUAGE_PER_WINDOW_KEY      = L"UserPreferencesMask";
constexpr unsigned int       REG_LANGUAGE_PER_WINDOW_OFFSET   = 31;
constexpr unsigned long long REG_LANGUAGE_PER_WINDOW_MASK     = (0x1 << REG_LANGUAGE_PER_WINDOW_OFFSET);

void LanguageSwitcher::getLanguageList() {
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
    auto newLanguage = HKL(categories[inImeMode].langs[categories[inImeMode].index].getLocaleId());
    SendMessage(GetForegroundWindow(), WM_INPUTLANGCHANGEREQUEST, 0, reinterpret_cast<LPARAM>(newLanguage));
}

void LanguageSwitcher::swapCategory() {
    inImeMode = !inImeMode;
    updateInputLanguage();
}

void LanguageSwitcher::nextLanguage() {
    categories[inImeMode].index++;
    if (categories[inImeMode].index >= categories[inImeMode].langs.size()) {
        categories[inImeMode].index = 0;
    }
    updateInputLanguage();
}

void LanguageSwitcher::lastLanguage() {
    categories[inImeMode].index--;
    if (categories[inImeMode].index >= categories[inImeMode].langs.size()) {
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
    return false; // todo
}

vector<LCID> LanguageSwitcher::getLanguageList(bool getImeLanguageList)
{
    vector<LCID> languageList;

    for (auto &lang : categories[getImeLanguageList].langs) {
        languageList.push_back(lang.getLocaleId());
    }

    return languageList;
}

LanguageSwitcher::LanguageSwitcher() : LanguageSwitcher(false) {}

LanguageSwitcher::LanguageSwitcher(bool defaultImeMode) {
    inImeMode = defaultImeMode;
    getLanguageList();

    for (auto &cate : categories) {
        if (cate.langs.empty()) {
            exit(0); // you don't need it
        }
    }

    updateInputLanguage();
}
