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
        atomic<bool> active = false;
        unsigned long delay = 0;
        unsigned long interval = 0;

    public:
        template <class Fn> void setTimeout(unsigned long delay, Fn&& fn);
        template <class Fn> void setInterval(unsigned long interval, Fn&& fn);
        template <class Fn> void stop(Fn&& fn); // run function if it's stopped before the timeout
        void stop();

    };

    template <class Fn> void Timer::setTimeout(unsigned long delay, Fn&& fn) {
        if(active) {
            return;
        }
        active = true;
        this->delay = delay;
        thread t([&] () {
            if(!active) return;
            this_thread::sleep_for(milliseconds(this->delay));
            if(!active) return;
            active = false;
            fn();
                 });
        t.detach();
    }

    template <class Fn> void Timer::setInterval(unsigned long interval, Fn&& fn) {
        if(active) {
            return;
        }
        active = true;
        this->interval = interval;
        thread t([&] () {
            while(active) {
                this_thread::sleep_for(milliseconds(this->interval));
                if(!active) return;
                fn();
            }
                 });
        t.detach();
    }

    template <class Fn> void Timer::stop(Fn&& fn) {
        if(active) {
            active = false;
            thread t(fn);
            t.detach();
        }
    }
}
