#if (LanguageSwitcherEnabled)
    $CapsLock::
        If (GetKeyState("CapsLock", "T")) {
            SetCapsLockState Off
        } else {
            KeyWait, CapsLock, T0.5
            If (ErrorLevel) {
                SetCapsLockState On
                KeyWait, CapsLock
            } else {
                DllCall(onCapsLockPtr)
            }
        }
    Return
#if
