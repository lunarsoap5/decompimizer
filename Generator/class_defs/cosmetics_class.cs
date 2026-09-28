
public class TextureRecolor
{
    public string? ArchiveDirectory { get; set; }
    public List<TextureRecolorOptions>? TextureOptions { get; set; }

    public TextureRecolor( string arcDir, List<TextureRecolorOptions> texOptions)
    {
        ArchiveDirectory = arcDir;
        TextureOptions = texOptions;
    }
}

public class TextureRecolorOptions
{
    public string? FileName {get;set;}
    public uint TextureIndex {get;set;}
    public TextureRecolorType RecolorType {get;set;} // 0 for grayscale, 1 for palette recolor, 2 for hue recolor
    public RgbaColor OldColor {get;set;}
    public RgbaColor NewColor {get;set;}
    public int Tolerance {get;set;}

    public TextureRecolorOptions(string fName, uint index, TextureRecolorType type, RgbaColor oldColor, RgbaColor newColor, int tol)
    {
        FileName = fName;
        TextureIndex = index;
        RecolorType = type;
        OldColor = oldColor;
        NewColor = newColor;
        Tolerance = tol;
    }
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
        List<TextureRecolor> recolorOptions =
            [
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Kmdl.arc",
                    [
                        // Hero's Clothes
                        new TextureRecolorOptions(
                            @"bmwr/al.bmd",
                            0, // Tunic Body
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0x0, 0x0, 0x0, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            0
                        ),
                    ]
                ),
                // Item Icons
                new TextureRecolor(
                    @"extractedISO/root/res/Layout/itemicon.arc",
                    [
                        // Ordon Sword Icon
                        new TextureRecolorOptions(
                            @"timg/tt_kokirinoken_s3_tc.bti",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(75, 75, 75, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                        // Master Sword Icon
                        new TextureRecolorOptions(
                            @"timg/ni_mastersword_48.bti",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(75, 75, 75, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                        // Wooden Sword Icon
                        new TextureRecolorOptions(
                            @"timg/im_kinobou_48.bti",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(75, 75, 75, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                        // Memo Icon
                        new TextureRecolorOptions(
                            @"timg/im_kakioki_48.bti",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(180, 30, 30, 255),
                            new RgbaColor(77, 53, 41, 255),
                            25
                        ),
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
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
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
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                        // Ordon Sword
                        new TextureRecolorOptions(
                            @"bmwr/al_swa.bmd",
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                        // Master Sword - Handle
                        new TextureRecolorOptions(
                            @"bmwe/al_swm.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                        // Master Sword - Blade
                        new TextureRecolorOptions(
                            @"bmwe/al_swm.bmd",
                            2,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                    ]
                ),
                // MstrSword
                new TextureRecolor(
                    @"extractedISO/root/res/Object/MstrSword.arc",
                    [
                        // Master Sword - Handle
                        new TextureRecolorOptions(
                            @"bmdr/o_al_swm.bmd",
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                        // Master Sword - Blade
                        new TextureRecolorOptions(
                            @"bmdr/o_al_swm.bmd",
                            3,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                    ]
                ),
                // Wmdl - Wolf Link and Midna on Back
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Wmdl.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmwr/wl.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(96, 93, 84, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
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
                        new TextureRecolorOptions(
                            @"bmdr/o_gd_memo.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(180, 30, 30, 255), 
                            new RgbaColor(77, 53, 41, 255),
                            25
                        ),
                    ]
                ),
                // Custom Sketch Actor
                new TextureRecolor(
                    @"extractedISO/root/res/Object/O_gD_mem3.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/o_gd_memo.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(180, 30, 30, 255), 
                            new RgbaColor(76,119,180, 255),
                            25
                        ),
                    ]
                ),
                // Custom Unpowered Rod Actor
                new TextureRecolor(
                    @"extractedISO/root/res/Object/O_gD_CROD1.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/o_gd_al_crod.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(33, 20, 20, 255), 
                            new RgbaColor(33, 20, 20, 255), 
                            25
                        ),
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
                        new TextureRecolorOptions(
                            @"bmdv/gb.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(61, 66, 64, 255),
                            enemyColor,
                            35
                        ),
                    ]
                ),
                // Bari
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_dk.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/dk.bmd",
                            0, // Head
                            TextureRecolorType.Greyscale,
                            new RgbaColor(61, 66, 64, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/dk.bmd",
                            1, // Tentacles
                            TextureRecolorType.Greyscale,
                            new RgbaColor(61, 66, 64, 255),
                            enemyColor,
                            25
                        )
                    ]
                ),
                // Beamos - GM
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Obj_bm.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/bm.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(27, 18, 13, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Beamos - ToT
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Obj_lv6bm.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmde/bm6.bmd",
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(27, 18, 13, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_bm6.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmde/bm6.bmd",
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(27, 18, 13, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Big Baba
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_gb.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/gb.bmd", // Head
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(25, 15, 14, 255),
                            enemyColor,
                            25
                        ),
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
                // Boomskit
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_cr.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/cr.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(23, 23, 24, 255),
                            enemyColor,
                            20
                        ),
                    ]
                ),
                // Bubble
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_bu.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/bu.bmd",
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(23, 23, 24, 255),
                            enemyColor,
                            20
                        ),
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
                        new TextureRecolorOptions(
                            @"bmde/kk.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(61, 66, 64, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmde/kk_weapon.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(61, 66, 64, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Chu Worm
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_sm.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/sc.bmd",
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(19, 22, 9, 255),
                            enemyColor,
                            20
                        ),
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
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_arm_l.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_arm_r.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_chest_b.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_chest_f.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_head_b.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_head_f.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_shoulder_l.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_shoulder_r.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_waist_b.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_waist_f.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_waist_l.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn_armor_waist_r.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
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
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_arm_l.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_arm_r.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_chest_b.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_chest_f.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_head_b.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_head_a.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_shoulder_l.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_shoulder_r.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_waist_b.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_waist_f.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_waist_l.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_armor_waist_r.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(22, 15, 16, 255),
                            enemyColor,
                            20
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/tn2_shield.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(35, 31, 25, 255),
                            enemyColor,
                            20
                        ),
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
                        new TextureRecolorOptions(
                            @"bmde/fl_model.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Guay
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ge.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/ge_model.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/ge_model.bmd",
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Helmasaur/Helmasaurus
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_mm.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/dm.bmd", // Helmasaurus
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/mm.bmd", // Helmasaur
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            enemyColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/mm.bmd", // Helmasaur
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Helmasaur/Helmasaurus Armor
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_mm_mt.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/dm_met.bmd", // Helmasaur Armor
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            heartColor,
                            25
                        ),
                        new TextureRecolorOptions(
                            @"bmdr/mt.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0, 0, 0, 255),
                            heartColor,
                            25
                        ),
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
                        new TextureRecolorOptions(
                            @"bmdr/dn.bmd",
                            1,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(51, 54, 42, 255),
                            heartColor,
                            25
                        ),
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
                        new TextureRecolorOptions(
                            @"bmdr/sw.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
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
                        new TextureRecolorOptions(
                            @"bmdr/fs.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
                    ]
                ),
                // Rat
                new TextureRecolor(
                    @"extractedISO/root/res/Object/E_ms.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmdr/ms.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(51, 54, 42, 255),
                            enemyColor,
                            25
                        ),
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
