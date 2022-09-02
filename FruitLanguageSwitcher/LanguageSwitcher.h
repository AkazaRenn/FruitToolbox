#pragma once

#include <vector>
#include "Language.h"
#include <map>

using namespace std;

struct languageCategory {
	vector<Language> langs;
	unsigned int index = 0;
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
	void fixImeConversionMode(HWND hWnd);
};

