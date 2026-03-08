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
#include "rando/tools/draw.h"
#include "m_Do/m_Do_ext.h"

randoInfo_c g_randoInfo;

int randoInfo_c::_create() {
    mFrameCounter = 0;
    mInitialized = true;
    isWolfDomeDrawn = false;
    rainbowPhaseAngle = 0.f;
    eventItemStatus = QUEUE_EMPTY;
    hasPendingToDChange = false;
    g_customMenuRing._initialize();
    g_seedInfo._create();
    randoCreateHeap();

    mpItemWheelText = new (getRandoHeap(), 4) J2DTextBox(); 
    mpItemWheelText->setFont(mDoExt_getMesgFont());
    mpItemWheelText->setFontSize(24.0f, 24.0f);
    mpItemWheelText->setString("Example Text");
    
    return 1;
}

int randoInfo_c::_delete() {
    mInitialized = false;
    delete mpItemWheelText;
    mpItemWheelText = NULL;
    return 1;
}

int randoInfo_c::execute() {
    if (!mInitialized) {
        return 0;
    }

    g_customMenuRing.resetRingDrawnThisFrame();
    g_customMenuRing.changeQuestItem(true);

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

    // Every 300 frames, set rupees to a random value
    mFrameCounter++;
    if (mFrameCounter >= 300) {
        mFrameCounter = 0;
        u16 randomRupees = (u16)cM_rndF(1000.0f);
        dComIfGs_setRupee(randomRupees);
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
            adjustMidnaHairColor();
        }
        if (g_seedInfo.isWolfDomeRainbow() && daAlink_getAlinkActorClass()->checkWolfLockAttackChargeState() && isWolfDomeDrawn)
        {
            replaceEquipItemColor();
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
    // Test drawing from rando heap
    /*
    if (mpItemWheelText != NULL)
    {
        mpItemWheelText->draw(100.f, 100.f, 608.f, HBIND_LEFT);
    }
        */

    if (dComIfGs_isDungeonItemBossKey() && getDrawBigKey())
    {
        getBigKeyIconPtr()->draw(535.f, 340.f, 30.f, 30.f, false, false, false);
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
    // addItemToEventQueue(item);
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
