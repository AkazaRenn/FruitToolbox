#pragma once

#include "pch.h"
#include "Timer.h"

namespace FruitLanguageSwitcher {
    void Timer::stop() {
        active = false;
    }
}