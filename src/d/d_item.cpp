/**
 * d_item.cpp
 * Item Get Handling
 */

#include "d/dolzel.h" // IWYU pragma: keep

#include "d/d_item.h"
#include "d/d_com_inf_game.h"
#include "d/d_meter2_info.h"
#include "rando/tools/verifyItemFunctions.h"
#include "rando/rando.h"
#include "rando/seed/seed.h"
#include "rando/data/flags.h"

static void (*item_func_ptr[256])() = {
    item_func_HEART,
    item_func_GREEN_RUPEE,
    item_func_BLUE_RUPEE,
    item_func_YELLOW_RUPEE,
    item_func_RED_RUPEE,
    item_func_PURPLE_RUPEE,
    item_func_ORANGE_RUPEE,
    item_func_SILVER_RUPEE,
    item_func_S_MAGIC,
    item_func_L_MAGIC,
    item_func_BOMB_5,
    item_func_BOMB_10,
    item_func_BOMB_20,
    item_func_BOMB_30,
    item_func_ARROW_10,
    item_func_ARROW_20,
    item_func_ARROW_30,
    item_func_ARROW_1,
    item_func_PACHINKO_SHOT,
    item_func_FOOLISH_ITEM,
    item_func_ORDON_PORTAL,
    item_func_SOUTH_FARON_PORTAL,
    item_func_WATER_BOMB_5,
    item_func_WATER_BOMB_10,
    item_func_WATER_BOMB_20,
    item_func_WATER_BOMB_30,
    item_func_BOMB_INSECT_5,
    item_func_BOMB_INSECT_10,
    item_func_BOMB_INSECT_20,
    item_func_BOMB_INSECT_30,
    item_func_RECOVER_FAILY,
    item_func_TRIPLE_HEART,
    item_func_SMALL_KEY,
    item_func_KAKERA_HEART,
    item_func_UTUWA_HEART,
    item_func_MAP,
    item_func_COMPUS,
    item_func_DUNGEON_EXIT,
    item_func_BOSS_KEY,
    item_func_DUNGEON_BACK,
    item_func_SWORD,
    item_func_MASTER_SWORD,
    item_func_WOOD_SHIELD,
    item_func_SHIELD,
    item_func_HYLIA_SHIELD,
    item_func_TKS_LETTER,
    item_func_WEAR_CASUAL,
    item_func_WEAR_KOKIRI,
    item_func_ARMOR,
    item_func_WEAR_ZORA,
    item_func_MAGIC_LV1,
    item_func_DUNGEON_EXIT_2,
    item_func_WALLET_LV1,
    item_func_WALLET_LV2,
    item_func_WALLET_LV3,
    item_func_noentry,
    item_func_noentry,
    item_func_UPPER_ZORAS_RIVER_PORTAL,
    item_func_CASTLE_TOWN_PORTAL,
    item_func_GERUDO_DESERT_PORTAL,
    item_func_NORTH_FARON_PORTAL,
    item_func_ZORAS_JEWEL,
    item_func_HAWK_EYE,
    item_func_WOOD_STICK,
    item_func_BOOMERANG,
    item_func_SPINNER,
    item_func_IRONBALL,
    item_func_BOW,
    item_func_HOOKSHOT,
    item_func_HVY_BOOTS,
    item_func_COPY_ROD,
    item_func_W_HOOKSHOT,
    item_func_KANTERA,
    item_func_LIGHT_SWORD,
    item_func_FISHING_ROD_1,
    item_func_PACHINKO,
    item_func_COPY_ROD_2,
    item_func_KAKARIKO_GORGE_PORTAL,
    item_func_KAKARIKO_VILLAGE_PORTAL,
    item_func_BOMB_BAG_LV2,
    item_func_BOMB_BAG_LV1,
    item_func_BOMB_IN_BAG,
    item_func_DEATH_MOUNTAIN_PORTAL,
    item_func_LIGHT_ARROW,
    item_func_ARROW_LV1,
    item_func_ARROW_LV2,
    item_func_ARROW_LV3,
    item_func_ZORAS_DOMAIN_PORTAL,
    item_func_LURE_ROD,
    item_func_BOMB_ARROW,
    item_func_HAWK_ARROW,
    item_func_BEE_ROD,
    item_func_JEWEL_ROD,
    item_func_WORM_ROD,
    item_func_JEWEL_BEE_ROD,
    item_func_JEWEL_WORM_ROD,
    item_func_EMPTY_BOTTLE,
    item_func_RED_BOTTLE,
    item_func_GREEN_BOTTLE,
    item_func_BLUE_BOTTLE,
    item_func_MILK_BOTTLE,
    item_func_HALF_MILK_BOTTLE,
    item_func_OIL_BOTTLE,
    item_func_WATER_BOTTLE,
    item_func_OIL_BOTTLE2,
    item_func_RED_BOTTLE2,
    item_func_UGLY_SOUP,
    item_func_HOT_SPRING,
    item_func_FAIRY_BOTTLE,
    item_func_HOT_SPRING2,
    item_func_OIL2,
    item_func_OIL,
    item_func_NORMAL_BOMB,
    item_func_WATER_BOMB,
    item_func_POKE_BOMB,
    item_func_FAIRY_DROP,
    item_func_WORM,
    item_func_DROP_BOTTLE,
    item_func_BEE_CHILD,
    item_func_CHUCHU_RARE,
    item_func_CHUCHU_RED,
    item_func_CHUCHU_BLUE,
    item_func_CHUCHU_GREEN,
    item_func_CHUCHU_YELLOW,
    item_func_CHUCHU_PURPLE,
    item_func_LV1_SOUP,
    item_func_LV2_SOUP,
    item_func_LV3_SOUP,
    item_func_LETTER,
    item_func_BILL,
    item_func_WOOD_STATUE,
    item_func_IRIAS_PENDANT,
    item_func_HORSE_FLUTE,
    item_func_FOREST_SMALL_KEY,
    item_func_MINES_SMALL_KEY,
    item_func_LAKEBED_SMALL_KEY,
    item_func_ARBITERS_SMALL_KEY,
    item_func_SNOWPEAK_SMALL_KEY,
    item_func_TEMPLE_OF_TIME_SMALL_KEY,
    item_func_CITY_SMALL_KEY,
    item_func_PALACE_SMALL_KEY,
    item_func_HYRULE_SMALL_KEY,
    item_func_CAMP_SMALL_KEY,
    item_func_LAKE_HYLIA_PORTAL,
    item_func_RAFRELS_MEMO,
    item_func_ASHS_SCRIBBLING,
    item_func_FOREST_BOSS_KEY,
    item_func_LAKEBED_BOSS_KEY,
    item_func_ARBITERS_BOSS_KEY,
    item_func_TEMPLE_OF_TIME_BOSS_KEY,
    item_func_CITY_BOSS_KEY,
    item_func_PALACE_BOSS_KEY,
    item_func_HYRULE_BOSS_KEY,
    item_func_FOREST_COMPASS,
    item_func_MINES_COMPASS,
    item_func_LAKEBED_COMPASS,
    item_func_CHUCHU_YELLOW2,
    item_func_OIL_BOTTLE3,
    item_func_SHOP_BEE_CHILD,
    item_func_CHUCHU_BLACK,
    item_func_LIGHT_DROP,
    item_func_DROP_CONTAINER,
    item_func_DROP_CONTAINER02,
    item_func_DROP_CONTAINER03,
    item_func_FILLED_CONTAINER,
    item_func_MIRROR_PIECE_2,
    item_func_MIRROR_PIECE_3,
    item_func_MIRROR_PIECE_4,
    item_func_ARBITERS_COMPASS,
    item_func_SNOWPEAK_COMPASS,
    item_func_TEMPLE_OF_TIME_COMPASS,
    item_func_CITY_COMPASS,
    item_func_PALACE_COMPASS,
    item_func_HYRULE_COMPASS,
    item_func_MIRROR_CHAMBER_PORTAL,
    item_func_SNOWPEAK_PORTAL,
    item_func_SMELL_YELIA_POUCH,
    item_func_SMELL_PUMPKIN,
    item_func_SMELL_POH,
    item_func_SMELL_FISH,
    item_func_SMELL_CHILDREN,
    item_func_SMELL_MEDICINE,
    item_func_FOREST_MAP,
    item_func_MINES_MAP,
    item_func_LAKEBED_MAP,
    item_func_ARBITERS_MAP,
    item_func_SNOWPEAK_MAP,
    item_func_TEMPLE_OF_TIME_MAP,
    item_func_CITY_MAP,
    item_func_PALACE_MAP,
    item_func_HYRULE_MAP,
    item_func_SACRED_GROVE_PORTAL,
    item_func_M_BEETLE,
    item_func_F_BEETLE,
    item_func_M_BUTTERFLY,
    item_func_F_BUTTERFLY,
    item_func_M_STAG_BEETLE,
    item_func_F_STAG_BEETLE,
    item_func_M_GRASSHOPPER,
    item_func_F_GRASSHOPPER,
    item_func_M_NANAFUSHI,
    item_func_F_NANAFUSHI,
    item_func_M_DANGOMUSHI,
    item_func_F_DANGOMUSHI,
    item_func_M_MANTIS,
    item_func_F_MANTIS,
    item_func_M_LADYBUG,
    item_func_F_LADYBUG,
    item_func_M_SNAIL,
    item_func_F_SNAIL,
    item_func_M_DRAGONFLY,
    item_func_F_DRAGONFLY,
    item_func_M_ANT,
    item_func_F_ANT,
    item_func_M_MAYFLY,
    item_func_F_MAYFLY,
    item_func_FUSED_SHADOW_1,
    item_func_FUSED_SHADOW_2,
    item_func_FUSED_SHADOW_3,
    item_func_MIRROR_PIECE_1,
    item_func_noentry,
    item_func_noentry,
    item_func_noentry,
    item_func_noentry,
    item_func_POU_SPIRIT,
    item_func_ENDING_BLOW,
    item_func_SHIELD_ATTACK,
    item_func_BACK_SLICE,
    item_func_HELM_SPLITTER,
    item_func_MORTAL_DRAW,
    item_func_JUMP_STRIKE,
    item_func_GREAT_SPIN,
    item_func_ELDIN_BRIDGE_PORTAL,
    item_func_ANCIENT_DOCUMENT,
    item_func_AIR_LETTER,
    item_func_ANCIENT_DOCUMENT2,
    item_func_LV7_DUNGEON_EXIT,
    item_func_LINKS_SAVINGS,
    item_func_SMALL_KEY2,
    item_func_POU_FIRE1,
    item_func_POU_FIRE2,
    item_func_POU_FIRE3,
    item_func_POU_FIRE4,
    item_func_BOSSRIDER_KEY,
    item_func_TOMATO_PUREE,
    item_func_TASTE,
    item_func_LV5_BOSS_KEY,
    item_func_SURFBOARD,
    item_func_KANTERA2,
    item_func_L2_KEY_PIECES1,
    item_func_L2_KEY_PIECES2,
    item_func_L2_KEY_PIECES3,
    item_func_KEY_OF_CARAVAN,
    item_func_LV2_BOSS_KEY,
    item_func_KEY_OF_FILONE,
    item_func_noentry,
};

inline void getItemFunc(u8 i_itemNo) {
    dComIfGs_onItemFirstBit(i_itemNo);
    item_func_ptr[i_itemNo]();
}

void execItemGet(u8 i_itemNo) {
    i_itemNo = verifyProgressiveItem(i_itemNo);
    getItemFunc(i_itemNo);
}

static int (*item_getcheck_func_ptr[256])() = {
    item_getcheck_func_HEART,
    item_getcheck_func_GREEN_RUPEE,
    item_getcheck_func_BLUE_RUPEE,
    item_getcheck_func_YELLOW_RUPEE,
    item_getcheck_func_RED_RUPEE,
    item_getcheck_func_PURPLE_RUPEE,
    item_getcheck_func_ORANGE_RUPEE,
    item_getcheck_func_SILVER_RUPEE,
    item_getcheck_func_S_MAGIC,
    item_getcheck_func_L_MAGIC,
    item_getcheck_func_BOMB_5,
    item_getcheck_func_BOMB_10,
    item_getcheck_func_BOMB_20,
    item_getcheck_func_BOMB_30,
    item_getcheck_func_ARROW_10,
    item_getcheck_func_ARROW_20,
    item_getcheck_func_ARROW_30,
    item_getcheck_func_ARROW_1,
    item_getcheck_func_PACHINKO_SHOT,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_WATER_BOMB_5,
    item_getcheck_func_WATER_BOMB_10,
    item_getcheck_func_WATER_BOMB_20,
    item_getcheck_func_WATER_BOMB_30,
    item_getcheck_func_BOMB_INSECT_5,
    item_getcheck_func_BOMB_INSECT_10,
    item_getcheck_func_BOMB_INSECT_20,
    item_getcheck_func_BOMB_INSECT_30,
    item_getcheck_func_RECOVER_FAILY,
    item_getcheck_func_TRIPLE_HEART,
    item_getcheck_func_SMALL_KEY,
    item_getcheck_func_KAKERA_HEART,
    item_getcheck_func_UTUWA_HEART,
    item_getcheck_func_MAP,
    item_getcheck_func_COMPUS,
    item_getcheck_func_DUNGEON_EXIT,
    item_getcheck_func_BOSS_KEY,
    item_getcheck_func_DUNGEON_BACK,
    item_getcheck_func_SWORD,
    item_getcheck_func_MASTER_SWORD,
    item_getcheck_func_WOOD_SHIELD,
    item_getcheck_func_SHIELD,
    item_getcheck_func_HYLIA_SHIELD,
    item_getcheck_func_TKS_LETTER,
    item_getcheck_func_WEAR_CASUAL,
    item_getcheck_func_WEAR_KOKIRI,
    item_getcheck_func_ARMOR,
    item_getcheck_func_WEAR_ZORA,
    item_getcheck_func_MAGIC_LV1,
    item_getcheck_func_DUNGEON_EXIT_2,
    item_getcheck_func_WALLET_LV1,
    item_getcheck_func_WALLET_LV2,
    item_getcheck_func_WALLET_LV3,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_ZORAS_JEWEL,
    item_getcheck_func_HAWK_EYE,
    item_getcheck_func_WOOD_STICK,
    item_getcheck_func_BOOMERANG,
    item_getcheck_func_SPINNER,
    item_getcheck_func_IRONBALL,
    item_getcheck_func_BOW,
    item_getcheck_func_HOOKSHOT,
    item_getcheck_func_HVY_BOOTS,
    item_getcheck_func_COPY_ROD,
    item_getcheck_func_W_HOOKSHOT,
    item_getcheck_func_KANTERA,
    item_getcheck_func_LIGHT_SWORD,
    item_getcheck_func_FISHING_ROD_1,
    item_getcheck_func_PACHINKO,
    item_getcheck_func_COPY_ROD_2,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_BOMB_BAG_LV2,
    item_getcheck_func_BOMB_BAG_LV1,
    item_getcheck_func_BOMB_IN_BAG,
    item_getcheck_func_noentry,
    item_getcheck_func_LIGHT_ARROW,
    item_getcheck_func_ARROW_LV1,
    item_getcheck_func_ARROW_LV2,
    item_getcheck_func_ARROW_LV3,
    item_getcheck_func_noentry,
    item_getcheck_func_LURE_ROD,
    item_getcheck_func_BOMB_ARROW,
    item_getcheck_func_HAWK_ARROW,
    item_getcheck_func_BEE_ROD,
    item_getcheck_func_JEWEL_ROD,
    item_getcheck_func_WORM_ROD,
    item_getcheck_func_JEWEL_BEE_ROD,
    item_getcheck_func_JEWEL_WORM_ROD,
    item_getcheck_func_EMPTY_BOTTLE,
    item_getcheck_func_RED_BOTTLE,
    item_getcheck_func_GREEN_BOTTLE,
    item_getcheck_func_BLUE_BOTTLE,
    item_getcheck_func_MILK_BOTTLE,
    item_getcheck_func_HALF_MILK_BOTTLE,
    item_getcheck_func_OIL_BOTTLE,
    item_getcheck_func_WATER_BOTTLE,
    item_getcheck_func_OIL_BOTTLE2,
    item_getcheck_func_RED_BOTTLE2,
    item_getcheck_func_UGLY_SOUP,
    item_getcheck_func_HOT_SPRING,
    item_getcheck_func_FAIRY_BOTTLE,
    item_getcheck_func_HOT_SPRING2,
    item_getcheck_func_OIL2,
    item_getcheck_func_OIL,
    item_getcheck_func_NORMAL_BOMB,
    item_getcheck_func_WATER_BOMB,
    item_getcheck_func_POKE_BOMB,
    item_getcheck_func_FAIRY_DROP,
    item_getcheck_func_WORM,
    item_getcheck_func_DROP_BOTTLE,
    item_getcheck_func_BEE_CHILD,
    item_getcheck_func_CHUCHU_RARE,
    item_getcheck_func_CHUCHU_RED,
    item_getcheck_func_CHUCHU_BLUE,
    item_getcheck_func_CHUCHU_GREEN,
    item_getcheck_func_CHUCHU_YELLOW,
    item_getcheck_func_CHUCHU_PURPLE,
    item_getcheck_func_LV1_SOUP,
    item_getcheck_func_LV2_SOUP,
    item_getcheck_func_LV3_SOUP,
    item_getcheck_func_LETTER,
    item_getcheck_func_BILL,
    item_getcheck_func_WOOD_STATUE,
    item_getcheck_func_IRIAS_PENDANT,
    item_getcheck_func_HORSE_FLUTE,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_RAFRELS_MEMO,
    item_getcheck_func_ASHS_SCRIBBLING,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_CHUCHU_YELLOW2,
    item_getcheck_func_OIL_BOTTLE3,
    item_getcheck_func_SHOP_BEE_CHILD,
    item_getcheck_func_CHUCHU_BLACK,
    item_getcheck_func_LIGHT_DROP,
    item_getcheck_func_DROP_CONTAINER,
    item_getcheck_func_DROP_CONTAINER02,
    item_getcheck_func_DROP_CONTAINER03,
    item_getcheck_func_FILLED_CONTAINER,
    item_getcheck_func_MIRROR_PIECE_2,
    item_getcheck_func_MIRROR_PIECE_3,
    item_getcheck_func_MIRROR_PIECE_4,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_SMELL_YELIA_POUCH,
    item_getcheck_func_SMELL_PUMPKIN,
    item_getcheck_func_SMELL_POH,
    item_getcheck_func_SMELL_FISH,
    item_getcheck_func_SMELL_CHILDREN,
    item_getcheck_func_SMELL_MEDICINE,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_M_BEETLE,
    item_getcheck_func_F_BEETLE,
    item_getcheck_func_M_BUTTERFLY,
    item_getcheck_func_F_BUTTERFLY,
    item_getcheck_func_M_STAG_BEETLE,
    item_getcheck_func_F_STAG_BEETLE,
    item_getcheck_func_M_GRASSHOPPER,
    item_getcheck_func_F_GRASSHOPPER,
    item_getcheck_func_M_NANAFUSHI,
    item_getcheck_func_F_NANAFUSHI,
    item_getcheck_func_M_DANGOMUSHI,
    item_getcheck_func_F_DANGOMUSHI,
    item_getcheck_func_M_MANTIS,
    item_getcheck_func_F_MANTIS,
    item_getcheck_func_M_LADYBUG,
    item_getcheck_func_F_LADYBUG,
    item_getcheck_func_M_SNAIL,
    item_getcheck_func_F_SNAIL,
    item_getcheck_func_M_DRAGONFLY,
    item_getcheck_func_F_DRAGONFLY,
    item_getcheck_func_M_ANT,
    item_getcheck_func_F_ANT,
    item_getcheck_func_M_MAYFLY,
    item_getcheck_func_F_MAYFLY,
    item_getcheck_func_FUSED_SHADOW_1,
    item_getcheck_func_FUSED_SHADOW_2,
    item_getcheck_func_FUSED_SHADOW_3,
    item_getcheck_func_MIRROR_PIECE_1,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_noentry,
    item_getcheck_func_POU_SPIRIT,
    item_getcheck_func_ENDING_BLOW,
    item_getcheck_func_SHIELD_ATTACK,
    item_getcheck_func_BACK_SLICE,
    item_getcheck_func_HELM_SPLITTER,
    item_getcheck_func_MORTAL_DRAW,
    item_getcheck_func_JUMP_STRIKE,
    item_getcheck_func_GREAT_SPIN,
    item_getcheck_func_noentry,
    item_getcheck_func_ANCIENT_DOCUMENT,
    item_getcheck_func_AIR_LETTER,
    item_getcheck_func_ANCIENT_DOCUMENT2,
    item_getcheck_func_LV7_DUNGEON_EXIT,
    item_getcheck_func_LINKS_SAVINGS,
    item_getcheck_func_SMALL_KEY2,
    item_getcheck_func_POU_FIRE1,
    item_getcheck_func_POU_FIRE2,
    item_getcheck_func_POU_FIRE3,
    item_getcheck_func_POU_FIRE4,
    item_getcheck_func_BOSSRIDER_KEY,
    item_getcheck_func_TOMATO_PUREE,
    item_getcheck_func_TASTE,
    item_getcheck_func_LV5_BOSS_KEY,
    item_getcheck_func_SURFBOARD,
    item_getcheck_func_KANTERA2,
    item_getcheck_func_L2_KEY_PIECES1,
    item_getcheck_func_L2_KEY_PIECES2,
    item_getcheck_func_L2_KEY_PIECES3,
    item_getcheck_func_KEY_OF_CARAVAN,
    item_getcheck_func_LV2_BOSS_KEY,
    item_getcheck_func_KEY_OF_FILONE,
    item_getcheck_func_noentry,
};

inline int getCheckItemFunc(u8 i_itemNo) {
    return item_getcheck_func_ptr[i_itemNo]();
}

int checkItemGet(u8 i_itemNo, int i_default) {
    int result = getCheckItemFunc(i_itemNo);

    if (result == -1) {
        result = i_default;
    }

    return result;
}

void item_func_HEART() {
    dComIfGp_setItemLifeCount(4.0f, 0);
}

void item_func_GREEN_RUPEE() {
    dComIfGp_setItemRupeeCount(1);
}

void item_func_BLUE_RUPEE() {
    dComIfGp_setItemRupeeCount(5);
}

void item_func_YELLOW_RUPEE() {
    dComIfGp_setItemRupeeCount(10);
}

void item_func_RED_RUPEE() {
    dComIfGp_setItemRupeeCount(20);
}

void item_func_PURPLE_RUPEE() {
    dComIfGp_setItemRupeeCount(50);
}

void item_func_ORANGE_RUPEE() {
    dComIfGp_setItemRupeeCount(100);
}

void item_func_SILVER_RUPEE() {
    dComIfGp_setItemRupeeCount(200);
}

void item_func_S_MAGIC() {
    dComIfGp_setItemMagicCount(4);
}

void item_func_L_MAGIC() {
    dComIfGp_setItemMagicCount(8);
}

void item_func_BOMB_5() {
    addBombCount(fpcNm_ITEM_NORMAL_BOMB, 5);
}

void item_func_BOMB_10() {
    addBombCount(fpcNm_ITEM_NORMAL_BOMB, 10);
}

void item_func_BOMB_20() {
    addBombCount(fpcNm_ITEM_NORMAL_BOMB, 20);
}

void item_func_BOMB_30() {
    addBombCount(fpcNm_ITEM_NORMAL_BOMB, 30);
}

void item_func_ARROW_10() {
    dComIfGp_setItemArrowNumCount(10);
}

void item_func_ARROW_20() {
    dComIfGp_setItemArrowNumCount(20);
}

void item_func_ARROW_30() {
    dComIfGp_setItemArrowNumCount(30);
}

void item_func_ARROW_1() {
    dComIfGp_setItemArrowNumCount(1);
}

void item_func_PACHINKO_SHOT() {
    dComIfGp_setItemPachinkoNumCount(50);
}

void item_func_FOOLISH_ITEM() {
    /*
    Pasting rando code until framework is implemented
    // Failsafe: Make sure the count does not somehow exceed 100
        rando::customItems::FoolishItems* foolishItemsPtr = rando::gRandomizer->getFoolishItemsPtr();
        const uint32_t triggerCount = foolishItemsPtr->getTriggerCount();
        if (triggerCount < 100)
        {
            foolishItemsPtr->setTriggerCount(static_cast<uint8_t>(triggerCount + 1));
        }
    */
}

void item_func_ORDON_PORTAL() {
    dComIfGs_onStageSwitch(0x0, 0x34); // Unlock Ordon Portal
}

void item_func_SOUTH_FARON_PORTAL() {
    dComIfGs_onStageSwitch(0x2, 0x47); // Unlock S Faron Portal
}

void item_func_WATER_BOMB_5() {
    addBombCount(fpcNm_ITEM_WATER_BOMB, 5);
}

void item_func_WATER_BOMB_10() {
    addBombCount(fpcNm_ITEM_WATER_BOMB, 10);
}

void item_func_WATER_BOMB_20() {
    addBombCount(fpcNm_ITEM_WATER_BOMB, 15);
}

void item_func_WATER_BOMB_30() {
    addBombCount(fpcNm_ITEM_WATER_BOMB, 3);
}

void item_func_BOMB_INSECT_5() {
    addBombCount(fpcNm_ITEM_POKE_BOMB, 5);
}

void item_func_BOMB_INSECT_10() {
    addBombCount(fpcNm_ITEM_POKE_BOMB, 10);
}

void item_func_BOMB_INSECT_20() {
    addBombCount(fpcNm_ITEM_POKE_BOMB, 3);
}

void item_func_BOMB_INSECT_30() {}

void item_func_RECOVER_FAILY() {
    dComIfGp_setItemLifeCount(32.0f, 0);
}

void item_func_TRIPLE_HEART() {}

void item_func_SMALL_KEY() {
    dComIfGp_setItemKeyNumCount(1);
}

void item_func_KAKERA_HEART() {
    dComIfGp_setItemMaxLifeCount(1);

    u8 maxLife = dComIfGs_getMaxLife() + 1;
    
    // Check if we have enough hearts to break the barrier.
    checkSetHCBarrierFlag(HC_Hearts, maxLife);

    // Check if we have enough hearts to unlock the BK check.
    checkSetHCBkFlag(HC_BK_Hearts, maxLife);
}

void item_func_UTUWA_HEART() {
    dComIfGp_setItemMaxLifeCount(5);

    f32 max_life = dComIfGs_getMaxLifeGauge();
    dComIfGp_setItemLifeCount(max_life, 0);

    stage_stag_info_class* stag_info = dComIfGp_getStageStagInfo();
    int tmp = dStage_stagInfo_GetSaveTbl(stag_info);
    // dComIfGs_onStageLife();

    u8 maxLife = dComIfGs_getMaxLife() + 5;
    
    // Check if we have enough hearts to break the barrier.
    checkSetHCBarrierFlag(HC_Hearts, maxLife);

    // Check if we have enough hearts to unlock the BK check.
    checkSetHCBkFlag(HC_BK_Hearts, maxLife);
}

void item_func_MAP() {
    dComIfGs_onDungeonItemMap();
}

void item_func_COMPUS() {
    dComIfGs_onDungeonItemCompass();
}

void item_func_DUNGEON_EXIT() {
    dComIfGs_onDungeonItemWarp();
    dComIfGs_setItem(SLOT_18, fpcNm_ITEM_DUNGEON_EXIT);
}

void item_func_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey();
}

void item_func_DUNGEON_BACK() {
    dComIfGs_setItem(SLOT_18, fpcNm_ITEM_DUNGEON_BACK);
}

void item_func_SWORD() {
    dComIfGs_setCollectSword(COLLECT_ORDON_SWORD);
    dComIfGs_setSelectEquipSword(fpcNm_ITEM_SWORD);
}

void item_func_MASTER_SWORD() {
    dComIfGs_setCollectSword(COLLECT_MASTER_SWORD);
    dComIfGs_setSelectEquipSword(fpcNm_ITEM_MASTER_SWORD);
}

void item_func_WOOD_SHIELD() {
    dComIfGs_setCollectShield(COLLECT_WOODEN_SHIELD);
    dComIfGs_setSelectEquipShield(fpcNm_ITEM_WOOD_SHIELD);
}

void item_func_SHIELD() {}

void item_func_HYLIA_SHIELD() {}

void item_func_TKS_LETTER() {
    dComIfGs_setItem(SLOT_18, fpcNm_ITEM_TKS_LETTER);
}

void item_func_WEAR_CASUAL() {
    dComIfGs_setSelectEquipClothes(fpcNm_ITEM_WEAR_CASUAL);
}

void item_func_WEAR_KOKIRI() {
    dComIfGs_setCollectClothes(KOKIRI_CLOTHES_FLAG);
    dComIfGs_setSelectEquipClothes(fpcNm_ITEM_WEAR_KOKIRI);
}

void item_func_ARMOR() {}

void item_func_WEAR_ZORA() {}

void item_func_MAGIC_LV1() {
    dComIfGs_onEventBit(0xD04); // Can transform at will
    dComIfGs_onEventBit(0x501); // Midna Charge Unlocked
}

void item_func_DUNGEON_EXIT_2() {
    dComIfGs_setItem(SLOT_18, fpcNm_ITEM_DUNGEON_EXIT);
}

void item_func_WALLET_LV1() {
    dComIfGs_setWalletSize(WALLET);
}

void item_func_WALLET_LV2() {
    dComIfGs_setWalletSize(BIG_WALLET);
    // Putting rando code here until the framework gets built:
    /*
    Basically we fill the wallet if desired and then we change the color of the rupee icon to red
    libtp::tp::J2DWindow::J2DWindow* windowPtr =
            libtp::tp::d_meter2_info::g_meter2_info.mMeterClass->mpMeterDraw->mpBigHeart->mWindow;

        if (seedPtr->walletsAreAutoFilled())
        {
            plrStatusAPtr->currentRupees = mod::user_patch::walletValues[seedPtr->getHeaderPtr()->getWalletSize()][1];
        }

        if (!windowPtr)
        {
            return;
        }

        for (uint32_t rupee = 0x1038; rupee <= 0x1044; rupee += 0x4)
        {
            *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(windowPtr) + rupee) = 0xff0000ff;
        }
    */
}

void item_func_WALLET_LV3() {
    dComIfGs_setWalletSize(GIANT_WALLET);
    // Putting rando code here until the framework gets built:
    /*
    Basically we fill the wallet if desired and then we change the color of the rupee icon to red
    libtp::tp::J2DWindow::J2DWindow* windowPtr =
            libtp::tp::d_meter2_info::g_meter2_info.mMeterClass->mpMeterDraw->mpBigHeart->mWindow;

        if (seedPtr->walletsAreAutoFilled())
        {
            plrStatusAPtr->currentRupees = mod::user_patch::walletValues[seedPtr->getHeaderPtr()->getWalletSize()][2];
        }

        if (!windowPtr)
        {
            return;
        }

        for (uint32_t rupee = 0x1038; rupee <= 0x1044; rupee += 0x4)
        {
            *reinterpret_cast<uint32_t*>(reinterpret_cast<uint32_t>(windowPtr) + rupee) = 0xff00ffff;
        }
    */
}

void item_func_UPPER_ZORAS_RIVER_PORTAL() {
    dComIfGs_onStageSwitch(0x4, 0x17); // Talked to Iza before portal
    dComIfGs_onStageSwitch(0x4, 0x37); // Talked to Iza after portal
    dComIfGs_onStageSwitch(0x4, 0x15); // Unlock UZR Portal
    dComIfGs_onEventBit(0xB80); // Declined to help Iza
    dComIfGs_onEventBit(0x1304); // Talked to Iza before UZR portal
    dComIfGs_onEventBit(0xB02); // Iza 1 Minigame Unlocked
}

void item_func_CASTLE_TOWN_PORTAL() {
    dComIfGs_onStageSwitch(0x6, 0x3); // Unlock Castle Town Portal
}

void item_func_GERUDO_DESERT_PORTAL() {
    dComIfGs_onStageSwitch(0xA, 0x15); // Unlock Desrt Portal
}

void item_func_NORTH_FARON_PORTAL() {
    dComIfGs_onStageSwitch(0x2, 0x2); // Unlock N Faron Portal
}

void item_func_ZORAS_JEWEL() {
    if (item_getcheck_func_FISHING_ROD_1()) {
        dComIfGs_setRodTypeLevelUp();
    } else {
        dComIfGs_setItem(SLOT_20, fpcNm_ITEM_ZORAS_JEWEL);
    }
}

void item_func_HAWK_EYE() {
    dComIfGs_setItem(SLOT_5, fpcNm_ITEM_HAWK_EYE);
}

void item_func_WOOD_STICK() {
    dComIfGs_setCollectSword(COLLECT_WOODEN_SWORD);
    dComIfGs_setSelectEquipSword(fpcNm_ITEM_WOOD_STICK);

    //dComIfGs_onSwitch(28, dComIfGp_roomControl_getStayNo());
}

void item_func_BOOMERANG() {
    dComIfGs_setItem(SLOT_0, fpcNm_ITEM_BOOMERANG);
}

void item_func_SPINNER() {
    dComIfGs_setItem(SLOT_2, fpcNm_ITEM_SPINNER);
}

void item_func_IRONBALL() {
    dComIfGs_setItem(SLOT_6, fpcNm_ITEM_IRONBALL);
}

void item_func_BOW() {
    dComIfGs_setItem(SLOT_4, fpcNm_ITEM_BOW);
    dComIfGs_setArrowNum(30);
    dComIfGs_setArrowMax(30);
}

void item_func_HOOKSHOT() {
    dComIfGs_setItem(SLOT_9, fpcNm_ITEM_HOOKSHOT);
}

void item_func_HVY_BOOTS() {
    dComIfGs_setItem(SLOT_3, fpcNm_ITEM_HVY_BOOTS);
}

void item_func_COPY_ROD() {
    dComIfGs_setItem(SLOT_8, fpcNm_ITEM_COPY_ROD);
}

void item_func_W_HOOKSHOT() {
    dComIfGs_setItem(SLOT_9, fpcNm_ITEM_NONE);
    dComIfGs_setItem(SLOT_10, fpcNm_ITEM_W_HOOKSHOT);
}

void item_func_KANTERA() {
    dComIfGs_setMaxOil(21600);
    dComIfGs_setOil(21600);
    dComIfGs_setItem(SLOT_1, fpcNm_ITEM_KANTERA);
}

void item_func_LIGHT_SWORD() {
    dComIfGs_setCollectSword(COLLECT_LIGHT_SWORD);
    dMeter2Info_setSword(fpcNm_ITEM_LIGHT_SWORD, false);
}

void item_func_FISHING_ROD_1() {
    dComIfGs_setItem(SLOT_20, fpcNm_ITEM_FISHING_ROD_1);
}

void item_func_PACHINKO() {
    u8 pachinko_max = dComIfGs_getPachinkoMax();
    dComIfGp_setItemPachinkoNumCount(pachinko_max);
    dComIfGs_setItem(SLOT_23, fpcNm_ITEM_PACHINKO);
}

void item_func_COPY_ROD_2() {
    dComIfGs_onEventBit(0x2580); // Power up dominion rod
}

void item_func_KAKARIKO_GORGE_PORTAL() {
    dComIfGs_onStageSwitch(0x6, 0x15); // Unlock Gorge Portal
}

void item_func_KAKARIKO_VILLAGE_PORTAL() {
    dComIfGs_onStageSwitch(0x3, 0x1F); // Unlock Kak Portal
}

void item_func_BOMB_BAG_LV2() {}

void item_func_BOMB_BAG_LV1() {
    dComIfGs_setEmptyBombBag(fpcNm_ITEM_NORMAL_BOMB, 30);
}

void item_func_BOMB_IN_BAG() {
    dComIfGs_setEmptyBombBag(fpcNm_ITEM_NORMAL_BOMB, 30);
}

void item_func_DEATH_MOUNTAIN_PORTAL() {
    dComIfGs_onStageSwitch(0x3, 0x15); // Unlock DM Portal
}

void item_func_LIGHT_ARROW() {
    dComIfGs_setItem(SLOT_4, fpcNm_ITEM_LIGHT_ARROW);
}

void item_func_ARROW_LV1() {
    dComIfGs_setArrowNum(60);
    dComIfGs_setArrowMax(60);
}

void item_func_ARROW_LV2() {
    dComIfGs_setArrowNum(60);
    dComIfGs_setArrowMax(60);
}

void item_func_ARROW_LV3() {
    dComIfGs_setArrowNum(100);
    dComIfGs_setArrowMax(100);
}

void item_func_ZORAS_DOMAIN_PORTAL() {
    dComIfGs_onStageSwitch(0x4, 0x2); // Unlock ZD Portal
}

void item_func_LURE_ROD() {}

void item_func_BOMB_ARROW() {}

void item_func_HAWK_ARROW() {}

void item_func_BEE_ROD() {}

void item_func_JEWEL_ROD() {}

void item_func_WORM_ROD() {}

void item_func_JEWEL_BEE_ROD() {}

void item_func_JEWEL_WORM_ROD() {}

void item_func_EMPTY_BOTTLE() {
    dComIfGs_setEmptyBottle();
}

void item_func_RED_BOTTLE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_RED_BOTTLE);
}

void item_func_GREEN_BOTTLE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_GREEN_BOTTLE);
}

void item_func_BLUE_BOTTLE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_BLUE_BOTTLE);
}

void item_func_MILK_BOTTLE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_MILK_BOTTLE);
}

void item_func_HALF_MILK_BOTTLE() {
    dComIfGs_setEmptyBottle(fpcNm_ITEM_HALF_MILK_BOTTLE);
}

void item_func_OIL_BOTTLE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_OIL_BOTTLE);
}

void item_func_WATER_BOTTLE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_WATER_BOTTLE);
}

void item_func_OIL_BOTTLE2() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_OIL_BOTTLE);
}

void item_func_RED_BOTTLE2() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_RED_BOTTLE);
}

void item_func_UGLY_SOUP() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_UGLY_SOUP);
}

void item_func_HOT_SPRING() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_HOT_SPRING);
}

void item_func_FAIRY_BOTTLE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_FAIRY);
}

void item_func_HOT_SPRING2() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_HOT_SPRING);
}

void item_func_OIL2() {}

void item_func_OIL() {}

void item_func_NORMAL_BOMB() {
    dComIfGs_setEmptyBombBag(fpcNm_ITEM_NORMAL_BOMB, 60);
}

void item_func_WATER_BOMB() {
    dComIfGs_setEmptyBombBag();
    dComIfGs_setEmptyBombBagItemIn(fpcNm_ITEM_WATER_BOMB, true);
}

void item_func_POKE_BOMB() {
    dComIfGs_setEmptyBombBag();
    dComIfGs_setEmptyBombBagItemIn(fpcNm_ITEM_POKE_BOMB, true);
}

void item_func_FAIRY_DROP() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_FAIRY_DROP);
}

void item_func_WORM() {}

void item_func_DROP_BOTTLE() {
    dComIfGs_setEmptyBottle(fpcNm_ITEM_FAIRY_DROP);
}

void item_func_BEE_CHILD() {
    int bottleIdx;
    int i;

    for (bottleIdx = 0xFF, i = 0; i < 4; i++) {
        u8 getItem = dComIfGs_getItem(i + SLOT_11, true);

        if (getItem == fpcNm_ITEM_EMPTY_BOTTLE) {
            bottleIdx = i;
            break;
        }
    }

    if (bottleIdx != 0xff) {
        dComIfGs_setBottleNum(bottleIdx, 10);
        dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_BEE_CHILD);
    }
}

void item_func_CHUCHU_RARE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_CHUCHU_RARE);
}

void item_func_CHUCHU_RED() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_CHUCHU_RED);
}

void item_func_CHUCHU_BLUE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_CHUCHU_BLUE);
}

void item_func_CHUCHU_GREEN() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_CHUCHU_GREEN);
}

void item_func_CHUCHU_YELLOW() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_CHUCHU_YELLOW);
}

void item_func_CHUCHU_PURPLE() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_CHUCHU_PURPLE);
}

void item_func_LV1_SOUP() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_LV1_SOUP);
}

void item_func_LV2_SOUP() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_LV2_SOUP);
}

void item_func_LV3_SOUP() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_LV3_SOUP);
}

void item_func_LETTER() {
    if (dComIfGs_getItem(SLOT_21, true) != fpcNm_ITEM_HORSE_FLUTE)
    {
        dComIfGs_setItem(SLOT_21, fpcNm_ITEM_LETTER);
    }
}

void item_func_BILL() {
    if (dComIfGs_getItem(SLOT_21, true) != fpcNm_ITEM_HORSE_FLUTE)
    {
        dComIfGs_setItem(SLOT_21, fpcNm_ITEM_BILL);
    }
}

void item_func_WOOD_STATUE() {
    /* dSv_event_flag_c::F_283 - Hyrule Field - Get wood carving */
    dComIfGs_onEventBit(dSv_event_flag_c::saveBitLabels[283]);
    if (dComIfGs_getItem(SLOT_21, true) != fpcNm_ITEM_HORSE_FLUTE)
    {
        dComIfGs_setItem(SLOT_21, fpcNm_ITEM_WOOD_STATUE);
    }
}

void item_func_IRIAS_PENDANT() {
    if (dComIfGs_getItem(SLOT_21, true) != fpcNm_ITEM_HORSE_FLUTE)
    {
        dComIfGs_setItem(SLOT_21, fpcNm_ITEM_IRIAS_PENDANT);
    }
}

void item_func_HORSE_FLUTE() {
    if (dComIfGs_getItem(SLOT_21, true) == fpcNm_ITEM_NONE)
    {
        dComIfGs_setItem(SLOT_21, fpcNm_ITEM_HORSE_FLUTE);
    }
}

void item_func_FOREST_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x10);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x10);
    dComIfGs_setKeyNum(0x10, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x10, currentTotalKeys + 1);
}

void item_func_MINES_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x11);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x11);
    dComIfGs_setKeyNum(0x11, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x11, currentTotalKeys + 1);
}

void item_func_LAKEBED_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x12);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x12);
    dComIfGs_setKeyNum(0x12, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x12, currentTotalKeys + 1);
}

void item_func_ARBITERS_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x13);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x13);
    dComIfGs_setKeyNum(0x13, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x13, currentTotalKeys + 1);
}

void item_func_SNOWPEAK_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x14);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x14);
    dComIfGs_setKeyNum(0x14, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x14, currentTotalKeys + 1);
}

void item_func_TEMPLE_OF_TIME_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x15);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x15);
    dComIfGs_setKeyNum(0x15, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x15, currentTotalKeys + 1);
}

void item_func_CITY_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x16);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x16);
    dComIfGs_setKeyNum(0x16, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x16, currentTotalKeys + 1);
}

void item_func_PALACE_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x17);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x17);
    dComIfGs_setKeyNum(0x17, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x17, currentTotalKeys + 1);
}

void item_func_HYRULE_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0x18);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0x18);
    dComIfGs_setKeyNum(0x18, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0x18, currentTotalKeys + 1);
}

void item_func_CAMP_SMALL_KEY() {
    u8 currentKeys = dComIfGs_getKeyNum(0xA);
    u8 currentTotalKeys = dComIfGs_getTotalKeyNum(0xA);
    dComIfGs_setKeyNum(0xA, currentKeys + 1);
    dComIfGs_setTotalKeyNum(0xA, currentTotalKeys + 1);
}

void item_func_LAKE_HYLIA_PORTAL() {
    dComIfGs_onStageSwitch(0x4, 0xA); // Unlock Lake Portal
}

void item_func_RAFRELS_MEMO() {
    dComIfGs_setItem(SLOT_7, fpcNm_ITEM_RAFRELS_MEMO);
}

void item_func_ASHS_SCRIBBLING() {
    if (!dComIfGs_isEventBit(0x3B80)) // Got earring from Ralis
    {
        dComIfGs_setItem(SLOT_19, fpcNm_ITEM_ASHS_SCRIBBLING);
    }
}

void item_func_FOREST_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x10);
}

void item_func_LAKEBED_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x12);
}

void item_func_ARBITERS_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x13);
}

void item_func_TEMPLE_OF_TIME_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x15);
}

void item_func_CITY_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x16);
}

void item_func_PALACE_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x17);
}

void item_func_HYRULE_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x18);
}

void item_func_FOREST_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x10);
}

void item_func_MINES_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x11);
}

void item_func_LAKEBED_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x12);
}

void item_func_CHUCHU_YELLOW2() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_CHUCHU_YELLOW);
}

void item_func_OIL_BOTTLE3() {
    dComIfGs_setEmptyBottle(fpcNm_ITEM_OIL_BOTTLE);
}

void item_func_SHOP_BEE_CHILD() {
    item_func_BEE_CHILD();
}

void item_func_CHUCHU_BLACK() {
    dComIfGs_setEmptyBottleItemIn(fpcNm_ITEM_CHUCHU_BLACK);
}

void item_func_LIGHT_DROP() {}

void item_func_DROP_CONTAINER() {
    dComIfGs_onLightDropGetFlag(FARON_VESSEL);
}

void item_func_DROP_CONTAINER02() {
    dComIfGs_onLightDropGetFlag(ELDIN_VESSEL);
}

void item_func_DROP_CONTAINER03() {
    dComIfGs_onLightDropGetFlag(LANAYRU_VESSEL);
    dComIfGs_onEventBit(0x1E80); // Malo Mart Fundraising Starts
}

void item_func_FILLED_CONTAINER() {}

void item_func_MIRROR_PIECE_2() {
    dComIfGs_onCollectMirror(1);
    
    // Check if the requirement for the HC barrier is set to shards, and if so, set the flag
    checkSetHCBarrierFlag(HC_Mirror_Shards, 2);

    // Check if the requirement for the HC BK is set to shards, and if so, set the flag
    checkSetHCBkFlag(HC_BK_Mirror_Shards, 2);
}

void item_func_MIRROR_PIECE_3() {
    dComIfGs_onCollectMirror(2);
    
    // Check if the requirement for the HC barrier is set to shards, and if so, set the flag
    checkSetHCBarrierFlag(HC_Mirror_Shards, 3);

    // Check if the requirement for the HC BK is set to shards, and if so, set the flag
    checkSetHCBkFlag(HC_BK_Mirror_Shards, 3);
    
}

void item_func_MIRROR_PIECE_4() {
    dComIfGs_onCollectMirror(3);
    
    // If the player has the palace requirement set to Mirror Shards.
    if (g_seedInfo.getHeaderPtr()->getPalaceRequirements() == PoT_Mirror_Shards)
    {
        dComIfGs_onEventBit(FIXED_THE_MIRROR_OF_TWILIGHT);
    }

    // Check if the requirement for the HC barrier is set to shards, and if so, set the flag
    checkSetHCBarrierFlag(HC_Mirror_Shards, 4);

    // Check if the requirement for the HC BK is set to shards, and if so, set the flag
    checkSetHCBkFlag(HC_BK_Mirror_Shards, 4);
    
}

void item_func_ARBITERS_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x13);
}

void item_func_SNOWPEAK_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x14);
}

void item_func_TEMPLE_OF_TIME_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x15);
}

void item_func_CITY_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x16);
}

void item_func_PALACE_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x17);
}

void item_func_HYRULE_COMPASS() {
    dComIfGs_onDungeonItemCompass(0x18);
}

void item_func_MIRROR_CHAMBER_PORTAL() {
    dComIfGs_onStageSwitch(0xA, 0x28); // Unlock MC Portal
}

void item_func_SNOWPEAK_PORTAL() {
    dComIfGs_onStageSwitch(0x8, 0x15); // Unlock Snowpeak Portal
}

void item_func_SMELL_YELIA_POUCH() {}

void item_func_SMELL_PUMPKIN() {}

void item_func_SMELL_POH() {}

void item_func_SMELL_FISH() {}

void item_func_SMELL_CHILDREN() {}

void item_func_SMELL_MEDICINE() {}

void item_func_FOREST_MAP() {
    dComIfGs_onDungeonItemMap(0x10);
}

void item_func_MINES_MAP() {
    dComIfGs_onDungeonItemMap(0x11);
}

void item_func_LAKEBED_MAP() {
    dComIfGs_onDungeonItemMap(0x12);
}

void item_func_ARBITERS_MAP() {
    dComIfGs_onDungeonItemMap(0x13);
}

void item_func_SNOWPEAK_MAP() {
    dComIfGs_onDungeonItemMap(0x14);
}

void item_func_TEMPLE_OF_TIME_MAP() {
    dComIfGs_onDungeonItemMap(0x15);
}

void item_func_CITY_MAP() {
    dComIfGs_onDungeonItemMap(0x16);
}

void item_func_PALACE_MAP() {
    dComIfGs_onDungeonItemMap(0x17);
}

void item_func_HYRULE_MAP() {
    dComIfGs_onDungeonItemMap(0x18);
}

void item_func_SACRED_GROVE_PORTAL() {
    dComIfGs_onStageSwitch(0x7, 0x64); // Unlock Grove Portal
}

void item_func_M_BEETLE() {}

void item_func_F_BEETLE() {}

void item_func_M_BUTTERFLY() {}

void item_func_F_BUTTERFLY() {}

void item_func_M_STAG_BEETLE() {}

void item_func_F_STAG_BEETLE() {}

void item_func_M_GRASSHOPPER() {}

void item_func_F_GRASSHOPPER() {}

void item_func_M_NANAFUSHI() {}

void item_func_F_NANAFUSHI() {}

void item_func_M_DANGOMUSHI() {}

void item_func_F_DANGOMUSHI() {}

void item_func_M_MANTIS() {}

void item_func_F_MANTIS() {}

void item_func_M_LADYBUG() {}

void item_func_F_LADYBUG() {}

void item_func_M_SNAIL() {}

void item_func_F_SNAIL() {}

void item_func_M_DRAGONFLY() {}

void item_func_F_DRAGONFLY() {}

void item_func_M_ANT() {}

void item_func_F_ANT() {}

void item_func_M_MAYFLY() {}

void item_func_F_MAYFLY() {}

void item_func_FUSED_SHADOW_1() {
    dComIfGs_onCollectCrystal(0);
    
    // Check if the requirement for the HC barrier is set to shadows, and if so, set the flag
    checkSetHCBarrierFlag(HC_Fused_Shadows, 1);

    // Check if the requirement for the HC BK is set to shadows, and if so, set the flag
    checkSetHCBkFlag(HC_BK_Fused_Shadows, 1);
}

void item_func_FUSED_SHADOW_2() {
    dComIfGs_onCollectCrystal(1);
    
    // Check if the requirement for the HC barrier is set to shadows, and if so, set the flag
    checkSetHCBarrierFlag(HC_Fused_Shadows, 2);

    // Check if the requirement for the HC BK is set to shadows, and if so, set the flag
    checkSetHCBkFlag(HC_BK_Fused_Shadows, 2);
}

void item_func_FUSED_SHADOW_3() {
    dComIfGs_onCollectCrystal(2);
    
    // If the player has the palace requirement set to Fused Shadows.
    if (g_seedInfo.getHeaderPtr()->getPalaceRequirements() == PoT_Fused_Shadows)
    {
        dComIfGs_onEventBit(FIXED_THE_MIRROR_OF_TWILIGHT);
    }

    // Check if the requirement for the HC barrier is set to shadows, and if so, set the flag
    checkSetHCBarrierFlag(HC_Fused_Shadows, 3);

    // Check if the requirement for the HC BK is set to shadows, and if so, set the flag
    checkSetHCBkFlag(HC_BK_Fused_Shadows, 3);
}

void item_func_MIRROR_PIECE_1() {
    dComIfGs_onCollectMirror(0);
    
    // Check if the requirement for the HC barrier is set to shards, and if so, set the flag
    checkSetHCBarrierFlag(HC_Mirror_Shards, 1);

    // Check if the requirement for the HC BK is set to shards, and if so, set the flag
    checkSetHCBkFlag(HC_BK_Mirror_Shards, 1);
}

void item_func_POU_SPIRIT() {
    dComIfGs_addPohSpiritNum();
    
    // Check if the HC barrier requires poes and if we have enough poe souls to set the flag.
    checkSetHCBarrierFlag(HC_Poe_Souls, dComIfGs_getPohSpiritNum());

    // Check if the HC bk check requires poes and if we have enough poe souls to unlock the check.
    checkSetHCBkFlag(HC_BK_Poe_Souls, dComIfGs_getPohSpiritNum());
}

void item_func_ENDING_BLOW() {
    dComIfGs_onEventBit(0x2904);
}

void item_func_SHIELD_ATTACK() {
    dComIfGs_onEventBit(0x2908);
}

void item_func_BACK_SLICE() {
    dComIfGs_onEventBit(0x2902);
}

void item_func_HELM_SPLITTER() {
    dComIfGs_onEventBit(0x2901);
}

void item_func_MORTAL_DRAW() {
    dComIfGs_onEventBit(0x2A80);
}

void item_func_JUMP_STRIKE() {
    dComIfGs_onEventBit(0x2A40);
}

void item_func_GREAT_SPIN() {
    dComIfGs_onEventBit(0x2A20);
}

void item_func_ELDIN_BRIDGE_PORTAL() {
    dComIfGs_onStageSwitch(0x6, 0x63); // Unlock Eldin Bridge Portal
}

void item_func_ANCIENT_DOCUMENT() {
    dComIfGs_setItem(SLOT_22, fpcNm_ITEM_ANCIENT_DOCUMENT);
}

void item_func_AIR_LETTER() {
    u8 letterCount = dComIfGs_getAncientDocumentNum();
    dComIfGs_setAncientDocumentNum(letterCount + 1);
}

void item_func_ANCIENT_DOCUMENT2() {
    dComIfGs_onEventBit(0x3B08); // Repaired Cannon at Lake
    dComIfGs_setAncientDocumentNum(6);
    dComIfGs_setItem(SLOT_22, fpcNm_ITEM_ANCIENT_DOCUMENT2);
}

void item_func_LV7_DUNGEON_EXIT() {
    dComIfGs_setItem(SLOT_18, fpcNm_ITEM_LV7_DUNGEON_EXIT);
}

void item_func_LINKS_SAVINGS() {
    dComIfGp_setItemRupeeCount(50);
}

void item_func_SMALL_KEY2() {
    dComIfGs_onStageSwitch(0x2, 0x14); // Unlock North Faron Gate
}

void item_func_POU_FIRE1() {}

void item_func_POU_FIRE2() {}

void item_func_POU_FIRE3() {}

void item_func_POU_FIRE4() {}

void item_func_BOSSRIDER_KEY() {
    dComIfGs_onStageSwitch(0x3, 0x69); // Started Escort
    dComIfGs_onStageSwitch(0x3, 0x65); // Followed Rutella to Graveyard
    dComIfGs_onEventBit(0x840); // Started Zora Escort
    dComIfGs_onEventBit(0x810); // Completed Zora Escort
}

void item_func_TOMATO_PUREE() {
    dComIfGs_onEventBit(0x480); // Told Yeta about Pumpkin
    dComIfGs_onEventBit(0x2); // Yeto put Pumpkin in Soup
    dComIfGs_onEventBit(0x1440); // SPR Lobby Door Unlocked
    dComIfGs_onStageSwitch(0x14, 0x12); // Unlock North Door
}

void item_func_TASTE() {
    dComIfGs_onEventBit(0x120); // Told Yeta about Cheese
    dComIfGs_onEventBit(0x1); // Yeto put Pumpkin in Soup
    dComIfGs_onEventBit(0x1420); // SPR Lobby west Door Unlocked
    dComIfGs_onStageSwitch(0x14, 0x13); // Unlock West Door
}

void item_func_LV5_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x14);
}

void item_func_SURFBOARD() {}

void item_func_KANTERA2() {}

void item_func_L2_KEY_PIECES1() {}

void item_func_L2_KEY_PIECES2() {}

void item_func_L2_KEY_PIECES3() {}

void item_func_KEY_OF_CARAVAN() {}

void item_func_LV2_BOSS_KEY() {
    dComIfGs_onDungeonItemBossKey(0x11);
    execItemGet(fpcNm_ITEM_L2_KEY_PIECES3);
}

void item_func_KEY_OF_FILONE() {
    dComIfGs_onStageSwitch(0x2, 0xC); // Unlock Coro Gate
}

void item_func_noentry() {}

int item_getcheck_func_noentry() {
    return -1;
}

int item_getcheck_func_HEART() {
    return -1;
}

int item_getcheck_func_GREEN_RUPEE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_GREEN_RUPEE);
}

int item_getcheck_func_BLUE_RUPEE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BLUE_RUPEE);
}

int item_getcheck_func_YELLOW_RUPEE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_YELLOW_RUPEE);
}

int item_getcheck_func_RED_RUPEE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_RED_RUPEE);
}

int item_getcheck_func_PURPLE_RUPEE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_PURPLE_RUPEE);
}

int item_getcheck_func_ORANGE_RUPEE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_ORANGE_RUPEE);
}

int item_getcheck_func_SILVER_RUPEE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_SILVER_RUPEE);
}

int item_getcheck_func_S_MAGIC() {
    return -1;
}

int item_getcheck_func_L_MAGIC() {
    return -1;
}

int item_getcheck_func_BOMB_5() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_5);
}

int item_getcheck_func_BOMB_10() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_10);
}

int item_getcheck_func_BOMB_20() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_20);
}

int item_getcheck_func_BOMB_30() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_30);
}

int item_getcheck_func_ARROW_10() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_ARROW_10);
}

int item_getcheck_func_ARROW_20() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_ARROW_20);
}

int item_getcheck_func_ARROW_30() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_ARROW_30);
}

int item_getcheck_func_ARROW_1() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_ARROW_1);
}

int item_getcheck_func_PACHINKO_SHOT() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_PACHINKO_SHOT);
}

int item_getcheck_func_WATER_BOMB_5() {
    return -1;
}

int item_getcheck_func_WATER_BOMB_10() {
    return -1;
}

int item_getcheck_func_WATER_BOMB_20() {
    return -1;
}

int item_getcheck_func_WATER_BOMB_30() {
    return -1;
}

int item_getcheck_func_BOMB_INSECT_5() {
    return -1;
}

int item_getcheck_func_BOMB_INSECT_10() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_INSECT_10);
}

int item_getcheck_func_BOMB_INSECT_20() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_INSECT_20);
}

int item_getcheck_func_BOMB_INSECT_30() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_INSECT_30);
}

int item_getcheck_func_RECOVER_FAILY() {
    return -1;
}

int item_getcheck_func_TRIPLE_HEART() {
    return -1;
}

int item_getcheck_func_SMALL_KEY() {
    return -1;
}

int item_getcheck_func_KAKERA_HEART() {
    return -1;
}

int item_getcheck_func_UTUWA_HEART() {
    return -1;
}

int item_getcheck_func_MAP() {
    return dComIfGs_isDungeonItemMap();
}

int item_getcheck_func_COMPUS() {
    return -1;
}

int item_getcheck_func_DUNGEON_EXIT() {
    return dComIfGs_getItem(SLOT_18, true) == fpcNm_ITEM_DUNGEON_EXIT ? TRUE : FALSE;
}

int item_getcheck_func_BOSS_KEY() {
    return dComIfGs_isDungeonItemBossKey();
}

int item_getcheck_func_DUNGEON_BACK() {
    return dComIfGs_getItem(SLOT_18, true) == fpcNm_ITEM_DUNGEON_BACK ? TRUE : FALSE;
}

int item_getcheck_func_SWORD() {
    return dComIfGs_isCollectSword(COLLECT_ORDON_SWORD);
}

int item_getcheck_func_MASTER_SWORD() {
    return dComIfGs_isCollectSword(COLLECT_MASTER_SWORD);
}

int item_getcheck_func_WOOD_SHIELD() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_WOOD_SHIELD);
}

int item_getcheck_func_SHIELD() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_SHIELD);
}

int item_getcheck_func_HYLIA_SHIELD() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_HYLIA_SHIELD);
}

int item_getcheck_func_TKS_LETTER() {
    return dComIfGs_getItem(SLOT_18, true) == fpcNm_ITEM_TKS_LETTER ? TRUE : FALSE;
}

int item_getcheck_func_WEAR_CASUAL() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_WEAR_CASUAL);
}

int item_getcheck_func_WEAR_KOKIRI() {
    return dComIfGs_isCollectClothing(KOKIRI_CLOTHES_FLAG);
}

int item_getcheck_func_ARMOR() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_ARMOR);
}

int item_getcheck_func_WEAR_ZORA() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_WEAR_ZORA);
}

int item_getcheck_func_MAGIC_LV1() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_MAGIC_LV1);
}

int item_getcheck_func_DUNGEON_EXIT_2() {
    return dComIfGs_getItem(SLOT_18, true) == fpcNm_ITEM_DUNGEON_EXIT ? TRUE : FALSE;
}

int item_getcheck_func_WALLET_LV1() {
    return -1;
}

int item_getcheck_func_WALLET_LV2() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_WALLET_LV2);
}

int item_getcheck_func_WALLET_LV3() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_WALLET_LV3);
}

int item_getcheck_func_ZORAS_JEWEL() {
    return dComIfGs_getItem(SLOT_20, true) == fpcNm_ITEM_ZORAS_JEWEL ? TRUE : FALSE;
}

int item_getcheck_func_HAWK_EYE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_HAWK_EYE);
}

int item_getcheck_func_WOOD_STICK() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_WOOD_STICK);
}

int item_getcheck_func_BOOMERANG() {
    return dComIfGs_getItem(SLOT_0, true) == fpcNm_ITEM_BOOMERANG ? TRUE : FALSE;
}

int item_getcheck_func_SPINNER() {
    return dComIfGs_getItem(SLOT_2, true) == fpcNm_ITEM_SPINNER ? TRUE : FALSE;
}

int item_getcheck_func_IRONBALL() {
    return dComIfGs_getItem(SLOT_6, true) == fpcNm_ITEM_IRONBALL ? TRUE : FALSE;
}

int item_getcheck_func_BOW() {
    return dComIfGs_getItem(SLOT_4, false) == fpcNm_ITEM_BOW ? TRUE : FALSE;
}

int item_getcheck_func_HOOKSHOT() {
    return dComIfGs_getItem(SLOT_9, true) == fpcNm_ITEM_HOOKSHOT ? TRUE : FALSE;
}

int item_getcheck_func_HVY_BOOTS() {
    return dComIfGs_getItem(SLOT_3, true) == fpcNm_ITEM_HVY_BOOTS ? TRUE : FALSE;
}

int item_getcheck_func_COPY_ROD() {
    return dComIfGs_getItem(SLOT_8, true) == fpcNm_ITEM_COPY_ROD ? TRUE : FALSE;
}

int item_getcheck_func_W_HOOKSHOT() {
    return dComIfGs_getItem(SLOT_10, true) == fpcNm_ITEM_W_HOOKSHOT ? TRUE : FALSE;
}

int item_getcheck_func_KANTERA() {
    return dComIfGs_getItem(SLOT_1, true) == fpcNm_ITEM_KANTERA ? TRUE : FALSE;
}

int item_getcheck_func_LIGHT_SWORD() {
    return dComIfGs_isCollectSword(COLLECT_LIGHT_SWORD);
}

int item_getcheck_func_FISHING_ROD_1() {
    return (dComIfGs_getItem(SLOT_20, true) == fpcNm_ITEM_FISHING_ROD_1 ||
            dComIfGs_getItem(SLOT_20, true) == fpcNm_ITEM_BEE_ROD ||
            dComIfGs_getItem(SLOT_20, true) == fpcNm_ITEM_WORM_ROD ||
            dComIfGs_getItem(SLOT_20, true) == fpcNm_ITEM_JEWEL_ROD ||
            dComIfGs_getItem(SLOT_20, true) == fpcNm_ITEM_JEWEL_BEE_ROD ||
            dComIfGs_getItem(SLOT_20, true) == fpcNm_ITEM_JEWEL_WORM_ROD) ?
               TRUE :
               FALSE;
}

int item_getcheck_func_PACHINKO() {
    return dComIfGs_getItem(SLOT_23, true) == fpcNm_ITEM_PACHINKO ? TRUE : FALSE;
}

int item_getcheck_func_COPY_ROD_2() {
    return -1;
}

int item_getcheck_func_BOMB_BAG_LV2() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_BAG_LV2);
}

int item_getcheck_func_BOMB_BAG_LV1() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_BAG_LV1);
}

int item_getcheck_func_BOMB_IN_BAG() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_IN_BAG);
}

int item_getcheck_func_LIGHT_ARROW() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_LIGHT_ARROW);
}

int item_getcheck_func_ARROW_LV1() {
    return (dComIfGs_getItem(SLOT_4, false) == fpcNm_ITEM_BOW && dComIfGs_getArrowMax() >= 30) ? TRUE : FALSE;
}

int item_getcheck_func_ARROW_LV2() {
    return (dComIfGs_getItem(SLOT_4, false) == fpcNm_ITEM_BOW && dComIfGs_getArrowMax() >= 60) ? TRUE : FALSE;
}

int item_getcheck_func_ARROW_LV3() {
    return (dComIfGs_getItem(SLOT_4, false) == fpcNm_ITEM_BOW && dComIfGs_getArrowMax() >= 100) ? TRUE : FALSE;
}

int item_getcheck_func_LURE_ROD() {
    return -1;
}

int item_getcheck_func_BOMB_ARROW() {
    return -1;
}

int item_getcheck_func_HAWK_ARROW() {
    return -1;
}

int item_getcheck_func_BEE_ROD() {
    return item_getcheck_func_FISHING_ROD_1();
}

int item_getcheck_func_JEWEL_ROD() {
    return item_getcheck_func_FISHING_ROD_1();
}

int item_getcheck_func_WORM_ROD() {
    return item_getcheck_func_FISHING_ROD_1();
}

int item_getcheck_func_JEWEL_BEE_ROD() {
    return item_getcheck_func_FISHING_ROD_1();
}

int item_getcheck_func_JEWEL_WORM_ROD() {
    return item_getcheck_func_FISHING_ROD_1();
}

int item_getcheck_func_EMPTY_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_EMPTY_BOTTLE);
}

int item_getcheck_func_RED_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_RED_BOTTLE);
}

int item_getcheck_func_GREEN_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_GREEN_BOTTLE);
}

int item_getcheck_func_BLUE_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_BLUE_BOTTLE);
}

int item_getcheck_func_MILK_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_MILK_BOTTLE);
}

int item_getcheck_func_HALF_MILK_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_HALF_MILK_BOTTLE);
}

int item_getcheck_func_OIL_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_OIL_BOTTLE);
}

int item_getcheck_func_WATER_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_WATER_BOTTLE);
}

int item_getcheck_func_OIL_BOTTLE2() {
    return dComIfGs_checkBottle(fpcNm_ITEM_OIL_BOTTLE_2);
}

int item_getcheck_func_RED_BOTTLE2() {
    return dComIfGs_checkBottle(fpcNm_ITEM_RED_BOTTLE_2);
}

int item_getcheck_func_UGLY_SOUP() {
    return dComIfGs_checkBottle(fpcNm_ITEM_UGLY_SOUP);
}

int item_getcheck_func_HOT_SPRING() {
    return dComIfGs_checkBottle(fpcNm_ITEM_HOT_SPRING);
}

int item_getcheck_func_FAIRY_BOTTLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_FAIRY);
}

int item_getcheck_func_HOT_SPRING2() {
    return dComIfGs_checkBottle(fpcNm_ITEM_HOT_SPRING);
}

int item_getcheck_func_OIL2() {
    return dComIfGs_checkBottle(fpcNm_ITEM_OIL2);
}

int item_getcheck_func_OIL() {
    return dComIfGs_checkBottle(fpcNm_ITEM_OIL);
}

int item_getcheck_func_NORMAL_BOMB() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_NORMAL_BOMB);
}

int item_getcheck_func_WATER_BOMB() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_WATER_BOMB);
}

int item_getcheck_func_POKE_BOMB() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_POKE_BOMB);
}

int item_getcheck_func_FAIRY_DROP() {
    return dComIfGs_checkBottle(fpcNm_ITEM_FAIRY_DROP);
}

int item_getcheck_func_WORM() {
    return dComIfGs_checkBottle(fpcNm_ITEM_WORM);
}

int item_getcheck_func_DROP_BOTTLE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_DROP_BOTTLE);
}

int item_getcheck_func_BEE_CHILD() {
    return -1;
}

int item_getcheck_func_CHUCHU_RARE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_CHUCHU_RARE);
}

int item_getcheck_func_CHUCHU_RED() {
    return dComIfGs_checkBottle(fpcNm_ITEM_CHUCHU_RED);
}

int item_getcheck_func_CHUCHU_BLUE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_CHUCHU_BLUE);
}

int item_getcheck_func_CHUCHU_GREEN() {
    return dComIfGs_checkBottle(fpcNm_ITEM_CHUCHU_GREEN);
}

int item_getcheck_func_CHUCHU_YELLOW() {
    return dComIfGs_checkBottle(fpcNm_ITEM_CHUCHU_YELLOW);
}

int item_getcheck_func_CHUCHU_PURPLE() {
    return dComIfGs_checkBottle(fpcNm_ITEM_CHUCHU_PURPLE);
}

int item_getcheck_func_LV1_SOUP() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_LV1_SOUP);
}

int item_getcheck_func_LV2_SOUP() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_LV2_SOUP);
}

int item_getcheck_func_LV3_SOUP() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_LV3_SOUP);
}

int item_getcheck_func_LETTER() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_LETTER);
}

int item_getcheck_func_BILL() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BILL);
}

int item_getcheck_func_WOOD_STATUE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_WOOD_STATUE);
}

int item_getcheck_func_IRIAS_PENDANT() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_IRIAS_PENDANT);
}

int item_getcheck_func_HORSE_FLUTE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_HORSE_FLUTE);
}

int item_getcheck_func_CAMP_SMALL_KEY() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_CAMP_SMALL_KEY);
}

int item_getcheck_func_RAFRELS_MEMO() {
    return dComIfGs_getItem(SLOT_19, true) == fpcNm_ITEM_RAFRELS_MEMO ? TRUE : FALSE;
}

int item_getcheck_func_ASHS_SCRIBBLING() {
    return dComIfGs_getItem(SLOT_19, true) == fpcNm_ITEM_ASHS_SCRIBBLING ? TRUE : FALSE;
}

int item_getcheck_func_CHUCHU_YELLOW2() {
    return dComIfGs_checkBottle(fpcNm_ITEM_CHUCHU_YELLOW2);
}

int item_getcheck_func_OIL_BOTTLE3() {
    return -1;
}

int item_getcheck_func_SHOP_BEE_CHILD() {
    return -1;
}

int item_getcheck_func_CHUCHU_BLACK() {
    return dComIfGs_checkBottle(fpcNm_ITEM_CHUCHU_BLACK);
}

int item_getcheck_func_LIGHT_DROP() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_LIGHT_DROP);
}

int item_getcheck_func_DROP_CONTAINER() {
    return dComIfGs_isLightDropGetFlag(FARON_VESSEL);
}

int item_getcheck_func_DROP_CONTAINER02() {
    return dComIfGs_isLightDropGetFlag(ELDIN_VESSEL);
}

int item_getcheck_func_DROP_CONTAINER03() {
    return dComIfGs_isLightDropGetFlag(LANAYRU_VESSEL);
}

int item_getcheck_func_FILLED_CONTAINER() {
    return -1;
}

int item_getcheck_func_MIRROR_PIECE_2() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_MIRROR_PIECE_2);
}

int item_getcheck_func_MIRROR_PIECE_3() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_MIRROR_PIECE_3);
}

int item_getcheck_func_MIRROR_PIECE_4() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_MIRROR_PIECE_4);
}

int item_getcheck_func_SMELL_YELIA_POUCH() {
    return dComIfGs_getCollectSmell() == fpcNm_ITEM_SMELL_YELIA_POUCH ? TRUE : FALSE;
}

int item_getcheck_func_SMELL_PUMPKIN() {
    return -1;
}

int item_getcheck_func_SMELL_POH() {
    return dComIfGs_getCollectSmell() == fpcNm_ITEM_SMELL_POH ? TRUE : FALSE;
}

int item_getcheck_func_SMELL_FISH() {
    return dComIfGs_getCollectSmell() == fpcNm_ITEM_SMELL_FISH ? TRUE : FALSE;
}

int item_getcheck_func_SMELL_CHILDREN() {
    return dComIfGs_getCollectSmell() == fpcNm_ITEM_SMELL_CHILDREN ? TRUE : FALSE;
}

int item_getcheck_func_SMELL_MEDICINE() {
    return dComIfGs_getCollectSmell() == fpcNm_ITEM_SMELL_MEDICINE ? TRUE : FALSE;
}

int item_getcheck_func_M_BEETLE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_BEETLE);
}

int item_getcheck_func_F_BEETLE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_BEETLE);
}

int item_getcheck_func_M_BUTTERFLY() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_BUTTERFLY);
}

int item_getcheck_func_F_BUTTERFLY() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_BUTTERFLY);
}

int item_getcheck_func_M_STAG_BEETLE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_STAG_BEETLE);
}

int item_getcheck_func_F_STAG_BEETLE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_STAG_BEETLE);
}

int item_getcheck_func_M_GRASSHOPPER() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_GRASSHOPPER);
}

int item_getcheck_func_F_GRASSHOPPER() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_GRASSHOPPER);
}

int item_getcheck_func_M_NANAFUSHI() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_NANAFUSHI);
}

int item_getcheck_func_F_NANAFUSHI() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_NANAFUSHI);
}

int item_getcheck_func_M_DANGOMUSHI() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_DANGOMUSHI);
}

int item_getcheck_func_F_DANGOMUSHI() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_DANGOMUSHI);
}

int item_getcheck_func_M_MANTIS() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_MANTIS);
}

int item_getcheck_func_F_MANTIS() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_MANTIS);
}

int item_getcheck_func_M_LADYBUG() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_LADYBUG);
}

int item_getcheck_func_F_LADYBUG() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_LADYBUG);
}

int item_getcheck_func_M_SNAIL() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_SNAIL);
}

int item_getcheck_func_F_SNAIL() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_SNAIL);
}

int item_getcheck_func_M_DRAGONFLY() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_DRAGONFLY);
}

int item_getcheck_func_F_DRAGONFLY() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_DRAGONFLY);
}

int item_getcheck_func_M_ANT() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_ANT);
}

int item_getcheck_func_F_ANT() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_ANT);
}

int item_getcheck_func_M_MAYFLY() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_M_MAYFLY);
}

int item_getcheck_func_F_MAYFLY() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_F_MAYFLY);
}

int item_getcheck_func_FUSED_SHADOW_1() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_FUSED_SHADOW_1);
}

int item_getcheck_func_FUSED_SHADOW_2() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_FUSED_SHADOW_2);
}

int item_getcheck_func_FUSED_SHADOW_3() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_FUSED_SHADOW_3);
}

int item_getcheck_func_MIRROR_PIECE_1() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_MIRROR_PIECE_1);
}

int item_getcheck_func_POU_SPIRIT() {
    return dComIfGs_getPohSpiritNum();
}

int item_getcheck_func_ENDING_BLOW() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_ENDING_BLOW);
}

int item_getcheck_func_SHIELD_ATTACK() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_SHIELD_ATTACK);
}

int item_getcheck_func_BACK_SLICE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BACK_SLICE);
}

int item_getcheck_func_HELM_SPLITTER() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_HELM_SPLITTER);
}

int item_getcheck_func_MORTAL_DRAW() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_MORTAL_DRAW);
}

int item_getcheck_func_JUMP_STRIKE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_JUMP_STRIKE);
}

int item_getcheck_func_GREAT_SPIN() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_GREAT_SPIN);
}

int item_getcheck_func_ANCIENT_DOCUMENT() {
    return dComIfGs_getItem(SLOT_22, true) == fpcNm_ITEM_ANCIENT_DOCUMENT ? TRUE : FALSE;
}

int item_getcheck_func_AIR_LETTER() {
    return dComIfGs_getItem(SLOT_22, true) == fpcNm_ITEM_AIR_LETTER ? TRUE : FALSE;
}

int item_getcheck_func_ANCIENT_DOCUMENT2() {
    return dComIfGs_getItem(SLOT_22, true) == fpcNm_ITEM_ANCIENT_DOCUMENT2 ? TRUE : FALSE;
}

int item_getcheck_func_LV7_DUNGEON_EXIT() {
    return dComIfGs_getItem(SLOT_18, true) == fpcNm_ITEM_LV7_DUNGEON_EXIT ? TRUE : FALSE;
}

int item_getcheck_func_LINKS_SAVINGS() {
    return -1;
}

int item_getcheck_func_SMALL_KEY2() {
    return -1;
}

int item_getcheck_func_POU_FIRE1() {
    return -1;
}

int item_getcheck_func_POU_FIRE2() {
    return -1;
}

int item_getcheck_func_POU_FIRE3() {
    return -1;
}

int item_getcheck_func_POU_FIRE4() {
    return -1;
}

int item_getcheck_func_BOSSRIDER_KEY() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_BOSSRIDER_KEY);
}

int item_getcheck_func_TOMATO_PUREE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_TOMATO_PUREE);
}

int item_getcheck_func_TASTE() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_TASTE);
}

int item_getcheck_func_LV5_BOSS_KEY() {
    return dComIfGs_isDungeonItemBossKey();
}

int item_getcheck_func_SURFBOARD() {
    return -1;
}

int item_getcheck_func_KANTERA2() {
    return -1;
}

int item_getcheck_func_L2_KEY_PIECES1() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_L2_KEY_PIECES1);
}

int item_getcheck_func_L2_KEY_PIECES2() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_L2_KEY_PIECES2);
}

int item_getcheck_func_L2_KEY_PIECES3() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_L2_KEY_PIECES3);
}

int item_getcheck_func_KEY_OF_CARAVAN() {
    return dComIfGs_isItemFirstBit(fpcNm_ITEM_KEY_OF_CARAVAN);
}

int item_getcheck_func_LV2_BOSS_KEY() {
    return dComIfGs_isDungeonItemBossKey();
}

int item_getcheck_func_KEY_OF_FILONE() {
    return dComIfGs_getKeyNum();
}

int isBomb(u8 i_itemNo) {
    int is_bomb = false;

    if (i_itemNo == fpcNm_ITEM_BOMB_5 || i_itemNo == fpcNm_ITEM_BOMB_10 || i_itemNo == fpcNm_ITEM_BOMB_20 || i_itemNo == fpcNm_ITEM_BOMB_30 ||
        i_itemNo == fpcNm_ITEM_NORMAL_BOMB | i_itemNo == fpcNm_ITEM_WATER_BOMB || i_itemNo == fpcNm_ITEM_POKE_BOMB)
    {
        is_bomb = true;
    }

    return is_bomb;
}

int isArrow(u8 i_itemNo) {
    int is_arrow = false;

    if (i_itemNo == fpcNm_ITEM_ARROW_1 || i_itemNo == fpcNm_ITEM_ARROW_10 || i_itemNo == fpcNm_ITEM_ARROW_20 || i_itemNo == fpcNm_ITEM_ARROW_30)
    {
        is_arrow = true;
    }

    return is_arrow;
}

BOOL isBottleItem(u8 i_itemNo) {
    switch (i_itemNo) {
    case fpcNm_ITEM_OIL_BOTTLE3:
    case fpcNm_ITEM_EMPTY_BOTTLE:
    case fpcNm_ITEM_RED_BOTTLE:
    case fpcNm_ITEM_GREEN_BOTTLE:
    case fpcNm_ITEM_BLUE_BOTTLE:
    case fpcNm_ITEM_MILK_BOTTLE:
    case fpcNm_ITEM_HALF_MILK_BOTTLE:
    case fpcNm_ITEM_OIL_BOTTLE:
    case fpcNm_ITEM_WATER_BOTTLE:
    case fpcNm_ITEM_OIL_BOTTLE_2:
    case fpcNm_ITEM_RED_BOTTLE_2:
    case fpcNm_ITEM_UGLY_SOUP:
    case fpcNm_ITEM_HOT_SPRING:
    case fpcNm_ITEM_FAIRY:
    case fpcNm_ITEM_FAIRY_DROP:
    case fpcNm_ITEM_WORM:
    case fpcNm_ITEM_BEE_CHILD:
    case fpcNm_ITEM_CHUCHU_RARE:
    case fpcNm_ITEM_CHUCHU_RED:
    case fpcNm_ITEM_CHUCHU_BLUE:
    case fpcNm_ITEM_CHUCHU_GREEN:
    case fpcNm_ITEM_CHUCHU_YELLOW:
    case fpcNm_ITEM_CHUCHU_PURPLE:
    case fpcNm_ITEM_LV1_SOUP:
    case fpcNm_ITEM_LV2_SOUP:
    case fpcNm_ITEM_LV3_SOUP:
    case fpcNm_ITEM_CHUCHU_BLACK:
    case fpcNm_ITEM_POU_FIRE1:
    case fpcNm_ITEM_POU_FIRE2:
    case fpcNm_ITEM_POU_FIRE3:
    case fpcNm_ITEM_POU_FIRE4:
        return TRUE;
    default:
        return FALSE;
    }
}

BOOL isHeart(u8 i_itemNo) {
    BOOL is_heart = false;

    if (i_itemNo == fpcNm_ITEM_HEART || i_itemNo == fpcNm_ITEM_TRIPLE_HEART) {
        is_heart = true;
    }

    return is_heart;
}

BOOL isInsect(u8 i_itemNo) {
    BOOL is_insect = false;

    switch (i_itemNo) {
    case fpcNm_ITEM_M_BEETLE:
    case fpcNm_ITEM_F_BEETLE:
    case fpcNm_ITEM_M_BUTTERFLY:
    case fpcNm_ITEM_F_BUTTERFLY:
    case fpcNm_ITEM_M_STAG_BEETLE:
    case fpcNm_ITEM_F_STAG_BEETLE:
    case fpcNm_ITEM_M_GRASSHOPPER:
    case fpcNm_ITEM_F_GRASSHOPPER:
    case fpcNm_ITEM_M_NANAFUSHI:
    case fpcNm_ITEM_F_NANAFUSHI:
    case fpcNm_ITEM_M_DANGOMUSHI:
    case fpcNm_ITEM_F_DANGOMUSHI:
    case fpcNm_ITEM_M_MANTIS:
    case fpcNm_ITEM_F_MANTIS:
    case fpcNm_ITEM_M_LADYBUG:
    case fpcNm_ITEM_F_LADYBUG:
    case fpcNm_ITEM_M_SNAIL:
    case fpcNm_ITEM_F_SNAIL:
    case fpcNm_ITEM_M_DRAGONFLY:
    case fpcNm_ITEM_F_DRAGONFLY:
    case fpcNm_ITEM_M_ANT:
    case fpcNm_ITEM_F_ANT:
    case fpcNm_ITEM_M_MAYFLY:
    case fpcNm_ITEM_F_MAYFLY:
        is_insect = true;
    }

    return is_insect;
}

u8 check_itemno(int i_itemNo) {
    if (!dComIfGs_isGetMagicUseFlag() && (i_itemNo == fpcNm_ITEM_S_MAGIC || i_itemNo == fpcNm_ITEM_L_MAGIC)) {
        return fpcNm_ITEM_GREEN_RUPEE;
    }

    if (i_itemNo == fpcNm_ITEM_ARROW_1) {
        if (!dComIfGs_isItemFirstBit(fpcNm_ITEM_BOW)) {
            return fpcNm_ITEM_GREEN_RUPEE;
        }
    } else {
        if (isArrow(i_itemNo)) {
            if (!dComIfGs_isItemFirstBit(fpcNm_ITEM_BOW)) {
                return fpcNm_ITEM_GREEN_RUPEE;
            }

            if (g_dComIfG_gameInfo.play.getLayerNo(0) == 0xD ||
                g_dComIfG_gameInfo.play.getLayerNo(0) == 0xE)
            {
                const char* stage_name = dComIfGp_getStartStageName();
                // D_MN08: Palace of Twilight
                if (strncmp(stage_name, "D_MN08", 6)) {
                    return fpcNm_ITEM_GREEN_RUPEE;
                }
            }
        }
    }

    if (!dComIfGs_isItemFirstBit(fpcNm_ITEM_BOMB_BAG_LV1) && isBomb(i_itemNo)) {
        return fpcNm_ITEM_GREEN_RUPEE;
    } else {
        if (i_itemNo == fpcNm_ITEM_TRIPLE_HEART) {
            i_itemNo = fpcNm_ITEM_HEART;
        }
        if (!checkItemGet(fpcNm_ITEM_PACHINKO, 1) && i_itemNo == fpcNm_ITEM_PACHINKO_SHOT) {
            i_itemNo = fpcNm_ITEM_GREEN_RUPEE;
        }
        if (i_itemNo == fpcNm_ITEM_S_MAGIC || i_itemNo == fpcNm_ITEM_L_MAGIC) {
            i_itemNo = fpcNm_ITEM_GREEN_RUPEE;
        }
    }
    return i_itemNo;
}

int addBombCount(u8 i_bombType, u8 i_addNum) {
    u8 bombType[3];
    int bombNum[3];

    for (u8 i = 0; i < 3; i++) {
        bombType[i] = dComIfGs_getItem(i + SLOT_15, false);

        if (bombType[i] == fpcNm_ITEM_BOMB_BAG_LV1) {
            bombNum[i] = 0;
        } else if (bombType[i] == i_bombType) {
            bombNum[i] = dComIfGs_getBombNum(i);
        } else {
            bombNum[i] = -1;
        }
    }

    for (u8 i = 0; i < 3; i++) {
        int bombIdx = -1;
        int var_r22 = -1;

        for (u8 j = 0; j < 3; j++) {
            if (bombNum[j] == 0) {
                bombIdx = j;
                var_r22 = 0;
            }
        }

        for (u8 k = 0; k < 3; k++) {
            if (bombNum[k] > 0 && bombNum[k] > var_r22 &&
                bombNum[k] != dComIfGs_getBombMax(bombType[k]))
            {
                bombIdx = k;
                var_r22 = bombNum[k];
            }
        }

        if (bombIdx == -1) {
            return i_addNum;
        } else if (var_r22 == 0) {
            if (dComIfGs_getBombMax(i_bombType) >= i_addNum) {
                dComIfGs_setEmptyBombBagItemIn(i_bombType, i_addNum, true);
                return 0;
            } else {
                dComIfGs_setEmptyBombBagItemIn(i_bombType, i_addNum, true);
                i_addNum = i_addNum - dComIfGs_getBombMax(i_bombType);
            }
        } else {
            if (dComIfGs_getBombMax(bombType[bombIdx]) >= var_r22 + i_addNum) {
                dComIfGp_setItemBombNumCount(bombIdx, i_addNum);
                return 0;
            } else {
                dComIfGp_setItemBombNumCount(bombIdx, i_addNum);
                i_addNum = i_addNum - (dComIfGs_getBombMax(bombType[bombIdx]) - var_r22);
            }
        }

        bombNum[bombIdx] = dComIfGs_getBombMax(bombType[bombIdx]);
    }

    return i_addNum;
}

u8* dEnemyItem_c::mData;
