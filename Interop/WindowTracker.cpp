#include "pch.h"

#include "WindowTracker.h"

using namespace std;
using namespace FruitToolbox::Interop::Unmanaged;

void CALLBACK WindowTracker::onNewFloatWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (isWindow(hwnd, idObject, idChild) &&
        !IsZoomed(hwnd) &&
        !maxWindows.contains(hwnd)) {
        thread(newFloatWindowHandler, hwnd).detach();
    }
}

void CALLBACK WindowTracker::onMaxUnmaxWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (isWindow(hwnd, idObject, idChild)) {
        if (!maxWindows.contains(hwnd) &&
            IsZoomed(hwnd)) {
            maxWindows.insert(hwnd);
            thread(maxWindowHandler, hwnd).detach();
        } else if (
            maxWindows.contains(hwnd) &&
            !IsZoomed(hwnd) &&
            !IsIconic(hwnd)) {
            maxWindows.erase(hwnd);
            thread(unmaxWindowHandler, hwnd).detach();
        }
    }
}

void CALLBACK WindowTracker::onMinWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (maxWindows.contains(hwnd)) {
        thread(minWindowHandler, hwnd).detach();
    }
}

void CALLBACK WindowTracker::onCloseWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (idObject == OBJID_WINDOW &&
        idChild == CHILDID_SELF &&
        maxWindows.contains(hwnd)) {
        Sleep(100);
        if (!IsWindow(hwnd)) {
            maxWindows.erase(hwnd);
            thread(closeWindowHandler, hwnd).detach();
        }
    }
}

void CALLBACK WindowTracker::onWindowTitleChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (isWindow(hwnd, OBJID_WINDOW, CHILDID_SELF) &&
        maxWindows.contains(hwnd)) {
        thread(windowTitleChangeHandler, hwnd).detach();
    }
}

bool WindowTracker::EnumWindowsProc(HWND hwnd, LPARAM lParam) {
    if (hwnd != GetShellWindow() &&
        hwnd != GetDesktopWindow() &&
        isWindow(hwnd, OBJID_WINDOW, CHILDID_SELF)) {
        if (IsZoomed(hwnd)) {
            maxWindows.insert(hwnd);
            maxWindowHandler(hwnd);
        } else {
            newFloatWindowHandler(hwnd);
        }
        Sleep(200);
    }
    return true;
}

// Need to match resetFields()
onWindowChangeCallback WindowTracker::newFloatWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::maxWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::unmaxWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::minWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::closeWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::windowTitleChangeHandler = nullptr;

vector<HWINEVENTHOOK> WindowTracker::hooks = {};
set<HWND> WindowTracker::maxWindows = {};
void WindowTracker::resetFields() {
    newFloatWindowHandler = nullptr;
    maxWindowHandler = nullptr;
    unmaxWindowHandler = nullptr;
    minWindowHandler = nullptr;
    closeWindowHandler = nullptr;
    windowTitleChangeHandler = nullptr;

    hooks = {};
    maxWindows = {};
}

void WindowTracker::sortCurrentWindows() {
    EnumWindows((WNDENUMPROC)EnumWindowsProc, 0);
}

bool WindowTracker::start(
    onWindowChangeCallback _newFloatWindowHandler,
    onWindowChangeCallback _maxWindowHandler,
    onWindowChangeCallback _unmaxWindowHandler,
    onWindowChangeCallback _minWindowHandler,
    onWindowChangeCallback _closeWindowHandler,
    onWindowChangeCallback _onWindowTitleChangeHandler) {
    // Initialized before, don't do it again
    if (!hooks.empty()) return false;

    if (((newFloatWindowHandler = _newFloatWindowHandler) == nullptr) ||
        ((maxWindowHandler = _maxWindowHandler) == nullptr) ||
        ((unmaxWindowHandler = _unmaxWindowHandler) == nullptr) ||
        ((minWindowHandler = _minWindowHandler) == nullptr) ||
        ((closeWindowHandler = _closeWindowHandler) == nullptr) ||
        ((windowTitleChangeHandler = _onWindowTitleChangeHandler) == nullptr)) {
        stop();
        return false;
    }

    sortCurrentWindows();

    hooks.push_back(SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, NULL, onNewFloatWindow, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, NULL, onMaxUnmaxWindow, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZESTART, NULL, onMinWindow, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_DESTROY, NULL, onCloseWindow, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, NULL, onWindowTitleChange, 0, 0, WINEVENT_OUTOFCONTEXT));

    for (const auto hook : hooks) {
        if (hook == nullptr) {
            stop();
            return false;
        }
    }

    return true;
}

void WindowTracker::stop() {
    for (const auto hook : hooks) {
        UnhookWinEvent(hook);
    }

    resetFields();
}
