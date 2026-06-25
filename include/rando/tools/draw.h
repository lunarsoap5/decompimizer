// This code is copied/modified from the decompgz proejct and has been modified to fit the needs of htis framework.
// https://github.com/zsrtp/decompgz

#ifndef DRAW_H
#define DRAW_H

#include <dolphin/gx.h>

struct ResTIMG;

void drawRectOutline(f32 x, f32 y, f32 w, f32 h, f32 thickness, GXColor color);
void drawFilledRect(f32 x, f32 y, f32 w, f32 h, GXColor color);
void drawFilledRoundedTopRect(f32 x, f32 y, f32 w, f32 h, f32 radius, GXColor color);
void drawVerticalLine(f32 x, f32 y1, f32 y2, f32 thickness, GXColor color);
void drawHorizontalLine(f32 x1, f32 x2, f32 y, f32 thickness, GXColor color);
void drawFilledCircle(f32 cx, f32 cy, f32 radius, GXColor fillColor, GXColor outlineColor,
                        f32 outlineWidth);
GXColor getThemedBorderColor(u32 theme, u8 alpha);
GXColor getThemedHighlightColor(u32 theme, u8 alpha);
GXColor getThemedSeparatorColor(u32 theme, u8 alpha);
int randoPrint(int x, int y, u32 color, char const* string, ...);

#endif // DRAW_H
