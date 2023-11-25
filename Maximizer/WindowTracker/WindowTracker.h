#pragma once
#include <vector>
#include <set>

using namespace std;

namespace FruitToolbox {
    typedef void(__stdcall* onWindowChangeCallback)(HWND);

    class WindowTracker
    {
    private:
        static onWindowChangeCallback newFloatWindowHandler;
        static onWindowChangeCallback maxWindowHandler;
        static onWindowChangeCallback unmaxWindowHandler;
        static onWindowChangeCallback minWindowHandler;
        static onWindowChangeCallback closeWindowHandler;

        static HWND shellWindow;
        static vector<HWINEVENTHOOK> hooks;
        static set<HWND> maxWindows;

        static void resetFields();
        static void sortCurrentWindows();
 
        static bool CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam);

        static void CALLBACK onNewFloatWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        static void CALLBACK onMaxUnmaxWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        static void CALLBACK onMinWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
        static void CALLBACK onCloseWindow(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
    public:
        static bool start(onWindowChangeCallback _onNewFloatWindowHandler,
                          onWindowChangeCallback _onMaxWindowHandler,
                          onWindowChangeCallback _onUnmaxWindowHandler,
                          onWindowChangeCallback _onMinWindowHandler,
                          onWindowChangeCallback _onCloseWindowHandler);
        static void stop();
    };
}
