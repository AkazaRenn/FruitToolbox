#pragma once
#pragma unmanaged

// from https://github.com/99x/timercpp/blob/master/timercpp.h
#include <iostream>
#include <atomic>
#include <functional>
#include <chrono>

namespace FruitLanguageSwitcher {
    using namespace std;
    using namespace chrono;

    class Timer {
        atomic<bool> active { false };

    public:
        template <class Fn> void setTimeout(int delay, Fn&& fn);
        template <class Fn> void setInterval(int interval, Fn&& fn);
        template <class Fn> void stop(Fn&& fn); // run function if it's stopped before the timeout
        void stop();

    };

    template <class Fn> void Timer::setTimeout(int delay, Fn&& fn) {
        if(active.load()) {
            return;
        }
        active = true;
        thread t([&] () {
            if(!active.load()) return;
            this_thread::sleep_for(milliseconds(delay));
            if(!active.load()) return;
            active = false;
            fn();
                 });
        t.detach();
    }

    template <class Fn> void Timer::setInterval(int interval, Fn&& fn) {
        if(active.load()) {
            return;
        }
        active = true;
        thread t([&] () {
            while(active.load()) {
                this_thread::sleep_for(milliseconds(interval));
                if(!active.load()) return;
                fn();
            }
                 });
        t.detach();
    }

    template <class Fn> void Timer::stop(Fn&& fn) {
        if(active.load()) {
            active = false;
            thread t(fn);
            t.detach();
        }
    }
}
