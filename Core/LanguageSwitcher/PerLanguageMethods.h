#pragma once

#include "Language.h"

namespace FruitLanguageSwitcher {
    constexpr UINT KEYEVENTF_KEYDOWN = 0;
    constexpr UINT IMC_GETCONVERSIONMODE = 0x1;
    constexpr UINT IMC_SETCONVERSIONMODE = 0x2;

    typedef bool (*keyHandler)(); // return true if the captured key should be passed through
    typedef bool (*conversionModeGetter)(HWND);
    typedef void (*conversionModeSetter)(HWND);

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
        [] (HWND hwnd) -> void {
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

    inline static PerLanguageMethods getPerLanguageMethods(LCID lcid) {
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
