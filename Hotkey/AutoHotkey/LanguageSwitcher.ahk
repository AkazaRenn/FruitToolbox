#if (LanguageSwitcherEnabled)
    $CapsLock::
        If (GetKeyState("CapsLock", "T")) {
            SetCapsLockState Off
            DllCall(onCapsLockOffPtr)
        } else {
            KeyWait, CapsLock, T0.5
            If (ErrorLevel) {
                SetCapsLockState On
                DllCall(onCapsLockOnPtr)
                KeyWait, CapsLock
            } else {
                DllCall(onCapsLockLanguageSwitchPtr)
            }
        }
    Return

    $~#^Space::
    $~#+Space::
    $~#Space::
        KeyWait, LWin, U
        DllCall(onLanguageChangePtr)
    Return

    #if (RAltModifierEnabled)
        $RAlt Up::
            DllCall(onRAltUpPtr)
        Return
    #if
#if