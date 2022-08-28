#include <iostream>
#include <windows.h>
#include "LanguageSwitcher.h"

#define HOTKEY_ANOTHER_LANG_LIST_MODIFIER (MOD_ALT | MOD_NOREPEAT)
#define HOTKEY_ANOTHER_LANG_LIST_KEY      (66) // B key

#define HOTKEY_NEXT_LANG_MODIFIER         (MOD_ALT | MOD_NOREPEAT)
#define HOTKEY_NEXT_LANG_KEY              (78) // N key

using namespace std;

enum HotkeyMessageId { SwapCategory, NextLanguage, LastLanguage };

bool registerHotkeys() {
    if (RegisterHotKey(NULL, SwapCategory, HOTKEY_ANOTHER_LANG_LIST_MODIFIER, HOTKEY_ANOTHER_LANG_LIST_KEY)) {
        cout << "Hotkey 'ALT+b' registered, using MOD_NOREPEAT flag\n" << endl;
    }
    
    if (RegisterHotKey(NULL, NextLanguage, HOTKEY_NEXT_LANG_MODIFIER, HOTKEY_NEXT_LANG_KEY)) {
        cout << "Hotkey 'ALT+n' registered, using MOD_NOREPEAT flag\n" << endl;
    }

    return true;
}


int main()
{
    LanguageSwitcher switcher;

    if (!registerHotkeys()) {
        exit(-1);
    }

    MSG msg = { 0 };
    while (GetMessage(&msg, NULL, 0, 0) != 0)
    {
        if (msg.message == WM_HOTKEY)
        {
            if (msg.wParam == SwapCategory) {
                cout << "alt + b received\n" << endl;
                switcher.swapCategory();
            }
            else if (msg.wParam == NextLanguage) {
                cout << "alt + n received\n" << endl;
                switcher.nextLanguage();
            }
        }
    }
}
