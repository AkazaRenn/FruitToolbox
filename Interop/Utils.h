#pragma once
namespace FruitToolbox {
namespace Interop {
public ref class Utils
{
public:
    constexpr static int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    constexpr static int DWMWCP_DONOTROUND = 1;
    static void SetBorderlessWindow(System::IntPtr hwnd);
    static System::String^ GetWindowTitle(System::IntPtr hwnd);
    static void Unfocus();
};
}
}
