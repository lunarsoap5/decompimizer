#ifndef TOOLS_H
#define TOOLS_H

#include "dolphin/types.h"
#include "dolphin/gx/GXStruct.h"
#include "SSystem/SComponent/c_xyz.h"
#include "SSystem/SComponent/c_sxyz.h"
#include "JSystem/JKernel/JKRHeap.h"
#include "JSystem/J2DGraph/J2DPicture.h"

bool playerIsInRoomStage(s32 room, const char* stage);
void checkTransformFromWolf();
u8 setNextWarashibeItem();
void offWarashibeItem(u8 item);
int initCreatePlayerItem(uint item, uint flag, const cXyz* pos, int roomNo, const csXyz* angle, const cXyz* scale);
bool checkButtonComboAnalog(uint combo);
void handleQuickTransform();
int readFile(const char* file, bool allocFromHead, u8** dataOut);
GXColor getRainbowRGB(f32 amplitude);
void adjustMidnaHairColor(GXColor);
int getStageID(const char* stage);
void replaceEquipItemColor(GXColor wave1RGBA, GXColor wave2RGBA);
void replaceEquipItemColor(GXColor);
bool checkToTSwordReqEquip();
int getCurrentStageID();
bool checkButtonsHeld(u32);
bool checkButtonsPressedThisFrame(u32);
void randoCreateHeap();
JKRHeap* getRandoHeap();
J2DPicture* randoCopyItemArchiveTexture(JKRArchive*, const char*, JKRHeap*, J2DPicture*, ResTIMG*); 

#endif  // TOOLS_H
