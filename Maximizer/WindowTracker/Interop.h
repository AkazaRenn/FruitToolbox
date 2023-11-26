#pragma once

#include "pch.h"

namespace FruitToolbox {
    namespace Maximizer {
        public ref class Interop {
        public:
            delegate void windowChangedCallbackDelegate(HWND hwnd);

            static bool start(windowChangedCallbackDelegate^ _onNewFloatWindowHandler,
                windowChangedCallbackDelegate^ _onMaxWindowHandler,
                windowChangedCallbackDelegate^ _onUnmaxWindowHandler,
                windowChangedCallbackDelegate^ _onMinWindowHandler,
                windowChangedCallbackDelegate^ _onCloseWindowHandler);
            static void stop();
        };
    }
}

