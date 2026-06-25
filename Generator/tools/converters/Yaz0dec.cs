// Yaz0 Decoder - C# port
// Original C++ version 1.0 (20050213) by thakis

using System;
using System.IO;

internal class Yaz0dec
{
    // ── Byte-swap helper ───────────────────────────────────────────────────────

    static uint SwapU32(uint d) =>
        ((d & 0xFF) << 24) | (((d >> 8) & 0xFF) << 16) | (((d >> 16) & 0xFF) << 8) | (d >> 24);

    // ── Decoder ────────────────────────────────────────────────────────────────

    struct DecodeResult
    {
        public int SrcPos;
        public int DstPos;
    }

    static DecodeResult DecodeYaz0(
        byte[] src,
        int srcOffset,
        int srcSize,
        byte[] dst,
        int uncompressedSize
    )
    {
        var r = new DecodeResult { SrcPos = 0, DstPos = 0 };

        uint validBitCount = 0; // number of valid bits left in current code byte
        byte currCodeByte = 0;

        while (r.DstPos < uncompressedSize)
        {
            // Read a new code byte when the current one is exhausted
            if (validBitCount == 0)
            {
                currCodeByte = src[srcOffset + r.SrcPos];
                r.SrcPos++;
                validBitCount = 8;
            }

            if ((currCodeByte & 0x80) != 0)
            {
                // Straight copy
                dst[r.DstPos] = src[srcOffset + r.SrcPos];
                r.DstPos++;
                r.SrcPos++;
            }
            else
            {
                // RLE back-reference
                byte byte1 = src[srcOffset + r.SrcPos];
                byte byte2 = src[srcOffset + r.SrcPos + 1];
                r.SrcPos += 2;

                uint dist = (uint)(((byte1 & 0xF) << 8) | byte2);
                uint copySource = (uint)(r.DstPos - (dist + 1));
                uint numBytes = (uint)(byte1 >> 4);

                if (numBytes == 0)
                {
                    numBytes = src[srcOffset + r.SrcPos] + 0x12u;
                    r.SrcPos++;
                }
                else
                {
                    numBytes += 2;
                }

                // Copy the run
                for (uint i = 0; i < numBytes; i++)
                {
                    dst[r.DstPos] = dst[copySource];
                    copySource++;
                    r.DstPos++;
                }
            }

            // Shift to the next bit in the code byte
            currCodeByte <<= 1;
            validBitCount--;
        }

        return r;
    }

    // ── Block scanner & writer ─────────────────────────────────────────────────

    static string DecodeAll(byte[] src, int srcSize, string srcName)
    {
        int readBytes = 0;
        // Build output filename: "<srcName> <hex_offset>.rarc"
        string dstName = srcName;

        while (readBytes < srcSize)
        {
            // Scan forward for the 'Yaz0' magic bytes
            while (
                readBytes + 3 < srcSize
                && !(
                    src[readBytes] == 'Y'
                    && src[readBytes + 1] == 'a'
                    && src[readBytes + 2] == 'z'
                    && src[readBytes + 3] == '0'
                )
            )
            {
                readBytes++;
            }

            if (readBytes + 3 >= srcSize)
            {
                return dstName; // nothing left to decode and/or the file is already decoded
            }
            readBytes += 4; // skip 'Yaz0' magic

            // Read uncompressed size (big-endian u32 at current position)
            uint size = SwapU32(BitConverter.ToUInt32(src, readBytes));
            //Console.WriteLine($"Writing {dstName}");
            //Console.WriteLine($"Writing 0x{size:X} bytes");

            readBytes += 12; // 4-byte size + 8 bytes unused

            byte[] dst = new byte[size + 0x1000];
            DecodeResult r = DecodeYaz0(src, readBytes, srcSize - readBytes, dst, (int)size);
            readBytes += r.SrcPos;

            //Console.WriteLine($"Read 0x{readBytes:X} bytes from input");

            File.WriteAllBytes(dstName, dst[..r.DstPos]);
        }
        return dstName;
    }

    // ── Entry point ────────────────────────────────────────────────────────────

    public static string InitYaz0Decode(string args)
    {
        string filePath = args;
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"File not found: {filePath}");
            return "";
        }

        byte[] buffer = File.ReadAllBytes(filePath);
        //Console.WriteLine($"Input file size: 0x{buffer.Length:X}");

        return DecodeAll(buffer, buffer.Length, filePath);
        ;
    }
}
