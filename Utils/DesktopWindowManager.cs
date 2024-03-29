﻿using System.Runtime.InteropServices;

namespace FruitToolbox.Utils;

// From https://github.com/microsoft/PowerToys/pull/16542/files
internal static partial class DesktopWindowManager
{
    const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    const int DWMWCP_DONOTROUND = 1;

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    public static void DisableRoundCorners(IntPtr window)
    {
        // Set window corner preference on Windows 11 to "Do not round"
        int cornerPreference = DWMWCP_DONOTROUND;
        DwmSetWindowAttribute(window, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
    }
}
