
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
                            new RgbaColor(111, 196, 251, 255),
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
