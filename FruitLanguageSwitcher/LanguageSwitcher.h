#pragma once

#include <vector>
#include "Language.h"

struct languageCategory {
	vector<Language> langs;
	unsigned int index = 0;
} ;

class LanguageSwitcher
{
private:
	languageCategory categories[2];
	void getLanguageList();
	bool inImeMode;
	void updateInputLanguage();

public:
	explicit LanguageSwitcher();
	//explicit LanguageSwitcher(vector<unsigned long> imeLangOrder);

	void swapCategory();
	void nextLanguage();
	void lastLanguage();
};

