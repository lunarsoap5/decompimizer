/**
 * d_a_shop_item_static.cpp
 *
 */

#include "d/dolzel.h" // IWYU pragma: keep

#include "d/d_a_shop_item_static.h"

csXyz* daShopItem_c::getRotateP() {
    return &current.angle;
}

cXyz* daShopItem_c::getPosP() {
    return &current.pos;
}

ResourceData daShopItem_c::mData[33] = {
    /* 0x00 */{"B_mD_sold", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, -0x8000, 0}, 0, 0, 0, 0, -1, -1}, // Sold out sign
    /* 0x01 */{"O_mD_marm", 4, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Magic armor sold out
    /* 0x02 */{"O_mD_hati", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Sera Shop Larva
    /* 0x03 */{"B_mD_milk", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Sera Shop Milk
    /* 0x04 */{"B_mD_oil", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Sera Shop Oil
    /* 0x05 */{"O_mD_pach", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Sera Shop Slingshot
    /* 0x06 */{"O_mD_arw", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Kak Malo Mart Arrows
    /* 0x07 */{"O_mD_hawk", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Kak Malo Mart Hawkeye
    /* 0x08 */{"O_mD_SHB", 3, -1, -1, -1, -1, -1, -1, 30.0f, 1.0f, 0, {0, 0x7FFF, 0}, 0, 0, 0, 0, -1, -1}, // Kak Malo Mart Wood Shield
    /* 0x09 */{"O_mD_SHA", 3, -1, -1, -1, -1, -1, -1, 30.0f, 1.0f, 0, {0, 0x7FFF, 0}, 0, 0, 0, 0, -1, -1}, // Kak Malo Mart Hylian Shield
    /* 0x0A */{"O_mD_red", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Kak Malo Mart Red Potion
    /* 0x0B */{"B_mD_oil", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Kak Goron Shop Oil
    /* 0x0C */{"O_mD_red", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Kak Goron Shop Red Potion
    /* 0x0D */{"O_mD_blue", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Kak Goron Shop Blue Potion
    /* 0x0E */{"O_mD_bomb", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1}, // Barnes Bomb
    /* 0x0F */{"O_mD_pg", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1}, // Barnes Water Bomb
    /* 0x10 */{"O_mD_bi", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1}, // Barnes Bombling
    /* 0x11 */{"B_mD_oil", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // DM Goron Oil
    /* 0x12 */{"O_mD_SHB", 3, -1, -1, -1, -1, -1, -1, 30.0f, 1.0f, 0, {0, 0x7FFF, 0}, 0, 0, 0, 0, -1, -1}, // DM Goron Wood Shield
    /* 0x13 */{"B_mD_milk", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // DM Goron Milk
    /* 0x14 */{"O_mD_arw", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // DM Goron Arrows
    /* 0x15 */{"O_mD_red", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Castle Malo Mart Red Potion
    /* 0x16 */{"O_mD_blue", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Castle Malo Mart Blue Potion
    /* 0x17 */{"O_mD_arw", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Castle Malo Mart Arrows
    /* 0x18 */{"O_mD_bomb", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1}, // Castle Malo Mart Bomb
    /* 0x19 */{"O_mD_pg", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1}, // Castle Malo Mart Water Bomb
    /* 0x1A */{"O_mD_bi", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1}, // Castle Malo Mart Bombling
    /* 0x1B */{"O_mD_marm", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // Castle Malo Mart Magic Armor
    /* 0x1C */{"O_mD_bomb", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1}, // City Bomb
    /* 0x1D */{"O_mD_red", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // City Red Potion
    /* 0x1E */{"O_mD_arw", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // City Arrow
    /* 0x1F */{"O_mD_blue", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // City Blue Potion
    /* 0x20 */{"B_mD_oil", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1}, // City Oil
    /* Vanilla struct values
    {"B_mD_sold", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, -0x8000, 0}, 0, 0, 0, 0, -1, -1},
    {"B_mD_oil", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_red", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"B_mD_milk", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_bott", 6, 12, -1, -1, -1, 9, 15, 0.0f, 1.0f, 0, {0, 0, 0}, 4, 0, 0, 0, 3, 1},
    {"O_mD_arw", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_SHB", 3, -1, -1, -1, -1, -1, -1, 30.0f, 1.0f, 0, {0, 0x7FFF, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_SHA", 3, -1, -1, -1, -1, -1, -1, 30.0f, 1.0f, 0, {0, 0x7FFF, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_bomb", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1},
    {"O_mD_pg", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1},
    {"O_mD_bi", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1},
    {"O_mD_bmcs", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1},
    {"O_mD_bmc2", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1},
    {"O_mD_jira", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, 0, -1},
    {"O_mD_bott", 6, 12, -1, -1, -1, 9, 15, 0.0f, 1.0f, 0, {0, 0, 0}, 4, 0, 0, 0, 1, 0},
    {"O_mD_hati", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_pach", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_blue", 3, -1, -1, -1, -1, -1, -11, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_hawk", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_marm", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_marm", 4, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_gren", 3, -1, -1, -1, -1, -1, -1, 0.0f, 1.0f, 0, {0, 0, 0}, 0, 0, 0, 0, -1, -1},
    {"O_mD_bott", 6, 12, -1, -1, -1, 9, 15, 0.0f, 1.0f, 0, {0, 0, 0}, 4, 0, 0, 0, 12, 0},
    */
};

int CheckShopItemCreateHeap(fopAc_ac_c* i_this) {
    daShopItem_c* a_this = static_cast<daShopItem_c*>(i_this);

    u8 a_ShopItemID = a_this->getShopItemID();
    return a_this->CreateItemHeap(daShopItem_c::mData[a_ShopItemID].get_arcName(),
                                  daShopItem_c::mData[a_ShopItemID].get_bmdName(),
                                  daShopItem_c::mData[a_ShopItemID].get_btk1Name(),
                                  daShopItem_c::mData[a_ShopItemID].get_bpk1Name(),
                                  daShopItem_c::mData[a_ShopItemID].get_bck1Name(),
                                  daShopItem_c::mData[a_ShopItemID].get_bxa1Name(),
                                  daShopItem_c::mData[a_ShopItemID].get_brk1Name(),
                                  daShopItem_c::mData[a_ShopItemID].get_btp1Name());
}
