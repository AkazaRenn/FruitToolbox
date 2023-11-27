#pragma once

#include "pch.h"
#include "Main.h"
#include "LanguageSwitcher.h"
#include "WindowTracker.h"

using namespace System::Runtime::InteropServices;
using namespace FruitToolbox::Interop;

bool LanguageSwitcher::start(onLanguageChangeCallbackDelegate^ handler) {
    static Unmanaged::onLanguageChangeCallback delegatePtr = nullptr;
    delegatePtr = (Unmanaged::onLanguageChangeCallback)(Marshal::GetFunctionPointerForDelegate(handler).ToPointer());
    return Unmanaged::LanguageSwitcher::start(delegatePtr);
}

void LanguageSwitcher::stop() {
    Unmanaged::LanguageSwitcher::stop();
}

void LanguageSwitcher::updateInputLanguage(bool doCallback) {
    Unmanaged::LanguageSwitcher::updateInputLanguage(doCallback);
}

bool LanguageSwitcher::swapCategory() {
    return Unmanaged::LanguageSwitcher::swapCategory();
}

bool LanguageSwitcher::getCategory() {
    return Unmanaged::LanguageSwitcher::getCategory();
}

void LanguageSwitcher::setCurrentLanguage(int lcid) {
    Unmanaged::LanguageSwitcher::setCurrentLanguage(lcid);
}

void LanguageSwitcher::onRaltUp() {
    Unmanaged::LanguageSwitcher::onRaltUp();
}


bool WindowTracker::start(
    windowChangedCallbackDelegate^ _onNewFloatWindowHandler,
    windowChangedCallbackDelegate^ _onMaxWindowHandler,
    windowChangedCallbackDelegate^ _onUnmaxWindowHandler,
    windowChangedCallbackDelegate^ _onMinWindowHandler,
    windowChangedCallbackDelegate^ _onCloseWindowHandler) {
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

void WindowTracker::stop() {
    Unmanaged::WindowTracker::stop();
}
