#include "pch.h"

#include "WindowTracker.h"
#include <bitset>

using namespace std;
using namespace FruitToolbox::Interop::Unmanaged;

void CALLBACK WindowTracker::onForeground(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (validSource(idObject, idChild) &&
        !maxWindows.contains(hwnd) &&
        !IsZoomed(hwnd) &&
        isWindow(hwnd) &&
        !isSnappedWindow(hwnd)) {
        thread(newFloatWindowHandler, hwnd).detach();
    } else if (GetWindowLong(hwnd, GWL_EXSTYLE) == 0x8200088L) {
        thread(taskViewHandler, hwnd).detach();
    }
}

void CALLBACK WindowTracker::onObjMove(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (validSource(idObject, idChild)) {
        if (maxWindows.contains(hwnd)) {
            if (!IsZoomed(hwnd) &&
                !IsIconic(hwnd)) {
                maxWindows.erase(hwnd);
                if (isSnappedWindow(hwnd)) {
                    snappedWindows.insert(hwnd);
                    // max to snapped, rename desktop
                } else {
                    thread(unmaxWindowHandler, hwnd).detach();
                }
            }
        } else if (
            IsZoomed(hwnd) &&
            isWindow(hwnd)) {
            maxWindows.insert(hwnd);
            snappedWindows.erase(hwnd);
            thread(maxWindowHandler, hwnd).detach();
        } else if (isSnappedWindow(hwnd) && !snappedWindows.contains(hwnd)) {
            // new snapped window
            // if in auto desktop, do nothing
            // if in home, make a new one
            snappedWindows.insert(hwnd);
        } else if (!isSnappedWindow(hwnd) && snappedWindows.contains(hwnd)) {
            // snapped window to another state that is not max
            // if still windows in this desktop, do nothing
            // else close desktop
            snappedWindows.erase(hwnd);
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
            snappedWindows.erase(hwnd);
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
        } else if (isSnappedWindow(hwnd)) {
            snappedWindows.insert(hwnd);
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
set<HWND> WindowTracker::snappedWindows = {};
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
    snappedWindows = {};
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

// From https://chromium.googlesource.com/chromium/src/+/master/ui/views/win/hwnd_message_handler.cc#286
bool WindowTracker::isSnappedWindow(HWND hwnd) {
    // IsWindowArranged() is not a part of any header file.
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-iswindowarranged
    static const HMODULE user32 = LoadLibraryA("User32.dll");
    using IsWindowArrangedFuncType = BOOL(WINAPI*)(HWND);
    static const auto IsWindowArranged =
        reinterpret_cast<IsWindowArrangedFuncType>(
            GetProcAddress(user32, "IsWindowArranged"));
    return IsWindowArranged ? IsWindowArranged(hwnd) : false;
}
