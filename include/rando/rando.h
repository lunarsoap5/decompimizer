#ifndef RANDO_H
#define RANDO_H
#define EVENT_ITEM_QUEUE_SIZE 0x10

#include "dolphin/types.h"
#include "JSystem/J2DGraph/J2DTextbox.h"
#include "JSystem/J2DGraph/J2DPicture.h"
#include "dolphin/gx/GXStruct.h"
#include "d/d_stage.h"

enum TimeChange
{
    NO_CHANGE = 0,
    CHANGE_TO_NIGHT,
    CHANGE_TO_DAY,
};

enum EventItemStatus
{
    QUEUE_EMPTY,
    ITEM_IN_QUEUE,
    CLEAR_QUEUE,
};

class randoInfo_c
{
   public:
    randoInfo_c() { mInitialized = false; }

    int _create();
    int _delete();
    int execute();
    int draw();
    bool checkValidTransformAnywhere();
    u8 getPoeItem(u8 bitSw);
    void handlePoeItem(u8 bitSw);
    u8 getEventItem(u8 flag);
    void addItemToEventQueue(u8 item);
    void initGiveItemToPlayer();
    void handleBonkDamage();
    void handleTimeOfDayChange();
    void handleTimeSpeed();
    void offLoad();
    bool checkFoolishItemEffectReady();
    void handleFoolishItem();

    u8 getGiveItemToPlayerStatus() { return eventItemStatus; }
    u16 getLastButtonInput() { return m_LastButtonInput; }
    float getPrevFrameAnalogR() { return prevFrameAnalogR; }
    u8 getTimeChange() { return mTimeChange; }
    bool getRoomReloadingState() { return roomReloadingState; }
    bool getHasPendingToDChange() { return hasPendingToDChange; }
    J2DPicture* getDPadIconPtr() { return dPadIconPtr; }
    J2DPicture* getBigKeyIconPtr() { return bigKeyIconPtr; }
    J2DPicture* getDunMapIconPtr() { return dunMapIconPtr; }
    J2DPicture* getDunCompassIconPtr() { return dunCompassIconPtr; }
    bool getDrawBigKey() { return drawBigKey; }
    GXColor getGlobalRGB() { return globalRGB; }
    bool getShowTriforceCount() { return isShowTriforceCount; }

    void setGiveItemToPlayerStatus(u8 status) { eventItemStatus = status; }
    void setLastButtonInput(u16 buttonInput) { m_LastButtonInput = buttonInput; }
    void setPrevFrameAnalogR(float value) { prevFrameAnalogR = value; }
    void setHasPendingToDChange(bool hasPending) { hasPendingToDChange = hasPending; }
    void setTimeChange(u8 newTimeChange) { mTimeChange = newTimeChange; }
    void setRoomReloadingState(bool newState) { roomReloadingState = newState; }
    void setDPadIconPtr(J2DPicture* ptr) { dPadIconPtr = ptr; }
    void setBigKeyIconPtr(J2DPicture* ptr) { bigKeyIconPtr = ptr; }
    void setDunMapIconPtr(J2DPicture* ptr) { dunMapIconPtr = ptr; }
    void setDunCompassIconPtr(J2DPicture* ptr) { dunCompassIconPtr = ptr; }
    void setDrawBigKey(bool val) { drawBigKey = val; }
    void showTriforceCount() { isShowTriforceCount = true; }
    void hideTriforceCount() { isShowTriforceCount = false; }

    /* 0x00 */ bool mInitialized;
    /* 0x01 */ u8 eventItemStatus;
    /* 0x02 */ u16 mFrameCounter;
    /* 0x04 */ u16 m_LastButtonInput;
    /* 0x06 */ bool isShowTriforceCount;
    /* 0x08 */ float prevFrameAnalogR;
    /* 0x0C */ f32 rainbowPhaseAngle;
    /* 0x10 */ bool isWolfDomeDrawn;
    /* 0x11 */ bool hasPendingToDChange;
    /* 0x12 */ u8 mTimeChange;
    /* 0x13 */ u8 eventItemQueue[EVENT_ITEM_QUEUE_SIZE];
    /* 0x23 */ bool roomReloadingState;
    /* 0x24 */ J2DPicture* dPadIconPtr;
    /* 0x28 */ J2DPicture* bigKeyIconPtr;
    /* 0x2C */ J2DPicture* dunMapIconPtr;
    /* 0x30 */ J2DPicture* dunCompassIconPtr;
    /* 0x34 */ J2DPicture* cheeseIconPtr;
    /* 0x38 */ J2DPicture* pumpkinIconPtr;
    /* 0x3C */ J2DPicture* bedKeyIconPtr;
    /* 0x40 */ J2DPicture* gmKeyIconPtr;
    /* 0x44 */ ResTIMG* bigKeyIconBuf;
    /* 0x48 */ ResTIMG* dunMapIconBuf;
    /* 0x4C */ ResTIMG* dunCompassIconBuf;
    /* 0x50 */ ResTIMG* cheeseIconBuf;
    /* 0x54 */ ResTIMG* pumpkinIconBuf;
    /* 0x58 */ ResTIMG* bedKeyIconBuf;
    /* 0x5C */ ResTIMG* gmKeyIconBuf;
    /* 0x60 */ bool drawBigKey;
    /* 0x61 */ dStage_startStage_c lastSavableStart;
    /* 0x70 */ GXColor globalRGB;
    /* 0x74 */ u8 mFoolishItemCount;
};

void checkSetHCBkFlag(u8 req, u8 currentCount);
void checkSetHCBarrierFlag(u8 req, u8 currentCount);
void rollCredits();

extern randoInfo_c g_randoInfo;

#endif // RANDO_H
