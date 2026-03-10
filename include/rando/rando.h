#ifndef RANDO_H
#define RANDO_H
#define EVENT_ITEM_QUEUE_SIZE 0x10

#include "dolphin/types.h"
#include "JSystem/J2DGraph/J2DTextbox.h"
#include "JSystem/J2DGraph/J2DPicture.h"

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

class randoInfo_c {
public:
    randoInfo_c() { mInitialized = false; }

    int _create();
    int _delete();
    int execute();
    int draw();
    bool checkValidTransformAnywhere();
    int getBugReward(u8 bugId);
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

    u8 getGiveItemToPlayerStatus() { return eventItemStatus;}
    u16 getLastButtonInput() { return m_LastButtonInput;}
    float getPrevFrameAnalogR() { return prevFrameAnalogR;}
    u8 getTimeChange() { return mTimeChange; }
    bool getRoomReloadingState() { return roomReloadingState; }
    bool getHasPendingToDChange() { return hasPendingToDChange; }
    J2DPicture* getDPadIconPtr() { return dPadIconPtr;}
    J2DPicture* getBigKeyIconPtr() { return bigKeyIconPtr;}
    J2DPicture* getDunMapIconPtr() { return dunMapIconPtr;}
    J2DPicture* getDunCompassIconPtr() { return dunCompassIconPtr;}
    bool getDrawBigKey() { return drawBigKey;}
    
    void setGiveItemToPlayerStatus(u8 status) { eventItemStatus = status;}
    void setLastButtonInput(u16 buttonInput) { m_LastButtonInput = buttonInput;}
    void setPrevFrameAnalogR(float value) { prevFrameAnalogR = value;}
    void setHasPendingToDChange(bool hasPending) { hasPendingToDChange = hasPending; }
    void setTimeChange(u8 newTimeChange) { mTimeChange = newTimeChange; }
    void setRoomReloadingState(bool newState) { roomReloadingState = newState; }
    void setDPadIconPtr(J2DPicture* ptr) { dPadIconPtr = ptr;}
    void setBigKeyIconPtr(J2DPicture* ptr) { bigKeyIconPtr = ptr;}
    void setDunMapIconPtr(J2DPicture* ptr) { dunMapIconPtr = ptr;}
    void setDunCompassIconPtr(J2DPicture* ptr) { dunCompassIconPtr = ptr;}
    void setDrawBigKey(bool val) { drawBigKey = val; }

    bool mInitialized;
    u8 eventItemStatus;
    u16 mFrameCounter;
    u16 m_LastButtonInput;
    float prevFrameAnalogR;
    f32 rainbowPhaseAngle;
    bool isWolfDomeDrawn;
    bool hasPendingToDChange;
    u8 mTimeChange;
    u8 eventItemQueue[EVENT_ITEM_QUEUE_SIZE];
    bool roomReloadingState;
    J2DPicture* dPadIconPtr;
    J2DPicture* bigKeyIconPtr;
    J2DPicture* dunMapIconPtr;
    J2DPicture* dunCompassIconPtr;
    ResTIMG* bigKeyIconBuf;
    ResTIMG* dunMapIconBuf;
    ResTIMG* dunCompassIconBuf;
    bool drawBigKey;
};

void checkSetHCBkFlag(u8 req, u8 currentCount);
void checkSetHCBarrierFlag(u8 req, u8 currentCount);

extern randoInfo_c g_randoInfo;

#endif  // RANDO_H
