// Based on various public resources and SuperBMD created by Sage-of-Mirrors

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
    //   [btiTblOffset] array of texCount × 0x20-byte BTI headers
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
    //   2. BTI headers           (texCount × 0x20, pal/img offsets patched later)
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
        // Collect unique names in encounter order
        var seen = new Dictionary<string, (int palOff, int imgOff)>();
        var paletteOffsets = new int[texCount]; // from chunk start
        var imageOffsets = new int[texCount]; // from chunk start

        // --- palette data first ---
        foreach (var tex in Textures)
        {
            if (!seen.ContainsKey(tex.Name))
                seen[tex.Name] = (-1, -1); // placeholder; fill below
        }

        // Two-pass: palette pass then image pass (mirrors SuperBMD)
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
            // If no palette data the offset is still recorded (will be 0-relative, unused)
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
        // SuperBMD: offset = chunkOffset - headerOffsetFromChunkStart
        for (int i = 0; i < texCount; i++)
        {
            int headerChunkOffset = btiTblStart + i * 0x20; // from chunk start
            string name = Textures[i].Name;

            int palChunkOff = palChunkOffsets[name];
            int imgChunkOff = imgChunkOffsets[name];

            // Relative to this header's position
            int relPal = Textures[i].PaletteData.Length > 0 ? palChunkOff - headerChunkOffset : 0;
            int relImg = imgChunkOff - headerChunkOffset;

            // Write the full BTI header now that we know the offsets
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

        // Compute string area: pack all names
        var stringArea = new List<byte>();
        var stringOffsets = new int[count];
        for (int i = 0; i < count; i++)
        {
            stringOffsets[i] = entryHeaderSize + stringArea.Count;
            byte[] nameBytes = Encoding.ASCII.GetBytes(textures[i].Name);
            stringArea.AddRange(nameBytes);
            stringArea.Add(0); // null terminator
        }

        // Write header
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

// ============================================================
//  BmdTexture – one BTI texture embedded inside a BMD TEX1 chunk
// ============================================================
public class BmdTexture
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
        // 0x10..0x13 — unknown, leave zero
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
        // Standalone BTI: header at 0, image data at 0x20,
        // palette data (if any) immediately after image data.
        // Offsets in header are relative to header start (= file start).
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

    // ----------------------------------------------------------
    // Import from standalone .bti file
    // ----------------------------------------------------------
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

    // ----------------------------------------------------------
    // Compute total image data size across all mipmaps
    // ----------------------------------------------------------
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

    // Block geometry — matches gclib BLOCK_WIDTHS / BLOCK_HEIGHTS / BLOCK_DATA_SIZES
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
