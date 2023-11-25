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
#if
