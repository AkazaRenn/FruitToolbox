#pragma once

#include "pch.h"
#include "Main.h"
#include "LanguageSwitcher.h"
#include "WindowTracker.h"

using namespace System::Runtime::InteropServices;
using namespace FruitToolbox::Interop;


// class LanguageSwitcher
bool LanguageSwitcher::Start(
        OnLanguageChangeCallbackDelegate^ _categorySwapHandler,
        OnLanguageChangeCallbackDelegate^ _newLanguageHandler) {
    static Unmanaged::onLanguageChangeCallback categorySwapHandler = nullptr;
    static Unmanaged::onLanguageChangeCallback newLanguageHandler = nullptr;

    return Unmanaged::LanguageSwitcher::start(
        categorySwapHandler = GetCallbackPtr(_categorySwapHandler),
        newLanguageHandler = GetCallbackPtr(_newLanguageHandler)
    );
}

void LanguageSwitcher::Stop() {
    Unmanaged::LanguageSwitcher::stop();
}

void LanguageSwitcher::UpdateInputLanguage() {
    Unmanaged::LanguageSwitcher::updateInputLanguage();
}

bool LanguageSwitcher::SwapCategory() {
    return Unmanaged::LanguageSwitcher::swapCategory();
}

bool LanguageSwitcher::GetCategory() {
    return Unmanaged::LanguageSwitcher::getCategory();
}

void LanguageSwitcher::SetCurrentLanguage(int lcid) {
    Unmanaged::LanguageSwitcher::setCurrentLanguage(lcid);
}

void LanguageSwitcher::OnRaltUp() {
    Unmanaged::LanguageSwitcher::onRaltUp();
}

Unmanaged::onLanguageChangeCallback LanguageSwitcher::GetCallbackPtr(OnLanguageChangeCallbackDelegate^ delegate) {
    return (Unmanaged::onLanguageChangeCallback)(Marshal::GetFunctionPointerForDelegate(delegate).ToPointer());
}


// class WindowTracker
bool WindowTracker::Start(
    WindowChangedCallbackDelegate^ _newFloatWindowHandler,
    WindowChangedCallbackDelegate^ _maxWindowHandler,
    WindowChangedCallbackDelegate^ _unmaxWindowHandler,
    WindowChangedCallbackDelegate^ _minWindowHandler,
    WindowChangedCallbackDelegate^ _closeWindowHandler,
    WindowChangedCallbackDelegate^ _windowTitleChangeHandler) {
    static Unmanaged::onWindowChangeCallback newFloatWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback maxWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback unmaxWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback minWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback closeWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback windowTitleChangeHandlerPtr = nullptr;

    return Unmanaged::WindowTracker::start(
        newFloatWindowHandlerPtr = GetCallbackPtr(_newFloatWindowHandler),
        maxWindowHandlerPtr = GetCallbackPtr(_maxWindowHandler),
        unmaxWindowHandlerPtr = GetCallbackPtr(_unmaxWindowHandler),
        minWindowHandlerPtr = GetCallbackPtr(_minWindowHandler),
        closeWindowHandlerPtr = GetCallbackPtr(_closeWindowHandler),
        windowTitleChangeHandlerPtr = GetCallbackPtr(_windowTitleChangeHandler)
    );
}

void WindowTracker::Stop() {
    Unmanaged::WindowTracker::stop();
}

Unmanaged::onWindowChangeCallback WindowTracker::GetCallbackPtr(WindowChangedCallbackDelegate^ delegate) {
    return (Unmanaged::onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(delegate).ToPointer());
}


// class Utils
void Utils::SetBorderlessWindow(IntPtr hwnd) {
    int cornerPreference = DWMWCP_DONOTROUND;
    DwmSetWindowAttribute(
        static_cast<HWND>(hwnd.ToPointer()),
        DWMWA_WINDOW_CORNER_PREFERENCE,
        &cornerPreference,
        sizeof(cornerPreference));
}

void Utils::UnminimizeInBackground(IntPtr hwnd) {
    ShowWindow(static_cast<HWND>(hwnd.ToPointer()), SW_SHOWNOACTIVATE);
}   

String^ Utils::GetWindowTitle(IntPtr hwnd) {
    wchar_t title[MAX_PATH];
    GetWindowTextW(static_cast<HWND>(hwnd.ToPointer()), title, ARRAYSIZE(title));
    return Marshal::PtrToStringUni(static_cast<IntPtr>(title));
}

void Utils::Unfocus() {
    SetForegroundWindow(GetShellWindow());
}

bool Utils::InFullScreen() {
    QUERY_USER_NOTIFICATION_STATE state;
    SHQueryUserNotificationState(&state);
    return
        (state == QUNS_BUSY) ||
        (state == QUNS_RUNNING_D3D_FULL_SCREEN) ||
        (state == QUNS_PRESENTATION_MODE);
}
