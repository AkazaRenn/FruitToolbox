#pragma once

#include <functional>
#include "Language.hpp"

#define isKeyDown(nVirtKey) (GetKeyState(nVirtKey) & 0x8000)

using namespace std;

namespace FruitToolbox {
namespace Interop {
namespace Unmanaged {
constexpr UINT KEYEVENTF_KEYDOWN = 0;
constexpr UINT IMC_GETCONVERSIONMODE = 0x1;
constexpr UINT IMC_SETCONVERSIONMODE = 0x2;

typedef function<void(void)> keyHandler; // return true if the captured key should be passed through
typedef function<bool(HWND)> conversionModeGetter;
typedef function<void(HWND)> conversionModeSetter;

struct PerLanguageMethods {
    keyHandler onRaltUp;
    conversionModeGetter inConversionMode;
    conversionModeSetter fixConversionMode;
};

static PerLanguageMethods NonImeLanguageMethods = {
    []()-> void {},
    [](HWND hwnd) -> bool {
        return true;
    },
    [](HWND hwnd) -> void {},
};

static PerLanguageMethods JapaneseMethods = {
    []() -> void {
        keybd_event(VK_NONCONVERT, 0, KEYEVENTF_KEYDOWN, 0);
        keybd_event(VK_NONCONVERT, 0, KEYEVENTF_KEYUP, 0);
    },
    [](HWND hwnd) -> bool {
        return false;
    },
    [](HWND hwnd) -> void {
        keybd_event(VK_IME_ON, 0, KEYEVENTF_KEYDOWN, 0);
        keybd_event(VK_IME_ON, 0, KEYEVENTF_KEYUP, 0);
    },
};

constexpr UINT ChineseImeConversionModeCode = 1;
static PerLanguageMethods ChineseMethods{
    []() -> void {},
    [](HWND hwnd) -> bool {
        return SendMessage(ImmGetDefaultIMEWnd(hwnd), WM_IME_CONTROL, IMC_GETCONVERSIONMODE, 0) == ChineseImeConversionModeCode;
    },
    [](HWND hwnd) -> void {
        SendMessage(ImmGetDefaultIMEWnd(hwnd), WM_IME_CONTROL, IMC_SETCONVERSIONMODE, ChineseImeConversionModeCode);
    },
};

static inline PerLanguageMethods& getPerLanguageMethods(LCID lcid) {
    switch (lcid) {
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
}
}
