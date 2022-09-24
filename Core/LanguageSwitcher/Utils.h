#pragma once

#include <Shellapi.h>

namespace FruitLanguageSwitcher {
    inline static bool isInGameMode() {
        // From https://github.com/microsoft/PowerToys/blob/main/src/common/utils/game_mode.h
        QUERY_USER_NOTIFICATION_STATE notification_state;
        if (SHQueryUserNotificationState(&notification_state) != S_OK)
        {
            return false;
        }
        return (notification_state == QUNS_RUNNING_D3D_FULL_SCREEN);
    }
}