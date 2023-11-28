#pragma once

#include "pch.h"
#include "Modules.h"
#include "LanguageSwitcher.h"
#include "WindowTracker.h"

using namespace System::Runtime::InteropServices;
using namespace FruitToolbox::Interop;

bool LanguageSwitcher::Start(OnLanguageChangeCallbackDelegate^ handler) {
    static Unmanaged::onLanguageChangeCallback delegatePtr = nullptr;
    delegatePtr = (Unmanaged::onLanguageChangeCallback)(Marshal::GetFunctionPointerForDelegate(handler).ToPointer());
    return Unmanaged::LanguageSwitcher::start(delegatePtr);
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


bool WindowTracker::Start(
    WindowChangedCallbackDelegate^ _onNewFloatWindowHandler,
    WindowChangedCallbackDelegate^ _onMaxWindowHandler,
    WindowChangedCallbackDelegate^ _onUnmaxWindowHandler,
    WindowChangedCallbackDelegate^ _onMinWindowHandler,
    WindowChangedCallbackDelegate^ _onCloseWindowHandler) {
    static Unmanaged::onWindowChangeCallback onNewFloatWindowHandlerPtr = nullptr;
    onNewFloatWindowHandlerPtr = (Unmanaged::onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onNewFloatWindowHandler).ToPointer());

    static Unmanaged::onWindowChangeCallback onMaxWindowHandlerPtr = nullptr;
    onMaxWindowHandlerPtr = (Unmanaged::onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onMaxWindowHandler).ToPointer());

    static Unmanaged::onWindowChangeCallback onUnmaxWindowHandlerPtr = nullptr;
    onUnmaxWindowHandlerPtr = (Unmanaged::onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onUnmaxWindowHandler).ToPointer());

    static Unmanaged::onWindowChangeCallback onMinWindowHandlerPtr = nullptr;
    onMinWindowHandlerPtr = (Unmanaged::onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onMinWindowHandler).ToPointer());

    static Unmanaged::onWindowChangeCallback onCloseWindowHandlerPtr = nullptr;
    onCloseWindowHandlerPtr = (Unmanaged::onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onCloseWindowHandler).ToPointer());

    return Unmanaged::WindowTracker::start(
        onNewFloatWindowHandlerPtr,
        onMaxWindowHandlerPtr,
        onUnmaxWindowHandlerPtr,
        onMinWindowHandlerPtr,
        onCloseWindowHandlerPtr);
}

void WindowTracker::Stop() {
    Unmanaged::WindowTracker::stop();
}
