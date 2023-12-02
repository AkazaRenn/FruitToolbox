#if (SwapVirtualDesktopHotkeysEnabled)
    $#Left::
        Send #^{Left}
    Return

    $#Right::
        Send #^{Right}
    Return

    $#Up::
        Send #{Tab}
    Return

    $#Down::
        DllCall(onGuiDownPtr)
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
