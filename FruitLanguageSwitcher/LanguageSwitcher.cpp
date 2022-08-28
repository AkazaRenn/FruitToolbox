#include "LanguageSwitcher.h"
#include <vector>

constexpr unsigned long REG_LANGUAGE_MULTI_SZ_MAX_LENGTH = 1024;
constexpr LPCWSTR       REG_LANGUAGES_DIR                = L"Control Panel\\International\\User Profile";
constexpr LPCWSTR       REG_LANGUAGES_KEY                = L"Languages";

void LanguageSwitcher::getLanguageList() {
    WCHAR buffer[REG_LANGUAGE_MULTI_SZ_MAX_LENGTH];
    DWORD dwLen = sizeof(buffer);
    RegGetValueW(HKEY_CURRENT_USER, REG_LANGUAGES_DIR, REG_LANGUAGES_KEY, RRF_RT_REG_MULTI_SZ, NULL, buffer, &dwLen);

    for (size_t i = 0; i < REG_LANGUAGE_MULTI_SZ_MAX_LENGTH; i++) {
        if (buffer[i] == L'\0') {
            break;
        }

        auto newLang = Language(buffer + i);
        newLang.isImeLanguage() ? categories[1].langs.push_back(newLang) : categories[0].langs.push_back(newLang);

        i += wcslen(buffer + i);
    }
}

void LanguageSwitcher::updateInputLanguage()
{
    auto newLanguage = HKL(categories[inImeMode].langs[categories[inImeMode].index].getLocaleId());
    SendMessageW(GetForegroundWindow(), WM_INPUTLANGCHANGEREQUEST, 0, reinterpret_cast<LPARAM>(newLanguage));
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
    if (categories[inImeMode].index < 0) {
        categories[inImeMode].index = categories[inImeMode].langs.size() - 1;
    }
    updateInputLanguage();
}


LanguageSwitcher::LanguageSwitcher() {
    inImeMode = false;
    getLanguageList();

    for (auto cate : categories) {
        if (cate.langs.empty()) {
            exit(0); // you don't need it
        }
    }

    updateInputLanguage();
}