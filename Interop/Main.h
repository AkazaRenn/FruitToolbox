#pragma once

#include "pch.h"
#include "LanguageSwitcher.h"
#include "WindowTracker.h"

namespace FruitToolbox {
namespace Interop {
public ref class LanguageSwitcher {
public:
    delegate void OnLanguageChangeCallbackDelegate(int lcid, bool imeMode);

    static bool Start(
        OnLanguageChangeCallbackDelegate^ _categorySwapHandler,
        OnLanguageChangeCallbackDelegate^ _newLanguageHandler);
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
        WindowChangedCallbackDelegate^ _newFloatWindowHandler,
        WindowChangedCallbackDelegate^ _maxWindowHandler,
        WindowChangedCallbackDelegate^ _unmaxWindowHandler,
        WindowChangedCallbackDelegate^ _minWindowHandler,
        WindowChangedCallbackDelegate^ _closeWindowHandler,
        WindowChangedCallbackDelegate^ _windowTitleChangeHandler);
    static void Stop();
private:
    static Unmanaged::onWindowChangeCallback GetCallbackPtr(WindowChangedCallbackDelegate^ delegate);
};


public ref class Utils {
public:
    static void SetBorderlessWindow(System::IntPtr hwnd);
    static System::String^ GetWindowTitle(System::IntPtr hwnd);
    static void Unfocus();
    static bool InFullScreen();
};
}
}

