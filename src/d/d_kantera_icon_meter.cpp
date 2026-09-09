#include "d/dolzel.h" // IWYU pragma: keep

#include "d/d_kantera_icon_meter.h"
#include "JSystem/J2DGraph/J2DGrafContext.h"
#include "JSystem/J2DGraph/J2DScreen.h"
#include "d/d_com_inf_game.h"
#include "d/d_meter_HIO.h"
#include "d/d_pane_class.h"
#include "rando/seed/seed.h"
#include "rando/tools/tools.h"

dKantera_icon_c::dKantera_icon_c()
{
    initiate();
}

dKantera_icon_c::~dKantera_icon_c()
{
    delete mpKanteraIcon->getScreen();
    delete mpKanteraIcon;
    mpKanteraIcon = NULL;

    delete mpParent;
    mpParent = NULL;

    delete mpGauge;
    mpGauge = NULL;
}

void dKantera_icon_c::initiate()
{
    mpKanteraIcon = new dDlst_KanteraIcon_c();

    J2DScreen* scrn = new J2DScreen();
    scrn->setPriority("zelda_kantera_icon_mater.blo", 0x20000, dComIfGp_getMain2DArchive());
    dPaneClass_showNullPane(scrn);
    mpKanteraIcon->setScreen(scrn);

    mpParent = new CPaneMgr(scrn, MULTI_CHAR('kan_m_n'), 2, NULL);

    mpGauge = new CPaneMgr(scrn, MULTI_CHAR('yellow_m'), 0, NULL);
}

void dKantera_icon_c::setAlphaRate(f32 alphaRate)
{
    mpParent->setAlphaRate(alphaRate);
}

void dKantera_icon_c::setPos(f32 x, f32 y)
{
    mpParent->translate(x + g_drawHIO.mLanternIconMeterPosX, y + g_drawHIO.mLanternIconMeterPosY);
}

void dKantera_icon_c::setScale(f32 h, f32 v)
{
    mpParent->scale(h * g_drawHIO.mLanternIconMeterSize, v * g_drawHIO.mLanternIconMeterSize);
}

void dKantera_icon_c::setNowGauge(u16 h, u16 v)
{
    mpGauge->scale((f32)v / (f32)h, 1.0f);
    if (!g_seedInfo.isLanternRainbow())
    {
        u8* lanternColorPtr = g_seedInfo.getHeaderPtr()->getLanternColorPtr();
        mpGauge->setBlackWhite(JUtility::TColor(lanternColorPtr[0], lanternColorPtr[1], lanternColorPtr[2], 255),
                               JUtility::TColor(lanternColorPtr[0], lanternColorPtr[1], lanternColorPtr[2], 255));
    }
    else
    {
        GXColor rgbColor = getRainbowRGB(127.5f);

        mpGauge->setBlackWhite(JUtility::TColor(rgbColor.r / 2, rgbColor.g / 2, rgbColor.b / 2, 255),
                               JUtility::TColor(rgbColor.r / 2, rgbColor.g / 2, rgbColor.b / 2, 255));
    }
}

void dDlst_KanteraIcon_c::draw()
{
    J2DGrafContext* curGraf = dComIfGp_getCurrentGrafPort();
    curGraf->setup2D();
    mp_scrn->draw(0.0f, 0.0f, curGraf);
}

dDlst_KanteraIcon_c::~dDlst_KanteraIcon_c() {}
