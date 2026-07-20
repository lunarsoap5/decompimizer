// C# port of the classic Yaz0 encoder (originally by shevious, based on thakis's yaz0dec).

using System;
using System.IO;

namespace RarcTools
{
    public class Yaz0Encoder
    {
        private const int MaxDistance = 0x1000; // 4096 — max lookback window
        private const int MinMatchLength = 3; // matches shorter than this aren't worth encoding

        private uint _lookaheadNumBytes;
        private uint _lookaheadMatchPos;
        private bool _lookaheadPending;

        /// <summary>
        /// Encodes the given bytes as Yaz0 and writes the full file (header + compressed data)
        /// to destPath.
        /// </summary>
        public void EncodeToFile(byte[] src, string destPath)
        {
            using FileStream fs = new FileStream(destPath, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new BinaryWriter(fs);

            // 4-byte magic
            writer.Write((byte)'Y');
            writer.Write((byte)'a');
            writer.Write((byte)'z');
            writer.Write((byte)'0');

            // 4-byte uncompressed size, big-endian
            WriteU32BE(writer, (uint)src.Length);

            // 8 bytes unused/dummy padding
            writer.Write(new byte[8]);

            EncodeYaz0(src, fs);
        }

        /// <summary>
        /// Encodes the given bytes as Yaz0 and returns the full file contents (header + data)
        /// as a byte array, without touching disk.
        /// </summary>
        public byte[] Encode(byte[] src)
        {
            using MemoryStream ms = new MemoryStream();
            using (
                BinaryWriter writer = new BinaryWriter(
                    ms,
                    System.Text.Encoding.ASCII,
                    leaveOpen: true
                )
            )
            {
                writer.Write((byte)'Y');
                writer.Write((byte)'a');
                writer.Write((byte)'z');
                writer.Write((byte)'0');
                WriteU32BE(writer, (uint)src.Length);
                writer.Write(new byte[8]);
            }

            EncodeYaz0(src, ms);
            return ms.ToArray();
        }

        // Simple, straightforward match finder: scans backward up to MaxDistance bytes
        // looking for the longest run that matches the data starting at `pos`.
        private uint SimpleEnc(byte[] src, int size, int pos, out uint matchPosOut)
        {
            int startPos = pos - MaxDistance;
            uint numBytes = 1;
            uint matchPos = 0;

            if (startPos < 0)
                startPos = 0;

            for (int i = startPos; i < pos; i++)
            {
                int j;
                for (j = 0; j < size - pos; j++)
                {
                    if (src[i + j] != src[j + pos])
                        break;
                }
                if (j > numBytes)
                {
                    numBytes = (uint)j;
                    matchPos = (uint)i;
                }
            }

            matchPosOut = matchPos;
            if (numBytes == 2)
                numBytes = 1;
            return numBytes;
        }

        private uint NintendoEnc(byte[] src, int size, int pos, out uint matchPosOut)
        {
            uint numBytes;

            if (_lookaheadPending)
            {
                matchPosOut = _lookaheadMatchPos;
                _lookaheadPending = false;
                return _lookaheadNumBytes;
            }

            numBytes = SimpleEnc(src, size, pos, out uint matchPos);
            matchPosOut = matchPos;

            if (numBytes >= MinMatchLength)
            {
                uint numBytes1 = SimpleEnc(src, size, pos + 1, out uint nextMatchPos);
                if (numBytes1 >= numBytes + 2)
                {
                    numBytes = 1;
                    _lookaheadPending = true;
                    _lookaheadNumBytes = numBytes1;
                    _lookaheadMatchPos = nextMatchPos;
                }
            }

            return numBytes;
        }

        private void EncodeYaz0(byte[] src, Stream dstStream)
        {
            int srcSize = src.Length;
            int srcPos = 0;

            byte[] dst = new byte[24]; // 8 codes * 3 bytes maximum
            int dstPos = 0;

            uint validBitCount = 0; // number of valid bits left in "code" byte
            byte currCodeByte = 0;

            // Reset lookahead state for this encode pass
            _lookaheadPending = false;
            _lookaheadNumBytes = 0;
            _lookaheadMatchPos = 0;

            while (srcPos < srcSize)
            {
                uint numBytes = NintendoEnc(src, srcSize, srcPos, out uint matchPos);

                if (numBytes < MinMatchLength)
                {
                    // Straight copy of a single literal byte
                    dst[dstPos] = src[srcPos];
                    dstPos++;
                    srcPos++;
                    // Set flag bit for straight copy
                    currCodeByte |= (byte)(0x80 >> (int)validBitCount);
                }
                else
                {
                    // RLE (back-reference) part
                    uint dist = (uint)(srcPos - matchPos - 1);

                    if (numBytes >= 0x12) // 3-byte encoding
                    {
                        byte byte1 = (byte)(0 | (dist >> 8));
                        byte byte2 = (byte)(dist & 0xFF);
                        dst[dstPos++] = byte1;
                        dst[dstPos++] = byte2;

                        // Maximum run length for 3-byte encoding
                        if (numBytes > 0xFF + 0x12)
                            numBytes = 0xFF + 0x12;

                        byte byte3 = (byte)(numBytes - 0x12);
                        dst[dstPos++] = byte3;
                    }
                    else // 2-byte encoding
                    {
                        byte byte1 = (byte)(((numBytes - 2) << 4) | (dist >> 8));
                        byte byte2 = (byte)(dist & 0xFF);
                        dst[dstPos++] = byte1;
                        dst[dstPos++] = byte2;
                    }

                    srcPos += (int)numBytes;
                }

                validBitCount++;

                // Flush eight codes
                if (validBitCount == 8)
                {
                    dstStream.WriteByte(currCodeByte);
                    dstStream.Write(dst, 0, dstPos);
                    dstStream.Flush();

                    currCodeByte = 0;
                    validBitCount = 0;
                    dstPos = 0;
                }
            }

            // Flush any remaining partial code group
            if (validBitCount > 0)
            {
                dstStream.WriteByte(currCodeByte);
                dstStream.Write(dst, 0, dstPos);
            }
        }

        private static void WriteU32BE(BinaryWriter writer, uint value)
        {
            writer.Write((byte)(value >> 24));
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)(value & 0xFF));
        }
    }
}
