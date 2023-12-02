#pragma once

#include <Windows.h>
#include <iostream>
#include <set>

using namespace std;

namespace FruitToolbox {
namespace Interop {
namespace Unmanaged {
constexpr LCID zh_TW = 0x404; // Chinese (Traditional, Taiwan)
constexpr LCID ja_JP = 0x411; // Japanese (Japan)
constexpr LCID ko_KR = 0x412; // Korean (Korea)
constexpr LCID am_ET = 0x45E; // Amharic (Ethiopia)
constexpr LCID ti_ET = 0x473; // Tigrinya (Ethiopia)
constexpr LCID zh_CN = 0x804; // Chinese (Simplified, PRC)
constexpr LCID ti_ER = 0x873; // Tigrinya (Eritrea)
constexpr LCID zh_HK = 0xC04; // Chinese (Traditional, Hong Kong S.A.R.)
constexpr LCID zh_SG = 0x1004; // Chinese (Simplified, Singapore)
constexpr LCID zh_MO = 0x1404; // Chinese (Traditional, Macao S.A.R.)

static const set<LCID> ImeLanguages = {
    zh_TW,
    ja_JP,
    ko_KR,
    am_ET,
    ti_ET,
    zh_CN,
    ti_ER,
    zh_HK,
    zh_SG,
    zh_MO,
};

static constexpr LCID LANGUAGE_TO_LOCALE_OFFSET = 0x400;
static constexpr bool IS_LOCALE(LCID lcid) {
    return lcid >= LANGUAGE_TO_LOCALE_OFFSET;
}
static constexpr LCID LANGUAGE_TO_LOCALE(LCID lcid) {
    return lcid + LANGUAGE_TO_LOCALE_OFFSET;
}

class Language {
private:
    LCID localeId;
    wstring localeName;

public:
    explicit Language() = default;

    explicit Language(const WCHAR* localeName)
        : Language(LocaleNameToLCID(localeName, LOCALE_ALLOW_NEUTRAL_NAMES)) {
    }

    explicit Language(WCHAR* localeName)
        : Language(LocaleNameToLCID(localeName, LOCALE_ALLOW_NEUTRAL_NAMES)) {
    }

    explicit Language(LCID localeId) {
        if (!IS_LOCALE(localeId)) {
            localeId = LANGUAGE_TO_LOCALE(localeId);
        }
        this->localeId = localeId;
        WCHAR localeNameCharArr[LOCALE_NAME_MAX_LENGTH];
        LCIDToLocaleName(this->localeId, localeNameCharArr, LOCALE_NAME_MAX_LENGTH, LOCALE_ALLOW_NEUTRAL_NAMES);
        localeName = wstring(localeNameCharArr);
    }

    LCID getLocaleId() const {
        return localeId;
    }

    bool isImeLanguage() const {
        return ImeLanguages.count(localeId);
    }
};
}
}
}
