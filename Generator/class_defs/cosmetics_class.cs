
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
    public int TextureIndex {get;set;}
    public TextureRecolorType RecolorType {get;set;} // 0 for grayscale, 1 for palette recolor, 2 for hue recolor
    public RgbaColor OldColor {get;set;}
    public RgbaColor NewColor {get;set;}
    public int Tolerance {get;set;}

    public TextureRecolorOptions(string fName, int index, TextureRecolorType type, RgbaColor oldColor, RgbaColor newColor, int tol)
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
    Hue = 2
}

public static class CosmeticFunctions
{
    public static List<TextureRecolor> GenerateTextureCosmetics()
    {
        List<TextureRecolor> recolorOptions =
            [
                new TextureRecolor(
                    @"extractedISO/root/res/Object/Kmdl.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmwr/al.bmd",
                            0,
                            TextureRecolorType.Greyscale,
                            new RgbaColor(0x0, 0x0, 0x0, 255),
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            0
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
                        new TextureRecolorOptions(
                            @"bmdr/al_sp.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(66, 36, 16, 255), // roughly red
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
                // HyShd - Hylian shield
                new TextureRecolor(
                    @"extractedISO/root/res/Object/HyShd.arc",
                    [
                        new TextureRecolorOptions(
                            @"bmwr/al_sha.bmd",
                            0,
                            TextureRecolorType.Hue,
                            new RgbaColor(33, 44, 66, 255), // Blue background
                            new RgbaColor(0x9b, 0x6e, 0xab, 255),
                            25
                        ),
                    ]
                ),
            ];
        return recolorOptions;
    }
}
