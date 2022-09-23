#pragma once

#include <Windows.h>
#include <iostream>
#include <set>

using namespace std;

namespace FruitLanguageSwitcher {
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

    const static set<LCID> ImeLanguages = {
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

    class Language {
    private:
        LCID localeId;
        wstring localeName;

    public:
        explicit Language(LCID localeId);
        explicit Language(WCHAR* localeName);
        explicit Language(const WCHAR* localeName);

        LCID getLocaleId();
        bool isImeLanguage();
    };
}
