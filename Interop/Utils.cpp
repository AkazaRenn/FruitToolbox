#include "pch.h"
#include "Utils.h"

using namespace System;
using namespace Runtime::InteropServices;

using namespace FruitToolbox::Interop;

void Utils::SetBorderlessWindow(IntPtr hwnd) {
    int cornerPreference = DWMWCP_DONOTROUND;
    DwmSetWindowAttribute(
        static_cast<HWND>(hwnd.ToPointer()),
        DWMWA_WINDOW_CORNER_PREFERENCE,
        &cornerPreference,
        sizeof(cornerPreference));
}

String^ Utils::GetWindowTitle(IntPtr hwnd) {
    wchar_t title[256];
    GetWindowTextW(static_cast<HWND>(hwnd.ToPointer()), title, sizeof(title) / sizeof(title[0]));
    return Marshal::PtrToStringUni(static_cast<IntPtr>(title));
}

void Utils::Unfocus() {
    SetForegroundWindow(GetShellWindow());
}
