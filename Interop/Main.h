#pragma once

#include "pch.h"
#include "LanguageSwitcher.h"
#include "WindowTracker.h"

namespace FruitToolbox {
namespace Interop {
public ref class LanguageSwitcher {
public:
    delegate void OnLanguageChangeCallbackDelegate(int lcid);

    static bool Start(OnLanguageChangeCallbackDelegate^ handler);
    static void Stop();
    static void UpdateInputLanguage();
    static bool SwapCategory();
    static bool GetCategory();
    static void SetCurrentLanguage(int lcid);
    static void OnRaltUp();
private:
    static Unmanaged::onLanguageChangeCallback GetCallbackPtr(OnLanguageChangeCallbackDelegate^ delegate);
};

public ref class WindowTracker {
public:
    delegate void WindowChangedCallbackDelegate(System::IntPtr hwnd);

    static bool Start(
        WindowChangedCallbackDelegate^ _onNewFloatWindowHandler,
        WindowChangedCallbackDelegate^ _onMaxWindowHandler,
        WindowChangedCallbackDelegate^ _onUnmaxWindowHandler,
        WindowChangedCallbackDelegate^ _onMinWindowHandler,
        WindowChangedCallbackDelegate^ _onCloseWindowHandler,
        WindowChangedCallbackDelegate^ _onWindowTitleChangeHandler);
    static void Stop();
private:
    static Unmanaged::onWindowChangeCallback GetCallbackPtr(WindowChangedCallbackDelegate^ delegate);
};


public ref class Utils {
public:
    static void SetBorderlessWindow(System::IntPtr hwnd);
    static System::String^ GetWindowTitle(System::IntPtr hwnd);
    static void Unfocus();
};
}
}

