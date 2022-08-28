#pragma once

#include <Windows.h>
#include <iostream>
#include <set>

using namespace std;

const static set<LCID> imeLangSet = {
	0x404, // zh-TW, Chinese (Traditional, Taiwan)
	0x411, // ja-JP, Japanese (Japan)
	0x412, // ko-KR, Korean (Korea)
	0x45E, // am-ET, Amharic (Ethiopia)
	0x473, // ti-ET, Tigrinya (Ethiopia)
	0x804, // zh-CN, Chinese (Simplified, PRC)
	0x873, // ti-ER, Tigrinya (Eritrea)
	0xC04, // zh-HK, Chinese (Traditional, Hong Kong S.A.R.)
	0x1004, // zh-SG, Chinese (Simplified, Singapore)
	0x1404, // zh-MO, Chinese (Traditional, Macao S.A.R.)
};

class Language
{
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
