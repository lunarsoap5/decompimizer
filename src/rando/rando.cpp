#include "rando/rando.h"
#include "rando/seed/seed.h"
#include "rando/tools/tools.h"
#include "rando/data/flags.h"
#include "rando/data/stages.h"
#include "rando/tools/verifyItemFunctions.h"
#include "rando/itemWheelMenu.h"
#include "d/d_com_inf_game.h"
#include "SSystem/SComponent/c_math.h"
#include "d/actor/d_a_alink.h"
#include "d/d_meter2.h"
#include "d/d_meter2_info.h"
#include "d/d_meter2_draw.h"
#include "m_Do/m_Do_controller_pad.h"
#include "JSystem/J2DGraph/J2DPrint.h"
#include "JSystem/J2DGraph/J2DTextBox.h"
#include "JSystem/J2DGraph/J2DOrthoGraph.h"
#include "JSystem/J2DGraph/J2DScreen.h"
#include "rando/tools/draw.h"
#include "m_Do/m_Do_ext.h"
#include <cstdio>

randoInfo_c g_randoInfo;

int randoInfo_c::_create() {
    mFrameCounter = 0;
    mInitialized = true;
    isWolfDomeDrawn = false;
    rainbowPhaseAngle = 0.f;
    eventItemStatus = QUEUE_EMPTY;
    hasPendingToDChange = false;
    
    randoCreateHeap();
    g_customMenuRing._initialize();
    g_seedInfo._create();

    // Initialize custom icons we will use
    ResTIMG const* dpadIcon = (ResTIMG const*)dComIfGp_getMain2DArchive()->getResource(0x54494D47, "font_51.bti");
            
    if (dpadIcon)
        g_randoInfo.setDPadIconPtr(new (getRandoHeap(), 4) J2DPicture(dpadIcon));

    JKRHeap* heap = getRandoHeap();
    ResTIMG* tex = (ResTIMG*)dComIfGp_getItemIconArchive()->getResource('TIMG', "im_item_icon_boss_key_48.bti");;
    
    if (tex != NULL) {
        u32 imgSize = GXGetTexBufferSize(tex->width, tex->height, tex->format,
                                            tex->mipmapEnabled, tex->mipmapCount);
        u32 totalSize = tex->imageOffset + imgSize;
        if (tex->indexTexture && tex->numColors > 0) {
            u32 palEnd = tex->paletteOffset + tex->numColors * 2;
            if (palEnd > totalSize) {
                totalSize = palEnd;
            }
        }
        bigKeyIconBuf = (ResTIMG*)heap->alloc(totalSize, 32);
        if (bigKeyIconBuf != NULL) {
            memcpy(bigKeyIconBuf, tex, totalSize);
            DCStoreRangeNoSync(bigKeyIconBuf, totalSize);
            bigKeyIconPtr = new (heap, 4) J2DPicture(bigKeyIconBuf);
        }
    }

    tex = (ResTIMG*)dComIfGp_getItemIconArchive()->getResource('TIMG', "tt_map_48.bti");;
    
    if (tex != NULL) {
        u32 imgSize = GXGetTexBufferSize(tex->width, tex->height, tex->format,
                                            tex->mipmapEnabled, tex->mipmapCount);
        u32 totalSize = tex->imageOffset + imgSize;
        if (tex->indexTexture && tex->numColors > 0) {
            u32 palEnd = tex->paletteOffset + tex->numColors * 2;
            if (palEnd > totalSize) {
                totalSize = palEnd;
            }
        }
        dunMapIconBuf = (ResTIMG*)heap->alloc(totalSize, 32);
        if (dunMapIconBuf != NULL) {
            memcpy(dunMapIconBuf, tex, totalSize);
            DCStoreRangeNoSync(dunMapIconBuf, totalSize);
            dunMapIconPtr = new (heap, 4) J2DPicture(dunMapIconBuf);
        }
    }

    tex = (ResTIMG*)dComIfGp_getItemIconArchive()->getResource('TIMG', "tt_kmps_48.bti");;
    
    if (tex != NULL) {
        u32 imgSize = GXGetTexBufferSize(tex->width, tex->height, tex->format,
                                            tex->mipmapEnabled, tex->mipmapCount);
        u32 totalSize = tex->imageOffset + imgSize;
        if (tex->indexTexture && tex->numColors > 0) {
            u32 palEnd = tex->paletteOffset + tex->numColors * 2;
            if (palEnd > totalSize) {
                totalSize = palEnd;
            }
        }
        dunCompassIconBuf = (ResTIMG*)heap->alloc(totalSize, 32);
        if (dunCompassIconBuf != NULL) {
            memcpy(dunCompassIconBuf, tex, totalSize);
            DCStoreRangeNoSync(dunCompassIconBuf, totalSize);
            dunCompassIconPtr = new (heap, 4) J2DPicture(dunCompassIconBuf);
        }
    }

    tex = (ResTIMG*)dComIfGp_getItemIconArchive()->getResource('TIMG', "im_pumpkin_48.bti");;
    
    if (tex != NULL) {
        u32 imgSize = GXGetTexBufferSize(tex->width, tex->height, tex->format,
                                            tex->mipmapEnabled, tex->mipmapCount);
        u32 totalSize = tex->imageOffset + imgSize;
        if (tex->indexTexture && tex->numColors > 0) {
            u32 palEnd = tex->paletteOffset + tex->numColors * 2;
            if (palEnd > totalSize) {
                totalSize = palEnd;
            }
        }
        pumpkinIconBuf = (ResTIMG*)heap->alloc(totalSize, 32);
        if (pumpkinIconBuf != NULL) {
            memcpy(pumpkinIconBuf, tex, totalSize);
            DCStoreRangeNoSync(pumpkinIconBuf, totalSize);
            pumpkinIconPtr = new (heap, 4) J2DPicture(pumpkinIconBuf);
        }
    }

    tex = (ResTIMG*)dComIfGp_getItemIconArchive()->getResource('TIMG', "im_cheese_48.bti");;
    
    if (tex != NULL) {
        u32 imgSize = GXGetTexBufferSize(tex->width, tex->height, tex->format,
                                            tex->mipmapEnabled, tex->mipmapCount);
        u32 totalSize = tex->imageOffset + imgSize;
        if (tex->indexTexture && tex->numColors > 0) {
            u32 palEnd = tex->paletteOffset + tex->numColors * 2;
            if (palEnd > totalSize) {
                totalSize = palEnd;
            }
        }
        cheeseIconBuf = (ResTIMG*)heap->alloc(totalSize, 32);
        if (cheeseIconBuf != NULL) {
            memcpy(cheeseIconBuf, tex, totalSize);
            DCStoreRangeNoSync(cheeseIconBuf, totalSize);
            cheeseIconPtr = new (heap, 4) J2DPicture(cheeseIconBuf);
        }
    }

    tex = (ResTIMG*)dComIfGp_getItemIconArchive()->getResource('TIMG', "ni_mkey_parts3_get_47_56.bti");;
    
    if (tex != NULL) {
        u32 imgSize = GXGetTexBufferSize(tex->width, tex->height, tex->format,
                                            tex->mipmapEnabled, tex->mipmapCount);
        u32 totalSize = tex->imageOffset + imgSize;
        if (tex->indexTexture && tex->numColors > 0) {
            u32 palEnd = tex->paletteOffset + tex->numColors * 2;
            if (palEnd > totalSize) {
                totalSize = palEnd;
            }
        }
        gmKeyIconBuf = (ResTIMG*)heap->alloc(totalSize, 32);
        if (gmKeyIconBuf != NULL) {
            memcpy(gmKeyIconBuf, tex, totalSize);
            DCStoreRangeNoSync(gmKeyIconBuf, totalSize);
            gmKeyIconPtr = new (heap, 4) J2DPicture(gmKeyIconBuf);
        }
    }

    tex = (ResTIMG*)dComIfGp_getItemIconArchive()->getResource('TIMG', "ni_key_shinshitu_48.bti");;
    
    if (tex != NULL) {
        u32 imgSize = GXGetTexBufferSize(tex->width, tex->height, tex->format,
                                            tex->mipmapEnabled, tex->mipmapCount);
        u32 totalSize = tex->imageOffset + imgSize;
        if (tex->indexTexture && tex->numColors > 0) {
            u32 palEnd = tex->paletteOffset + tex->numColors * 2;
            if (palEnd > totalSize) {
                totalSize = palEnd;
            }
        }
        bedKeyIconBuf = (ResTIMG*)heap->alloc(totalSize, 32);
        if (bedKeyIconBuf != NULL) {
            memcpy(bedKeyIconBuf, tex, totalSize);
            DCStoreRangeNoSync(bedKeyIconBuf, totalSize);
            bedKeyIconPtr = new (heap, 4) J2DPicture(bedKeyIconBuf);
        }
    }

    return 1;
}

int randoInfo_c::_delete() {
    mInitialized = false;
    return 1;
}

int randoInfo_c::execute() {
    if (!mInitialized) {
        return 0;
    }

    g_customMenuRing.resetRingDrawnThisFrame();
    g_customMenuRing.changeQuestItem(true);
    globalRGB = getRainbowRGB(127.5f);

    const uint currentButtons = mDoCPd_c::getHold(PAD_1);

    if (currentButtons != getLastButtonInput())
    {
        // Store before processing since we (potentially) un-set the padInfo values later
        setLastButtonInput(currentButtons);
    }

    if (checkButtonComboAnalog(PAD_TRIGGER_R | PAD_BUTTON_Y))
    {
        handleQuickTransform();
    }
    else if (daAlink_getAlinkActorClass() && checkButtonsHeld(PAD_TRIGGER_R) && g_seedInfo.spinnerSpeedIsIncreased())
    {
        fopAc_ac_c* spinnerActor = (fopAc_ac_c*)daAlink_getAlinkActorClass()->getSpinnerActor();

        if (spinnerActor)
        {
            float spinnerSpeed = spinnerActor->speedF;
            if (spinnerSpeed < 60.f)
            {
                spinnerSpeed += 2.f;
                spinnerActor->speedF = spinnerSpeed;
            }
        }
    }

    

    // Always check for and handle time of day changes
    if (getTimeChange() != NO_CHANGE)
    {
        handleTimeSpeed();
    }

    bool currentReloadingState;
    // Any custom functionality that relies on Link's actor being on a stage
    if (daAlink_getAlinkActorClass())
    {
        if (g_seedInfo.isMidnaHairRainbow())
        {
            adjustMidnaHairColor(globalRGB);
        }
        if (g_seedInfo.isWolfDomeRainbow() && daAlink_getAlinkActorClass()->checkWolfLockAttackChargeState() && isWolfDomeDrawn)
        {
            replaceEquipItemColor(globalRGB);
        }
        currentReloadingState = daAlink_getAlinkActorClass()->checkRestartRoom();
        // Handle giving item to the player at any time.
        initGiveItemToPlayer();
    }
    else
    {
        currentReloadingState = true;
    }

    bool prevReloadingState = getRoomReloadingState();
    if (!currentReloadingState)
    {
        if (prevReloadingState)
        {
            offLoad();
        }
    }
    setRoomReloadingState(currentReloadingState);

    // COpypasta old rando code until I build the framework out.
    /*if (!libtp::tp::d_a_alink::checkStageName(libtp::data::stage::allStages[libtp::data::stage::StageIDs::Title_Screen]))
    {
        handleFoolishItem(randoPtr);
    }*/

    // Main code as ran, so update any previous frame variables.
    setPrevFrameAnalogR(mDoCPd_c::getAnalogR(PAD_1));
    return 1;
}

int randoInfo_c::draw() {

    if (dComIfGs_isDungeonItemBossKey() && getDrawBigKey() && daAlink_getAlinkActorClass() && !daAlink_getAlinkActorClass()->checkEventRun())
    {
        f32 keyYPos = 340.f;
        if (dComIfGs_getKeyNum() == 0)
        {
            keyYPos += 20.f;
        }

        switch(getCurrentStageID())
        {
            case Goron_Mines:
            {
                gmKeyIconPtr->draw(535.f, keyYPos, 30.f, 30.f, false, false, false);
                break;
            }

            case Snowpeak_Ruins:
            {
                bedKeyIconPtr->draw(535.f, keyYPos, 30.f, 30.f, false, false, false);
                break;
            }

            default:
            {
                getBigKeyIconPtr()->draw(535.f, keyYPos, 30.f, 30.f, false, false, false);
                break;
            }
        }
    }
    return 1;
}

bool randoInfo_c::checkValidTransformAnywhere()
{
    if (!g_seedInfo.canTransformAnywhere())
    {
        return false;
    }

    // Check if the player is currently playing the goats minigame.
    if (daAlink_c::checkStageName("F_SP00"))
    {
        switch(g_dComIfG_gameInfo.play.getStartStageLayer())
        {
            // Layer 4 and 5 is used when the minigame is taking place.
            case 4:
            case 5:
            {
                // Return false as the game will crash if the player is in wolf form after the minigame ends.
                return false;
            }
            default:
            {
                break;
            }
        }
    }

    // Return true as the bool is set and there are no conflicting scenerios to prevent transformation
    return true;
}

int randoInfo_c::getBugReward(u8 bugId)
{
    const EntryInfo* bugRewardCheckInfoPtr = g_seedInfo.getHeaderPtr()->getBugRewardCheckInfoPtr();
    const u32 numBugRewardChecks = bugRewardCheckInfoPtr->getNumEntries();
    const BugReward* bugRewardChecks = g_seedInfo.getBugRewardChecksPtr();

    for (int i = 0; i < numBugRewardChecks; i++)
    {
        const BugReward* currentBugRewardCheck = &bugRewardChecks[i];
        if (bugId == currentBugRewardCheck->getBugId())
        {
            // Return new item
            return (u8)currentBugRewardCheck->getItemId();
        }
    }

    // Default
    return bugId;
}

u8 randoInfo_c::getPoeItem(u8 bitSw)
{
    /*
    Once the infrastructure is built the code will look like the following:
    u8 item = replacePoeReward(); we will probably build the functionality out instead of calling another func though.
    */ 
    return fpcNm_ITEM_POU_SPIRIT;
}

void randoInfo_c::handlePoeItem(u8 bitSw)
{
    u8 item = getPoeItem(bitSw);
    addItemToEventQueue(item);
    daAlink_getAlinkActorClass()->procWolfAtnActorMoveInit();
}

u8 randoInfo_c::getEventItem(u8 flag)
{
    const uint32_t numEventChecks = g_seedInfo.getHeaderPtr()->getEventItemCheckInfoPtr()->getNumEntries();
    const EventItem* eventChecks = g_seedInfo.getEventChecksPtr();

    for (uint32_t i = 0; i < numEventChecks; i++)
    {
        const EventItem* currentEventCheck = &eventChecks[i];
        if (flag == currentEventCheck->getFlag())
        {
            // Return new item
            return currentEventCheck->getItemID();
        }
    }

    // Currently we just use the vanilla item ID as the flag since the scope of these checks are limited at the moment.
    return flag;
}

void randoInfo_c::addItemToEventQueue(u8 item)
{
    for (int i = 0; i < EVENT_ITEM_QUEUE_SIZE; i++)
    {
        if (eventItemQueue[i] == 0)
        {
            eventItemQueue[i] = item;
            break;
        }
    }
}

void randoInfo_c::initGiveItemToPlayer()
{
    switch (daAlink_getAlinkActorClass()->mProcID)
    {
        case daAlink_c::PROC_WAIT:
        case daAlink_c::PROC_TIRED_WAIT:
        case daAlink_c::PROC_MOVE:
        case daAlink_c::PROC_WOLF_WAIT:
        case daAlink_c::PROC_WOLF_TIRED_WAIT:
        case daAlink_c::PROC_WOLF_MOVE:
        case daAlink_c::PROC_ATN_MOVE:
        case daAlink_c::PROC_WOLF_ATN_AC_MOVE:
        {
            // Check if link is currently in a cutscene
            if (daAlink_getAlinkActorClass()->checkEventRun())
            {
                break;
            }

            // Ensure that link is not currently in a message-based event.
            if (daAlink_getAlinkActorClass()->getEventId() != 0)
            {
                break;
            }

            u8 itemToGive = 0xFF;

            for (int i = 0; i < EVENT_ITEM_QUEUE_SIZE; i++)
            {
                const u8 storedItem = eventItemQueue[i];

                if (storedItem)
                {
                    const u8 giveItemToPlayerStatus = getGiveItemToPlayerStatus();

                    // If we have the call to clear the queue, then we want to clear the item and break out.
                    if (giveItemToPlayerStatus == CLEAR_QUEUE)
                    {
                        eventItemQueue[i] = 0;
                        setGiveItemToPlayerStatus(QUEUE_EMPTY);
                        break;
                    }

                    // If the queue is empty and we have an item to give, update the queue state.
                    else if (giveItemToPlayerStatus == QUEUE_EMPTY)
                    {
                        setGiveItemToPlayerStatus(ITEM_IN_QUEUE);
                    }

                    itemToGive = verifyProgressiveItem(storedItem);
                    break;
                }
            }

            // if there is no item to give, break out of the case.
            if (itemToGive == 0xFF)
            {
                break;
            }

            g_dComIfG_gameInfo.play.getEvent()->setGtItm(itemToGive);

            // Set the process value for getting an item to start the "get item" cutscene when next available.
            daAlink_getAlinkActorClass()->mProcID = daAlink_c::PROC_GET_ITEM;

            //  Get the event index for the "Get Item" event.
            const s16 eventIdx = dComIfGp_getEventManager().getEventIdx((fopAc_ac_c*)daAlink_getAlinkActorClass(),"DEFAULT_GETITEM",0xFF);

            // Finally we want to modify the event stack to prioritize our custom event so that it happens next.
            fopAcM_orderChangeEventId(daAlink_getAlinkActorClass(), eventIdx, 1, 0xFFFF);
        }
        default:
        {
            break;
        }
    }
}

void randoInfo_c::handleBonkDamage()
{
    if (!g_seedInfo.bonksDoDamage())
    {
        return;
    }

    u8 currentDamageMultiplier = g_seedInfo.getHeaderPtr()->getDamageMagnification();
    u16 currentHealth = dComIfGs_getLife();
    int newHealth;

    if (dComIfGs_getTransformStatus())
    {
        // Wolf takes double damage
        newHealth = currentHealth - (2 * currentDamageMultiplier);
    }
    else
    {
        newHealth = currentHealth - currentDamageMultiplier;
    }

    // Make sure an underflow doesn't occur
    if (newHealth < 0)
    {
        newHealth = 0;
    }
    dComIfGs_setLife(newHealth);
}

void randoInfo_c::handleTimeOfDayChange()
{
    if (dComIfGp_roomControl_getTimePass())
    {
        // No point in changing values if we are already changing the time.
        if (getTimeChange() == NO_CHANGE)
        {
            if (!dKy_daynight_check()) // Day time
            {
                setTimeChange(CHANGE_TO_NIGHT);
            }
            else
            {
                setTimeChange(CHANGE_TO_DAY);
            }
            g_env_light.time_change_rate = 1.f; // Increase time speed
        }
    }
    else
    {
        if (!dKy_daynight_check()) // Day time
        {
            dComIfGs_setTime(285.f);
        }
        else
        {
            dComIfGs_setTime(105.f);
        }
        dComIfGp_setEnableNextStage();
    }
}

void randoInfo_c::handleTimeSpeed()
{

    if (!dKy_daynight_check()) // Day time
    {
        if (getTimeChange() == CHANGE_TO_DAY)
        {
            g_env_light.time_change_rate = 0.012f; // Set time speed to normal
            setTimeChange(NO_CHANGE);
        }
    }
    else if (getTimeChange() == CHANGE_TO_NIGHT)
    {
        g_env_light.time_change_rate = 0.012f; // Set time speed to normal
        setTimeChange(NO_CHANGE);
    }
}

void randoInfo_c::offLoad()
{
    if ((getCurrentStageID() == City_in_the_Sky) && (dStage_roomControl_c::mStayNo == 0) && (dComIfGp_getStartStagePoint() == 3))
    {
        // Fan in the main room active
        dComIfGs_offSaveSwitch(0xA);

        // Main Room 1F explored
        dComIfGs_offSaveSwitch(0xF);
    }

    if (playerIsInRoomStage(1, allStages[Sacred_Grove]))
    {
        // If the portal in SG isn't active then we want to spawn the shadow beasts.
        if (!dComIfGs_isSaveSwitch(0x64))
        {
            dComIfGs_onSvOneZoneSwitch(0, 0xE);
        }
    }

    if ((getCurrentStageID() == Ordon_Ranch) && (dComIfGp_getStartStagePoint() == 1))
    {
        // Clear the danBit that starts a conversation when entering the ranch so the player can do goats as needed.
        dComIfGs_offSaveDunSwitch(0x1);
    }

    if (getCurrentStageID() == Cave_of_Ordeals)
    {
        // If we've reached certain checkpoints in CoO, we don't want to force the player to have to re-do everything. 
        if(dComIfGs_isEventBit(ORDON_SPRING_HAS_FARIES))
        {
            u32 danAddr = reinterpret_cast<u32>(&g_dComIfG_gameInfo.info.getDan());
            if(dComIfGs_isEventBit(FARON_SPRING_HAS_FARIES))
            {
                if(dComIfGs_isEventBit(ELDIN_SPRING_HAS_FARIES))
                {
                    if(dComIfGs_isEventBit(LANAYRU_SPRING_HAS_FARIES))
                    {
                        if(dComIfGs_isEventBit(SPRING_SPIRITS_CAN_GIVE_FARY_TEARS))
                        {
                            // Open all 50 CoO doors
                            *reinterpret_cast<u32*>(danAddr + 4) = 0xFFFFFFFF;
                            *reinterpret_cast<u32*>(danAddr + 8) = 0x0005FFFF;
                        }
                        else
                        {
                            // Open doors to CoO 1-40
                            *reinterpret_cast<u32*>(danAddr + 4) = 0xFFFFFFFF;
                            *reinterpret_cast<u32*>(danAddr + 8) = 0x000400FF;
                        }
                    }
                    else
                    {
                        // Open doors to CoO 1-30
                        *reinterpret_cast<u32*>(danAddr + 4) = 0x3FFFFFFF;
                    }
                }
                else
                {
                    // Open doors to CoO 1-20
                    *reinterpret_cast<u32*>(danAddr + 4) = 0xFFFFF;
                }
            }
            else
            {
                // Open doors to CoO 1-10
                *reinterpret_cast<u32*>(danAddr + 4) = 0x3FF;
            }
        }
    }
}

void checkSetHCBarrierFlag(u8 req, u8 currentCount)
{
    if (req != g_seedInfo.getHeaderPtr()->getCastleRequirements())
    {
        return;
    }
    
    if (currentCount >= g_seedInfo.getHeaderPtr()->getBarrierReqCount())
    {
        dComIfGs_onEventBit(BARRIER_GONE);
    }
}

void checkSetHCBkFlag(u8 req, u8 currentCount)
{
    if (req != g_seedInfo.getHeaderPtr()->getHcBkRequirement())
    {
        return;
    }
    
    if (currentCount >= g_seedInfo.getHeaderPtr()->getHcBkReqCount())
    {
        dComIfGs_onStageSwitch(0x18, 0x4B);
    }
}

bool checkFoolishItemEffectReady()
{
    // Verify Link is loaded on the map.
    if (!daAlink_getAlinkActorClass())
    {
        return false;
    }

    // Ensure Link is not in a cutscene
    if (daAlink_getAlinkActorClass()->checkEventRun())
    {
        return false;
    }

    // Make sure Link isn't riding anything
    if (daAlink_getAlinkActorClass()->checkRide())
    {
        return false;
    }

    // Ensure there are pointers to the mMeterClass and mpMeterDraw structs
    if (!dMeter2Info_getMeterClass())
    {
        return false;
    }

    if (!dMeter2Info_getMeterClass()->getMeterDrawPtr())
    {
        return false;
    }

    // Make sure Z button isn't dimmed
    if (dMeter2Info_getMeterClass()->getMeterDrawPtr()->getZButtonAlpha() != 1.f)
    {
        return false;
    }

    switch (daAlink_getAlinkActorClass()->mProcID)
    {
        case daAlink_c::PROC_TALK:
        case daAlink_c::PROC_WOLF_SWIM_MOVE:
        case daAlink_c::PROC_SWIM_MOVE:
        case daAlink_c::PROC_SWIM_WAIT:
        case daAlink_c::PROC_WOLF_SWIM_WAIT:
        case daAlink_c::PROC_SWIM_UP:
        case daAlink_c::PROC_SWIM_DIVE:
        {
            return false;
        }
        default:
        {
            break;
        }
    }
    return true;
}
