using System;
using System.Reflection;

using WindowsDesktop;

namespace FruitToolbox.Maximizer;

internal static class VDHelper
{
    const int DefaultRetry = 10;
    const int DefaultWait = 50;

    public static VirtualDesktop Current
    {
        get
        {
            return Try(() => VirtualDesktop.Current);
        }
    }

    public static VirtualDesktop Right
    {
        get
        {
            return Try(() => Current.GetRight());
        }
    }

    public static VirtualDesktop[] Desktops
    {
        get
        {
            return Try(() => VirtualDesktop.GetDesktops());
        }
    }

    public static VirtualDesktop Create()
    {
        return Try(() => VirtualDesktop.Create());
    }

    public static void Remove(Guid desktopId)
    {
        Remove(FromId(desktopId));
    }

    public static void Remove(VirtualDesktop desktop)
    {
        Try(() => desktop.Remove());
    }

    public static VirtualDesktop GetLeft(VirtualDesktop desktop)
    {
        return Try(() => desktop.GetLeft());
    }

    public static VirtualDesktop GetRight(VirtualDesktop desktop)
    {
        return Try(() => desktop.GetRight());
    }

    public static VirtualDesktop FromId(Guid id)
    {
        return Try(() => VirtualDesktop.FromId(id));
    }

    public static void PinWindow(nint window)
    {
        Try(() => VirtualDesktop.PinWindow(window));
    }

    public static void UnpinWindow(nint window)
    {
        Try(() => VirtualDesktop.UnpinWindow(window));
    }

    public static void MoveToDesktop(nint window, VirtualDesktop desktop)
    {
        Try(() => VirtualDesktop.MoveToDesktop(window, desktop));
    }

    public static void Switch(Guid desktopId)
    {
        Switch(FromId(desktopId));
    }

    public static void Switch(VirtualDesktop desktop)
    {
        Try(() => desktop.Switch());
    }

    public static void Move(Guid desktopId, int index)
    {
        Move(FromId(desktopId), index);
    }

    public static void Move(VirtualDesktop desktop, int index)
    {
        Try(() => desktop.Move(index));
    }

    public static T Try<T>(Func<T> function, int maxRetry = DefaultRetry, int wait = DefaultWait)
    {
        int retry = 0;
        do
        {
            try
            {
                return function();
            } catch
            {
                Thread.Sleep(wait);
            }
        } while(retry++ < maxRetry);

        return default;
    }

    public static void Try(Action function, int maxRetry = DefaultRetry, int wait = DefaultWait)
    {
        int retry = 0;
        do
        {
            try
            {
                function();
                return;
            } catch
            {
                Thread.Sleep(wait);
            }
        } while(retry++ < maxRetry);
    }
}
