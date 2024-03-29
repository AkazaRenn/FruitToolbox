﻿namespace FruitToolbox.Utils;

using HWND = nint;

internal class Constants
{
    public const string AppID = "AkazaRenn.82975CBC0BB1_fhf2jh1qk9hx4!App";

    public delegate void LanguageEventHandler(object sender, LanguageEvent e);
    public class LanguageEvent(int lcid): EventArgs
    {
        public int LCID { get; } = lcid;
    }

    public delegate void WindowEventHandler(object sender, HWND e);
    public class WindowEvent(HWND hWnd): EventArgs
    {
        public HWND HWnd { get; } = hWnd;
    }
}
