#ifndef VERIFY_ITEM_FUNCTIONS_H
#define VERIFY_ITEM_FUNCTIONS_H

#include "dolphin/types.h"

bool isMajorItem(uint);
bool isKeyItem(uint);
bool haveItem(uint item);
uint getProgressiveSword();
uint getProgressiveBow();
uint getProgressiveSkill();
uint getProgressiveSkybook();
uint getProgressiveKeyShard();
uint getProgressiveMirrorShard();
uint getProgressiveFusedShadow();
u8 getWarashibeItemCount();
uint verifyProgressiveItem(uint item);

#endif  // VERIFY_ITEM_FUNCTIONS_H
