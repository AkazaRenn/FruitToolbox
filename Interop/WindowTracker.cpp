#include "pch.h"
#include "WindowTracker.h"

using namespace std;
using namespace FruitToolbox::Interop::Unmanaged;

void CALLBACK WindowTracker::onNewFloatWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (idObject == OBJID_WINDOW &&
        idChild == CHILDID_SELF &&
        IsWindow(hwnd) &&
        !IsZoomed(hwnd) &&
        IsWindowVisible(hwnd) &&
        GetWindowTextLengthW(hwnd) > 0) {
        newFloatWindowHandler(hwnd);
    }
}

void CALLBACK WindowTracker::onMaxUnmaxWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (idObject == OBJID_WINDOW && IsWindowVisible(hwnd)){
        if (maxWindows.count(hwnd) <= 0 &&
            IsZoomed(hwnd) && 
            IsWindow(hwnd) &&
            GetWindowTextLengthW(hwnd) > 0) {
            maxWindows.insert(hwnd);
            maxWindowHandler(hwnd);
        }
        else if (idObject == OBJID_WINDOW &&
            idChild == CHILDID_SELF &&
            maxWindows.count(hwnd) > 0 &&
            !IsZoomed(hwnd) &&
            !IsIconic(hwnd)) {
            maxWindows.erase(hwnd);
            unmaxWindowHandler(hwnd);
        }
    }
}

void CALLBACK WindowTracker::onMinWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (maxWindows.count(hwnd) > 0) {
        maxWindows.erase(hwnd);
        minWindowHandler(hwnd);
    }
}

void CALLBACK WindowTracker::onCloseWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime) {
    if (idObject == OBJID_WINDOW &&
        idChild == CHILDID_SELF &&
        maxWindows.count(hwnd) > 0) {
        if (!IsWindow(hwnd))
        {
            maxWindows.erase(hwnd);
            closeWindowHandler(hwnd);
        }
    }
}

// Need to match resetFields()
onWindowChangeCallback WindowTracker::newFloatWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::maxWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::unmaxWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::minWindowHandler = nullptr;
onWindowChangeCallback WindowTracker::closeWindowHandler = nullptr;

HWND WindowTracker::shellWindow = 0;
vector<HWINEVENTHOOK> WindowTracker::hooks = {};
set<HWND> WindowTracker::maxWindows = {};
void WindowTracker::resetFields() {
    newFloatWindowHandler = nullptr;
    maxWindowHandler = nullptr;
    unmaxWindowHandler = nullptr;
    minWindowHandler = nullptr;
    closeWindowHandler = nullptr;

    shellWindow = 0;
    hooks = {};
    maxWindows = {};
}

bool WindowTracker::EnumWindowsProc(HWND hwnd, LPARAM lParam) {
    if ((hwnd != shellWindow) && IsWindow(hwnd))
    {
        if (IsZoomed(hwnd))
        {
            maxWindows.insert(hwnd);
            maxWindowHandler(hwnd);
        }
        else if (!IsIconic(hwnd))
        {
            newFloatWindowHandler(hwnd);
        }
    }
    return true;
}

void WindowTracker::sortCurrentWindows() {
    shellWindow = GetShellWindow();
    EnumWindows((WNDENUMPROC)EnumWindowsProc, 0);
}

bool WindowTracker::start(
    onWindowChangeCallback _newFloatWindowHandler,
    onWindowChangeCallback _maxWindowHandler,
    onWindowChangeCallback _unmaxWindowHandler,
    onWindowChangeCallback _minWindowHandler,
    onWindowChangeCallback _closeWindowHandler) {
    if(((newFloatWindowHandler = _newFloatWindowHandler) == nullptr) ||
       ((maxWindowHandler = _maxWindowHandler) == nullptr) ||
       ((unmaxWindowHandler = _unmaxWindowHandler) == nullptr) ||
       ((minWindowHandler = _minWindowHandler) == nullptr) ||
       ((closeWindowHandler = _closeWindowHandler) == nullptr)) {
        return false;
    }

    sortCurrentWindows();

    hooks.push_back(SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, NULL, onNewFloatWindow, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, NULL, onMaxUnmaxWindow, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZESTART, NULL, onMinWindow, 0, 0, WINEVENT_OUTOFCONTEXT));
    hooks.push_back(SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_DESTROY, NULL, onCloseWindow, 0, 0, WINEVENT_OUTOFCONTEXT));

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