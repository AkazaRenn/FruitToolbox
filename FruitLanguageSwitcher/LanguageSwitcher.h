#pragma once

#include <vector>
#include "Language.h"
#include <map>

struct languageCategory {
	vector<Language> langs;
	unsigned int index = 0;
} ;

struct imeConversionModeCode {
	UINT noConversionModeCode;
	UINT conversionModeCode;
};

const std::map<long, imeConversionModeCode> imeConversionModeCodeMap {
	{0x404,  {0, 1}}, // zh-TW, Chinese (Traditional, Taiwan)
	{0x411,  {9, 27}}, // ja-JP, Japanese (Japan)
	//{0x412,  {}}, // ko-KR, Korean (Korea)
	//{0x45E,  {}}, // am-ET, Amharic (Ethiopia)
	//{0x473,  {}}, // ti-ET, Tigrinya (Ethiopia)
	//{0x804,  {}}, // zh-CN, Chinese (Simplified, PRC)
	//{0x873,  {}}, // ti-ER, Tigrinya (Eritrea)
	//{0xC04,  {}}, // zh-HK, Chinese (Traditional, Hong Kong S.A.R.)
	//{0x1004, {}}, // zh-SG, Chinese (Simplified, Singapore)
	//{0x1404, {}}, // zh-MO, Chinese (Traditional, Macao S.A.R.)
};

class LanguageSwitcher
{
private:
	languageCategory categories[2];
	void buildLanguageList();
	bool inImeMode;
	void updateInputLanguage();
	void fixImeConversionMode(HWND hWnd, LCID language);

public:
	explicit LanguageSwitcher(bool defaultImeMode);
	explicit LanguageSwitcher();
	//explicit LanguageSwitcher(vector<unsigned long> imeLangOrder);

	void swapCategory();
	void nextLanguage();
	void lastLanguage();
	bool isInImeMode();
	LCID getCurrentLanguage();
	bool setCurrentLanguage(LCID lcid);
	vector<LCID> getLanguageList(bool getImeLanguageList);
};

