#include "pch.h"
#include "Language.h"

using namespace FruitToolbox::LanguageSwitcher::Core;

constexpr long LANGUAGE_TO_LOCALE_OFFSET = 0x400;
#define		   IS_LOCALE(lcid)			 (lcid >= LANGUAGE_TO_LOCALE_OFFSET)
#define		   LANGUAGE_TO_LOCALE(lcid)	 (lcid += LANGUAGE_TO_LOCALE_OFFSET)

Language::Language()
    : Language((LCID)0) {}

Language::Language(const WCHAR* localeName)
    : Language((WCHAR*)localeName) {}

Language::Language(WCHAR* localeName)
    : Language(LocaleNameToLCID(localeName, LOCALE_ALLOW_NEUTRAL_NAMES)) {}

Language::Language(LCID localeId) {
    if(!IS_LOCALE(localeId)) {
        LANGUAGE_TO_LOCALE(localeId);
    }
    this->localeId = localeId;
    WCHAR localeNameCharArr[LOCALE_NAME_MAX_LENGTH];
    LCIDToLocaleName(this->localeId, localeNameCharArr, LOCALE_NAME_MAX_LENGTH, LOCALE_ALLOW_NEUTRAL_NAMES);
    localeName = wstring(localeNameCharArr);
}

LCID Language::getLocaleId() {
    return localeId;
}

bool Language::isImeLanguage() {
    return ImeLanguages.count(localeId);
}
