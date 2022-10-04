#pragma once

#include "Language.h"
#include <functional>

#define isKeyDown(nVirtKey) (GetKeyState(nVirtKey) & 0x8000)

namespace FruitLanguageSwitcher {
    using namespace std;

    constexpr UINT KEYEVENTF_KEYDOWN = 0;
    constexpr UINT IMC_GETCONVERSIONMODE = 0x1;
    constexpr UINT IMC_SETCONVERSIONMODE = 0x2;

    typedef function<bool(void)> keyHandler; // return true if the captured key should be passed through
    typedef function<bool(HWND)> conversionModeGetter;
    typedef function<void(HWND)> conversionModeSetter;

    struct PerLanguageMethods {
        keyHandler onRaltDown;
        keyHandler onRaltUp;
        conversionModeGetter inConversionMode;
        conversionModeSetter fixConversionMode;
    };

    static PerLanguageMethods NonImeLanguageMethods = {
        []()-> bool {
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0); // RAlt to AltGr
            return true;
        },
        [] ()-> bool {
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
            return true;
        },
        [] (HWND hwnd) -> bool {
            return true;
        },
        [] (HWND hwnd) -> void {},
    };

    static PerLanguageMethods JapaneseMethods = {
        []() -> bool {
            keybd_event(VK_NONCONVERT, 0, KEYEVENTF_KEYDOWN, 0);
            return false;
        },
        [] () -> bool {
            keybd_event(VK_NONCONVERT, 0, KEYEVENTF_KEYUP, 0);
            return false;
        },
        [] (HWND hwnd) -> bool {
            return false;
        },
        //[TODO] ctrl + VK_CONVERT will wake the IME config menu which I have no idea why it's designed like that.
        // Only solution would be hooking LCTRL as well but I don't want to do that.
        // But... but! If I make further chagnes for GUI and let LWIN up being the only time to apply the language change,
        // it might be fine by adding a loop waiting for ctrl release.
        // So yes, [TODO] until switcher GUI implemented.
        [] (HWND hwnd) -> void {
            // this is the [TODO]: while(isKeyDown(VK_LCONTROL));
            keybd_event(VK_CONVERT, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(VK_CONVERT, 0, KEYEVENTF_KEYUP, 0);
        },
    };

    constexpr UINT ChineseImeConversionModeCode = 1;
    static PerLanguageMethods ChineseMethods {
        []() -> bool {
            return true;
        },
        [] () -> bool {
            return true;
        },
        [] (HWND hwnd) -> bool {
            return SendMessage(ImmGetDefaultIMEWnd(hwnd), WM_IME_CONTROL, IMC_GETCONVERSIONMODE, 0) == ChineseImeConversionModeCode;
        },
        [] (HWND hwnd) -> void {
            SendMessage(ImmGetDefaultIMEWnd(hwnd), WM_IME_CONTROL, IMC_SETCONVERSIONMODE, ChineseImeConversionModeCode);
        },
    };

    inline static PerLanguageMethods& getPerLanguageMethods(LCID lcid) {
        switch(lcid) {
        case zh_TW:
        case zh_CN:
        case zh_HK:
        case zh_SG:
        case zh_MO:
        case ko_KR:
            return ChineseMethods;
        case ja_JP:
            return JapaneseMethods;
        case am_ET:
        case ti_ET:
        case ti_ER:
        default:
            return NonImeLanguageMethods;
        }
    }
}
