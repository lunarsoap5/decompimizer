#include "rando/itemWheelMenu.h"
#include "rando/rando.h"
#include "d/d_meter_HIO.h"
#include "rando/tools/draw.h"
#include "rando/seed/seed.h"
#include "rando/tools/tools.h"
#include "JSystem/J2DGraph/J2DTextBox.h"
#include "JSystem/J2DGraph/J2DPrint.h"

customMenuRing_c g_customMenuRing;

int customMenuRing_c::_initialize() {
    setRingOpen(false);
    g_customMenuRing.mpItemWheelText = new (getRandoHeap(), 4) J2DTextBox(); 
    g_customMenuRing.mpItemWheelText->setFont(mDoExt_getMesgFont());
    g_customMenuRing.mpItemWheelText->setFontSize(16.0f, 16.0f);
    g_customMenuRing.mpItemWheelText->setLineSpace(16.f);
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

void customMenuRing_c::setUpItemWheelMenuText()
{
    // Draw the text box
    
    char itemWheelTextBuf[100];

    // Draw current seed name
    //snprintf(tempBuf, sizeof(tempBuf), "Seed: %s\n\n", g_seedInfo.getHeaderPtr()->getSeedNamePtr());
    //strcat(itemWheelTextBuf, tempBuf);
    
    // Get Fused Shadow Count
    u32 shadowsCount = 0;
    u32 shardsCount = 0;
    u8 collectedShadows = dComIfGs_getCollectCrystal();
    u8 collectedShards = dComIfGs_getCollectMirror();

    for (int b = 5; b < 8; b++)
    {
        if ((collectedShadows << b) & 0x80)
        {
            shadowsCount++;
        }
    }

    for (int b = 4; b < 8; b++)
    {
        if ((collectedShards << b) & 0x80)
        {
            shardsCount++;
        }
    }
    snprintf(itemWheelTextBuf, sizeof(itemWheelTextBuf), "Shadows: %d/3\nShards: %d/4\n\n%-8s%-d (%d)\n", shadowsCount, shardsCount, "FT", 4, 4);
    
    mpItemWheelText->setString(itemWheelTextBuf);
}

void customMenuRing_c::handleItemWheelMenu(dMenu_Ring_c* dMenuRingPtr)
{
    const f32 ringPosX = dMenuRingPtr->getXPos();
    const f32 ringPosY = dMenuRingPtr->getYPos();
    f32 windowPosXOffset = 40.f;
    f32 windowPosYOffset = 34.f;
    GXColor colorBlk = {0, 0, 0, 255};

    // Draw the background first
    drawFilledRect(ringPosX + windowPosXOffset, ringPosY + windowPosYOffset, 528.f, 380.f, colorBlk);

    windowPosXOffset += 7.f;
    windowPosYOffset += 20.f;

    // Draw the text
    mpItemWheelText->draw(ringPosX + windowPosXOffset, ringPosY + windowPosYOffset);

    g_randoInfo.getBigKeyIconPtr()->draw(ringPosX + windowPosXOffset + 100.f, ringPosY + windowPosYOffset + 25.f, 30.f, 30.f, false, false, false);

    g_randoInfo.getDunMapIconPtr()->draw(ringPosX + windowPosXOffset + 135.f, ringPosY + windowPosYOffset + 25.f, 30.f, 30.f, false, false, false);

    g_randoInfo.getDunCompassIconPtr()->draw(ringPosX + windowPosXOffset + 170.f, ringPosY + windowPosYOffset + 25.f, 30.f, 30.f, false, false, false);

    return;
}
