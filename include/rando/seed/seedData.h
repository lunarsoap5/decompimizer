#include "dolphin/types.h"
#include "dolphin/gx/GXStruct.h"

class RegionFlag
{
    public:
    RegionFlag() {}
    ~RegionFlag() {}

    u8 getRegionId() const { return region_id; }
    u8 getBitId() const { return bit_id; }

    private:
    u8 region_id;
    u8 bit_id;
};

class EventFlag
{
    public:
    EventFlag() {}
    ~EventFlag() {}

    u8 getOffset() const { return offset; }
    u8 getFlag() const { return flag; }

    private:
    u8 offset;
    u8 flag;
};

enum MirrorChamberRequirement
{
    Open = 0x0,
    Barrier = 0x1,
    Closed = 0x2,
};

class BossCheck
{
    public:
    BossCheck() {}
    ~BossCheck() {}

    u16 getStageIDX() const { return stageIDX; }
    u8 getItem() const { return item; }

    private:
    u16 stageIDX; // The stage where the replacement is taking place.
    u8 item;      // New item id
    u8 padding;
};

class HiddenSkillCheck
{
    public:
    HiddenSkillCheck() {}
    ~HiddenSkillCheck() {}

    u8 getStageIDX() const { return stageIDX; }
    u8 getRoomID() const { return roomID; }
    u8 getItemID() const { return itemID; }

    private:
    u8 stageIDX; // The ID of the stage that Golden Wolf was located in
    u8 roomID;   // The room of the stage that the Golden Wolf was located in.
    u8 itemID;   // The item to be given when in the above stage and room.
    u8 padding;
};

class BugReward
{
    public:
    BugReward() {}
    ~BugReward() {}

    u16 getBugId() const { return bugID; }
    u8 getItemId() const { return itemID; }

    private:
    u16 bugID; // The bug that link is showing to Agitha
    u8 itemID; // The item that Agitha will give Link.
    u8 padding;
};

class PoeReward
{
    public:
    PoeReward() {}
    ~PoeReward() {}

    u8 getStageIDX() const { return stageIDX; }
    u8 getFlag() const { return flag; }
    u8 getItem() const { return item; }

    private:
    u8 stageIDX;
    u8 flag; // Flag used for identification
    u8 item; // New item id
    u8 padding;
};

// These items are given either during cutscenes or at a specific time.
class EventItem
{
    public:
    EventItem() {}
    ~EventItem() {}

    u8 getItemID() const { return itemID; }
    u8 getStageIDX() const { return stageIDX; }
    u8 getRoomID() const { return roomID; }
    u8 getFlag() const { return flag; }

    private:
    u8 itemID;   // The item to be given.
    u8 stageIDX; // The stage that the event is in.
    u8 roomID;   // The room that the event is in.
    u8 flag;     // The unique identifier used to disinguish between checks in the same room.
};

enum RawRGBId
{
    LanternGlow,
    LightSwordGlow,
    Hearts,
    ABtn,
    BBtn,
    XBtn,
    YBtn,
    ZBtn
};

enum CastleEntryRequirements
{
    HC_Open = 0,
    HC_Fused_Shadows = 1,
    HC_Mirror_Shards,
    HC_Dungeons,
    HC_Vanilla,
    HC_Poe_Souls,
    HC_Hearts,
};

enum CastleBkRequirements
{
    HC_BK_None = 0,
    HC_BK_Fused_Shadows = 1,
    HC_BK_Mirror_Shards,
    HC_BK_Dungeons,
    HC_BK_Poe_Souls,
    HC_BK_Hearts,
};

enum PalaceEntryRequirements
{
    PoT_Open = 0,
    PoT_Fused_Shadows = 1,
    PoT_Mirror_Shards,
    PoT_Vanilla
};

enum StartingTimeOfDay
{
    Morning = 0,
    Noon = 1,
    Evening = 2,
    Night = 3
};

class RawRGBTable
{
    public:
    RawRGBTable() {}
    ~RawRGBTable() {}

    const u8* getLanternColorPtr() const { return reinterpret_cast<const u8*>(&lanternColor); }

    const u8* getWolfDomeAttackColorPtr() const
    {
        return reinterpret_cast<const u8*>(&wolfDomeAttackColor);
    }

    uint getLanternColor() const { return lanternColor; }
    GXColor getLightSwordColor() const { return lightSwordColor; }
    uint getHeartColor() const { return heartColor; }
    uint getAButtonColor() const { return aButtonColor; }
    uint getBButtonColor() const { return bButtonColor; }
    uint getXButtonColor() const { return xButtonColor; }
    uint getYButtonColor() const { return yButtonColor; }
    uint getZButtonColor() const { return zButtonColor; }
    uint getWolfDomeAttackColor() const { return wolfDomeAttackColor; }

    private:
    uint lanternColor;
    GXColor lightSwordColor;
    uint heartColor;
    uint aButtonColor;
    uint bButtonColor;
    uint xButtonColor;
    uint yButtonColor;
    uint zButtonColor;
    uint wolfDomeAttackColor;
};

class GoldenWolfItemReplacement
{
    public:
    GoldenWolfItemReplacement() {}
    ~GoldenWolfItemReplacement() {}

    int getItemActorId() const { return itemActorId; }
    s16 getFlag() const { return flag; }
    u8 getMarkerFlag() const { return markerFlag; }

    void setItemActorId(int id) { itemActorId = id; }
    void setFlag(s16 value) { flag = value; }
    void setMarkerFlag(u8 value) { markerFlag = value; }

    private:
    int itemActorId; // Global actor id for the spawned item
    s16 flag;        // Flag associated with the current golden wolf
    u8 markerFlag;  // Flag associated with the current golden wolf's marker on the map
};

class EntranceInfo
{
    public:
    EntranceInfo() {}
    ~EntranceInfo() {}

    u8 getStageIDX() { return mStageIDX; }
    u8 getRoomIDX() { return mRoomIDX; }
    u8 getSpawn() { return mSpawn; }
    u8 getState() { return mState; }

    private:
    u8 mStageIDX;
    u8 mRoomIDX;
    u8 mSpawn;
    u8 mState;
};

class ShopCheck
{
    public:
    ShopCheck() {}
    ~ShopCheck() {}

    u8 getShopItemID() const { return shopItemID; }
    u8 getReplacementItemID() const { return replacementItemID; }

    private:
    u8 shopItemID;        // target item id
    u8 replacementItemID; // replacement item id
    u8 padding[2];
};
