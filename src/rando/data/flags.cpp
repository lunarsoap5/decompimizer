#include "rando/data/flags.h"
#include "rando/data/stages.h"
#include "rando/tools/tools.h"

GoldenWolfFlags getCurrentGoldenWolfFlags(u8 roomNo)
{
    GoldenWolfFlags flags;

    switch (getCurrentStageID())
    {
        case Ordon_Spring:
            flags.mapMarkerFlag = 0x41;
            flags.howledAtStoneFlag = HOWLED_AT_DEATH_MOUNTAIN_STONE;
            flags.obtainedItemFlag = GOT_SKILL_FROM_ORDON_WOLF;
            break;
        case Faron_Woods:
            flags.mapMarkerFlag = 0xFF;
            flags.howledAtStoneFlag = 0xFFFF;
            flags.obtainedItemFlag = 0x3C10; // Custom flag for rando
            break;
        case Kakariko_Graveyard:
            flags.mapMarkerFlag = 0x79;
            flags.howledAtStoneFlag = HOWLED_AT_SNOWPEAK_STONE;
            flags.obtainedItemFlag = GOT_SKILL_FROM_GRAVEYARD_WOLF;
            break;
        case Outside_Castle_Town:
            if (roomNo == 8)
            {
                flags.mapMarkerFlag = 0x29;
                flags.howledAtStoneFlag = HOWLED_AT_UPPER_ZORAS_RIVER_STONE;
                flags.obtainedItemFlag = GOT_SKILL_FROM_WEST_CT_WOLF;
            }
            else
            {
                flags.mapMarkerFlag = 0x2A;
                flags.howledAtStoneFlag = HOWLED_AT_SACRED_GROVE_OUTSIDE_STONE;
                flags.obtainedItemFlag = GOT_SKILL_FROM_SOUTH_CT_FIELD_WOLF;
            }
            break;
        case Castle_Town:
            flags.mapMarkerFlag = 0x32;
            flags.howledAtStoneFlag = HOWLED_AT_HIDDEN_VILLAGE_STONE;
            flags.obtainedItemFlag = GOT_SKILL_FROM_BARRIER_WOLF;
            break;
        case Gerudo_Desert:
            flags.mapMarkerFlag = 0x32;
            flags.howledAtStoneFlag = HOWLED_AT_LAKE_HYLIA_STONE;
            flags.obtainedItemFlag = GOT_SKILL_FROM_BULBLIN_CAMP_WOLF;
            break;
        default:
            flags.mapMarkerFlag = 0xFF;
            flags.howledAtStoneFlag = 0xFFFF;
            flags.obtainedItemFlag = 0xFFFF;
            break;
    }

    return flags;
}