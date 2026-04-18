#ifndef RETURNPLACES_H
#define RETURNPLACES_H

#include "dolphin/types.h"
#include <string>
#include "rando/returnPlaces.h"

const ReturnPlace* ReturnPlaceSection::getReturnPlace(u8 stageIDX, s8 roomNo, s8 point, s8 layer) const
{

    const u8* matchIndexPtr = NULL;
    const u8* headerPtr = (const u8*)(&numComparisons);
    const Comparison* comparisonsTable = (const Comparison*)(headerPtr + comparisonsOffset);

    u32 totalEntries = numComparisons;
    for (u32 i = 0; i < totalEntries; i++)
    {
        const Comparison* comparison = &comparisonsTable[i];
        if (comparison->stageIDX == stageIDX)
        {
            s8 compRoomNo = comparison->roomNo;
            s8 compPoint = comparison->point;
            s8 compLayer = comparison->layer;

            if ((compRoomNo == -1 || compRoomNo == roomNo) && (compPoint == -1 || compPoint == point) &&
                (compLayer == -1 || compLayer == layer))
            {
                const u8* matchIndexTable = headerPtr + matchIndexOffset;
                matchIndexPtr = &matchIndexTable[i];
                break;
            }
        }
    }

    // Returns nullptr if no mapping was found, else returns a pointer to the new place to store. A stageIdx of 0xFF
    // in the returned pointer represents that there is no valid returnPlace.
    if (matchIndexPtr != NULL)
    {
        u8 index = *matchIndexPtr;
        if (index < this->numReturnPlaces)
        {
            const ReturnPlace* returnPlacesTable =
                (const ReturnPlace*)(headerPtr + returnPlacesOffset);
            return &returnPlacesTable[index];
        }
    }
    return NULL;
}

#endif  // RETURNPLACES_H
