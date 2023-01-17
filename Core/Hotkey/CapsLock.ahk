CapsLock::
    If (GetKeyState("CapsLock", "T")) {
        SetCapsLockState Off
    } else {
        KeyWait, CapsLock, U T0.5
        If (ErrorLevel) {
            SetCapsLockState On
            KeyWait, CapsLock, U
        } else {
           SendPipeMessage(OnCapsLock)
        }
    }
Return
