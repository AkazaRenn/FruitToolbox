#pragma once

#include "pch.h"
#include "Modules.h"
#include "LanguageSwitcher.h"
#include "WindowTracker.h"

using namespace System::Runtime::InteropServices;
using namespace FruitToolbox::Interop;

bool LanguageSwitcher::Start(OnLanguageChangeCallbackDelegate^ handler) {
    static Unmanaged::onLanguageChangeCallback delegatePtr = nullptr;
    return Unmanaged::LanguageSwitcher::start(
        delegatePtr = GetCallbackPtr(handler)
    );
}

void LanguageSwitcher::Stop() {
    Unmanaged::LanguageSwitcher::stop();
}

void LanguageSwitcher::UpdateInputLanguage(bool doCallback) {
    Unmanaged::LanguageSwitcher::updateInputLanguage(doCallback);
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


bool WindowTracker::Start(
    WindowChangedCallbackDelegate^ _onNewFloatWindowHandler,
    WindowChangedCallbackDelegate^ _onMaxWindowHandler,
    WindowChangedCallbackDelegate^ _onUnmaxWindowHandler,
    WindowChangedCallbackDelegate^ _onMinWindowHandler,
    WindowChangedCallbackDelegate^ _onCloseWindowHandler,
    WindowChangedCallbackDelegate^ _onWindowTitleChangeHandler) {
    static Unmanaged::onWindowChangeCallback onNewFloatWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback onMaxWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback onUnmaxWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback onMinWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback onCloseWindowHandlerPtr = nullptr;
    static Unmanaged::onWindowChangeCallback onWindowTitleChangeHandlerPtr = nullptr;

    return Unmanaged::WindowTracker::start(
        onNewFloatWindowHandlerPtr = GetCallbackPtr(_onNewFloatWindowHandler),
        onMaxWindowHandlerPtr = GetCallbackPtr(_onMaxWindowHandler),
        onUnmaxWindowHandlerPtr = GetCallbackPtr(_onUnmaxWindowHandler),
        onMinWindowHandlerPtr = GetCallbackPtr(_onMinWindowHandler),
        onCloseWindowHandlerPtr = GetCallbackPtr(_onCloseWindowHandler),
        onWindowTitleChangeHandlerPtr = GetCallbackPtr(_onWindowTitleChangeHandler)
    );
}

void WindowTracker::Stop() {
    Unmanaged::WindowTracker::stop();
}

Unmanaged::onWindowChangeCallback WindowTracker::GetCallbackPtr(WindowChangedCallbackDelegate^ delegate) {
    return (Unmanaged::onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(delegate).ToPointer());
}
