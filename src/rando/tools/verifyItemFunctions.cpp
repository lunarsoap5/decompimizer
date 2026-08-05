#include "rando/tools/verifyItemFunctions.h"
#include "d/d_com_inf_game.h"
#include "d/actor/d_a_alink.h"
#include "d/d_item.h"

bool haveItem(uint item)
{
    return checkItemGet((u8)item, 1);
}

uint getProgressiveSword()
{
    static const u8 progressiveItemsList[] = {fpcNm_ITEM_WOOD_STICK, fpcNm_ITEM_SWORD, fpcNm_ITEM_MASTER_SWORD};

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
    return fpcNm_ITEM_LIGHT_SWORD;
};

uint getProgressiveBow()
{
    static const u8 progressiveItemsList[] = {fpcNm_ITEM_BOW, fpcNm_ITEM_ARROW_LV2};

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
    return fpcNm_ITEM_ARROW_LV3;
};

uint getProgressiveSkill()
{
    static const u8 progressiveItemsList[] = {fpcNm_ITEM_ENDING_BLOW, fpcNm_ITEM_SHIELD_ATTACK, fpcNm_ITEM_BACK_SLICE, fpcNm_ITEM_HELM_SPLITTER, fpcNm_ITEM_MORTAL_DRAW, fpcNm_ITEM_JUMP_STRIKE};

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
    return fpcNm_ITEM_GREAT_SPIN;
};

uint getProgressiveSkybook()
{
    if (!haveItem(fpcNm_ITEM_ANCIENT_DOCUMENT2))
    {
        if (haveItem(fpcNm_ITEM_ANCIENT_DOCUMENT))
        {
            if (dComIfGs_getAncientDocumentNum() != 5)
            {
                return fpcNm_ITEM_AIR_LETTER;
            }
        }
        else
        {
            return fpcNm_ITEM_ANCIENT_DOCUMENT;
        }
    }

    // All previous obtained, so return last upgrade
    return fpcNm_ITEM_ANCIENT_DOCUMENT2;
};

uint getProgressiveKeyShard()
{
    static const u8 progressiveItemsList[] = {fpcNm_ITEM_L2_KEY_PIECES1, fpcNm_ITEM_L2_KEY_PIECES2};

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
    return fpcNm_ITEM_LV2_BOSS_KEY;
};

uint getProgressiveMirrorShard()
{
    static const u8 progressiveItemsList[] = {fpcNm_ITEM_MIRROR_PIECE_1, fpcNm_ITEM_MIRROR_PIECE_2, fpcNm_ITEM_MIRROR_PIECE_3};

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
    return fpcNm_ITEM_MIRROR_PIECE_4;
};

uint getProgressiveFusedShadow()
{
    static const u8 progressiveItemsList[] = {fpcNm_ITEM_FUSED_SHADOW_1, fpcNm_ITEM_FUSED_SHADOW_2};

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
    return fpcNm_ITEM_FUSED_SHADOW_3;
};

u8 getWarashibeItemCount()
{
    static const u8 itemsList[] = {fpcNm_ITEM_LETTER, fpcNm_ITEM_BILL, fpcNm_ITEM_WOOD_STATUE, fpcNm_ITEM_IRIAS_PENDANT, fpcNm_ITEM_HORSE_FLUTE};
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
        case fpcNm_ITEM_WOOD_STICK:
        case fpcNm_ITEM_SWORD:
        case fpcNm_ITEM_MASTER_SWORD:
        case fpcNm_ITEM_LIGHT_SWORD:
        {
            item = getProgressiveSword();
            break;
        }
        case fpcNm_ITEM_BOW:
        case fpcNm_ITEM_ARROW_LV2:
        case fpcNm_ITEM_ARROW_LV3:
        {
            item = getProgressiveBow();
            break;
        }
        case fpcNm_ITEM_WALLET_LV2:
        case fpcNm_ITEM_WALLET_LV3:
        {
            if (haveItem(fpcNm_ITEM_WALLET_LV2))
            {
                item = fpcNm_ITEM_WALLET_LV3;
            }
            else
            {
                item = fpcNm_ITEM_WALLET_LV2;
            }
            break;
        }
        case fpcNm_ITEM_ENDING_BLOW:
        case fpcNm_ITEM_SHIELD_ATTACK:
        case fpcNm_ITEM_BACK_SLICE:
        case fpcNm_ITEM_HELM_SPLITTER:
        case fpcNm_ITEM_MORTAL_DRAW:
        case fpcNm_ITEM_JUMP_STRIKE:
        case fpcNm_ITEM_GREAT_SPIN:
        {
            item = getProgressiveSkill();
            break;
        }
        case fpcNm_ITEM_HOOKSHOT:
        case fpcNm_ITEM_W_HOOKSHOT:
        {
            // If we have either clawshot, we want to return the double no matter what.
            // We check for both in this case because the game unsets the clawshot flag once the double has been obtained.
            if (haveItem(fpcNm_ITEM_HOOKSHOT) || haveItem(fpcNm_ITEM_W_HOOKSHOT))
            {
                item = fpcNm_ITEM_W_HOOKSHOT;
            }
            else
            {
                item = fpcNm_ITEM_HOOKSHOT;
            }
            break;
        }
        case fpcNm_ITEM_ANCIENT_DOCUMENT:
        case fpcNm_ITEM_AIR_LETTER:
        case fpcNm_ITEM_ANCIENT_DOCUMENT2:
        {
            item = getProgressiveSkybook();
            break;
        }
        case fpcNm_ITEM_L2_KEY_PIECES1:
        case fpcNm_ITEM_L2_KEY_PIECES2:
        case fpcNm_ITEM_LV2_BOSS_KEY:
        {
            item = getProgressiveKeyShard();
            break;
        }
        case fpcNm_ITEM_COPY_ROD:
        case fpcNm_ITEM_COPY_ROD_2:
        {
            if (haveItem(fpcNm_ITEM_COPY_ROD))
            {
                item = fpcNm_ITEM_COPY_ROD_2;
            }
            else
            {
                item = fpcNm_ITEM_COPY_ROD;
            }
            break;
        }
        case fpcNm_ITEM_FISHING_ROD_1:
        case fpcNm_ITEM_ZORAS_JEWEL:
        {
            if (haveItem(fpcNm_ITEM_FISHING_ROD_1))
            {
                item = fpcNm_ITEM_ZORAS_JEWEL;
            }
            else
            {
                item = fpcNm_ITEM_FISHING_ROD_1;
            }
            break;
        }
        case fpcNm_ITEM_MIRROR_PIECE_1:
        case fpcNm_ITEM_MIRROR_PIECE_2:
        case fpcNm_ITEM_MIRROR_PIECE_3:
        case fpcNm_ITEM_MIRROR_PIECE_4:
        {
            item = getProgressiveMirrorShard();
            break;
        }
        case fpcNm_ITEM_FUSED_SHADOW_1:
        case fpcNm_ITEM_FUSED_SHADOW_2:
        case fpcNm_ITEM_FUSED_SHADOW_3:
        {
            item = getProgressiveFusedShadow();
            break;
        }
        case fpcNm_ITEM_ARROW_10:
        case fpcNm_ITEM_ARROW_20:
        case fpcNm_ITEM_ARROW_30:
        {
            if (!haveItem(fpcNm_ITEM_BOW))
            {
                item = fpcNm_ITEM_BLUE_RUPEE;
            }
            break;
        }
        case fpcNm_ITEM_BOMB_5:
        case fpcNm_ITEM_BOMB_10:
        case fpcNm_ITEM_BOMB_20:
        case fpcNm_ITEM_BOMB_30:
        case fpcNm_ITEM_WATER_BOMB_5:
        case fpcNm_ITEM_WATER_BOMB_10:
        case fpcNm_ITEM_WATER_BOMB_20:
        case fpcNm_ITEM_WATER_BOMB_30:
        case fpcNm_ITEM_BOMB_INSECT_5:
        case fpcNm_ITEM_BOMB_INSECT_10:
        case fpcNm_ITEM_BOMB_INSECT_20:
        case fpcNm_ITEM_BOMB_INSECT_30:
        {
            if (!haveItem(fpcNm_ITEM_BOMB_IN_BAG))
            {
                item = fpcNm_ITEM_BLUE_RUPEE;
            }
            break;
        }
        case fpcNm_ITEM_PACHINKO_SHOT:
        {
            if (!haveItem(fpcNm_ITEM_PACHINKO))
            {
                item = fpcNm_ITEM_BLUE_RUPEE;
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
