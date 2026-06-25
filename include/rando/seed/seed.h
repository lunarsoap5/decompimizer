#ifndef SEED_H
#define SEED_H

#include "dolphin/types.h"
#include "rando/seed/seedData.h"
#include "rando/returnPlaces.h"
#include <string>

enum SeedEnabledFlag
{
    TRANSFORM_ANYWHERE = 0,
    QUICK_TRANSFORM,
    INCREASE_SPINNER_SPEED,
    BONKS_DO_DAMAGE,
    AUTOFILL_WALLETS,
    MODIFY_SHOP_MODELS,
    RAINBOW_LANTERN,
    RAINBOW_MIDNA,
    RAINBOW_LIGHT_SWORD,
    LIGHT_SWORD_ALWAYS_ON,
    RAINBOW_LOCK_DOME,
    RETURN_MONEY_TO_CHEST,
    SKIP_MINOR_CUTSCENES,
    OPEN_MAP,
    REMOVE_IB_LIMIT,
    DISABLE_BATTLE_MUSIC,
    SET_INSTANT_TEXT,
    SKIP_MAJOR_CUTSCENES,
    INVERT_CAMERA_AXIS,
    MAKE_LIGHT_SWORD_GLOW,
};

bool flagIsEnabled(const uint* bitfieldPtr, uint totalFlags, uint flag);

class EntryInfo
{
    public:
    EntryInfo() {}
    ~EntryInfo() {}

    u16 getNumEntries() const { return numEntries; }
    u16 getDataOffset() const { return dataOffset; }

    private:
    u16 numEntries;
    u16 dataOffset;
};

class seedHeaderInfo_c {
public:
    seedHeaderInfo_c(const void* fileData) { memcpy(this, fileData, sizeof(seedHeaderInfo_c));}
    int _create();
    int _delete();

    bool magicIsValid() const
    {
        return memcmp(&magic[0], "TPR", sizeof(magic)) == 0;
    }

    const char* getSeedNamePtr() const { return &seedName[0]; }
    u16 getHeaderSize() const { return headerSize; }
    u16 getDataSize() const { return dataSize; }
    uint getTotalSize() const { return totalSize; }

    u16 getMaloShopDonationAmount() const { return maloShopDonationAmount; }
    u8 getCastleRequirements() const { return castleRequirements; }
    u8 getPalaceRequirements() const { return palaceRequirements; }
    u8 getMapClearBits() const { return mapClearBits; }
    u8 getDamageMagnification() const { return damageMagnification; }
    u8 getToTSwordRequirement() const { return totSwordRequirement; }
    u8 getMirrorChamberRequirement() const { return mirrorChamberEntrance; }
    u8 getBarrierReqCount() const { return barrierReqCount; }
    u8 getHcBkReqCount() const { return hcBkReqCount; }
    u8 getHcBkRequirement() const { return hcBkRequirement; }
    u16 getSmallWalletMax() const { return smallWalletMax; }
    u16 getBigWalletMax() const { return bigWalletMax; }
    u16 getGiantWalletMax() const { return giantWalletMax; }
    GXColorS10 getNormalColor() const {return l_normalColor; }
    GXColor getNormalKColor() const { return l_normalKColor; }
    GXColor getNormalKColor2() const { return l_normalKColor2; }
    GXColorS10 getBigColor() const { return l_bigColor; }
    GXColor getBigKColor() const { return l_bigKColor; }
    GXColor getLNormalKColor() const { return l_lNormalKColor; }
    GXColor getLNormalKColor2() const { return l_lNormalKColor2; }
    GXColorS10 getLBigColor() const { return l_lBigColor; }
    GXColor getLBigKColor2() const { return l_lBigKColor2; }
    GXColor getWolfDomeAttackWave1Color() const { return wolfDomeAttackWave1Color; }
    GXColor getWolfDomeAttackWave2Color() const { return wolfDomeAttackWave2Color; }
    const u8* getWolfDomeAttackWave1ColorPtr() const { return (u8*)&wolfDomeAttackWave1Color; }
    const u8* getWolfDomeAttackWave2ColorPtr() const { return (u8*)&wolfDomeAttackWave2Color; }
    GXColor getLightSwordGlowColor() const { return lightSwordGlowColor; }
    uint getLanternColor() const { return lanternColor; }
    u8* getLanternColorPtr() const { return (u8*)&lanternColor; }
    EntranceInfo getSpawnInfo() const { return spawnInfo; }
    u16 getReturnPlaceSectionOffset() const {return returnPlaceSectionOffset; }

    //const EntryInfo* getVolatilePatchInfoPtr() const { return &volatilePatchInfo; }
    //const EntryInfo* getOneTimePatchInfoPtr() const { return &oneTimePatchInfo; }
    const EntryInfo* getFlagBitfieldPtr() const { return &flagBitfieldInfo; }
    const EntryInfo* getEventFlagsInfoPtr() const { return &eventFlagsInfo; }
    const EntryInfo* getRegionFlagsInfoPtr() const { return &regionFlagsInfo; }
    const EntryInfo* getBossCheckInfoPtr() const { return &bossCheckInfo; }
    const EntryInfo* getHiddenSkillCheckInfoPtr() const { return &hiddenSkillCheckInfo; }
    const EntryInfo* getPoeRewardInfoPtr() const { return &poeRewardInfo; }
    const EntryInfo* getBugRewardCheckInfoPtr() const { return &bugRewardCheckInfo; }
    const EntryInfo* getEventItemCheckInfoPtr() const { return &eventItemCheckInfo; }
    const EntryInfo* getStartingItemCheckInfoPtr() const { return &startingItemInfo; }
    const EntryInfo* getShopItemCheckInfoPtr() const { return &shopItemCheckInfo; }
    
    /* 0x00 */ char magic[3]; // Not null terminated, should always be TPR
    /* 0x03 */ char seedName[33];
    /* 0x24 */ u16 headerSize; // Total size of the header in bytes
    /* 0x26 */ u16 dataSize;   // Total number of bytes of seed data
    /* 0x28 */ uint totalSize;  // Total number of bytes in the GCI

    // BitArray where each bit represents a patch/modification to be applied for this playthrough; these
    // patchs/modifications must be applied every time a file is loaded
    ///* 0x30 */ EntryInfo volatilePatchInfo;
    // Note for moving forward: we can add functionality for most of these patches pretty easily
    // example: for faron twilight, just check if we set the evt bit and run the code from there.

    // BitArray where each bit represents a patch/modification to be applied for this playthrough; these
    // patchs/modifications must be applied only when a seed is loaded
    // /* 0x34 */ EntryInfo oneTimePatchInfo;
    // Note for moving forward: we can just set up the code to check for a bool since we won't have custom functions. 

    // BitArray where each bit represents an arbitrary flag indicated by the SeedEnabledFlag enum
    /* 0x2C */ EntryInfo flagBitfieldInfo;

    /* 0x30 */ EntryInfo eventFlagsInfo;  // eventFlags that need to be set for this seed
    /* 0x34 */ EntryInfo regionFlagsInfo; // regionFlags that need to be set, alternating
    /* 0x38 */ EntryInfo poeRewardInfo;
    /* 0x3C */ EntryInfo bossCheckInfo;
    /* 0x40 */ EntryInfo hiddenSkillCheckInfo;
    /* 0x44 */ EntryInfo bugRewardCheckInfo;
    /* 0x48 */ EntryInfo eventItemCheckInfo;
    /* 0x4C */ EntryInfo startingItemInfo;
    /* 0x50 */ u16 maloShopDonationAmount;
    /* 0x52 */ u8 castleRequirements;
    /* 0x53 */ u8 palaceRequirements;
    /* 0x54 */ u8 mapClearBits;
    /* 0x55 */ u8 damageMagnification;
    /* 0x56 */ u8 totSwordRequirement;
    /* 0x57 */ u8 mirrorChamberEntrance;
    /* 0x58 */ u8 barrierReqCount; // See below for notes
    /* 0x59 */ u8 hcBkRequirement;
    /* 0x5A */ u8 hcBkReqCount; // See below for notes  
    /* 0x5B */ u8 padding;
    /* 0x5C */ u16 smallWalletMax;
    /* 0x5E */ u16 bigWalletMax;
    /* 0x60 */ u16 giantWalletMax;
    /* 0x62 */ u16 padding2;
    /* 0x64 */ GXColorS10 l_normalColor;
    /* 0x6C */ GXColor l_normalKColor;
    /* 0x70 */ GXColor l_normalKColor2;
    /* 0x74 */ GXColorS10 l_bigColor;
    /* 0x7C */ GXColor l_bigKColor;
    /* 0x80 */ GXColor l_lNormalKColor;
    /* 0x84 */ GXColor l_lNormalKColor2;
    /* 0x88 */ GXColorS10 l_lBigColor;
    /* 0x90 */ GXColor l_lBigKColor2;
    /* 0x94 */ GXColor wolfDomeAttackWave1Color;
    /* 0x98 */ GXColor wolfDomeAttackWave2Color;
    /* 0x9C */ GXColor lightSwordGlowColor;
    /* 0xA0 */ uint lanternColor;
    /* 0xA4 */ EntranceInfo spawnInfo;
    /* 0xA8 */ EntryInfo shopItemCheckInfo;
    /* 0xAC */ u16 returnPlaceSectionOffset;
};

class seedInfo_c {
    public:
    
        int _create();
        int _delete();
        void initSeed();
        void setStaticGameValues();
    
        const seedHeaderInfo_c* getHeaderPtr() const { return m_Header; }
        bool seedIsLoaded() const { return m_GCIData; }

        const BossCheck* getBossChecksPtr() const { return m_BossChecks; }
        const HiddenSkillCheck* getHiddenSkillChecksPtr() const { return m_HiddenSkillChecks; }
        const BugReward* getBugRewardChecksPtr() const { return m_BugRewardChecks; }
        const PoeReward* getPoeRewardsPtr() const { return m_PoeRewards; }
        const EventItem* getEventChecksPtr() const { return m_EventChecks; }
        const ReturnPlaceSection* getReturnPlaceSectionPtr() const { return m_ReturnPlaceSection;}
        const RawRGBTable* getRawRGBTablePtr() const { return m_RawRGBTable; }

        bool flagBitfieldFlagIsEnabled(uint flag) const;

        bool canTransformAnywhere() const { return flagBitfieldFlagIsEnabled(TRANSFORM_ANYWHERE); }
        bool canQuickTransform() const { return flagBitfieldFlagIsEnabled(QUICK_TRANSFORM); }
        bool bonksDoDamage() const { return flagBitfieldFlagIsEnabled(BONKS_DO_DAMAGE); }
        bool walletsAreAutoFilled() const { return flagBitfieldFlagIsEnabled(AUTOFILL_WALLETS); }
        bool shopModelsAreModified() const { return flagBitfieldFlagIsEnabled(MODIFY_SHOP_MODELS); }
        bool isLanternRainbow() const { return flagBitfieldFlagIsEnabled(RAINBOW_LANTERN); }
        bool isMidnaHairRainbow() const { return flagBitfieldFlagIsEnabled(RAINBOW_MIDNA); }
        bool isLightSwordRainbow() const { return flagBitfieldFlagIsEnabled(RAINBOW_LIGHT_SWORD); }
        bool isLightSwordAlwaysOn() const { return flagBitfieldFlagIsEnabled(LIGHT_SWORD_ALWAYS_ON); }
        bool isWolfDomeRainbow() const { return flagBitfieldFlagIsEnabled(RAINBOW_LOCK_DOME); }
        bool returnRupeeToChest() const { return flagBitfieldFlagIsEnabled(RETURN_MONEY_TO_CHEST);}
        bool skipMinorCutscenes() const { return flagBitfieldFlagIsEnabled(SKIP_MINOR_CUTSCENES);}
        bool isMapOpen() const { return flagBitfieldFlagIsEnabled(OPEN_MAP);}
        bool removeIBLimit() const { return flagBitfieldFlagIsEnabled(REMOVE_IB_LIMIT);}
        bool isDisableBattleMusic() const { return flagBitfieldFlagIsEnabled(DISABLE_BATTLE_MUSIC);}
        bool isInstantText() const { return flagBitfieldFlagIsEnabled(SET_INSTANT_TEXT);}
        bool skipMajorCutscenes() const { return flagBitfieldFlagIsEnabled(SKIP_MAJOR_CUTSCENES);}
        bool invertCameraAxis() const { return flagBitfieldFlagIsEnabled(INVERT_CAMERA_AXIS);}
        bool isLightSwordGlow() const { return flagBitfieldFlagIsEnabled(MAKE_LIGHT_SWORD_GLOW);}

        bool spinnerSpeedIsIncreased() const
        {
            return flagBitfieldFlagIsEnabled(INCREASE_SPINNER_SPEED);
        }

        void loadShopModels();
        void loadShuffledEntrances();
        void handleReturnToLocation(bool isReturnToDungeonEntrance);
        void loadBugRewards();
        
       private:
        void applyEventFlags();
        void applyRegionFlags();
        void giveStartingItems();
        void applySeedPatches();

        const seedHeaderInfo_c* m_Header;
        const u8* m_GCIData;

        const BossCheck* m_BossChecks;
        const HiddenSkillCheck* m_HiddenSkillChecks;
        const BugReward* m_BugRewardChecks;
        const PoeReward* m_PoeRewards;
        const EventItem* m_EventChecks;
        const ReturnPlaceSection* m_ReturnPlaceSection;
        const RawRGBTable* m_RawRGBTable;
    };

extern seedInfo_c g_seedInfo;

#endif  // SEED_H
