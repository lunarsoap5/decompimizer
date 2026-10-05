
public class TextureRecolor
{
    public string? ArchiveDirectory { get; set; }
    public List<TextureRecolorOptions>? TextureOptions { get; set; }

    public TextureRecolor(string arcDir, List<TextureRecolorOptions> texOptions)
    {
        ArchiveDirectory = arcDir;
        TextureOptions = texOptions;
    }
}

public class TextureRecolorOptions
{
    public string? FileName { get; set; }
    public uint TextureIndex { get; set; }
    public TextureRecolorType RecolorType { get; set; } // 0 for grayscale, 1 for palette recolor, 2 for hue recolor
    public RgbaColor OldColor { get; set; }
    public RgbaColor NewColor { get; set; }
    public int Tolerance { get; set; }

    public TextureRecolorOptions(string fName, uint index, TextureRecolorType type, RgbaColor oldColor, RgbaColor newColor, int tol)
    {
        FileName = fName;
        TextureIndex = index;
        RecolorType = type;
        OldColor = oldColor;
        NewColor = newColor;
        Tolerance = tol;
    }

    public static TextureRecolorOptions Greyscale(string fName, uint index, RgbaColor newColor)
        => new(fName, index, TextureRecolorType.Greyscale, new RgbaColor(0, 0, 0, 0), newColor, 0);

    public static TextureRecolorOptions Material(string fName, uint index, RgbaColor newColor)
        => new(fName, index, TextureRecolorType.Material, new RgbaColor(0, 0, 0, 0), newColor, 0);
}

public enum TextureRecolorType
{
    Greyscale = 0,
    Palette = 1,
    Hue = 2,
    Material = 3,
}

public static class CosmeticFunctions
{
    public static List<TextureRecolor> GenerateTextureCosmetics()
    {
        RgbaColor heartColor = new RgbaColor(0x0, 0x6e, 0xFF, 255);
        RgbaColor enemyColor = new RgbaColor(2, 93, 0, 255);
        RgbaColor purpleColor = new RgbaColor(0x9b, 0x6e, 0xab, 255);
        List<TextureRecolor> recolorOptions =
            [
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Kmdl.arc",
                    [
                        // Hero's Clothes
                        TextureRecolorOptions.Greyscale(@"bmwr/al.bmd", 0, purpleColor)
                    ]
                ),
                // Item Icons
                new TextureRecolor(
                    @"extractedISO/root/res/Layout/itemicon.arc",
                    [
                        // Ordon Sword Icon
                         TextureRecolorOptions.Greyscale(@"timg/tt_kokirinoken_s3_tc.bti",0, purpleColor),
                        // Master Sword Icon
                         TextureRecolorOptions.Greyscale(@"timg/ni_mastersword_48.bti",0, purpleColor),
                        // Wooden Sword Icon
                         TextureRecolorOptions.Greyscale(@"timg/im_kinobou_48.bti",0,purpleColor),
                        // Memo Icon
                         TextureRecolorOptions.Greyscale(@"timg/im_kakioki_48.bti",0,purpleColor),
                    ]
                ),
                // Mmdl - Magic Armor
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Mmdl.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmwr/ml.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(180, 30, 30, 255), // roughly red - Red Leather Part of the body
                            purpleColor,
                            25
                        ),
                    ]
                ),
                // Alink - Equipment
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Alink.arc",
                    [
                        // Spinner
                        new TextureRecolorOptions(
                            @"bmdr/al_sp.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(66, 36, 16, 255), // roughly red
                            purpleColor,
                            25
                        ),
                        // Ordon Sword
                        TextureRecolorOptions.Greyscale(@"bmwr/al_swa.bmd",1,purpleColor),
                        // Master Sword - Handle
                        TextureRecolorOptions.Greyscale(@"bmwe/al_swm.bmd",0,purpleColor),
                        // Master Sword - Blade
                        TextureRecolorOptions.Greyscale(@"bmwe/al_swm.bmd",2,purpleColor),
                    ]
                ),
                // MstrSword
                new TextureRecolor(
                    @"extractedISO/root/res/Object/MstrSword.arc",
                    [
                        // Master Sword - Handle
                         TextureRecolorOptions.Greyscale(@"bmdr/o_al_swm.bmd",1,purpleColor),
                        // Master Sword - Blade
                         TextureRecolorOptions.Greyscale(@"bmdr/o_al_swm.bmd",3,purpleColor),
                    ]
                ),
                // Wmdl - Wolf Link and Midna on Back
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Wmdl.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmwr/wl.bmd",0,purpleColor),
                    ]
                ),
                // Always
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Always.arc",
                    [
                        // Piece of Heart
                        new TextureRecolorOptions(
                            @"bmde/o_g_hutk.bmd",
                            0xFF010103,
                            TextureRecolorType.Material,
                            new RgbaColor(0, 0, 0, 255),
                            heartColor,
                            25
                        ),
                        // Heart Container
                        new TextureRecolorOptions(
                            @"bmde/o_g_hutu.bmd",
                            0xFF010103,
                            TextureRecolorType.Material,
                            new RgbaColor(0, 0, 0, 255),
                            heartColor,
                            25
                        ),
                        // Heart Refill
                        new TextureRecolorOptions(
                            @"bmde/o_g_hart.bmd",
                            0xFF010100,
                            TextureRecolorType.Material,
                            new RgbaColor(0, 0, 0, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                // Demo 31
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Demo31_10.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmde/demo31_oghart_cut10_gp_1.bmd", // Demo - Heart
                            0xFF010100,
                            TextureRecolorType.Material,
                            new RgbaColor(0, 0, 0, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                new TextureRecolor(
                    @"extractedISO/root/res/Object/O_gD_hutk.arc",
                    [
                        // Get Display - Piece of Heart
                        new TextureRecolorOptions(
                            @"bmde/o_gd_hutk.bmd",
                            0xFF010103,
                            TextureRecolorType.Material,
                            new RgbaColor(0, 0, 0, 255),
                            new RgbaColor(0x0, 0x6e, 0xFF, 255),
                            25
                        ),
                    ]
                ),
                new TextureRecolor(
                    @"extractedISO/root/res/Object/O_gD_hutu.arc",
                    [
                        // Get Display - Heart Container
                        new TextureRecolorOptions(
                            @"bmde/o_gd_hutu.bmd",
                            0xFF010103,
                            TextureRecolorType.Material,
                            new RgbaColor(0, 0, 0, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                // Memo Actor
                new TextureRecolor(
                    @"extractedISO/root/res/Object/O_gD_mem2.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/o_gd_memo.bmd",0,new RgbaColor(77, 53, 41, 255)),
                    ]
                ),
                // Custom Sketch Actor
                new TextureRecolor(
                    @"extractedISO/root/res/Object/O_gD_mem3.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/o_gd_memo.bmd",0,new RgbaColor(76,119,180, 255)),
                    ]
                ),
                // Custom Unpowered Rod Actor
                new TextureRecolor(
                    @"extractedISO/root/res/Object/O_gD_CROD1.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/o_gd_al_crod.bmd",0,new RgbaColor(33, 20, 20, 255)),
                    ]
                ),

                // Enemy color palette changes
                // Armos
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ai.arc",
                    [
                        // Armos 
                        new TextureRecolorOptions(
                            @"bmdr/ai.bmd",
                            0xFFFF0201,
                            TextureRecolorType.Material,
                            new RgbaColor(0, 0, 0, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Baba Serpent
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_db.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/db.bmd",
                            1,
                            TextureRecolorType.Hue,
                            new RgbaColor(41, 11, 7, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Baby Gohma
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_gm.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdv/gb.bmd",0,enemyColor),
                    ]
                ),
                // Bari
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_dk.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/dk.bmd",0, enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/dk.bmd",1, enemyColor)
                    ]
                ),
                // Beamos - GM
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Obj_bm.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/bm.bmd",0,enemyColor),
                    ]
                ),
                // Beamos - ToT
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Obj_lv6bm.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmde/bm6.bmd",1,enemyColor),
                    ]
                ),
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_bm6.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmde/bm6.bmd",1,enemyColor),
                    ]
                ),
                // Big Baba
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_gb.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/gb.bmd", 0,enemyColor),
                        new TextureRecolorOptions(
                            @"bmdr/gf.bmd", // Base/Roots
                            1,
                            TextureRecolorType.Hue,
                            new RgbaColor(61, 40, 42, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Bokoblin - Blue
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_oc.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/oc.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(29, 28, 35, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Bokoblin - Red
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_oc2.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/oc2.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(29, 20, 25, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Bombling
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_bi.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/bi.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(20, 23, 8, 255),
                            heartColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/bi.bmd",
                            3,
                            TextureRecolorType.Hue,
                            new RgbaColor(20, 23, 8, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                // Bombfish
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_bg.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/bg.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(7, 19, 20, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Bomskit
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_cr.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/cr.bmd",0,enemyColor),
                    ]
                ),
                // Bubble
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_bu.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/bu.bmd",1,enemyColor),
                    ]
                ),
                // Bulblin
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_rd.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/rd.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(19, 22, 9, 255),
                            enemyColor,
                            20
                        ),
                    ]
                ),
                // Chilfos
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_kk.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmde/kk.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmde/kk_weapon.bmd",0,enemyColor),
                    ]
                ),
                // Chu Worm
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_sm.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/sc.bmd",1,enemyColor),
                    ]
                ),
                // Darknut - ToT
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_tnp.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/tn.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_arm_l.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_arm_r.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_chest_b.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_chest_f.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_head_b.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_head_f.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_shoulder_l.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_shoulder_r.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_waist_b.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_waist_f.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_waist_l.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn_armor_waist_r.bmd",0,enemyColor),
                        new TextureRecolorOptions(
                            @"bmdr/tn_shield.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(35, 31, 25, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_sword_a.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(49, 41, 28, 255),
                            enemyColor,
                            20
                        ),
                    ]
                ),
                // Darknut - Enemy
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_tnp2.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/tn2.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_arm_l.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_arm_r.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_chest_b.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_chest_f.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_head_b.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_head_a.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_shoulder_l.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_shoulder_r.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_waist_b.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_waist_f.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_armor_waist_l.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale( @"bmdr/tn2_armor_waist_r.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/tn2_shield.bmd",0,enemyColor),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_sword_a.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(49, 41, 28, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_mace.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(45, 41, 32, 255),
                            enemyColor,
                            20
                        ),
                    ]
                ),
                // Deku Baba
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_hb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/hb.bmd",
                            1,
                            TextureRecolorType.Hue,
                            new RgbaColor(42, 49, 48, 255),
                            enemyColor,
                            80
                        ),
                    ]
                ),
                // Deku Like
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_DF.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/df.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(32, 15, 16, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Dodongo
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_dd.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/dd.bmd",
                            1,
                            TextureRecolorType.Hue,
                            new RgbaColor(64, 14, 3, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Dinalfos
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_mf.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/mf.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(16, 7, 0, 255), // Note current hue modification doesn't play well with white/silver, so we target the scale color instead of the armor
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Keese
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ba.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/ba.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(37, 19, 3, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Keese - Fire
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_fb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/fb.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(38, 7, 0, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Water Toadpoli
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_tk.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/tk.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(12, 24, 27, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Fire Toadpoli
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_tk2.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/tk2.bmd",
                            2,
                            TextureRecolorType.Hue,
                            new RgbaColor(27, 9, 3, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Freezard
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_fl.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmde/fl_model.bmd",0,enemyColor),
                    ]
                ),
                // Guay
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ge.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/ge_model.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/ge_model.bmd",1,enemyColor),
                    ]
                ),
                // Helmasaur/Helmasaurus
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_mm.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/dm.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/mm.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/mm.bmd",1,enemyColor),
                    ]
                ),
                // Helmasaur/Helmasaurus Armor
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_mm_mt.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/dm_met.bmd",0,heartColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/mt.bmd",0,heartColor),
                    ]
                ),
                // Kargarok - Enemy
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_kr.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/kr.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(39, 29, 22, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Kargarok - Plumm
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_kc.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/kc.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(39, 29, 22, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Leever
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_rb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/rb.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(22, 47, 25, 255),
                            heartColor,
                            80
                        ),
                    ]
                ),
                // Lizalfos
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_dn.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/dn.bmd",1,heartColor),
                    ]
                ),
                // Mini Freezard
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_fz.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmde/fz.bmd",
                            0xFF010100,
                            TextureRecolorType.Material,
                            new RgbaColor(0, 0, 0, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Moldorm
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_sw.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/sw.bmd",0,enemyColor),
                    ]
                ),
                // Poe - Normal
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_hp.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/ef_glow.bmd",
                            0xFFFF0200, // outer lantern glow
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/ef_glow.bmd",
                            0xFFFF0100, // inner lantern glow
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Poe - AG
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_po.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/ef_glow.bmd",
                            0xFFFF0200, // outer lantern glow
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/ef_glow.bmd",
                            0xFFFF0100, // inner lantern glow
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Puppet
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_fs.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/fs.bmd",0,enemyColor),
                    ]
                ),
                // Rat
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ms.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/ms.bmd",0,enemyColor),
                    ]
                ),
                // Redead Knight
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_gi.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/gi.bmd",
                            1,
                            TextureRecolorType.Hue,
                            new RgbaColor(50, 46, 45, 255),
                            enemyColor,
                            80
                        ),
                    ]
                ),
                // Shadow Beast
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_s2.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/s2.bmd",1,enemyColor),
                    ]
                ),
                // Shadow Bulblin
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_rdy.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/yb.bmd",1,enemyColor),
                    ]
                ),
                // Shadow Deku Baba
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_yd.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/yd.bmd",2,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/yd.bmd",0,enemyColor),
                    ]
                ),
                // Shadow Insect - Winged
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_tm.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/tm_tw.bmd",
                            0xFF01FF03,
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Shadow Insect - Ground
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ym.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/ym_tw.bmd",
                            0xFF01FF02,
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Shadow Kargorok - Carrier
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_yc.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/yc.bmd",1,enemyColor),
                    ]
                ),
                // Shadow Kargorok - Enemy
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_yr.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/yr.bmd",1,enemyColor),
                    ]
                ),
                // Shadow Keese
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_yk.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/yk.bmd",0,enemyColor),
                    ]
                ),
                // Shadow Vermin
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_yg.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/yg.bmd",0,enemyColor),
                    ]
                ),
                // Shell Blade
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_sb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/sb.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(31, 18, 16, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Skullfish
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_sg.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/sg.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(41, 20, 12, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Skulltula
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_st.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/st2.bmd",
                            1,
                            TextureRecolorType.Hue,
                            new RgbaColor(31, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/st.bmd",
                            1,
                            TextureRecolorType.Hue,
                            new RgbaColor(31, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Stalfos
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_sf.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/sf.bmd",0,enemyColor),
                    ]
                ),
                // Stalhound
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_sh.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/sh.bmd",0,enemyColor),
                    ]
                ),
                // Stalkin
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_bs.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/bs.bmd",0,enemyColor),
                    ]
                ),
                // Staltroop
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_zs.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmde/zs.bmd",0,enemyColor),
                    ]
                ),
                // Tektite - Blue
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ttb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/tt_b.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(19, 28, 35, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Tektite - Red
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ttr.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/tt.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(42, 14, 6, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Toado
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ot.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/ot.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(19, 25, 31, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Torch Slug
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_hm.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/hm.bmd",
                            0xFF00FF00,
                            TextureRecolorType.Material,
                            new RgbaColor(19, 25, 31, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Walltula
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ws.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/ws.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(42, 9, 22, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // White Wolfos
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ww.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/ww.bmd",0,enemyColor),
                    ]
                ),
                // Young Gohma
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_kg.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/kg.bmd",
                            3,
                            TextureRecolorType.Hue,
                            new RgbaColor(19, 27, 32, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Zant Head
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_zm.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/zm.bmd",0,enemyColor),
                    ]
                ),
                // Zant Hand
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_zh.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdv/zh.bmd",0,enemyColor),
                    ]
                ),
                // Ook
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_mk.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/bm.bmd",0,heartColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/bm.bmd",1,heartColor),
                        new TextureRecolorOptions(
                            @"bmdr/mk.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(89, 37, 24, 255),
                            heartColor,
                            20
                        ),
                    ]
                ),
                // Ook - Diababa
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_mb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/mb.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(89, 37, 24, 255),
                            heartColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/mg.bmd",
                            1,
                            TextureRecolorType.Hue,
                            new RgbaColor(20, 23, 8, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                // Dangoro
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_gob.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/mg.bmd",0,enemyColor),
                    ]
                ),
                // Deku Toad
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_dt.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdv/dt.bmd",5,heartColor),
                    ]
                ),
                // Death Sword
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_va.arc",
                    [
                        TextureRecolorOptions.Greyscale(@"bmde/va_weapon.bmd",1,enemyColor),
                    ]
                ),
                // Darkhammer
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_th.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdv/th.bmd",
                            0xFFFF0100,
                            TextureRecolorType.Material,
                            new RgbaColor(61, 60, 51, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Darkhammer - Ball and Chain
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_th_ball.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmde/ib.bmd",0,enemyColor),
                    ]
                ),
                // Skull Kid
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_pm.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/pm.bmd",
                            4,
                            TextureRecolorType.Hue,
                            new RgbaColor(43, 26, 13, 255),
                            enemyColor,
                            25
                        ),
                        /*new TextureRecolorOptions(
                            @"bmdr/pm.bmd",
                            3, enemyColor// This is the leaf on his back in case we want to change it later.
                            
                        ),*/
                    ]
                ),
                // King Bulblin
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_rdb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/rb.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(25, 28, 16, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                // Aeralfos
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_gg.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdr/gg.bmd",0,enemyColor),
                    ]
                ),
                // Phantom Zant
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_pz.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdv/pz.bmd",
                            0xFFFF0000,
                            TextureRecolorType.Material,
                            new RgbaColor(25, 28, 16, 255),
                            heartColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdv/pz.bmd",
                            0xFFFF0200, // The red lines on zant's outfit
                            TextureRecolorType.Material,
                            new RgbaColor(25, 28, 16, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdv/pz.bmd",
                            0xFFFF0002,
                            TextureRecolorType.Material,
                            new RgbaColor(25, 28, 16, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                // Diababa
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_bq.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdv/bq.bmd",
                            8,
                            TextureRecolorType.Hue,
                            new RgbaColor(49, 18, 13, 255),
                            heartColor,
                            25
                        ),
                         TextureRecolorOptions.Greyscale(@"bmdv/bq.bmd",0,heartColor),
                    ]
                ),
                // Diababa - Baba Head
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_bh.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdv/bh.bmd",
                            3,
                            TextureRecolorType.Hue,
                            new RgbaColor(56, 17, 18, 255),
                            heartColor,
                            25
                        ),
                         TextureRecolorOptions.Greyscale(@"bmdv/bh.bmd",0,heartColor),
                    ]
                ),
                // Fyrus
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_fm.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/fm.bmd",
                            0xFF00FF01,
                            TextureRecolorType.Material,
                            new RgbaColor(29, 33, 25, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/fm.bmd",
                            0xFF00FF02,
                            TextureRecolorType.Material,
                            new RgbaColor(19, 27, 32, 255),
                            enemyColor,
                            25
                        ),
                         TextureRecolorOptions.Greyscale(@"bmde/fm_core.bmd",2,enemyColor),
                    ]
                ),
                // Morpheel
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_oh.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdv/oi_head.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(48, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdv/oi_head.bmd",
                            3,
                            TextureRecolorType.Hue,
                            new RgbaColor(48, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdv/oh_core.bmd",
                            3,
                            TextureRecolorType.Hue,
                            new RgbaColor(48, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                         TextureRecolorOptions.Greyscale(@"bmdv/oh.bmd",0,enemyColor),
                        new TextureRecolorOptions(
                            @"bmdr/oi_body.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(48, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/oi_fina.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(48, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/oi_finb.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(48, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/oi_finc.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(48, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/oi_tail.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(48, 4, 3, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Stallord
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_ds.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmde/znta.bmd", // Zant
                            0xFFFF0100,
                            TextureRecolorType.Material,
                            new RgbaColor(56, 17, 18, 255),
                            heartColor,
                            25
                        ),
                         TextureRecolorOptions.Greyscale(@"bmde/znta.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmde/znta.bmd",2,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmde/zk.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmde/ds.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmde/ds_head.bmd",0,enemyColor),
                    ]
                ),
                // Blizetta
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_yo.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmde/yo01.bmd",
                            0xFFFF0100,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo02.bmd",
                            0xFFFF0100,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0100,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0101,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0102,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0103,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0104,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0105,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0106,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0107,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_core.bmd",
                            0xFFFF0108,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_ice.bmd",
                            0xFFFF0100,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                        new TextureRecolorOptions(
                            @"bmde/yo_ice.bmd",
                            0xFFFF0200,
                            TextureRecolorType.Material,
                            new RgbaColor(53, 49, 37, 255),
                            enemyColor,
                            0
                        ),
                    ]
                ),
                // Armogohma
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_gm.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmdv/goma.bmd",1,enemyColor),
                    ]
                ),
                // Argorok
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_dr.arc",
                    [
                         TextureRecolorOptions.Greyscale(@"bmde/dr.bmd",5,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmde/dr.bmd",3,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmde/dr.bmd",0,enemyColor),
                         TextureRecolorOptions.Greyscale(@"bmde/dr_part_a.bmd",0,heartColor),
                         TextureRecolorOptions.Greyscale(@"bmde/dr_part_b.bmd",0,heartColor),
                         TextureRecolorOptions.Greyscale(@"bmde/dr_part_c.bmd",0,heartColor),
                         TextureRecolorOptions.Greyscale(@"bmde/dr_part_dl.bmd",0,heartColor),
                         TextureRecolorOptions.Greyscale(@"bmde/dr_part_dr.bmd",0,heartColor),
                    ]
                ),
                // Zant
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_zan.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/zan.bmd",
                            0xFFFF0202,
                            TextureRecolorType.Material,
                            new RgbaColor(56, 17, 18, 255),
                            heartColor,
                            25
                        ),
                         TextureRecolorOptions.Greyscale(@"bmdr/zan.bmd",0, heartColor),
                         TextureRecolorOptions.Greyscale(@"bmdr/zan.bmd",1, enemyColor),
                        TextureRecolorOptions.Greyscale(@"bmdr/zz.bmd",1, enemyColor),
                    ]
                ),
                // Twilit Bloat
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_yb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/yb_tw.bmd",
                            0xFF01FF00,
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/yb_tw.bmd",
                            0xFF01FF01,
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/yb_tw.bmd",
                            0xFF01FF02,
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/yb_tw.bmd",
                            0xFF01FF04,
                            TextureRecolorType.Material,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Puppet Zelda
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Hzelda.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/hzelda.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(25, 18, 22, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Beast Ganon
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_mgn.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/mgn.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(42, 23, 19, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                // Ganondorf - Boss
                new TextureRecolor(
                    @"extractedISO/root/res/Object/B_gnd.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdv/egnd.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(54, 47, 38, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdv/egnd.bmd",
                            6,
                            TextureRecolorType.Hue,
                            new RgbaColor(42, 23, 19, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
                // Ganondorf
                new TextureRecolor(
                    @"extractedISO/root/res/Object/gnd.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/gnd.bmd",
                            3,
                            TextureRecolorType.Hue,
                            new RgbaColor(54, 47, 38, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/gnd.bmd",
                            11,
                            TextureRecolorType.Hue,
                            new RgbaColor(42, 23, 19, 255),
                            heartColor,
                            25
                        ),
                    ]
                ),
            ];
        return recolorOptions;
    }

    // Prints a list of materials, their indexes, and any colors associated with registers
    public static void PrintMaterialDescriptions(string fileName)
    {
        var hrtBmd = new BmdFile(fileName);
        byte[] mat3Bytes1 = hrtBmd.GetRawChunk("MAT3");
        var mat31 = new Mat3Chunk(mat3Bytes1);

        // Print exactly where each material's color comes from:
        for (int i = 0; i < mat31.Materials.Count; i++)
            Console.WriteLine(mat31.DescribeMaterial(i));
    }
}
