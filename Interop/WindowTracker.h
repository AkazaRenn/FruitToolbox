#pragma once
#include <vector>
#include <set>

using namespace std;

namespace FruitToolbox {
namespace Interop {
namespace Unmanaged {
typedef void(__stdcall* onWindowChangeCallback)(HWND);

class WindowTracker
{
private:
    static onWindowChangeCallback newFloatWindowHandler;
    static onWindowChangeCallback taskViewHandler;
    static onWindowChangeCallback maxWindowHandler;
    static onWindowChangeCallback unmaxWindowHandler;
    static onWindowChangeCallback minWindowHandler;
    static onWindowChangeCallback closeWindowHandler;
    static onWindowChangeCallback windowTitleChangeHandler;

    static vector<HWINEVENTHOOK> hooks;
    static set<HWND> maxWindows;
    static set<HWND> snappedWindows;

    static inline bool validSource(LONG idObject, LONG idChild) {
        return
            (idObject == OBJID_WINDOW) &&
            (idChild == CHILDID_SELF);
    }

    static inline bool isWindow(HWND hwnd) {
        LONG exStyle;

        return
            (IsWindow(hwnd)) &&
            (IsWindowVisible(hwnd)) &&
            ((exStyle = GetWindowLong(hwnd, GWL_EXSTYLE)) & WS_EX_OVERLAPPEDWINDOW) &&
            !(exStyle & WS_EX_MDICHILD) &&
            (GetWindowTextLengthW(hwnd) > 0);
    }

    static void resetFields();
    static void sortCurrentWindows();
    static bool isSnappedWindow(HWND hwnd);

    static bool CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam);

    static void CALLBACK onForeground(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
    static void CALLBACK onObjMove(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
    static void CALLBACK onMinWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
    static void CALLBACK onObjDestroy(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
    static void CALLBACK obObjNameChange(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
public:
    static bool start(
        onWindowChangeCallback _newFloatWindowHandler,
        onWindowChangeCallback _taskViewHandler,
        onWindowChangeCallback _maxWindowHandler,
        onWindowChangeCallback _unmaxWindowHandler,
        onWindowChangeCallback _minWindowHandler,
        onWindowChangeCallback _closeWindowHandler,
        onWindowChangeCallback _windowTitleChangeHandler);
    static void stop();
};
}
}
}
