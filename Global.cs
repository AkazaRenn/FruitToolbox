using System;
using System.Runtime.InteropServices;

namespace FruitLanguageSwitcher
{
    internal class Constants
    {
        public const string AppID = "AkazaRenn.82975CBC0BB1_fhf2jh1qk9hx4!App";
    }

    internal static partial class Interop
    {
        #region disable round corner

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
        #endregion
    }
}
