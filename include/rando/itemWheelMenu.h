#ifndef ITEM_WHEEL_MENU_H
#define ITEM_WHEEL_MENU_H

#include "dolphin/types.h"
#include "d/d_menu_ring.h"
#include "JSystem/J2DGraph/J2DTextbox.h"

class customMenuRing_c {
public:
    J2DTextBox* mpItemWheelText;

    int _initialize();
    int _delete();
    int execute();
    int draw();
    void handleItemWheelMenu(dMenu_Ring_c*);
    void setUpItemWheelMenuText();

    bool isRingOpen() {return mRingOpen;}
    void setRingOpen(bool val) {mRingOpen = val;}
    void dontDisplayMenu() { shouldDisplayMenu = false; }
    void drawRingThisFrame() { ringDrawnThisFrame = true; }
    void setDisplayMenu(bool value) { shouldDisplayMenu = value; }
    bool shouldChangeQuestItem() { return updatedQuestItem; }
    void changeQuestItem(bool state) { updatedQuestItem = state; }
    bool shouldDrawRingThisFrame() {return ringDrawnThisFrame;}
    void resetRingDrawnThisFrame() {ringDrawnThisFrame = false;}

    bool mRingOpen;
    bool shouldDisplayMenu;
    bool ringDrawnThisFrame;
    bool updatedQuestItem;
};

void setHUDButtonsAlpha(bool);

extern customMenuRing_c g_customMenuRing;

#endif  // ITEM_WHEEL_MENU_H
