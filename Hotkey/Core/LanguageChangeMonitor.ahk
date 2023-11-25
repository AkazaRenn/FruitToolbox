#if (LanguageSwitcherEnabled)
    $~#^Space::
    $~#+Space::
    $~#Space::
        KeyWait, LWin, U
        DllCall(onLanguageChangePtr)
    Return
#if