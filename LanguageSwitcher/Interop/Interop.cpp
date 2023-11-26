#pragma once

#include "pch.h"
#include "Interop.h"
#include "Core.h"

using namespace System::Runtime::InteropServices;
using namespace FruitToolbox::LanguageSwitcher;

bool Interop::start(onLanguageChangeCallbackDelegate^ handler) {
    static onLanguageChangeCallback delegatePtr = nullptr;
    delegatePtr = (onLanguageChangeCallback)(Marshal::GetFunctionPointerForDelegate(handler).ToPointer());
    return Core::start(delegatePtr);
}

void Interop::stop() {
    Core::stop();
}

void Interop::updateInputLanguage(bool doCallback) {
    Core::updateInputLanguage(doCallback);
}

bool Interop::swapCategory() {
    return Core::swapCategory();
}

bool Interop::getCategory() {
    return Core::getCategory();
}

void Interop::setCurrentLanguage(int lcid) {
    Core::setCurrentLanguage(lcid);
}

void Interop::onRaltUp() {
    Core::onRaltUp();
}
