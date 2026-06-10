#include "rando/itemWheelMenu.h"
#include "rando/rando.h"
#include "rando/data/flags.h"
#include "rando/tools/verifyItemFunctions.h"
#include "d/d_meter_HIO.h"
#include "rando/tools/draw.h"
#include "rando/seed/seed.h"
#include "rando/tools/tools.h"
#include "JSystem/J2DGraph/J2DTextBox.h"
#include "JSystem/J2DGraph/J2DPrint.h"

customMenuRing_c g_customMenuRing;

int customMenuRing_c::_initialize() {
    setRingOpen(false);
    g_customMenuRing.mpItemWheelDunText = new (getRandoHeap(), 4) J2DTextBox(); 
    g_customMenuRing.mpItemWheelDunText->setFont(mDoExt_getMesgFont());
    g_customMenuRing.mpItemWheelDunText->setFontSize(16.0f, 16.0f);
    g_customMenuRing.mpItemWheelDunText->setLineSpace(25.f);
    g_customMenuRing.mpItemWheelKeyText = new (getRandoHeap(), 4) J2DTextBox(); 
    g_customMenuRing.mpItemWheelKeyText->setFont(mDoExt_getMesgFont());
    g_customMenuRing.mpItemWheelKeyText->setFontSize(16.0f, 16.0f);
    g_customMenuRing.mpItemWheelKeyText->setLineSpace(25.f);
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

char* getYesNoText(bool param)
{
    if (param)
    {
        return "Yes";
    }
    else
    {
        return "No";
    }
}

void customMenuRing_c::setUpItemWheelMenuText()
{
    // Draw the text box
    
    char itemWheelTextBuf[300];

    // Draw current seed name
    //snprintf(tempBuf, sizeof(tempBuf), "Seed: %s\n\n", g_seedInfo.getHeaderPtr()->getSeedNamePtr());
    //strcat(itemWheelTextBuf, tempBuf);
    
    // Get Fused Shadow Count
    u32 shadowsCount = 0;
    u32 shardsCount = 0;
    u8 collectedShadows = dComIfGs_getCollectCrystal();
    u8 collectedShards = dComIfGs_getCollectMirror();
    u8 faronKeyFlags[] = {0xC, 0x14};

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


    const u8 ftKeyNum = dComIfGs_getKeyNum(0x10);
    const u8 ftTotalKeyNum = dComIfGs_getTotalKeyNum(0x10);
    const u8 gmKeyNum = dComIfGs_getKeyNum(0x11);
    const u8 gmTotalKeyNum = dComIfGs_getTotalKeyNum(0x11);
    const u8 lbtKeyNum = dComIfGs_getKeyNum(0x12);
    const u8 lbtTotalKeyNum = dComIfGs_getTotalKeyNum(0x12);
    const u8 agKeyNum = dComIfGs_getKeyNum(0x13);
    const u8 agTotalKeyNum = dComIfGs_getTotalKeyNum(0x13);
    const u8 sprKeyNum = dComIfGs_getKeyNum(0x14);
    const u8 sprTotalKeyNum = dComIfGs_getTotalKeyNum(0x14);
    const u8 totKeyNum = dComIfGs_getKeyNum(0x15);
    const u8 totTotalKeyNum = dComIfGs_getTotalKeyNum(0x15);
    const u8 citsKeyNum = dComIfGs_getKeyNum(0x16);
    const u8 citsTotalKeyNum = dComIfGs_getTotalKeyNum(0x16);
    const u8 potKeyNum = dComIfGs_getKeyNum(0x17);
    const u8 potTotalKeyNum = dComIfGs_getTotalKeyNum(0x17);
    const u8 hcKeyNum = dComIfGs_getKeyNum(0x18);
    const u8 hcTotalKeyNum = dComIfGs_getTotalKeyNum(0x18);
    const u8 campKeyNum = dComIfGs_getKeyNum(0xA);
    const u8 campTotalKeyNum = dComIfGs_getTotalKeyNum(0xA);
    bool hasFaronGateKey = dComIfGs_isStageSwitch(0x2, 0x14);
    bool hasCoroGateKey = dComIfGs_isStageSwitch(0x2, 0xC);
    bool hasGateKey = haveItem(fpcNm_ITEM_BOSSRIDER_KEY);

    snprintf(itemWheelTextBuf, sizeof(itemWheelTextBuf), "Shadows: %d/3        Key Legend:\nShards: %d/4         Current (Total)\n\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n", shadowsCount, shardsCount, "Forest", "Mines", "Lakebed", "Arbiters", "Snowpeak", "Time", "City", "Palace", "Hyrule", "Desert", "Faron Gate", "Coro Gate", "Gate Keys");
    
    mpItemWheelDunText->setString(itemWheelTextBuf); 

    snprintf(itemWheelTextBuf, sizeof(itemWheelTextBuf), "\n\n\n%d (%d)\n%d (%d)\n%d (%d)\n%d (%d)\n%d (%d)\n%d (%d)\n%d (%d)\n%d (%d)\n%d (%d)\n%d (%d)\n%s\n%s\n%s\n", ftKeyNum, ftTotalKeyNum, gmKeyNum, gmTotalKeyNum, lbtKeyNum, lbtTotalKeyNum, agKeyNum, agTotalKeyNum, sprKeyNum, sprTotalKeyNum, totKeyNum, totTotalKeyNum, citsKeyNum, citsTotalKeyNum, potKeyNum, potTotalKeyNum, hcKeyNum, hcTotalKeyNum, campKeyNum, campTotalKeyNum, getYesNoText(hasFaronGateKey), getYesNoText(hasCoroGateKey),getYesNoText(hasGateKey));

    mpItemWheelKeyText->setString(itemWheelTextBuf); 
}

void customMenuRing_c::handleItemWheelMenu(dMenu_Ring_c* dMenuRingPtr)
{
    const f32 ringPosX = dMenuRingPtr->getXPos();
    const f32 ringPosY = dMenuRingPtr->getYPos();
    f32 windowPosXOffset = 95.f;
    f32 windowPosYOffset = 20.f;
    GXColor colorBlk = {0, 0, 0, 255};

    // Draw the background first
    
    drawFilledRect(ringPosX + windowPosXOffset, ringPosY + windowPosYOffset, 305.f, 410.f, colorBlk);

    windowPosXOffset += 7.f;
    windowPosYOffset += 20.f;

    // Draw the text
    mpItemWheelDunText->draw(ringPosX + windowPosXOffset, ringPosY + windowPosYOffset);
    mpItemWheelKeyText->draw(ringPosX + windowPosXOffset + 120.f, ringPosY + windowPosYOffset);

    windowPosYOffset += 58.f;

    for (int i = 0x10; i < 0x19; i++)
    {
        if (dComIfGs_isDungeonItemBossKey(i))
        {
            if (i == 0x11)
            {
                g_randoInfo.gmKeyIconPtr->draw(ringPosX + windowPosXOffset + 170.f, ringPosY + windowPosYOffset, 23.f, 23.f, false, false, false);
            }
            else if (i == 0x14)
            {
                g_randoInfo.bedKeyIconPtr->draw(ringPosX + windowPosXOffset + 170.f, ringPosY + windowPosYOffset, 23.f, 23.f, false, false, false);
            }
            else
            {
                g_randoInfo.getBigKeyIconPtr()->draw(ringPosX + windowPosXOffset + 170.f, ringPosY + windowPosYOffset, 23.f, 23.f, false, false, false);
            }
        }
        
        if (dComIfGs_isDungeonItemMap(i))
        {
            g_randoInfo.getDunMapIconPtr()->draw(ringPosX + windowPosXOffset + 195.f, ringPosY + windowPosYOffset, 23.f, 23.f, false, false, false);
        }
        if (dComIfGs_isDungeonItemCompass(i))
        {
            g_randoInfo.getDunCompassIconPtr()->draw(ringPosX + windowPosXOffset + 220.f, ringPosY + windowPosYOffset, 23.f, 23.f, false, false, false);
        }

        if (i == 0x14)
        {
            if (dComIfGs_isEventBit(TOLD_YETA_ABOUT_PUMPKIN))
            {
                g_randoInfo.pumpkinIconPtr->draw(ringPosX + windowPosXOffset + 245.f, ringPosY + windowPosYOffset, 23.f, 23.f, false, false, false);
            }
            if (dComIfGs_isEventBit(TOLD_YETA_ABOUT_CHEESE))
            {
                g_randoInfo.cheeseIconPtr->draw(ringPosX + windowPosXOffset + 270.f, ringPosY + windowPosYOffset, 23.f, 23.f, false, false, false);
            }
        }
        windowPosYOffset += 25.f;
    }

    return;
}
