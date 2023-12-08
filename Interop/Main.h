#pragma once

#include "pch.h"
#include "LanguageSwitcher.h"
#include "WindowTracker.h"

using namespace System;

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
    delegate void WindowChangedCallbackDelegate(IntPtr hwnd);

    static bool Start(
        WindowChangedCallbackDelegate^ _newFloatWindowHandler,
        WindowChangedCallbackDelegate^ _taskViewHandler,
        WindowChangedCallbackDelegate^ _maxWindowHandler,
        WindowChangedCallbackDelegate^ _unmaxWindowHandler,
        WindowChangedCallbackDelegate^ _minWindowHandler,
        WindowChangedCallbackDelegate^ _closeWindowHandler,
        WindowChangedCallbackDelegate^ _windowTitleChangeHandler);
    static void Stop();
private:
    static Unmanaged::onWindowChangeCallback GetCallbackPtr(WindowChangedCallbackDelegate^ delegate);
};


public value struct MonitorInfo {
    int X;
    int Y;
    int Width;
    int Height;
    int TaskbarHeight;
    double Scaling;
};

public ref class Utils {
public:
    static void SetBorderlessWindow(IntPtr hwnd);
    static void UnminimizeInBackground(IntPtr hwnd);
    static String^ GetWindowTitle(IntPtr hwnd);
    static void Unfocus();
    static bool InFullScreen();
    static MonitorInfo GetActiveMonitorInfo();
};
}
}

