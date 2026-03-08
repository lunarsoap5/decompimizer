#include "rando/itemWheelMenu.h"
#include "d/d_meter_HIO.h"
#include "rando/tools/draw.h"

customMenuRing_c g_customMenuRing;

int customMenuRing_c::_initialize() {
    setRingOpen(false);
    return 1;
}

void setHUDButtonsAlpha(bool menuIsDisplayed)
{
    if (!menuIsDisplayed)
    {
        // Display the HUD
        g_drawHIO.mMainHUDButtonsAlpha = 1.f;
    }
    else
    {
        // Don't display the HUD
        g_drawHIO.mMainHUDButtonsAlpha = 0.f;
    }
}

void customMenuRing_c::handleItemWheelMenu(dMenu_Ring_c* dMenuRingPtr)
{
    const f32 ringPosX = dMenuRingPtr->getXPos();
    const f32 ringPosY = dMenuRingPtr->getYPos();
    const f32 windowPosXOffset = 40.f;
    const f32 windowPosYOffset = 34.f;
    GXColor colorBlk = {0, 0, 0, 255};

    drawFilledRect(ringPosX + windowPosXOffset, ringPosY + windowPosYOffset, 528.f, 380.f, colorBlk);
    return;
}
