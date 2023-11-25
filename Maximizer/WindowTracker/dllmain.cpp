// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "WindowTracker.h"


using namespace FruitToolbox;

#define DLLEXPORT __declspec(dllexport)

extern "C" {
    DLLEXPORT bool WindowTracker_start(onWindowChangeCallback _onNewFloatWindowHandler,
        onWindowChangeCallback _onMaxWindowHandler,
        onWindowChangeCallback _onUnmaxWindowHandler,
        onWindowChangeCallback _onMinWindowHandler,
        onWindowChangeCallback _onCloseWindowHandler) {
        return WindowTracker::start(_onNewFloatWindowHandler,
            _onMaxWindowHandler,
            _onUnmaxWindowHandler,
            _onMinWindowHandler,
            _onCloseWindowHandler);
    }

    DLLEXPORT void WindowTracker_stop() {
        WindowTracker::stop();
    }
}