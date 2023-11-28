#include "pch.h"
#include "Utils.h"

using namespace FruitToolbox::Interop;

void Utils::SetBorderlessWindow(System::IntPtr hwnd) {
    int cornerPreference = DWMWCP_DONOTROUND;
    DwmSetWindowAttribute(
        static_cast<HWND>(hwnd.ToPointer()),
        DWMWA_WINDOW_CORNER_PREFERENCE,
        &cornerPreference,
        sizeof(cornerPreference));
}

int Utils::GetProcessId(System::IntPtr hwnd) {
    DWORD processId;
    GetWindowThreadProcessId(static_cast<HWND>(hwnd.ToPointer()), &processId);
    return processId;
}
