#include "rando/tools/verifyItemFunctions.h"
#include "d/actor/d_a_alink.h"
#include "d/d_com_inf_game.h"
#include "d/d_item.h"

bool haveItem(uint item)
{
    return checkItemGet((u8)item, 1);
}

bool isMajorItem(uint item)
{
    switch (item)
    {
        case dItemNo_WOOD_STICK_e:
        case dItemNo_SHIELD_e:
        case dItemNo_WOOD_SHIELD_e:
        case dItemNo_HYLIA_SHIELD_e:
        case dItemNo_WALLET_LV2_e: // Progressive Wallet
        case dItemNo_BOOMERANG_e:
        case dItemNo_KANTERA_e:
        case dItemNo_PACHINKO_e:
        case dItemNo_FISHING_ROD_1_e:
        case dItemNo_BOW_e:
        case dItemNo_BOMB_IN_BAG_e:
        case dItemNo_WEAR_ZORA_e:
        case dItemNo_HOOKSHOT_e:
        case dItemNo_RAFRELS_MEMO_e:
        case dItemNo_ASHS_SCRIBBLING_e:
        case dItemNo_SPINNER_e:
        case dItemNo_COPY_ROD_e:
        case dItemNo_ANCIENT_DOCUMENT_e:
        case dItemNo_LETTER_e:
        case dItemNo_BILL_e:
        case dItemNo_WOOD_STATUE_e:
        case dItemNo_IRIAS_PENDANT_e:
        case dItemNo_HORSE_FLUTE_e:
        case dItemNo_ENDING_BLOW_e:
        case dItemNo_EMPTY_BOTTLE_e:
        case dItemNo_HALF_MILK_BOTTLE_e:
        case dItemNo_OIL_BOTTLE3_e:
        case dItemNo_DROP_BOTTLE_e:
        case dItemNo_MAGIC_LV1_e:
        case dItemNo_IRONBALL_e:
        {
            return true;
        }
        default:
        {
            return false;
        }
    }
}

bool isKeyItem(uint item)
{
    switch (item)
    {
        case dItemNo_FOREST_SMALL_KEY_e:
        case dItemNo_MINES_SMALL_KEY_e:
        case dItemNo_LAKEBED_SMALL_KEY_e:
        case dItemNo_ARBITERS_SMALL_KEY_e:
        case dItemNo_SNOWPEAK_SMALL_KEY_e:
        case dItemNo_TEMPLE_OF_TIME_SMALL_KEY_e:
        case dItemNo_CITY_SMALL_KEY_e:
        case dItemNo_PALACE_SMALL_KEY_e:
        case dItemNo_HYRULE_SMALL_KEY_e:
        case dItemNo_FOREST_BOSS_KEY_e:
        case dItemNo_L2_KEY_PIECES1_e:
        case dItemNo_LAKEBED_BOSS_KEY_e:
        case dItemNo_ARBITERS_BOSS_KEY_e:
        case dItemNo_LV5_BOSS_KEY_e:
        case dItemNo_TEMPLE_OF_TIME_BOSS_KEY_e:
        case dItemNo_CITY_BOSS_KEY_e:
        case dItemNo_PALACE_BOSS_KEY_e:
        case dItemNo_HYRULE_BOSS_KEY_e:
        case dItemNo_KEY_OF_CARAVAN_e:
        case dItemNo_CAMP_SMALL_KEY_e:
        case dItemNo_KEY_OF_FILONE_e:
        case dItemNo_SMALL_KEY2_e:
        case dItemNo_TOMATO_PUREE_e:
        case dItemNo_TASTE_e:

        {
            return true;
        }
        default:
        {
            return false;
        }
    }
}

uint getProgressiveSword()
{
    static const u8 progressiveItemsList[] = {dItemNo_WOOD_STICK_e, dItemNo_SWORD_e, dItemNo_MASTER_SWORD_e};

    uint listLength = sizeof(progressiveItemsList) / sizeof(progressiveItemsList[0]);
    for (int i = 0; i < listLength; i++)
    {
        const uint item = progressiveItemsList[i];
        if (!haveItem(item))
        {
            return item;
        }
    }

    // All previous obtained, so return last upgrade
    return dItemNo_LIGHT_SWORD_e;
};

uint getProgressiveBow()
{
    static const u8 progressiveItemsList[] = {dItemNo_BOW_e, dItemNo_ARROW_LV2_e};

    uint listLength = sizeof(progressiveItemsList) / sizeof(progressiveItemsList[0]);
    for (int i = 0; i < listLength; i++)
    {
        const uint item = progressiveItemsList[i];
        if (!haveItem(item))
        {
            return item;
        }
    }

    // All previous obtained, so return last upgrade
    return dItemNo_ARROW_LV3_e;
};

uint getProgressiveSkill()
{
    static const u8 progressiveItemsList[] = {dItemNo_ENDING_BLOW_e,
                                              dItemNo_SHIELD_ATTACK_e,
                                              dItemNo_BACK_SLICE_e,
                                              dItemNo_HELM_SPLITTER_e,
                                              dItemNo_MORTAL_DRAW_e,
                                              dItemNo_JUMP_STRIKE_e};

    uint listLength = sizeof(progressiveItemsList) / sizeof(progressiveItemsList[0]);
    for (int i = 0; i < listLength; i++)
    {
        const uint item = progressiveItemsList[i];
        if (!haveItem(item))
        {
            return item;
        }
    }

    // All previous obtained, so return last upgrade
    return dItemNo_GREAT_SPIN_e;
};

uint getProgressiveSkybook()
{
    if (!haveItem(dItemNo_ANCIENT_DOCUMENT2_e))
    {
        if (haveItem(dItemNo_ANCIENT_DOCUMENT_e))
        {
            if (dComIfGs_getAncientDocumentNum() != 5)
            {
                return dItemNo_AIR_LETTER_e;
            }
        }
        else
        {
            return dItemNo_ANCIENT_DOCUMENT_e;
        }
    }

    // All previous obtained, so return last upgrade
    return dItemNo_ANCIENT_DOCUMENT2_e;
};

uint getProgressiveKeyShard()
{
    static const u8 progressiveItemsList[] = {dItemNo_L2_KEY_PIECES1_e, dItemNo_L2_KEY_PIECES2_e};

    uint listLength = sizeof(progressiveItemsList) / sizeof(progressiveItemsList[0]);
    for (int i = 0; i < listLength; i++)
    {
        const uint item = progressiveItemsList[i];
        if (!haveItem(item))
        {
            return item;
        }
    }

    // All previous obtained, so return last upgrade
    return dItemNo_LV2_BOSS_KEY_e;
};

uint getProgressiveMirrorShard()
{
    static const u8 progressiveItemsList[] = {dItemNo_MIRROR_PIECE_1_e, dItemNo_MIRROR_PIECE_2_e, dItemNo_MIRROR_PIECE_3_e};

    uint listLength = sizeof(progressiveItemsList) / sizeof(progressiveItemsList[0]);
    for (int i = 0; i < listLength; i++)
    {
        const uint item = progressiveItemsList[i];
        if (!haveItem(item))
        {
            return item;
        }
    }

    // All previous obtained, so return last upgrade
    return dItemNo_MIRROR_PIECE_4_e;
};

uint getProgressiveFusedShadow()
{
    static const u8 progressiveItemsList[] = {dItemNo_FUSED_SHADOW_1_e, dItemNo_FUSED_SHADOW_2_e};

    uint listLength = sizeof(progressiveItemsList) / sizeof(progressiveItemsList[0]);
    for (int i = 0; i < listLength; i++)
    {
        const uint item = progressiveItemsList[i];
        if (!haveItem(item))
        {
            return item;
        }
    }

    // All previous obtained, so return last upgrade
    return dItemNo_FUSED_SHADOW_3_e;
};

u8 getWarashibeItemCount()
{
    static const u8 itemsList[] = {dItemNo_LETTER_e,
                                   dItemNo_BILL_e,
                                   dItemNo_WOOD_STATUE_e,
                                   dItemNo_IRIAS_PENDANT_e,
                                   dItemNo_HORSE_FLUTE_e};
    u8 count = 0;

    uint listLength = sizeof(itemsList) / sizeof(itemsList[0]);
    for (int i = 0; i < listLength; i++)
    {
        const uint item = itemsList[i];
        if (haveItem(item))
        {
            count++;
        }
    }
    return count;
};

uint verifyProgressiveItem(uint item)
{
    switch (item)
    {
        case dItemNo_WOOD_STICK_e:
        case dItemNo_SWORD_e:
        case dItemNo_MASTER_SWORD_e:
        case dItemNo_LIGHT_SWORD_e:
        {
            item = getProgressiveSword();
            break;
        }
        case dItemNo_BOW_e:
        case dItemNo_ARROW_LV2_e:
        case dItemNo_ARROW_LV3_e:
        {
            item = getProgressiveBow();
            break;
        }
        case dItemNo_WALLET_LV2_e:
        case dItemNo_WALLET_LV3_e:
        {
            if (haveItem(dItemNo_WALLET_LV2_e))
            {
                item = dItemNo_WALLET_LV3_e;
            }
            else
            {
                item = dItemNo_WALLET_LV2_e;
            }
            break;
        }
        case dItemNo_ENDING_BLOW_e:
        case dItemNo_SHIELD_ATTACK_e:
        case dItemNo_BACK_SLICE_e:
        case dItemNo_HELM_SPLITTER_e:
        case dItemNo_MORTAL_DRAW_e:
        case dItemNo_JUMP_STRIKE_e:
        case dItemNo_GREAT_SPIN_e:
        {
            item = getProgressiveSkill();
            break;
        }
        case dItemNo_HOOKSHOT_e:
        case dItemNo_W_HOOKSHOT_e:
        {
            // If we have either clawshot, we want to return the double no matter what.
            // We check for both in this case because the game unsets the clawshot flag once the double
            // has been obtained.
            if (haveItem(dItemNo_HOOKSHOT_e) || haveItem(dItemNo_W_HOOKSHOT_e))
            {
                item = dItemNo_W_HOOKSHOT_e;
            }
            else
            {
                item = dItemNo_HOOKSHOT_e;
            }
            break;
        }
        case dItemNo_ANCIENT_DOCUMENT_e:
        case dItemNo_AIR_LETTER_e:
        case dItemNo_ANCIENT_DOCUMENT2_e:
        {
            item = getProgressiveSkybook();
            break;
        }
        case dItemNo_L2_KEY_PIECES1_e:
        case dItemNo_L2_KEY_PIECES2_e:
        case dItemNo_LV2_BOSS_KEY_e:
        {
            item = getProgressiveKeyShard();
            break;
        }
        case dItemNo_COPY_ROD_e:
        case dItemNo_COPY_ROD_2_e:
        {
            if (haveItem(dItemNo_COPY_ROD_e))
            {
                item = dItemNo_COPY_ROD_2_e;
            }
            else
            {
                item = dItemNo_COPY_ROD_e;
            }
            break;
        }
        case dItemNo_FISHING_ROD_1_e:
        case dItemNo_ZORAS_JEWEL_e:
        {
            if (haveItem(dItemNo_FISHING_ROD_1_e))
            {
                item = dItemNo_ZORAS_JEWEL_e;
            }
            else
            {
                item = dItemNo_FISHING_ROD_1_e;
            }
            break;
        }
        case dItemNo_MIRROR_PIECE_1_e:
        case dItemNo_MIRROR_PIECE_2_e:
        case dItemNo_MIRROR_PIECE_3_e:
        case dItemNo_MIRROR_PIECE_4_e:
        {
            item = getProgressiveMirrorShard();
            break;
        }
        case dItemNo_FUSED_SHADOW_1_e:
        case dItemNo_FUSED_SHADOW_2_e:
        case dItemNo_FUSED_SHADOW_3_e:
        {
            item = getProgressiveFusedShadow();
            break;
        }
        case dItemNo_ARROW_10_e:
        case dItemNo_ARROW_20_e:
        case dItemNo_ARROW_30_e:
        {
            if (!haveItem(dItemNo_BOW_e))
            {
                item = dItemNo_BLUE_RUPEE_e;
            }
            break;
        }
        case dItemNo_BOMB_5_e:
        case dItemNo_BOMB_10_e:
        case dItemNo_BOMB_20_e:
        case dItemNo_BOMB_30_e:
        case dItemNo_WATER_BOMB_5_e:
        case dItemNo_WATER_BOMB_10_e:
        case dItemNo_WATER_BOMB_20_e:
        case dItemNo_WATER_BOMB_30_e:
        case dItemNo_BOMB_INSECT_5_e:
        case dItemNo_BOMB_INSECT_10_e:
        case dItemNo_BOMB_INSECT_20_e:
        case dItemNo_BOMB_INSECT_30_e:
        {
            if (!haveItem(dItemNo_BOMB_IN_BAG_e))
            {
                item = dItemNo_BLUE_RUPEE_e;
            }
            break;
        }
        case dItemNo_PACHINKO_SHOT_e:
        {
            if (!haveItem(dItemNo_PACHINKO_e))
            {
                item = dItemNo_BLUE_RUPEE_e;
            }
            break;
        }
        default:
        {
            break;
        }
    }
    return item;
}
