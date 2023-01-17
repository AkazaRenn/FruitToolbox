$CapsLock::
    If (GetKeyState("CapsLock", "T")) {
        SetCapsLockState Off
    } else {
        KeyWait, CapsLock, U T0.5
        If (ErrorLevel) {
            SetCapsLockState On
            KeyWait, CapsLock, U
        } else {
            DllCall(ptr, "Int", onCapsLock)
        }
    }
Return
