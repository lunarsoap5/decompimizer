// This code is copied/modified from the decompgz proejct and has been modified to fit the needs of htis framework.
// https://github.com/zsrtp/decompgz

#include "rando/tools/draw.h"
#include "d/d_com_inf_game.h"
#include "JSystem/J2DGraph/J2DOrthoGraph.h"
#include "JSystem/JUtility/JUTTexture.h"
#include "JSystem/JUtility/JUTDbPrint.h"
#include "JSystem/JUtility/JUTReport.h"
#include "m_Do/m_Do_mtx.h"

#include <cmath>

static void setupGXPrimitive() {
    GXSetNumChans(1);
    GXSetNumTexGens(0);
    GXSetNumTevStages(1);
    GXSetTevOrder(GX_TEVSTAGE0, GX_TEXCOORD_NULL, GX_TEXMAP_NULL, GX_COLOR0A0);
    GXSetTevColorIn(GX_TEVSTAGE0, GX_CC_ZERO, GX_CC_ZERO, GX_CC_ZERO, GX_CC_RASC);
    GXSetTevColorOp(GX_TEVSTAGE0, GX_TEV_ADD, GX_TB_ZERO, GX_CS_SCALE_1, GX_TRUE, GX_TEVPREV);
    GXSetTevAlphaIn(GX_TEVSTAGE0, GX_CA_ZERO, GX_CA_ZERO, GX_CA_ZERO, GX_CA_RASA);
    GXSetTevAlphaOp(GX_TEVSTAGE0, GX_TEV_ADD, GX_TB_ZERO, GX_CS_SCALE_1, GX_TRUE, GX_TEVPREV);

    GXSetChanCtrl(GX_COLOR0A0, GX_FALSE, GX_SRC_REG, GX_SRC_VTX, 0, GX_DF_NONE, GX_AF_NONE);

    GXSetZMode(GX_FALSE, GX_ALWAYS, GX_FALSE);
    GXSetBlendMode(GX_BM_BLEND, GX_BL_SRCALPHA, GX_BL_INVSRCALPHA, GX_LO_OR);
    GXSetAlphaCompare(GX_ALWAYS, 0, GX_AOP_OR, GX_ALWAYS, 0);
    GXSetCullMode(GX_CULL_NONE);

    GXLoadPosMtxImm(g_mDoMtx_identity, GX_PNMTX0);
    GXSetCurrentMtx(0);

    GXClearVtxDesc();
    GXSetVtxDesc(GX_VA_POS, GX_DIRECT);
    GXSetVtxDesc(GX_VA_CLR0, GX_DIRECT);
    GXSetVtxAttrFmt(GX_VTXFMT0, GX_VA_POS, GX_POS_XY, GX_F32, 0);
    GXSetVtxAttrFmt(GX_VTXFMT0, GX_VA_CLR0, GX_CLR_RGBA, GX_RGBA8, 0);
}

void drawRectOutline(f32 x, f32 y, f32 w, f32 h, f32 thickness, GXColor color) {
    setupGXPrimitive();

    f32 x1 = x;
    f32 y1 = y;
    f32 x2 = x + w;
    f32 y2 = y + h;

    GXBegin(GX_QUADS, GX_VTXFMT0, 16);

    GXPosition2f32(x1, y1);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2, y1);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2, y1 + thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x1, y1 + thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);

    GXPosition2f32(x1, y2 - thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2, y2 - thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2, y2);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x1, y2);
    GXColor4u8(color.r, color.g, color.b, color.a);

    GXPosition2f32(x1, y1 + thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x1 + thickness, y1 + thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x1 + thickness, y2 - thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x1, y2 - thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);

    GXPosition2f32(x2 - thickness, y1 + thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2, y1 + thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2, y2 - thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2 - thickness, y2 - thickness);
    GXColor4u8(color.r, color.g, color.b, color.a);

    GXEnd();
}

void drawFilledRect(f32 x, f32 y, f32 w, f32 h, GXColor color) {
    setupGXPrimitive();

    GXBegin(GX_QUADS, GX_VTXFMT0, 4);
    GXPosition2f32(x, y);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + w, y);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + w, y + h);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x, y + h);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXEnd();
}

void drawFilledRoundedTopRect(f32 x, f32 y, f32 w, f32 h, f32 radius, GXColor color) {
    setupGXPrimitive();

    // Clamp radius to half of width/height
    if (radius > w / 2.0f) radius = w / 2.0f;
    if (radius > h / 2.0f) radius = h / 2.0f;

    // Draw main body (below the rounded corners)
    GXBegin(GX_QUADS, GX_VTXFMT0, 4);
    GXPosition2f32(x, y + radius);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + w, y + radius);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + w, y + h);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x, y + h);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXEnd();

    // Draw top middle strip (between the two corners)
    GXBegin(GX_QUADS, GX_VTXFMT0, 4);
    GXPosition2f32(x + radius, y);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + w - radius, y);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + w - radius, y + radius);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + radius, y + radius);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXEnd();

    // Draw rounded corners using triangle fans
    static const int CORNER_SEGMENTS = 8;
    static const f32 PI = 3.14159265f;

    // Top-left corner
    f32 cx = x + radius;
    f32 cy = y + radius;
    GXBegin(GX_TRIANGLEFAN, GX_VTXFMT0, CORNER_SEGMENTS + 2);
    GXPosition2f32(cx, cy);
    GXColor4u8(color.r, color.g, color.b, color.a);
    for (int i = 0; i <= CORNER_SEGMENTS; i++) {
        f32 angle = PI + (PI / 2.0f) * (f32)i / (f32)CORNER_SEGMENTS;
        GXPosition2f32(cx + radius * cosf(angle), cy + radius * sinf(angle));
        GXColor4u8(color.r, color.g, color.b, color.a);
    }
    GXEnd();

    // Top-right corner
    cx = x + w - radius;
    cy = y + radius;
    GXBegin(GX_TRIANGLEFAN, GX_VTXFMT0, CORNER_SEGMENTS + 2);
    GXPosition2f32(cx, cy);
    GXColor4u8(color.r, color.g, color.b, color.a);
    for (int i = 0; i <= CORNER_SEGMENTS; i++) {
        f32 angle = (3.0f * PI / 2.0f) + (PI / 2.0f) * (f32)i / (f32)CORNER_SEGMENTS;
        GXPosition2f32(cx + radius * cosf(angle), cy + radius * sinf(angle));
        GXColor4u8(color.r, color.g, color.b, color.a);
    }
    GXEnd();
}

void drawVerticalLine(f32 x, f32 y1, f32 y2, f32 thickness, GXColor color) {
    setupGXPrimitive();

    f32 halfThick = thickness / 2.0f;

    GXBegin(GX_QUADS, GX_VTXFMT0, 4);
    GXPosition2f32(x - halfThick, y1);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + halfThick, y1);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x + halfThick, y2);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x - halfThick, y2);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXEnd();
}

void drawHorizontalLine(f32 x1, f32 x2, f32 y, f32 thickness, GXColor color) {
    setupGXPrimitive();

    f32 halfThick = thickness / 2.0f;

    GXBegin(GX_QUADS, GX_VTXFMT0, 4);
    GXPosition2f32(x1, y - halfThick);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2, y - halfThick);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x2, y + halfThick);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXPosition2f32(x1, y + halfThick);
    GXColor4u8(color.r, color.g, color.b, color.a);
    GXEnd();
}

void drawFilledCircle(f32 cx, f32 cy, f32 radius, GXColor fillColor, GXColor outlineColor,
                        f32 outlineWidth) {
    setupGXPrimitive();

    static const int NUM_SEGMENTS = 16;

    f32 outerRadius = radius;
    GXBegin(GX_TRIANGLEFAN, GX_VTXFMT0, NUM_SEGMENTS + 2);
    GXPosition2f32(cx, cy);
    GXColor4u8(outlineColor.r, outlineColor.g, outlineColor.b, outlineColor.a);
    for (int i = 0; i <= NUM_SEGMENTS; i++) {
        f32 angle = (f32)i * (2.0f * 3.14159f / (f32)NUM_SEGMENTS);
        f32 x = cx + outerRadius * cosf(angle);
        f32 y = cy + outerRadius * sinf(angle);
        GXPosition2f32(x, y);
        GXColor4u8(outlineColor.r, outlineColor.g, outlineColor.b, outlineColor.a);
    }
    GXEnd();

    f32 innerRadius = radius - outlineWidth;
    GXBegin(GX_TRIANGLEFAN, GX_VTXFMT0, NUM_SEGMENTS + 2);
    GXPosition2f32(cx, cy);
    GXColor4u8(fillColor.r, fillColor.g, fillColor.b, fillColor.a);
    for (int i = 0; i <= NUM_SEGMENTS; i++) {
        f32 angle = (f32)i * (2.0f * 3.14159f / (f32)NUM_SEGMENTS);
        f32 x = cx + innerRadius * cosf(angle);
        f32 y = cy + innerRadius * sinf(angle);
        GXPosition2f32(x, y);
        GXColor4u8(fillColor.r, fillColor.g, fillColor.b, fillColor.a);
    }
    GXEnd();
}

static void getThemeRGB(u32 theme, u8& r, u8& g, u8& b) {
    r = (theme >> 24) & 0xFF;
    g = (theme >> 16) & 0xFF;
    b = (theme >> 8) & 0xFF;
}

static void warmBlendRGB(u8& r, u8& g, u8& b, f32 blend) {
    static const f32 WARM_R = 165.0f;
    static const f32 WARM_G = 218.0f;
    static const f32 WARM_B = 115.0f;
    r = (u8)(r * (1.0f - blend) + WARM_R * blend);
    g = (u8)(g * (1.0f - blend) + WARM_G * blend);
    b = (u8)(b * (1.0f - blend) + WARM_B * blend);
}

GXColor getThemedBorderColor(u32 theme, u8 alpha) {
    GXColor c;
    getThemeRGB(theme, c.r, c.g, c.b);
    warmBlendRGB(c.r, c.g, c.b, 0.52f);
    c.a = alpha;
    return c;
}

GXColor getThemedHighlightColor(u32 theme, u8 alpha) {
    GXColor c;
    u8 r, g, b;
    getThemeRGB(theme, r, g, b);
    warmBlendRGB(r, g, b, 0.52f);

    // Brighten toward white
    c.r = (u8)((r + 255) / 2);
    c.g = (u8)((g + 255) / 2);
    c.b = (u8)((b + 255) / 2);
    c.a = alpha;
    return c;
}

GXColor getThemedSeparatorColor(u32 theme, u8 alpha) {
    GXColor c;
    u8 r, g, b;
    getThemeRGB(theme, r, g, b);
    warmBlendRGB(r, g, b, 0.6f);
    c.r = (u8)(r * 0.85f);
    c.g = (u8)(g * 0.85f);
    c.b = (u8)(b * 0.85f);
    c.a = alpha;
    return c;
}

int randoPrint(int x, int y, u32 color, char const* string, ...) {
    JUTDbPrint::getManager()->setVisible(true);
    char buffer[256];

    va_list list;
    va_start(list, string);
    vsnprintf(buffer, sizeof(buffer), string, list);
    va_end(list);

    JUTDbPrint::getManager()->flush();

    static JUtility::TColor ShadowDarkColor(0, 0, 0, 0x80);
    JUTDbPrint::getManager()->setCharColor(ShadowDarkColor);

    JUTReport(x + 2, y + 2, buffer);
    JUTDbPrint::getManager()->flush();

    JUTDbPrint::getManager()->setCharColor(color);
    JUTReport(x, y, buffer);

    JUTDbPrint::getManager()->flush();
    return 1;
}
