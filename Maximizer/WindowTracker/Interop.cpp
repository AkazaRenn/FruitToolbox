#pragma once

#include "pch.h"
#include "Interop.h"
#include "WindowTracker.h"

using namespace System::Runtime::InteropServices;
using namespace FruitToolbox::Maximizer;

bool Interop::start(
    windowChangedCallbackDelegate^ _onNewFloatWindowHandler,
    windowChangedCallbackDelegate^ _onMaxWindowHandler,
    windowChangedCallbackDelegate^ _onUnmaxWindowHandler,
    windowChangedCallbackDelegate^ _onMinWindowHandler,
    windowChangedCallbackDelegate^ _onCloseWindowHandler) {
    static onWindowChangeCallback onNewFloatWindowHandlerPtr = nullptr;
    onNewFloatWindowHandlerPtr = (onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onNewFloatWindowHandler).ToPointer());

    static onWindowChangeCallback onMaxWindowHandlerPtr = nullptr;
    onMaxWindowHandlerPtr = (onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onMaxWindowHandler).ToPointer());

    static onWindowChangeCallback onUnmaxWindowHandlerPtr = nullptr;
    onUnmaxWindowHandlerPtr = (onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onUnmaxWindowHandler).ToPointer());

    static onWindowChangeCallback onMinWindowHandlerPtr = nullptr;
    onMinWindowHandlerPtr = (onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onMinWindowHandler).ToPointer());

    static onWindowChangeCallback onCloseWindowHandlerPtr = nullptr;
    onCloseWindowHandlerPtr = (onWindowChangeCallback)(Marshal::GetFunctionPointerForDelegate(_onCloseWindowHandler).ToPointer());

    return WindowTracker::start(
        onNewFloatWindowHandlerPtr,
        onMaxWindowHandlerPtr,
        onUnmaxWindowHandlerPtr,
        onMinWindowHandlerPtr,
        onCloseWindowHandlerPtr);
}

void Interop::stop() {
    WindowTracker::stop();
}
