#if (LWinRemapEnabled)
    <#V::>#V ; fix the non-sense problem with win+v
    $LWin Up::
      If (A_PriorKey = "LWin")
        Send ^+!#S ; or whatever shortcut you set for PTRun
    return
#if
