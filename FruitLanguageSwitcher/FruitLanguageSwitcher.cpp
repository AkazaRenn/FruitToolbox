// WinLangSwitcherConsole.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <windows.h>
#include <winreg.h>
#include "Language.h"
#include <vector>

#define REG_LANGUAGE_MULTI_SZ_MAX_LENGTH  (1024)
#define REG_LANGUAGES_DIR                 (L"Control Panel\\International\\User Profile")
#define REG_LANGUAGES_KEY                 (L"Languages")

#define HOTKEY_ANOTHER_LANG_LIST_ID       (1)
#define HOTKEY_ANOTHER_LANG_LIST_MODIFIER (MOD_ALT | MOD_NOREPEAT)
#define HOTKEY_ANOTHER_LANG_LIST_KEY      (66) // B key

#define HOTKEY_NEXT_LANG_ID               (2)
#define HOTKEY_NEXT_LANG_MODIFIER         (MOD_ALT | MOD_NOREPEAT)
#define HOTKEY_NEXT_LANG_KEY              (78) // N key

using namespace std;

typedef struct {
    vector<Language> list[2];
} languages;

void changeInputLang(HKL newLang)
{
    SendMessageW(GetForegroundWindow(), WM_INPUTLANGCHANGEREQUEST, 0, reinterpret_cast<LPARAM>(newLang));
}

languages getLangList() {
    vector<Language> keyboardLangs;
    vector<Language> imeLangs;
    WCHAR buffer[REG_LANGUAGE_MULTI_SZ_MAX_LENGTH];
    DWORD dwLen = sizeof(buffer);
    RegGetValueW(HKEY_CURRENT_USER, REG_LANGUAGES_DIR, REG_LANGUAGES_KEY, RRF_RT_REG_MULTI_SZ, NULL, buffer, &dwLen);

    for (size_t i = 0; i < REG_LANGUAGE_MULTI_SZ_MAX_LENGTH; i++) {
        if (buffer[i] == L'\0') {
            break;
        }

        auto newLang = Language(buffer + i);
        newLang.isImeLanguage() ? imeLangs.push_back(newLang) : keyboardLangs.push_back(newLang);

        i += wcslen(buffer + i);
    }

    return { {keyboardLangs, imeLangs} };
}

bool registerHotkeys() {
    if (RegisterHotKey(NULL, HOTKEY_ANOTHER_LANG_LIST_ID, HOTKEY_ANOTHER_LANG_LIST_MODIFIER, HOTKEY_ANOTHER_LANG_LIST_KEY)) {
        cout << "Hotkey 'ALT+b' registered, using MOD_NOREPEAT flag\n" << endl;
    }
    
    if (RegisterHotKey(NULL, HOTKEY_NEXT_LANG_ID, HOTKEY_NEXT_LANG_MODIFIER, HOTKEY_NEXT_LANG_KEY)) {
        cout << "Hotkey 'ALT+n' registered, using MOD_NOREPEAT flag\n" << endl;
    }

    return true;
}


int main()
{
    bool inImeMode = false;
    unsigned int currentLangIndex[2] = {0};

    auto langs = getLangList();

    if (!registerHotkeys()) {
        exit(-1);
    }

    MSG msg = { 0 };
    while (GetMessage(&msg, NULL, 0, 0) != 0)
    {
        if (msg.message == WM_HOTKEY)
        {
            if (msg.wParam == HOTKEY_ANOTHER_LANG_LIST_ID) {
                cout << "alt + b received\n" << endl;
                inImeMode = !inImeMode;
                changeInputLang(HKL(langs.list[inImeMode][currentLangIndex[inImeMode]].getLocaleId()));
            }
            else if (msg.wParam == HOTKEY_NEXT_LANG_ID) {
                cout << "alt + n received\n" << endl;
                currentLangIndex[inImeMode]++;
                if (currentLangIndex[inImeMode] >= langs.list[inImeMode].size()) {
                    currentLangIndex[inImeMode] = 0;
                }
                changeInputLang(HKL(langs.list[inImeMode][currentLangIndex[inImeMode]].getLocaleId()));
            }
        }
    }
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
