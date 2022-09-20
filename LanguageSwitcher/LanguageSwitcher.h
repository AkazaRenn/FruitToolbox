#pragma once

#include <vector>
#include "Language.h"
#include <map>

#define DLLEXPORT __declspec(dllexport)

using namespace std;

typedef void(__stdcall* OnLanguageChange)(bool inImeMode, unsigned int languageIndex);

namespace FruitLanguageSwitcher {
	struct languageCategory {
		vector<Language> langs;
		unsigned int index = 0;
	};

	class LanguageSwitcher
	{
	private:
		static LanguageSwitcher* instance;

		languageCategory categories[2];
		void buildLanguageList();
		bool inImeMode;
		void updateInputLanguage();
		void fixImeConversionMode(HWND hWnd);
		void fixImeConversionMode(HWND hWnd, LCID language);

		HWINEVENTHOOK windowChangeEvent;
		//void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
		OnLanguageChange onLanguageChange = nullptr;

	public:
		explicit LanguageSwitcher(bool defaultImeMode, vector<LCID> imeLanguageOrder);
		explicit LanguageSwitcher();
		~LanguageSwitcher();

		bool swapCategory();
		bool getCategory();
		unsigned int nextLanguage();
		unsigned int lastLanguage();
		LCID getCurrentLanguage();
		bool setCurrentLanguage(LCID lcid); // returns true if lcid is in the list, false otherwise
		vector<LCID> getLanguageList(bool getImeLanguageList);
		void setOnLanguageChange(OnLanguageChange handler);

		static bool registerHotkeys();
		static void CALLBACK onActiveWindowChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
	};
}

extern "C"
{
	DLLEXPORT void* LanguageSwitcher_new(bool defaultImeMode, DWORD* imeLanguageOrder, unsigned int n)
	{
		return (void*) new FruitLanguageSwitcher::LanguageSwitcher(defaultImeMode, vector<LCID>(imeLanguageOrder, imeLanguageOrder + n));
	}

	DLLEXPORT void LanguageSwitcher_delete(FruitLanguageSwitcher::LanguageSwitcher* s)
	{
		delete s;
	}

	DLLEXPORT bool LanguageSwitcher_swapCategory(FruitLanguageSwitcher::LanguageSwitcher * s)
	{
		return s->swapCategory();
	}

	DLLEXPORT bool LanguageSwitcher_getCategory(FruitLanguageSwitcher::LanguageSwitcher * s)
	{
		return s->getCategory();
	}

	DLLEXPORT unsigned int LanguageSwitcher_nextLanguage(FruitLanguageSwitcher::LanguageSwitcher * s)
	{
		return s->nextLanguage();
	}

	DLLEXPORT unsigned int LanguageSwitcher_lastLanguage(FruitLanguageSwitcher::LanguageSwitcher * s)
	{
		return s->lastLanguage();
	}

	DLLEXPORT DWORD LanguageSwitcher_getCurrentLanguage(FruitLanguageSwitcher::LanguageSwitcher * s)
	{
		return s->getCurrentLanguage();
	}

	DLLEXPORT bool LanguageSwitcher_setCurrentLanguage(FruitLanguageSwitcher::LanguageSwitcher * s, DWORD newLanguage)
	{
		return s->setCurrentLanguage(newLanguage);
	}

	DLLEXPORT unsigned int LanguageSwitcher_getLanguageList(FruitLanguageSwitcher::LanguageSwitcher * s, bool isImeLanguageList, DWORD * list)
	{
		auto langVec = s->getLanguageList(isImeLanguageList);
		list = &(langVec[0]);
		return langVec.size();
	}

	DLLEXPORT void LanguageSwitcher_orderLanguageList(FruitLanguageSwitcher::LanguageSwitcher * s, bool isImeLanguageList, DWORD * list)
	{
		return;
	}

	DLLEXPORT void LanguageSwitcher_setOnLanguageChange(FruitLanguageSwitcher::LanguageSwitcher* s, OnLanguageChange handler)
	{
		s->setOnLanguageChange(handler);
	}
}