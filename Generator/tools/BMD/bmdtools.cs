// Based on various public resources and SuperBMD created by Sage-of-Mirrors

/* Usage:

// Recoloring via greyscale
var testBmd = new BmdFile("al.bmd");
foreach (var tex in testBmd.Textures)
    Console.WriteLine($"{tex.Name}  {tex.Width}x{tex.Height}");
var upTex = testBmd.Textures[0];
   upTex.TintGrayscale(new RgbaColor(0xab,0x70,0x6e,255));
   testBmd.Save("al_new.bmd");

// Recoloring via hue
/ Test texture recoloring
var testBmd = new BmdFile("ml.bmd");
foreach (var tex in testBmd.Textures)
    Console.WriteLine($"{tex.Name}  {tex.Width}x{tex.Height}");
var upTex = testBmd.Textures[0];
   upTex.RecolorByHue(
    targetColor:  new RgbaColor(180, 30, 30, 255), // roughly red
    replacementColor: new RgbaColor(212, 175, 55, 255),   // roughly gold
    hueToleranceDegrees: 25);
   testBmd.Save("ml_new.bmd");
*/

/* Notes / caveats:
   - IA8 byte order and RGBA32 plane order follow a common GC/Wii
     convention; decode/encode are a matched pair so round-tripping
     is internally consistent even if this differs from another tool.
   - CMPR re-encoding uses a simple min/max-luminance endpoint pick.
     Fine for flat color swaps, lossy for gradients/dithering.
   - Recolor() maps colors 1:1; changing palette *size* (NumColors)
     is a bigger change not covered here (would require re-checking
     index bounds in paletted ImageData).
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public struct RgbaColor : IEquatable<RgbaColor>
{
    public byte R,
        G,
        B,
        A;

    public RgbaColor(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public override string ToString() => $"({R},{G},{B},{A})";

    public bool Equals(RgbaColor other) =>
        R == other.R && G == other.G && B == other.B && A == other.A;

    public override bool Equals(object obj) => obj is RgbaColor c && Equals(c);

    public override int GetHashCode() => (R << 24) | (G << 16) | (B << 8) | A;
}

public class BmdFile
{
    public List<BmdTexture> Textures { get; } = new();

    private readonly List<(string magic, byte[] data)> _chunks = new();
    private int _tex1ChunkIndex = -1;
    private string _fileType = "J3D2bmd3";

    public BmdFile() { }

    public BmdFile(string path) => Load(File.ReadAllBytes(path));

    public BmdFile(byte[] data) => Load(data);

    public void Load(byte[] data)
    {
        _chunks.Clear();
        Textures.Clear();
        _tex1ChunkIndex = -1;

        if (data.Length < 0x20)
            throw new InvalidDataException("File too small to be a BMD/BDL.");

        _fileType = Encoding.ASCII.GetString(data, 0, 8);
        if (!_fileType.StartsWith("J3D"))
            throw new InvalidDataException($"Not a J3D file (magic: {_fileType})");

        int chunkCount = (int)ReadU32(data, 0x0C);
        int pos = 0x20;

        for (int i = 0; i < chunkCount; i++)
        {
            if (pos + 8 > data.Length)
                throw new InvalidDataException("Truncated chunk header.");

            string magic = Encoding.ASCII.GetString(data, pos, 4);
            int chunkSize = (int)ReadU32(data, pos + 4);

            if (chunkSize < 8 || pos + chunkSize > data.Length)
                throw new InvalidDataException($"Invalid chunk size for '{magic}' at 0x{pos:X}.");

            byte[] chunkData = new byte[chunkSize];
            Array.Copy(data, pos, chunkData, 0, chunkSize);

            if (magic == "TEX1")
            {
                _tex1ChunkIndex = _chunks.Count;
                ParseTex1(chunkData);
            }

            _chunks.Add((magic, chunkData));
            pos += chunkSize;
        }

        if (_tex1ChunkIndex < 0)
            throw new InvalidDataException("No TEX1 chunk found in this BMD/BDL.");
    }

    public byte[] Save()
    {
        _chunks[_tex1ChunkIndex] = ("TEX1", BuildTex1());

        using var ms = new MemoryStream();
        ms.Write(new byte[0x20], 0, 0x20); // header placeholder

        foreach (var (_, chunkData) in _chunks)
            ms.Write(chunkData, 0, chunkData.Length);

        byte[] result = ms.ToArray();

        // Patch file header
        Encoding.ASCII.GetBytes(_fileType).CopyTo(result, 0);
        WriteU32(result, 0x08, (uint)result.Length);
        WriteU32(result, 0x0C, (uint)_chunks.Count);
        // 0x10..0x1F stay zero

        return result;
    }

    public byte[] GetRawChunk(string magic)
    {
        foreach (var (chunkMagic, data) in _chunks)
            if (chunkMagic == magic)
                return (byte[])data.Clone();
        return null;
    }

    public void SetRawChunk(string magic, byte[] newData)
    {
        for (int i = 0; i < _chunks.Count; i++)
        {
            if (_chunks[i].magic == magic)
            {
                _chunks[i] = (magic, newData);
                return;
            }
        }
        throw new InvalidOperationException(
            $"No existing '{magic}' chunk to replace - this file has no such chunk."
        );
    }

    public void Save(string path) => File.WriteAllBytes(path, Save());

    // ----------------------------------------------------------
    // TEX1 parsing
    //
    // Chunk layout (offsets from chunk start):
    //   0x00  char[4]  "TEX1"
    //   0x04  u32      chunk size
    //   0x08  u16      texture count
    //   0x0A  u16      0xFFFF
    //   0x0C  u32      offset to BTI header table  (from chunk start)
    //   0x10  u32      offset to string table       (from chunk start)
    //   0x14..0x1F     padding
    //   [btiTblOffset] array of texCount x 0x20-byte BTI headers
    //
    // Within each BTI header:
    //   palette_data_offset (0x0C) and image_data_offset (0x1C) are
    //   relative to the START OF THAT HEADER (not the chunk).
    // ----------------------------------------------------------
    private void ParseTex1(byte[] chunk)
    {
        int texCount = ReadU16(chunk, 0x08);
        int btiTblOffset = (int)ReadU32(chunk, 0x0C);
        int strTblOffset = (int)ReadU32(chunk, 0x10);

        for (int i = 0; i < texCount; i++)
        {
            int headerOffset = btiTblOffset + i * 0x20;
            string name = ReadStringTableEntry(chunk, strTblOffset, i);
            var tex = new BmdTexture(name);
            tex.ParseBtiHeader(chunk, headerOffset);
            Textures.Add(tex);
        }
    }

    // ----------------------------------------------------------
    // TEX1 rebuilding
    //
    // Write order (matches SuperBMD TEX1.cs exactly):
    //   1. Chunk header          (0x20 bytes, size/strTblOffset patched later)
    //   2. BTI headers           (texCount x 0x20, pal/img offsets patched later)
    //   3. Palette data          (one blob per unique name, each 0x20-padded)
    //   4. Image data            (one blob per unique name, no extra padding needed)
    //   5. String table          (patched offset written back to header at 0x10)
    //
    // palette_data_offset and image_data_offset in each header are
    // relative to that header's position (= btiTblOffset + i*0x20 from chunk start).
    // If a texture has the same name as a prior one it shares that prior one's data.
    // ----------------------------------------------------------
    private byte[] BuildTex1()
    {
        using var ms = new MemoryStream();

        int texCount = Textures.Count;
        int btiTblStart = 0x20; // always 0x20 from chunk start (SuperBMD hardcodes 32)

        // ---- 1. Chunk header placeholder ----
        ms.Write(new byte[0x20], 0, 0x20);

        // ---- 2. BTI header placeholders ----
        long headerBlockStart = ms.Position; // == btiTblStart
        ms.Write(new byte[texCount * 0x20], 0, texCount * 0x20);

        // ---- 3 & 4. Palette then image data, deduplicating by name ----
        var uniqueNames = new List<string>();
        var uniqueTexByName = new Dictionary<string, BmdTexture>();
        foreach (var tex in Textures)
        {
            if (!uniqueTexByName.ContainsKey(tex.Name))
            {
                uniqueNames.Add(tex.Name);
                uniqueTexByName[tex.Name] = tex;
            }
        }

        // Palette pass
        var palChunkOffsets = new Dictionary<string, int>();
        foreach (var name in uniqueNames)
        {
            var tex = uniqueTexByName[name];
            palChunkOffsets[name] = (int)ms.Position; // chunk-relative
            if (tex.PaletteData.Length > 0)
            {
                ms.Write(tex.PaletteData, 0, tex.PaletteData.Length);
                PadStream(ms, 32);
            }
        }

        // Image pass
        var imgChunkOffsets = new Dictionary<string, int>();
        foreach (var name in uniqueNames)
        {
            var tex = uniqueTexByName[name];
            imgChunkOffsets[name] = (int)ms.Position; // chunk-relative
            ms.Write(tex.ImageData, 0, tex.ImageData.Length);
        }

        // ---- 5. String table ----
        long strTblChunkOffset = ms.Position;
        byte[] strTbl = BuildStringTable(Textures);
        ms.Write(strTbl, 0, strTbl.Length);
        PadStream(ms, 32);

        long chunkSize = ms.Position;

        // ---- Patch chunk header ----
        byte[] chunk = ms.ToArray();
        Encoding.ASCII.GetBytes("TEX1").CopyTo(chunk, 0);
        WriteU32(chunk, 0x04, (uint)chunkSize);
        WriteU16(chunk, 0x08, (ushort)texCount);
        chunk[0x0A] = 0xFF;
        chunk[0x0B] = 0xFF;
        WriteU32(chunk, 0x0C, (uint)btiTblStart);
        WriteU32(chunk, 0x10, (uint)strTblChunkOffset);

        // ---- Patch per-header palette and image offsets ----
        for (int i = 0; i < texCount; i++)
        {
            int headerChunkOffset = btiTblStart + i * 0x20; // from chunk start
            string name = Textures[i].Name;

            int palChunkOff = palChunkOffsets[name];
            int imgChunkOff = imgChunkOffsets[name];

            int relPal = Textures[i].PaletteData.Length > 0 ? palChunkOff - headerChunkOffset : 0;
            int relImg = imgChunkOff - headerChunkOffset;

            Textures[i].WriteBtiHeader(chunk, headerChunkOffset, (uint)relImg, (uint)relPal);
        }

        return chunk;
    }

    // ----------------------------------------------------------
    // String table helpers
    //
    // Format:
    //   u16  entry_count
    //   u16  0xFFFF
    //   per entry: u16 name_hash, u16 string_offset_from_table_start
    //   null-terminated ASCII strings packed after the entry array
    // ----------------------------------------------------------
    private static byte[] BuildStringTable(List<BmdTexture> textures)
    {
        using var ms = new MemoryStream();

        int count = textures.Count;
        int entryHeaderSize = 4 + count * 4; // 4-byte preamble + 4 bytes per entry

        var stringArea = new List<byte>();
        var stringOffsets = new int[count];
        for (int i = 0; i < count; i++)
        {
            stringOffsets[i] = entryHeaderSize + stringArea.Count;
            byte[] nameBytes = Encoding.ASCII.GetBytes(textures[i].Name);
            stringArea.AddRange(nameBytes);
            stringArea.Add(0); // null terminator
        }

        WriteU16ToStream(ms, (ushort)count);
        WriteU16ToStream(ms, 0xFFFF);
        for (int i = 0; i < count; i++)
        {
            WriteU16ToStream(ms, ComputeNameHash(textures[i].Name));
            WriteU16ToStream(ms, (ushort)stringOffsets[i]);
        }
        ms.Write(stringArea.ToArray(), 0, stringArea.Count);

        return ms.ToArray();
    }

    private static string ReadStringTableEntry(byte[] chunk, int strTblOffset, int index)
    {
        int count = ReadU16(chunk, strTblOffset);
        if (index >= count)
            return $"texture_{index}";

        int entryOff = strTblOffset + 4 + index * 4;
        int strOff = ReadU16(chunk, entryOff + 2); // relative to string table start
        int absOff = strTblOffset + strOff;

        var sb = new StringBuilder();
        while (absOff < chunk.Length && chunk[absOff] != 0)
            sb.Append((char)chunk[absOff++]);
        return sb.ToString();
    }

    private static ushort ComputeNameHash(string name)
    {
        ushort hash = 0;
        foreach (char c in name)
            hash = (ushort)(hash * 3 + c);
        return hash;
    }

    // ----------------------------------------------------------
    // Binary / stream helpers
    // ----------------------------------------------------------
    static uint ReadU32(byte[] d, int o) =>
        (uint)((d[o] << 24) | (d[o + 1] << 16) | (d[o + 2] << 8) | d[o + 3]);

    static ushort ReadU16(byte[] d, int o) => (ushort)((d[o] << 8) | d[o + 1]);

    static void WriteU32(byte[] d, int o, uint v)
    {
        d[o] = (byte)(v >> 24);
        d[o + 1] = (byte)(v >> 16);
        d[o + 2] = (byte)(v >> 8);
        d[o + 3] = (byte)v;
    }

    static void WriteU16(byte[] d, int o, ushort v)
    {
        d[o] = (byte)(v >> 8);
        d[o + 1] = (byte)v;
    }

    static void WriteU16ToStream(Stream s, ushort v)
    {
        s.WriteByte((byte)(v >> 8));
        s.WriteByte((byte)v);
    }

    static void PadStream(Stream s, int alignment)
    {
        long rem = s.Position % alignment;
        if (rem != 0)
            s.Write(new byte[alignment - rem], 0, (int)(alignment - rem));
    }
}

public partial class BmdTexture
{
    public string Name { get; set; }

    // BTI header fields
    public byte ImageFormat { get; set; }
    public byte AlphaSetting { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public byte WrapS { get; set; }
    public byte WrapT { get; set; }
    public byte PalettesEnabled { get; set; }
    public byte PaletteFormat { get; set; }
    public ushort NumColors { get; set; }
    public byte MinFilter { get; set; }
    public byte MagFilter { get; set; }
    public byte MinLod { get; set; }
    public byte MaxLod { get; set; }
    public byte MipmapCount { get; set; }
    public byte Unknown3 { get; set; }
    public short LodBias { get; set; }

    // Raw pixel/palette blobs
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public byte[] PaletteData { get; set; } = Array.Empty<byte>();

    public BmdTexture(string name)
    {
        Name = name;
    }

    // ----------------------------------------------------------
    // Parse BTI header from a chunk buffer.
    // headerOffset = position of this 0x20-byte header within chunk[].
    // palette_data_offset and image_data_offset are relative to headerOffset.
    // ----------------------------------------------------------
    internal void ParseBtiHeader(byte[] chunk, int headerOffset)
    {
        ImageFormat = chunk[headerOffset + 0x00];
        AlphaSetting = chunk[headerOffset + 0x01];
        Width = ReadU16(chunk, headerOffset + 0x02);
        Height = ReadU16(chunk, headerOffset + 0x04);
        WrapS = chunk[headerOffset + 0x06];
        WrapT = chunk[headerOffset + 0x07];
        PalettesEnabled = chunk[headerOffset + 0x08];
        PaletteFormat = chunk[headerOffset + 0x09];
        NumColors = ReadU16(chunk, headerOffset + 0x0A);
        uint palRelOff = ReadU32(chunk, headerOffset + 0x0C);
        // 0x10..0x13 unknown, skip
        MinFilter = chunk[headerOffset + 0x14];
        MagFilter = chunk[headerOffset + 0x15];
        MinLod = chunk[headerOffset + 0x16];
        MaxLod = chunk[headerOffset + 0x17];
        MipmapCount = chunk[headerOffset + 0x18];
        if (MipmapCount == 0)
            MipmapCount = 1;
        Unknown3 = chunk[headerOffset + 0x19];
        LodBias = ReadS16(chunk, headerOffset + 0x1A);
        uint imgRelOff = ReadU32(chunk, headerOffset + 0x1C);

        int absImg = headerOffset + (int)imgRelOff;
        int imgSize = ComputeImageDataSize();
        ImageData = ReadBytes(chunk, absImg, imgSize);

        int palSize = NumColors * 2;
        if (palSize > 0 && palRelOff != 0)
        {
            int absPal = headerOffset + (int)palRelOff;
            PaletteData = ReadBytes(chunk, absPal, palSize);
        }
        else
        {
            PaletteData = Array.Empty<byte>();
        }
    }

    // ----------------------------------------------------------
    // Write 0x20-byte BTI header into an already-sized buffer.
    // relImageOffset / relPaletteOffset are relative to headerOffset.
    // ----------------------------------------------------------
    internal void WriteBtiHeader(
        byte[] buf,
        int headerOffset,
        uint relImageOffset,
        uint relPaletteOffset
    )
    {
        buf[headerOffset + 0x00] = ImageFormat;
        buf[headerOffset + 0x01] = AlphaSetting;
        WriteU16(buf, headerOffset + 0x02, Width);
        WriteU16(buf, headerOffset + 0x04, Height);
        buf[headerOffset + 0x06] = WrapS;
        buf[headerOffset + 0x07] = WrapT;
        buf[headerOffset + 0x08] = PalettesEnabled;
        buf[headerOffset + 0x09] = PaletteFormat;
        WriteU16(buf, headerOffset + 0x0A, NumColors);
        WriteU32(buf, headerOffset + 0x0C, relPaletteOffset);
        // 0x10..0x13 - unknown, leave zero
        buf[headerOffset + 0x14] = MinFilter;
        buf[headerOffset + 0x15] = MagFilter;
        buf[headerOffset + 0x16] = MinLod;
        buf[headerOffset + 0x17] = MaxLod;
        buf[headerOffset + 0x18] = MipmapCount;
        buf[headerOffset + 0x19] = Unknown3;
        WriteS16(buf, headerOffset + 0x1A, LodBias);
        WriteU32(buf, headerOffset + 0x1C, relImageOffset);
    }

    // ----------------------------------------------------------
    // Export as standalone .bti file
    // ----------------------------------------------------------
    public byte[] ExportBtiBytes()
    {
        uint imgOff = 0x20;
        uint palOff = PaletteData.Length > 0 ? (uint)(0x20 + ImageData.Length) : 0;

        int total = 0x20 + ImageData.Length + PaletteData.Length;
        byte[] bti = new byte[total];
        WriteBtiHeader(bti, 0, imgOff, palOff);
        Array.Copy(ImageData, 0, bti, 0x20, ImageData.Length);
        if (PaletteData.Length > 0)
            Array.Copy(PaletteData, 0, bti, 0x20 + ImageData.Length, PaletteData.Length);
        return bti;
    }

    public void ExportBtiFile(string path) => File.WriteAllBytes(path, ExportBtiBytes());

    public void ImportBtiBytes(byte[] bti)
    {
        if (bti.Length < 0x20)
            throw new InvalidDataException("BTI data too small.");

        byte newFmt = bti[0x00];
        if (newFmt != ImageFormat)
            Console.WriteLine(
                $"Warning: '{Name}' image format changed from 0x{ImageFormat:X2} to 0x{newFmt:X2}. "
                    + "BMD materials referencing this texture may need updating."
            );

        ImageFormat = bti[0x00];
        AlphaSetting = bti[0x01];
        Width = ReadU16(bti, 0x02);
        Height = ReadU16(bti, 0x04);
        WrapS = bti[0x06];
        WrapT = bti[0x07];
        PalettesEnabled = bti[0x08];
        PaletteFormat = bti[0x09];
        NumColors = ReadU16(bti, 0x0A);
        uint palOff = ReadU32(bti, 0x0C);
        MinFilter = bti[0x14];
        MagFilter = bti[0x15];
        MinLod = bti[0x16];
        MaxLod = bti[0x17];
        MipmapCount = bti[0x18];
        if (MipmapCount == 0)
            MipmapCount = 1;
        Unknown3 = bti[0x19];
        LodBias = ReadS16(bti, 0x1A);
        uint imgOff = ReadU32(bti, 0x1C);

        int imgSize = ComputeImageDataSize();
        ImageData = ReadBytes(bti, (int)imgOff, imgSize);

        int palSize = NumColors * 2;
        PaletteData =
            (palSize > 0 && palOff != 0)
                ? ReadBytes(bti, (int)palOff, palSize)
                : Array.Empty<byte>();
    }

    public void ImportBtiFile(string path) => ImportBtiBytes(File.ReadAllBytes(path));

    private int ComputeImageDataSize()
    {
        int blockW = GetBlockWidth();
        int blockH = GetBlockHeight();
        int blockBytes = GetBlockDataSize();
        int total = 0;
        int w = Width,
            h = Height;
        for (int m = 0; m < MipmapCount; m++)
        {
            int bw = (w + blockW - 1) / blockW;
            int bh = (h + blockH - 1) / blockH;
            total += bw * bh * blockBytes;
            w = Math.Max(1, w / 2);
            h = Math.Max(1, h / 2);
        }
        return total;
    }

    // Block geometry - matches gclib BLOCK_WIDTHS / BLOCK_HEIGHTS / BLOCK_DATA_SIZES
    private int GetBlockWidth() =>
        ImageFormat switch
        {
            0x00 => 8, // I4
            0x01 => 8, // I8
            0x02 => 8, // IA4
            0x03 => 4, // IA8
            0x04 => 4, // RGB565
            0x05 => 4, // RGB5A3
            0x06 => 4, // RGBA32
            0x08 => 8, // C4
            0x09 => 8, // C8
            0x0A => 4, // C14X2
            0x0E => 8, // CMPR (DXT1)
            _ => throw new NotSupportedException($"Unknown image format 0x{ImageFormat:X2}")
        };

    private int GetBlockHeight() =>
        ImageFormat switch
        {
            0x00 => 8, // I4
            0x01 => 4, // I8
            0x02 => 4, // IA4
            0x03 => 4, // IA8
            0x04 => 4, // RGB565
            0x05 => 4, // RGB5A3
            0x06 => 4, // RGBA32
            0x08 => 8, // C4
            0x09 => 4, // C8
            0x0A => 4, // C14X2
            0x0E => 8, // CMPR (DXT1)
            _ => throw new NotSupportedException($"Unknown image format 0x{ImageFormat:X2}")
        };

    private int GetBlockDataSize() =>
        ImageFormat switch
        {
            0x00 => 32, // I4
            0x01 => 32, // I8
            0x02 => 32, // IA4
            0x03 => 32, // IA8
            0x04 => 32, // RGB565
            0x05 => 32, // RGB5A3
            0x06 => 64, // RGBA32
            0x08 => 32, // C4
            0x09 => 32, // C8
            0x0A => 32, // C14X2
            0x0E => 32, // CMPR (DXT1)
            _ => throw new NotSupportedException($"Unknown image format 0x{ImageFormat:X2}")
        };

    // ----------------------------------------------------------
    // Binary helpers
    // ----------------------------------------------------------
    static ushort ReadU16(byte[] d, int o) => (ushort)((d[o] << 8) | d[o + 1]);

    static short ReadS16(byte[] d, int o) => (short)((d[o] << 8) | d[o + 1]);

    static uint ReadU32(byte[] d, int o) =>
        (uint)((d[o] << 24) | (d[o + 1] << 16) | (d[o + 2] << 8) | d[o + 3]);

    static void WriteU16(byte[] d, int o, ushort v)
    {
        d[o] = (byte)(v >> 8);
        d[o + 1] = (byte)v;
    }

    static void WriteS16(byte[] d, int o, short v) => WriteU16(d, o, (ushort)v);

    static void WriteU32(byte[] d, int o, uint v)
    {
        d[o] = (byte)(v >> 24);
        d[o + 1] = (byte)(v >> 16);
        d[o + 2] = (byte)(v >> 8);
        d[o + 3] = (byte)v;
    }

    static byte[] ReadBytes(byte[] d, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset + length > d.Length)
            throw new ArgumentOutOfRangeException(
                $"ReadBytes out of range: offset=0x{offset:X} length=0x{length:X} dataLen=0x{d.Length:X}"
            );
        var result = new byte[length];
        Array.Copy(d, offset, result, 0, length);
        return result;
    }
}

public partial class BmdTexture
{
    // PaletteFormat values (from header byte 0x09):
    //   0 = IA8, 1 = RGB565, 2 = RGB5A3

    public List<RgbaColor> GetPaletteColors()
    {
        var colors = new List<RgbaColor>();
        if (PaletteData.Length == 0)
            return colors;

        int count = PaletteData.Length / 2;
        for (int i = 0; i < count; i++)
        {
            ushort raw = (ushort)((PaletteData[i * 2] << 8) | PaletteData[i * 2 + 1]);
            colors.Add(DecodePaletteEntry(raw, PaletteFormat));
        }
        return colors;
    }

    public void SetPaletteColors(List<RgbaColor> colors)
    {
        if (colors.Count * 2 != PaletteData.Length)
            throw new ArgumentException(
                $"Color count ({colors.Count}) must match existing palette size ({PaletteData.Length / 2})."
                    + " Palette entry count can't change without also updating NumColors and re-checking index bounds against ImageData."
            );

        byte[] newData = new byte[PaletteData.Length];
        for (int i = 0; i < colors.Count; i++)
        {
            ushort raw = EncodePaletteEntry(colors[i], PaletteFormat);
            newData[i * 2] = (byte)(raw >> 8);
            newData[i * 2 + 1] = (byte)raw;
        }
        PaletteData = newData;
    }

    public void SetPaletteColor(int index, RgbaColor color)
    {
        if (index < 0 || index * 2 + 1 >= PaletteData.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        ushort raw = EncodePaletteEntry(color, PaletteFormat);
        PaletteData[index * 2] = (byte)(raw >> 8);
        PaletteData[index * 2 + 1] = (byte)raw;
    }

    private static RgbaColor DecodePaletteEntry(ushort raw, byte format)
    {
        switch (format)
        {
            case 0: // IA8: high byte = intensity, low byte = alpha
            {
                byte i = (byte)(raw >> 8);
                byte a = (byte)raw;
                return new RgbaColor(i, i, i, a);
            }
            case 1: // RGB565
                return DecodeRgb565(raw);
            case 2: // RGB5A3
                return DecodeRgb5A3(raw);
            default:
                throw new NotSupportedException($"Unknown palette format {format}");
        }
    }

    private static ushort EncodePaletteEntry(RgbaColor c, byte format)
    {
        switch (format)
        {
            case 0: // IA8
            {
                byte intensity = (byte)((c.R + c.G + c.B) / 3);
                return (ushort)((intensity << 8) | c.A);
            }
            case 1: // RGB565
                return EncodeRgb565(c);
            case 2: // RGB5A3
                return EncodeRgb5A3(c);
            default:
                throw new NotSupportedException($"Unknown palette format {format}");
        }
    }
}

public partial class BmdTexture
{
    public void Recolor(Dictionary<RgbaColor, RgbaColor> colorMap, int tolerance = 0)
    {
        bool isPaletted = ImageFormat == 0x08 || ImageFormat == 0x09 || ImageFormat == 0x0A;

        if (isPaletted)
        {
            var palette = GetPaletteColors();
            for (int i = 0; i < palette.Count; i++)
            {
                if (TryFindMatch(palette[i], colorMap, tolerance, out var replacement))
                    palette[i] = replacement;
            }
            SetPaletteColors(palette);
            // Palette-indexed pixel data is untouched; no mip regen needed.
        }
        else
        {
            var pixels = GetPixelColors();
            int w = pixels.GetLength(0),
                h = pixels.GetLength(1);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (TryFindMatch(pixels[x, y], colorMap, tolerance, out var replacement))
                        pixels[x, y] = replacement;
                }
            SetPixelColorsAndRegenerateMips(pixels);
        }
    }

    private static bool TryFindMatch(
        RgbaColor c,
        Dictionary<RgbaColor, RgbaColor> map,
        int tolerance,
        out RgbaColor replacement
    )
    {
        if (tolerance == 0)
            return map.TryGetValue(c, out replacement);

        foreach (var kv in map)
        {
            if (
                Math.Abs(c.R - kv.Key.R) <= tolerance
                && Math.Abs(c.G - kv.Key.G) <= tolerance
                && Math.Abs(c.B - kv.Key.B) <= tolerance
                && Math.Abs(c.A - kv.Key.A) <= tolerance
            )
            {
                replacement = kv.Value;
                return true;
            }
        }
        replacement = default;
        return false;
    }

    public RgbaColor[,] GetPixelColors()
    {
        var pixels = new RgbaColor[Width, Height];
        var palette =
            (ImageFormat == 0x08 || ImageFormat == 0x09 || ImageFormat == 0x0A)
                ? GetPaletteColors()
                : null;

        int blockW = GetBlockWidthPublic();
        int blockH = GetBlockHeightPublic();

        int pos = 0;
        for (int by = 0; by < Height; by += blockH)
            for (int bx = 0; bx < Width; bx += blockW)
            {
                pos = DecodeBlock(ImageData, pos, bx, by, blockW, blockH, palette, pixels);
            }

        return pixels;
    }

    public void SetPixelColors(RgbaColor[,] pixels)
    {
        if (pixels.GetLength(0) != Width || pixels.GetLength(1) != Height)
            throw new ArgumentException("Pixel grid dimensions must match texture Width/Height.");

        byte[] mip0 = EncodeLevel(pixels, Width, Height);

        int mip0Size = mip0.Length;
        if (ImageData.Length >= mip0Size)
        {
            Array.Copy(mip0, 0, ImageData, 0, mip0Size);
        }
        else
        {
            ImageData = mip0; // single-mip texture, or size mismatch fallback
        }
    }

    public void SetPixelColorsAndRegenerateMips(RgbaColor[,] basePixels)
    {
        if (basePixels.GetLength(0) != Width || basePixels.GetLength(1) != Height)
            throw new ArgumentException("Pixel grid dimensions must match texture Width/Height.");

        using var ms = new MemoryStream();

        var currentLevel = basePixels;
        int w = Width,
            h = Height;

        int levels = MipmapCount > 0 ? MipmapCount : 1;
        for (int m = 0; m < levels; m++)
        {
            byte[] encoded = EncodeLevel(currentLevel, w, h);
            ms.Write(encoded, 0, encoded.Length);

            int nextW = Math.Max(1, w / 2);
            int nextH = Math.Max(1, h / 2);
            if (m < levels - 1)
                currentLevel = BoxDownsample(currentLevel, w, h, nextW, nextH);

            w = nextW;
            h = nextH;
        }

        ImageData = ms.ToArray();
    }

    private static RgbaColor[,] BoxDownsample(
        RgbaColor[,] src,
        int srcW,
        int srcH,
        int dstW,
        int dstH
    )
    {
        var dst = new RgbaColor[dstW, dstH];
        double scaleX = (double)srcW / dstW;
        double scaleY = (double)srcH / dstH;

        for (int dy = 0; dy < dstH; dy++)
            for (int dx = 0; dx < dstW; dx++)
            {
                int sx0 = (int)(dx * scaleX);
                int sy0 = (int)(dy * scaleY);
                int sx1 = Math.Min(srcW, (int)((dx + 1) * scaleX));
                int sy1 = Math.Min(srcH, (int)((dy + 1) * scaleY));
                sx1 = Math.Max(sx1, sx0 + 1);
                sy1 = Math.Max(sy1, sy0 + 1);

                int sumR = 0,
                    sumG = 0,
                    sumB = 0,
                    sumA = 0,
                    count = 0;
                for (int sy = sy0; sy < sy1; sy++)
                    for (int sx = sx0; sx < sx1; sx++)
                    {
                        var c = src[sx, sy];
                        sumR += c.R;
                        sumG += c.G;
                        sumB += c.B;
                        sumA += c.A;
                        count++;
                    }

                dst[dx, dy] = new RgbaColor(
                    (byte)(sumR / count),
                    (byte)(sumG / count),
                    (byte)(sumB / count),
                    (byte)(sumA / count)
                );
            }

        return dst;
    }

    private byte[] EncodeLevel(RgbaColor[,] pixels, int w, int h)
    {
        int blockW = GetBlockWidthPublic();
        int blockH = GetBlockHeightPublic();
        int blockBytes = GetBlockDataSizePublic();

        int bwCount = (w + blockW - 1) / blockW;
        int bhCount = (h + blockH - 1) / blockH;
        byte[] data = new byte[bwCount * bhCount * blockBytes];

        int pos = 0;
        for (int by = 0; by < h; by += blockH)
            for (int bx = 0; bx < w; bx += blockW)
            {
                pos = EncodeBlock(data, pos, bx, by, blockW, blockH, pixels, w, h);
            }

        return data;
    }

    // Expose block-geometry helpers
    private int GetBlockWidthPublic() =>
        ImageFormat switch
        {
            0x00 => 8,
            0x01 => 8,
            0x02 => 8,
            0x03 => 4,
            0x04 => 4,
            0x05 => 4,
            0x06 => 4,
            0x08 => 8,
            0x09 => 8,
            0x0A => 4,
            0x0E => 8,
            _ => throw new NotSupportedException()
        };

    private int GetBlockHeightPublic() =>
        ImageFormat switch
        {
            0x00 => 8,
            0x01 => 4,
            0x02 => 4,
            0x03 => 4,
            0x04 => 4,
            0x05 => 4,
            0x06 => 4,
            0x08 => 8,
            0x09 => 4,
            0x0A => 4,
            0x0E => 8,
            _ => throw new NotSupportedException()
        };

    private int GetBlockDataSizePublic() =>
        ImageFormat switch
        {
            0x00 => 32,
            0x01 => 32,
            0x02 => 32,
            0x03 => 32,
            0x04 => 32,
            0x05 => 32,
            0x06 => 64,
            0x08 => 32,
            0x09 => 32,
            0x0A => 32,
            0x0E => 32,
            _ => throw new NotSupportedException()
        };

    private int DecodeBlock(
        byte[] data,
        int pos,
        int bx,
        int by,
        int bw,
        int bh,
        List<RgbaColor> palette,
        RgbaColor[,] outPixels
    )
    {
        switch (ImageFormat)
        {
            case 0x00: // I4
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x += 2)
                    {
                        byte b = data[pos++];
                        SetPixel(outPixels, bx + x, by + y, IntensityColor((byte)((b >> 4) * 17)));
                        if (bx + x + 1 < Width)
                            SetPixel(
                                outPixels,
                                bx + x + 1,
                                by + y,
                                IntensityColor((byte)((b & 0xF) * 17))
                            );
                    }
                break;

            case 0x01: // I8
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                        SetPixel(outPixels, bx + x, by + y, IntensityColor(data[pos++]));
                break;

            case 0x02: // IA4
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        byte b = data[pos++];
                        byte i = (byte)((b >> 4) * 17);
                        byte a = (byte)((b & 0xF) * 17);
                        SetPixel(outPixels, bx + x, by + y, new RgbaColor(i, i, i, a));
                    }
                break;

            case 0x03: // IA8
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        byte a = data[pos++];
                        byte i = data[pos++];
                        SetPixel(outPixels, bx + x, by + y, new RgbaColor(i, i, i, a));
                    }
                break;

            case 0x04: // RGB565
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        ushort raw = (ushort)((data[pos] << 8) | data[pos + 1]);
                        pos += 2;
                        SetPixel(outPixels, bx + x, by + y, DecodeRgb565(raw));
                    }
                break;

            case 0x05: // RGB5A3
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        ushort raw = (ushort)((data[pos] << 8) | data[pos + 1]);
                        pos += 2;
                        SetPixel(outPixels, bx + x, by + y, DecodeRgb5A3(raw));
                    }
                break;

            case 0x06: // RGBA32

                {
                    var buf = new RgbaColor[16];
                    for (int i = 0; i < 16; i++)
                    {
                        buf[i].A = data[pos++];
                        buf[i].R = data[pos++];
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        buf[i].G = data[pos++];
                        buf[i].B = data[pos++];
                    }
                    int idx = 0;
                    for (int y = 0; y < bh; y++)
                        for (int x = 0; x < bw; x++)
                            SetPixel(outPixels, bx + x, by + y, buf[idx++]);
                }
                break;

            case 0x08: // C4
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x += 2)
                    {
                        byte b = data[pos++];
                        int hi = b >> 4;
                        int lo = b & 0xF;
                        SetPixel(outPixels, bx + x, by + y, PaletteLookup(palette, hi));
                        if (bx + x + 1 < Width)
                            SetPixel(outPixels, bx + x + 1, by + y, PaletteLookup(palette, lo));
                    }
                break;

            case 0x09: // C8
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        int idx = data[pos++];
                        SetPixel(outPixels, bx + x, by + y, PaletteLookup(palette, idx));
                    }
                break;

            case 0x0A: // C14X2
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        ushort raw = (ushort)((data[pos] << 8) | data[pos + 1]);
                        pos += 2;
                        int idx = raw & 0x3FFF;
                        SetPixel(outPixels, bx + x, by + y, PaletteLookup(palette, idx));
                    }
                break;

            case 0x0E: // CMPR

                {
                    int[] subX = { 0, 4, 0, 4 };
                    int[] subY = { 0, 0, 4, 4 };
                    for (int s = 0; s < 4; s++)
                        pos = DecodeDxt1Block(data, pos, bx + subX[s], by + subY[s], outPixels);
                }
                break;

            default:
                throw new NotSupportedException(
                    $"Format 0x{ImageFormat:X2} not implemented for pixel decode."
                );
        }
        return pos;
    }

    private static RgbaColor PaletteLookup(List<RgbaColor> palette, int index)
    {
        if (palette == null || index < 0 || index >= palette.Count)
            return default;
        return palette[index];
    }

    private int DecodeDxt1Block(byte[] data, int pos, int bx, int by, RgbaColor[,] outPixels)
    {
        ushort c0raw = (ushort)((data[pos] << 8) | data[pos + 1]);
        pos += 2;
        ushort c1raw = (ushort)((data[pos] << 8) | data[pos + 1]);
        pos += 2;
        uint idxBits = (uint)(
            (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3]
        );
        pos += 4;

        var c0 = DecodeRgb565(c0raw);
        var c1 = DecodeRgb565(c1raw);
        var palette4 = new RgbaColor[4];
        palette4[0] = c0;
        palette4[1] = c1;
        if (c0raw > c1raw)
        {
            palette4[2] = Lerp(c0, c1, 1, 3);
            palette4[3] = Lerp(c0, c1, 2, 3);
        }
        else
        {
            palette4[2] = Lerp(c0, c1, 1, 2);
            palette4[3] = new RgbaColor(0, 0, 0, 0); // transparent
        }

        for (int i = 0; i < 16; i++)
        {
            int shift = 30 - i * 2;
            int sel = (int)((idxBits >> shift) & 0x3);
            int x = i % 4,
                y = i / 4;
            SetPixel(outPixels, bx + x, by + y, palette4[sel]);
        }
        return pos;
    }

    private static RgbaColor Lerp(RgbaColor a, RgbaColor b, int num, int den) =>
        new RgbaColor(
            (byte)((a.R * (den - num) + b.R * num) / den),
            (byte)((a.G * (den - num) + b.G * num) / den),
            (byte)((a.B * (den - num) + b.B * num) / den),
            255
        );

    private void SetPixel(RgbaColor[,] grid, int x, int y, RgbaColor c)
    {
        if (x < Width && y < Height)
            grid[x, y] = c;
    }

    private static RgbaColor IntensityColor(byte i) => new RgbaColor(i, i, i, 255);

    private static RgbaColor DecodeRgb565(ushort raw)
    {
        int r = (raw >> 11) & 0x1F,
            g = (raw >> 5) & 0x3F,
            b = raw & 0x1F;
        return new RgbaColor(
            (byte)((r << 3) | (r >> 2)),
            (byte)((g << 2) | (g >> 4)),
            (byte)((b << 3) | (b >> 2)),
            255
        );
    }

    private static RgbaColor DecodeRgb5A3(ushort raw)
    {
        if ((raw & 0x8000) != 0)
        {
            int r = (raw >> 10) & 0x1F,
                g = (raw >> 5) & 0x1F,
                b = raw & 0x1F;
            return new RgbaColor(
                (byte)((r << 3) | (r >> 2)),
                (byte)((g << 3) | (g >> 2)),
                (byte)((b << 3) | (b >> 2)),
                255
            );
        }
        int a = (raw >> 12) & 0x7,
            rr = (raw >> 8) & 0xF,
            gg = (raw >> 4) & 0xF,
            bb = raw & 0xF;
        return new RgbaColor(
            (byte)((rr << 4) | rr),
            (byte)((gg << 4) | gg),
            (byte)((bb << 4) | bb),
            (byte)((a << 5) | (a << 2) | (a >> 1))
        );
    }

    private int EncodeBlock(
        byte[] data,
        int pos,
        int bx,
        int by,
        int bw,
        int bh,
        RgbaColor[,] pixels,
        int w,
        int h
    )
    {
        switch (ImageFormat)
        {
            case 0x00: // I4
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x += 2)
                    {
                        byte hi = (byte)(GetPix(pixels, bx + x, by + y, w, h).R / 17);
                        byte lo =
                            (bx + x + 1 < w)
                                ? (byte)(GetPix(pixels, bx + x + 1, by + y, w, h).R / 17)
                                : (byte)0;
                        data[pos++] = (byte)((hi << 4) | lo);
                    }
                break;

            case 0x01: // I8
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                        data[pos++] = GetPix(pixels, bx + x, by + y, w, h).R;
                break;

            case 0x02: // IA4
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        var c = GetPix(pixels, bx + x, by + y, w, h);
                        data[pos++] = (byte)(((c.R / 17) << 4) | (c.A / 17));
                    }
                break;

            case 0x03: // IA8
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        var c = GetPix(pixels, bx + x, by + y, w, h);
                        data[pos++] = c.A;
                        data[pos++] = c.R;
                    }
                break;

            case 0x04: // RGB565
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        ushort raw = EncodeRgb565(GetPix(pixels, bx + x, by + y, w, h));
                        data[pos++] = (byte)(raw >> 8);
                        data[pos++] = (byte)raw;
                    }
                break;

            case 0x05: // RGB5A3
                for (int y = 0; y < bh; y++)
                    for (int x = 0; x < bw; x++)
                    {
                        ushort raw = EncodeRgb5A3(GetPix(pixels, bx + x, by + y, w, h));
                        data[pos++] = (byte)(raw >> 8);
                        data[pos++] = (byte)raw;
                    }
                break;

            case 0x06: // RGBA32

                {
                    var buf = new RgbaColor[16];
                    int idx = 0;
                    for (int y = 0; y < bh; y++)
                        for (int x = 0; x < bw; x++)
                            buf[idx++] = GetPix(pixels, bx + x, by + y, w, h);
                    foreach (var c in buf)
                    {
                        data[pos++] = c.A;
                        data[pos++] = c.R;
                    }
                    foreach (var c in buf)
                    {
                        data[pos++] = c.G;
                        data[pos++] = c.B;
                    }
                }
                break;

            case 0x0E: // CMPR

                {
                    int[] subX = { 0, 4, 0, 4 };
                    int[] subY = { 0, 0, 4, 4 };
                    for (int s = 0; s < 4; s++)
                        pos = EncodeDxt1Block(data, pos, bx + subX[s], by + subY[s], pixels, w, h);
                }
                break;

            default:
                throw new NotSupportedException(
                    $"Format 0x{ImageFormat:X2} not implemented for pixel encode."
                );
        }
        return pos;
    }

    private const byte Dxt1AlphaThreshold = 128;

    private int EncodeDxt1Block(
        byte[] data,
        int pos,
        int bx,
        int by,
        RgbaColor[,] pixels,
        int w,
        int h
    )
    {
        var block = new RgbaColor[16];
        for (int i = 0; i < 16; i++)
        {
            int x = i % 4,
                y = i / 4;
            block[i] = GetPix(pixels, bx + x, by + y, w, h);
        }

        bool needsAlpha = false;
        foreach (var p in block)
        {
            if (p.A < Dxt1AlphaThreshold)
            {
                needsAlpha = true;
                break;
            }
        }

        byte minR = 255,
            minG = 255,
            minB = 255,
            maxR = 0,
            maxG = 0,
            maxB = 0;
        bool any = false;
        foreach (var p in block)
        {
            if (needsAlpha && p.A < Dxt1AlphaThreshold)
                continue;
            any = true;
            if (p.R < minR)
                minR = p.R;
            if (p.G < minG)
                minG = p.G;
            if (p.B < minB)
                minB = p.B;
            if (p.R > maxR)
                maxR = p.R;
            if (p.G > maxG)
                maxG = p.G;
            if (p.B > maxB)
                maxB = p.B;
        }
        if (!any)
        {
            // Entire block is transparent - endpoints don't matter,
            // every pixel will be forced to the transparent index.
            minR = minG = minB = maxR = maxG = maxB = 0;
        }

        var lo = new RgbaColor(minR, minG, minB, 255);
        var hi = new RgbaColor(maxR, maxG, maxB, 255);

        ushort c0,
            c1;
        if (needsAlpha)
        {
            ushort ca = EncodeRgb565(hi);
            ushort cb = EncodeRgb565(lo);
            // Alpha mode requires c0 <= c1 (equality is fine); order by
            // raw value rather than by which was "hi"/"lo" in RGB terms.
            if (ca <= cb)
            {
                c0 = ca;
                c1 = cb;
            }
            else
            {
                c0 = cb;
                c1 = ca;
            }
        }
        else
        {
            c0 = EncodeRgb565(hi);
            c1 = EncodeRgb565(lo);
            // Opaque mode requires c0 > c1 strictly, or the block is
            // misinterpreted as alpha mode on decode.
            if (c0 <= c1)
            {
                if (c1 > 0)
                    c1 = (ushort)(c1 - 1);
                else
                    c0 = (ushort)(c0 + 1);
            }
        }

        var d0 = DecodeRgb565(c0);
        var d1 = DecodeRgb565(c1);
        var palette4 = new RgbaColor[4];
        palette4[0] = d0;
        palette4[1] = d1;
        if (needsAlpha)
        {
            palette4[2] = Lerp(d0, d1, 1, 2); // average - 3-color mode
            palette4[3] = new RgbaColor(0, 0, 0, 0); // reserved: transparent
        }
        else
        {
            palette4[2] = Lerp(d0, d1, 1, 3);
            palette4[3] = Lerp(d0, d1, 2, 3);
        }

        data[pos++] = (byte)(c0 >> 8);
        data[pos++] = (byte)c0;
        data[pos++] = (byte)(c1 >> 8);
        data[pos++] = (byte)c1;

        int matchLimit = needsAlpha ? 3 : 4; // don't nearest-match against the transparent slot
        uint idxBits = 0;
        for (int i = 0; i < 16; i++)
        {
            var c = block[i];

            int best;
            if (needsAlpha && c.A < Dxt1AlphaThreshold)
            {
                best = 3; // force transparent index
            }
            else
            {
                best = 0;
                int bestDist = int.MaxValue;
                for (int p = 0; p < matchLimit; p++)
                {
                    int dr = c.R - palette4[p].R,
                        dg = c.G - palette4[p].G,
                        db = c.B - palette4[p].B;
                    int dist = dr * dr + dg * dg + db * db;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = p;
                    }
                }
            }
            idxBits |= (uint)best << (30 - i * 2);
        }
        data[pos++] = (byte)(idxBits >> 24);
        data[pos++] = (byte)(idxBits >> 16);
        data[pos++] = (byte)(idxBits >> 8);
        data[pos++] = (byte)idxBits;
        return pos;
    }

    private RgbaColor GetPix(RgbaColor[,] grid, int x, int y, int w, int h) =>
        (x < w && y < h) ? grid[x, y] : default;

    private static ushort EncodeRgb565(RgbaColor c) =>
        (ushort)(((c.R >> 3) << 11) | ((c.G >> 2) << 5) | (c.B >> 3));

    private static ushort EncodeRgb5A3(RgbaColor c)
    {
        if (c.A >= 224)
            return (ushort)(0x8000 | ((c.R >> 3) << 10) | ((c.G >> 3) << 5) | (c.B >> 3));
        return (ushort)(((c.A >> 5) << 12) | ((c.R >> 4) << 8) | ((c.G >> 4) << 4) | (c.B >> 4));
    }
}

public partial class BmdTexture
{
    public void Grayscale()
    {
        bool isPaletted = ImageFormat == 0x08 || ImageFormat == 0x09 || ImageFormat == 0x0A;

        if (isPaletted)
        {
            var palette = GetPaletteColors();
            for (int i = 0; i < palette.Count; i++)
                palette[i] = ToGrayscale(palette[i]);
            SetPaletteColors(palette);
        }
        else
        {
            var pixels = GetPixelColors();
            int w = pixels.GetLength(0),
                h = pixels.GetLength(1);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixels[x, y] = ToGrayscale(pixels[x, y]);
            SetPixelColorsAndRegenerateMips(pixels);
        }
    }

    private static RgbaColor ToGrayscale(RgbaColor c)
    {
        byte lum = (byte)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
        return new RgbaColor(lum, lum, lum, c.A);
    }

    public void RecolorByHue(
        RgbaColor targetColor,
        RgbaColor replacementColor,
        double hueToleranceDegrees = 25,
        double minSaturation = 0.15,
        bool preserveBrightness = true
    )
    {
        var (targetHue, targetSat, _) = RgbToHsv(targetColor);
        var (replHue, replSat, replVal) = RgbToHsv(replacementColor);

        bool isPaletted = ImageFormat == 0x08 || ImageFormat == 0x09 || ImageFormat == 0x0A;

        RgbaColor Remap(RgbaColor c)
        {
            var (h, s, v) = RgbToHsv(c);
            if (s < minSaturation)
                return c; // neutral/grayscale pixel, leave alone

            double diff = Math.Abs(h - targetHue);
            diff = Math.Min(diff, 360 - diff); // wrap-around distance on the hue wheel
            if (diff > hueToleranceDegrees)
                return c; // not the material we're targeting

            // Swap hue/saturation to the replacement color, but keep
            // this pixel's own brightness so existing shading/highlights
            // on the material are preserved instead of flattened.
            double newV = preserveBrightness ? v : replVal;
            return HsvToRgb(replHue, replSat, newV, c.A);
        }

        if (isPaletted)
        {
            var palette = GetPaletteColors();
            for (int i = 0; i < palette.Count; i++)
                palette[i] = Remap(palette[i]);
            SetPaletteColors(palette);
        }
        else
        {
            var pixels = GetPixelColors();
            int w = pixels.GetLength(0),
                h2 = pixels.GetLength(1);
            for (int y = 0; y < h2; y++)
                for (int x = 0; x < w; x++)
                    pixels[x, y] = Remap(pixels[x, y]);
            SetPixelColorsAndRegenerateMips(pixels);
        }
    }

    public void RecolorByHueMulti(
        List<(RgbaColor target, RgbaColor replacement, double hueTolerance)> swaps,
        double minSaturation = 0.15,
        bool preserveBrightness = true
    )
    {
        bool isPaletted = ImageFormat == 0x08 || ImageFormat == 0x09 || ImageFormat == 0x0A;

        var prepared =
            new List<(double hue, double sat, double replHue, double replSat, double replVal, double tol)>();
        foreach (var (target, replacement, tol) in swaps)
        {
            var (th, ts, _) = RgbToHsv(target);
            var (rh, rs, rv) = RgbToHsv(replacement);
            prepared.Add((th, ts, rh, rs, rv, tol));
        }

        RgbaColor Remap(RgbaColor c)
        {
            var (h, s, v) = RgbToHsv(c);
            if (s < minSaturation)
                return c;

            foreach (var (targetHue, _, replHue, replSat, replVal, tol) in prepared)
            {
                double diff = Math.Abs(h - targetHue);
                diff = Math.Min(diff, 360 - diff);
                if (diff <= tol)
                {
                    double newV = preserveBrightness ? v : replVal;
                    return HsvToRgb(replHue, replSat, newV, c.A);
                }
            }
            return c; // no swap matched
        }

        if (isPaletted)
        {
            var palette = GetPaletteColors();
            for (int i = 0; i < palette.Count; i++)
                palette[i] = Remap(palette[i]);
            SetPaletteColors(palette);
        }
        else
        {
            var pixels = GetPixelColors();
            int w = pixels.GetLength(0),
                h = pixels.GetLength(1);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixels[x, y] = Remap(pixels[x, y]);
            SetPixelColorsAndRegenerateMips(pixels);
        }
    }

    private static (double h, double s, double v) RgbToHsv(RgbaColor c)
    {
        double r = c.R / 255.0,
            g = c.G / 255.0,
            b = c.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double h = 0;
        if (delta > 0.00001)
        {
            if (max == r)
                h = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                h = 60 * (((b - r) / delta) + 2);
            else
                h = 60 * (((r - g) / delta) + 4);
        }
        if (h < 0)
            h += 360;

        double s = max <= 0.00001 ? 0 : delta / max;
        double v = max;
        return (h, s, v);
    }

    private static RgbaColor HsvToRgb(double h, double s, double v, byte alpha)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double r = 0,
            g = 0,
            b = 0;
        if (h < 60)
        {
            r = c;
            g = x;
            b = 0;
        }
        else if (h < 120)
        {
            r = x;
            g = c;
            b = 0;
        }
        else if (h < 180)
        {
            r = 0;
            g = c;
            b = x;
        }
        else if (h < 240)
        {
            r = 0;
            g = x;
            b = c;
        }
        else if (h < 300)
        {
            r = x;
            g = 0;
            b = c;
        }
        else
        {
            r = c;
            g = 0;
            b = x;
        }

        return new RgbaColor(
            (byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255),
            (byte)Math.Round((b + m) * 255),
            alpha
        );
    }

    public void TintGrayscale(RgbaColor tintColor, double strength = 1.0)
    {
        var (tintHue, tintSat, _) = RgbToHsv(tintColor);
        strength = Math.Clamp(strength, 0.0, 1.0);

        RgbaColor Apply(RgbaColor c)
        {
            double lum = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
            var tinted = HsvToRgb(tintHue, tintSat, lum, c.A);

            if (strength >= 0.999)
                return tinted;

            byte grayVal = (byte)Math.Round(lum * 255);
            return new RgbaColor(
                (byte)Math.Round(grayVal + (tinted.R - grayVal) * strength),
                (byte)Math.Round(grayVal + (tinted.G - grayVal) * strength),
                (byte)Math.Round(grayVal + (tinted.B - grayVal) * strength),
                c.A
            );
        }

        bool isPaletted = ImageFormat == 0x08 || ImageFormat == 0x09 || ImageFormat == 0x0A;

        if (isPaletted)
        {
            var palette = GetPaletteColors();
            for (int i = 0; i < palette.Count; i++)
                palette[i] = Apply(palette[i]);
            SetPaletteColors(palette);
        }
        else
        {
            var pixels = GetPixelColors();
            int w = pixels.GetLength(0),
                h = pixels.GetLength(1);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixels[x, y] = Apply(pixels[x, y]);
            SetPixelColorsAndRegenerateMips(pixels);
        }
    }
}

public class Mat3Chunk
{
    public List<RgbaColor> MaterialColors { get; private set; } = new();
    public List<RgbaColor> AmbientColors { get; private set; } = new();
    public List<(short R, short G, short B, short A)> TevColors { get; private set; } = new(); // TEV registers: TevPrev/Reg0/Reg1/Reg2
    public List<RgbaColor> TevKonstColors { get; private set; } = new(); // K0-K3
    public List<TevOrderEntry> TevOrders { get; private set; } = new();
    public List<TevStageEntry> TevStages { get; private set; } = new();

    public List<Mat3Material> Materials { get; private set; } = new();

    private readonly byte[] _chunk;
    private readonly int _materialColorOffset;
    private readonly int _ambientColorOffset;
    private readonly int _tevColorOffset;
    private readonly int _tevKColorOffset;

    private enum OffsetIndex
    {
        MaterialData = 0,
        IndexData = 1,
        NameTable = 2,
        IndirectData = 3,
        CullMode = 4,
        MaterialColor = 5,
        ColorChannelCount = 6,
        ColorChannelData = 7,
        AmbientColorData = 8,
        LightData = 9,
        TexGenCount = 10,
        TexCoordData = 11,
        TexCoord2Data = 12,
        TexMatrixData = 13,
        TexMatrix2Data = 14,
        TexNoData = 15,
        TevOrderData = 16,
        TevColorData = 17,
        TevKColorData = 18,
        TevStageCount = 19,
        TevStageData = 20,
        TevSwapModeData = 21,
        TevSwapModeTable = 22,
        FogData = 23,
        AlphaCompareData = 24,
        BlendData = 25,
        ZModeData = 26,
        ZCompLoc = 27,
        DitherData = 28,
        NBTScaleData = 29
    }

    public Mat3Chunk(byte[] chunkBytes)
    {
        _chunk = (byte[])chunkBytes.Clone();

        int matCount = ReadU16(_chunk, 0x08);
        var offsets = new int[30];
        for (int i = 0; i < 30; i++)
            offsets[i] = ReadS32(_chunk, 0x0C + i * 4);

        int matDataOffset = offsets[(int)OffsetIndex.MaterialData];
        int indexDataOffset = offsets[(int)OffsetIndex.IndexData];
        int nameTableOffset = offsets[(int)OffsetIndex.NameTable];
        int texNoOffset = offsets[(int)OffsetIndex.TexNoData];

        _materialColorOffset = offsets[(int)OffsetIndex.MaterialColor];
        _ambientColorOffset = offsets[(int)OffsetIndex.AmbientColorData];
        _tevColorOffset = offsets[(int)OffsetIndex.TevColorData];
        _tevKColorOffset = offsets[(int)OffsetIndex.TevKColorData];
        int tevOrderOffset = offsets[(int)OffsetIndex.TevOrderData];
        int tevStageOffset = offsets[(int)OffsetIndex.TevStageData];

        int SizeOf(OffsetIndex idx) =>
            SizeOfSection(
                offsets,
                (int)idx,
                _chunk.Length - matDataOffset /*fallback, unused normally*/
            );

        MaterialColors = ReadColorArray(_materialColorOffset, SizeOf(OffsetIndex.MaterialColor));
        AmbientColors = ReadColorArray(_ambientColorOffset, SizeOf(OffsetIndex.AmbientColorData));
        TevKonstColors = ReadColorArray(_tevKColorOffset, SizeOf(OffsetIndex.TevKColorData));
        TevColors = ReadS16ColorArray(_tevColorOffset, SizeOf(OffsetIndex.TevColorData));

        int tevOrderSize = SizeOf(OffsetIndex.TevOrderData);
        for (int p = tevOrderOffset; p < tevOrderOffset + tevOrderSize; p += 4)
        {
            TevOrders.Add(
                new TevOrderEntry
                {
                    TexCoord = _chunk[p],
                    TexMap = _chunk[p + 1],
                    ChannelId = _chunk[p + 2]
                }
            );
        }

        int tevStageSize = SizeOf(OffsetIndex.TevStageData);
        for (int p = tevStageOffset; p < tevStageOffset + tevStageSize; p += 20)
            TevStages.Add(TevStageEntry.Read(_chunk, p));

        var names = ReadNameTable(_chunk, nameTableOffset);

        var remap = new int[matCount];
        for (int i = 0; i < matCount; i++)
            remap[i] = ReadS16(_chunk, indexDataOffset + i * 2);

        int highest = 0;
        foreach (var r in remap)
            if (r > highest)
                highest = r;

        var uniqueMats = new List<Mat3Material>();
        int pos = matDataOffset;
        for (int i = 0; i <= highest; i++)
        {
            uniqueMats.Add(Mat3Material.Read(_chunk, pos, texNoOffset));
            pos += 332;
        }

        Materials = new List<Mat3Material>();
        for (int i = 0; i < matCount; i++)
        {
            var m = uniqueMats[remap[i]].Clone();
            m.Name = i < names.Count ? names[i] : $"material_{i}";
            Materials.Add(m);
        }
    }

    public string DescribeMaterial(int materialIndex)
    {
        var m = Materials[materialIndex];
        var sb = new StringBuilder();
        sb.AppendLine($"Material '{m.Name}' (index {materialIndex})");

        for (int i = 0; i < 2; i++)
            if (m.MaterialColorIdx[i] >= 0)
                sb.AppendLine(
                    $"  MaterialColor[{i}] -> MaterialColors[{m.MaterialColorIdx[i]}] = {MaterialColors[m.MaterialColorIdx[i]]}"
                );

        for (int i = 0; i < 2; i++)
            if (m.AmbientColorIdx[i] >= 0)
                sb.AppendLine(
                    $"  AmbientColor[{i}] -> AmbientColors[{m.AmbientColorIdx[i]}] = {AmbientColors[m.AmbientColorIdx[i]]}"
                );

        for (int i = 0; i < 4; i++)
            if (m.KonstColorIdx[i] >= 0)
                sb.AppendLine(
                    $"  KonstColor[{i}] -> TevKonstColors[{m.KonstColorIdx[i]}] = {TevKonstColors[m.KonstColorIdx[i]]}"
                );

        for (int i = 0; i < 4; i++)
            if (m.TevColorIdx[i] >= 0)
                sb.AppendLine(
                    $"  TevColor[{i}] (register) -> TevColors[{m.TevColorIdx[i]}] = {TevColors[m.TevColorIdx[i]]}"
                );

        sb.AppendLine("  Active TEV stages:");
        for (int i = 0; i < 16; i++)
        {
            if (m.TevStageIdx[i] < 0)
                continue;
            var stage = TevStages[m.TevStageIdx[i]];
            int orderIdx = m.TevOrderIdx[i];
            bool usesTexture = orderIdx >= 0 && TevOrders[orderIdx].TexMap != 0xFF;
            bool usesRaster = orderIdx >= 0 && TevOrders[orderIdx].ChannelId != 0xFF;

            sb.AppendLine(
                $"    Stage {i}: color = {stage.ColorInA}*A {stage.ColorInB}*B {stage.ColorInC}*C {stage.ColorInD}*D "
                    + $"-> {stage.ColorRegId} | texture={(usesTexture ? "yes" : "NO")} raster={(usesRaster ? "yes" : "NO")} "
                    + $"konstColorSel={m.ColorSels[i]}"
            );
        }

        return sb.ToString();
    }

    public void SetMaterialColor(int index, RgbaColor color) => MaterialColors[index] = color;

    public void SetAmbientColor(int index, RgbaColor color) => AmbientColors[index] = color;

    public void SetTevKonstColor(int index, RgbaColor color) => TevKonstColors[index] = color;

    public void SetTevColor(int index, short r, short g, short b, short a) =>
        TevColors[index] = (r, g, b, a);

    public void RecolorMaterial(int materialIndex, RgbaColor newColor)
    {
        var m = Materials[materialIndex];
        foreach (var idx in m.MaterialColorIdx)
            if (idx >= 0)
                SetMaterialColor(idx, newColor);
        foreach (var idx in m.AmbientColorIdx)
            if (idx >= 0)
                SetAmbientColor(idx, newColor);
        foreach (var idx in m.KonstColorIdx)
            if (idx >= 0)
                SetTevKonstColor(idx, newColor);
        foreach (var idx in m.TevColorIdx)
            if (idx >= 0)
                SetTevColor(idx, newColor.R, newColor.G, newColor.B, newColor.A);
    }

    public byte[] GetPatchedChunkBytes()
    {
        byte[] result = (byte[])_chunk.Clone();

        for (int i = 0; i < MaterialColors.Count; i++)
            WriteColor(result, _materialColorOffset + i * 4, MaterialColors[i]);

        for (int i = 0; i < AmbientColors.Count; i++)
            WriteColor(result, _ambientColorOffset + i * 4, AmbientColors[i]);

        for (int i = 0; i < TevKonstColors.Count; i++)
            WriteColor(result, _tevKColorOffset + i * 4, TevKonstColors[i]);

        for (int i = 0; i < TevColors.Count; i++)
        {
            int p = _tevColorOffset + i * 8;
            WriteS16(result, p, TevColors[i].R);
            WriteS16(result, p + 2, TevColors[i].G);
            WriteS16(result, p + 4, TevColors[i].B);
            WriteS16(result, p + 6, TevColors[i].A);
        }

        return result;
    }

    private List<RgbaColor> ReadColorArray(int offset, int size)
    {
        var list = new List<RgbaColor>();
        if (offset <= 0)
            return list;
        int count = size / 4;
        for (int i = 0; i < count; i++)
            list.Add(
                new RgbaColor(
                    _chunk[offset + i * 4],
                    _chunk[offset + i * 4 + 1],
                    _chunk[offset + i * 4 + 2],
                    _chunk[offset + i * 4 + 3]
                )
            );
        return list;
    }

    private List<(short, short, short, short)> ReadS16ColorArray(int offset, int size)
    {
        var list = new List<(short, short, short, short)>();
        if (offset <= 0)
            return list;
        int count = size / 8;
        for (int i = 0; i < count; i++)
        {
            int p = offset + i * 8;
            list.Add(
                (
                    ReadS16(_chunk, p),
                    ReadS16(_chunk, p + 2),
                    ReadS16(_chunk, p + 4),
                    ReadS16(_chunk, p + 6)
                )
            );
        }
        return list;
    }

    private static int SizeOfSection(int[] offsets, int index, int fallback)
    {
        int start = offsets[index];
        if (start <= 0)
            return 0;

        int next = int.MaxValue;
        for (int i = 0; i < offsets.Length; i++)
        {
            if (i == index)
                continue;
            if (offsets[i] > start && offsets[i] < next)
                next = offsets[i];
        }
        return next == int.MaxValue ? fallback : next - start;
    }

    private static List<string> ReadNameTable(byte[] chunk, int offset)
    {
        var names = new List<string>();
        int count = ReadU16(chunk, offset);
        for (int i = 0; i < count; i++)
        {
            int entryOff = offset + 4 + i * 4;
            int strOff = ReadU16(chunk, entryOff + 2);
            int abs = offset + strOff;
            var sb = new StringBuilder();
            while (abs < chunk.Length && chunk[abs] != 0)
                sb.Append((char)chunk[abs++]);
            names.Add(sb.ToString());
        }
        return names;
    }

    private static void WriteColor(byte[] d, int o, RgbaColor c)
    {
        d[o] = c.R;
        d[o + 1] = c.G;
        d[o + 2] = c.B;
        d[o + 3] = c.A;
    }

    static ushort ReadU16(byte[] d, int o) => (ushort)((d[o] << 8) | d[o + 1]);

    static short ReadS16(byte[] d, int o) => (short)((d[o] << 8) | d[o + 1]);

    static int ReadS32(byte[] d, int o) =>
        (d[o] << 24) | (d[o + 1] << 16) | (d[o + 2] << 8) | d[o + 3];

    static void WriteS16(byte[] d, int o, short v)
    {
        d[o] = (byte)(v >> 8);
        d[o + 1] = (byte)v;
    }
}

public struct TevOrderEntry
{
    public byte TexCoord;
    public byte TexMap; // 0xFF = none
    public byte ChannelId; // 0xFF = none (no vertex/raster color read)
}

public struct TevStageEntry
{
    public byte ColorInA,
        ColorInB,
        ColorInC,
        ColorInD;
    public byte ColorOp,
        ColorBias,
        ColorScale;
    public bool ColorClamp;
    public byte ColorRegId;

    public byte AlphaInA,
        AlphaInB,
        AlphaInC,
        AlphaInD;
    public byte AlphaOp,
        AlphaBias,
        AlphaScale;
    public bool AlphaClamp;
    public byte AlphaRegId;

    public static TevStageEntry Read(byte[] d, int o)
    {
        // byte 0 is an unused/skip byte in the file format
        return new TevStageEntry
        {
            ColorInA = d[o + 1],
            ColorInB = d[o + 2],
            ColorInC = d[o + 3],
            ColorInD = d[o + 4],
            ColorOp = d[o + 5],
            ColorBias = d[o + 6],
            ColorScale = d[o + 7],
            ColorClamp = d[o + 8] != 0,
            ColorRegId = d[o + 9],
            AlphaInA = d[o + 10],
            AlphaInB = d[o + 11],
            AlphaInC = d[o + 12],
            AlphaInD = d[o + 13],
            AlphaOp = d[o + 14],
            AlphaBias = d[o + 15],
            AlphaScale = d[o + 16],
            AlphaClamp = d[o + 17] != 0,
            AlphaRegId = d[o + 18]
            // byte 19 is an unused/skip byte
        };
    }
}

public class Mat3Material
{
    public string Name;
    public int[] MaterialColorIdx = new int[2];
    public int[] AmbientColorIdx = new int[2];
    public int[] TextureIdx = new int[8]; // resolved TEX1 texture index, -1 = none
    public int[] KonstColorIdx = new int[4];
    public byte[] ColorSels = new byte[16]; // KonstColorSel per stage
    public byte[] AlphaSels = new byte[16]; // KonstAlphaSel per stage
    public int[] TevOrderIdx = new int[16];
    public int[] TevColorIdx = new int[4];
    public int[] TevStageIdx = new int[16];

    public static Mat3Material Read(byte[] d, int o, int texNoTableOffset)
    {
        var m = new Mat3Material();
        int p = o;

        p += 8; // Flag, CullMode, ColorChanCount, TexGenCount, TevStageCount, ZCompLoc, ZMode, Dither (all index bytes - skipped)

        m.MaterialColorIdx[0] = ReadS16(d, p);
        p += 2;
        m.MaterialColorIdx[1] = ReadS16(d, p);
        p += 2;

        p += 8; // ChannelControl indices x4 (short) - skipped

        m.AmbientColorIdx[0] = ReadS16(d, p);
        p += 2;
        m.AmbientColorIdx[1] = ReadS16(d, p);
        p += 2;

        p += 16; // LightingColor indices x8 - skipped
        p += 16; // TexCoord1Gen indices x8 - skipped
        p += 16; // PostTexCoordGen indices x8 - skipped
        p += 20; // TexMatrix1 indices x10 - skipped
        p += 40; // PostTexMatrix indices x20 - skipped

        for (int i = 0; i < 8; i++)
        {
            int texRemapIdx = ReadS16(d, p);
            p += 2;
            // TextureIndices in the file are indices into the TexNo
            // remap table (a short array at TexNoData), which in turn
            // gives the real TEX1 index. Resolve through it here.
            m.TextureIdx[i] =
                (texRemapIdx >= 0) ? ReadS16(d, texNoTableOffset + texRemapIdx * 2) : -1;
        }

        for (int i = 0; i < 4; i++)
        {
            m.KonstColorIdx[i] = ReadS16(d, p);
            p += 2;
        }

        for (int i = 0; i < 16; i++)
            m.ColorSels[i] = d[p++];
        for (int i = 0; i < 16; i++)
            m.AlphaSels[i] = d[p++];

        for (int i = 0; i < 16; i++)
        {
            m.TevOrderIdx[i] = ReadS16(d, p);
            p += 2;
        }
        for (int i = 0; i < 4; i++)
        {
            m.TevColorIdx[i] = ReadS16(d, p);
            p += 2;
        }
        for (int i = 0; i < 16; i++)
        {
            m.TevStageIdx[i] = ReadS16(d, p);
            p += 2;
        }

        // SwapModes x16, SwapTables x16, Fog, AlphaCompare, BlendMode, NBTScale
        // follow here but aren't parsed - not needed for color tracing.

        return m;
    }

    public Mat3Material Clone() =>
        new Mat3Material
        {
            Name = Name,
            MaterialColorIdx = (int[])MaterialColorIdx.Clone(),
            AmbientColorIdx = (int[])AmbientColorIdx.Clone(),
            TextureIdx = (int[])TextureIdx.Clone(),
            KonstColorIdx = (int[])KonstColorIdx.Clone(),
            ColorSels = (byte[])ColorSels.Clone(),
            AlphaSels = (byte[])AlphaSels.Clone(),
            TevOrderIdx = (int[])TevOrderIdx.Clone(),
            TevColorIdx = (int[])TevColorIdx.Clone(),
            TevStageIdx = (int[])TevStageIdx.Clone()
        };

    static short ReadS16(byte[] d, int o) => (short)((d[o] << 8) | d[o + 1]);
}
