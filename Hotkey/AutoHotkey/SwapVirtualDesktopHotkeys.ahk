#if (SwapVirtualDesktopHotkeysEnabled)
    $#Left::
        Send #^{Left}
    Return

    $#Right::
        Send #^{Right}
    Return

    $#Up::
        DllCall(onGuiUpPtr)
    Return

    $#Down::
        Send #d
    Return


    $#^Left::
        Send #{Left}
    Return

    $#^Right::
        Send #{Right}
    Return

    $#^Up::
        Send #{Up}
    Return

    $#^Down::
        Send #{Down}
    Return
#if
