#include "pch.h"

#include "WindowTracker.h"

using namespace std;
using namespace FruitToolbox::Interop::Unmanaged;

void CALLBACK WindowTracker::onForeground(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (validSource(idObject, idChild) &&
        !maxWindows.contains(hwnd) &&
        !IsZoomed(hwnd) &&
        isWindow(hwnd)) {
        thread(newFloatWindowHandler, hwnd).detach();
    } else if (GetWindowLong(hwnd, GWL_EXSTYLE) == 0x8200088L) {
        thread(taskViewHandler, hwnd).detach();
    }
}

void CALLBACK WindowTracker::onObjMove(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (validSource(idObject, idChild)) {
        if (maxWindows.contains(hwnd)) {
            if (
                !IsZoomed(hwnd) &&
                !IsIconic(hwnd) &&
                isWindow(hwnd)) {
                maxWindows.erase(hwnd);
                thread(unmaxWindowHandler, hwnd).detach();
            }
        } else if (
            IsZoomed(hwnd) &&
            isWindow(hwnd)) {
            maxWindows.insert(hwnd);
            thread(maxWindowHandler, hwnd).detach();
        }
    }
}

void CALLBACK WindowTracker::onMinWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (maxWindows.contains(hwnd)) {
        thread(minWindowHandler, hwnd).detach();
    }
}

void CALLBACK WindowTracker::onObjDestroy(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (validSource(idObject, idChild) &&
        maxWindows.contains(hwnd)) {
        Sleep(50);
        if (!IsWindow(hwnd)) {
            maxWindows.erase(hwnd);
            thread(closeWindowHandler, hwnd).detach();
        }
    }
}

void CALLBACK WindowTracker::obObjNameChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (validSource(idObject, idChild) &&
        maxWindows.contains(hwnd) &&
        isWindow(hwnd)) {
        thread(windowTitleChangeHandler, hwnd).detach();
    }
}

bool WindowTracker::EnumWindowsProc(HWND hwnd, LPARAM lParam) {
    if (hwnd != GetShellWindow() &&
        hwnd != GetDesktopWindow() &&
        isWindow(hwnd)) {
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
onWindowChangeCallback WindowTracker::taskViewHandler = nullptr;
onWindowChangeCallback WindowTracker::maxWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::unmaxWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::minWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::closeWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::windowTitleChangeHandler = nullptr;

vector<HWINEVENTHOOK> WindowTracker::hooks = {};
set<HWND> WindowTracker::maxWindows = {};
void WindowTracker::resetFields() {
    newFloatWindowHandler = nullptr;
    taskViewHandler = nullptr;
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
    onWindowChangeCallback _taskViewHandler,
    onWindowChangeCallback _maxWindowHandler,
    onWindowChangeCallback _unmaxWindowHandler,
    onWindowChangeCallback _minWindowHandler,
    onWindowChangeCallback _closeWindowHandler,
    onWindowChangeCallback _onWindowTitleChangeHandler) {
    // Initialized before, don't do it again
    if (!hooks.empty()) return false;

    if (((newFloatWindowHandler = _newFloatWindowHandler) == nullptr) ||
        ((taskViewHandler = _taskViewHandler) == nullptr) ||
        ((maxWindowHandler = _maxWindowHandler) == nullptr) ||
        ((unmaxWindowHandler = _unmaxWindowHandler) == nullptr) ||
        ((minWindowHandler = _minWindowHandler) == nullptr) ||
        ((closeWindowHandler = _closeWindowHandler) == nullptr) ||
        ((windowTitleChangeHandler = _onWindowTitleChangeHandler) == nullptr)) {
        stop();
        return false;
    }

    sortCurrentWindows();

    hooks.push_back(SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, NULL, onForeground, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, NULL, onObjMove, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZESTART, NULL, onMinWindow, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_DESTROY, NULL, onObjDestroy, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, NULL, obObjNameChange, 0, 0, WINEVENT_OUTOFCONTEXT));

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
