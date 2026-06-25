// RARC Dump - C# port
// Original C++ version 1.0 (20050213) by thakis

using System;
using System.IO;
using System.Text;

namespace RarcTools
{
    internal class RARCDump
    {
        struct RarcHeader
        {
            public string Type;
            public uint Size;
            public uint Unknown;
            public uint DataStartOffset;
            public uint[] Unknown2;

            public uint NumNodes;
            public uint[] Unknown3;
            public uint FileEntriesOffset;
            public uint Unknown4;
            public uint StringTableOffset;
            public uint[] Unknown5;
        }

        struct Node
        {
            public string Type;
            public uint FilenameOffset;
            public ushort Unknown;
            public ushort NumFileEntries;
            public uint FirstFileEntryOffset;
        }

        struct FileEntry
        {
            public ushort Id;
            public ushort Unknown;
            public ushort Unknown2;
            public ushort FilenameOffset;
            public uint DataOffset;
            public uint DataSize;
            public uint Zero;
        }

        static string GetString(BinaryReader br, long pos)
        {
            long temp = br.BaseStream.Position;
            br.BaseStream.Seek(pos, SeekOrigin.Begin);

            StringBuilder sb = new StringBuilder();

            byte b;
            while ((b = br.ReadByte()) != 0)
            {
                sb.Append((char)b);
            }

            br.BaseStream.Seek(temp, SeekOrigin.Begin);

            return sb.ToString();
        }

        static Node GetNode(int i, BinaryReader br)
        {
            br.BaseStream.Seek(0x40 + i * 0x10, SeekOrigin.Begin);

            Node n = new Node();

            n.Type = Encoding.ASCII.GetString(br.ReadBytes(4));
            n.FilenameOffset = ReadUInt32BE(br);
            n.Unknown = ReadUInt16BE(br);
            n.NumFileEntries = ReadUInt16BE(br);
            n.FirstFileEntryOffset = ReadUInt32BE(br);

            return n;
        }

        static FileEntry GetFileEntry(int i, RarcHeader h, BinaryReader br)
        {
            br.BaseStream.Seek(h.FileEntriesOffset + i * 0x14 + 0x20, SeekOrigin.Begin);

            FileEntry fe = new FileEntry();

            fe.Id = ReadUInt16BE(br);
            fe.Unknown = ReadUInt16BE(br);
            fe.Unknown2 = ReadUInt16BE(br);
            fe.FilenameOffset = ReadUInt16BE(br);
            fe.DataOffset = ReadUInt32BE(br);
            fe.DataSize = ReadUInt32BE(br);
            fe.Zero = ReadUInt32BE(br);

            return fe;
        }

        static void DumpNode(Node n, RarcHeader h, BinaryReader br, string directory)
        {
            string nodeName = GetString(br, n.FilenameOffset + h.StringTableOffset + 0x20);
            string nodeDirectory = Path.Combine(directory, nodeName);

            Directory.CreateDirectory(nodeDirectory);

            for (int i = 0; i < n.NumFileEntries; i++)
            {
                FileEntry curr = GetFileEntry((int)n.FirstFileEntryOffset + i, h, br);

                if (curr.Id == 0xFFFF)
                {
                    // Subdirectory
                    if (curr.FilenameOffset != 0 && curr.FilenameOffset != 2)
                    {
                        DumpNode(GetNode((int)curr.DataOffset, br), h, br, nodeDirectory);
                    }
                }
                else
                {
                    string currName = GetString(
                        br,
                        curr.FilenameOffset + h.StringTableOffset + 0x20
                    );

                    //Console.WriteLine($"{nodeName}/{currName}");

                    string filePath = Path.Combine(nodeDirectory, currName);
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        br.BaseStream.Seek(
                            curr.DataOffset + h.DataStartOffset + 0x20,
                            SeekOrigin.Begin
                        );

                        byte[] data = br.ReadBytes((int)curr.DataSize);
                        fs.Write(data, 0, data.Length);
                    }
                }
            }
        }

        static void ReadFile(BinaryReader br, string directory)
        {
            RarcHeader h = new RarcHeader();

            h.Type = Encoding.ASCII.GetString(br.ReadBytes(4));
            h.Size = ReadUInt32BE(br);
            h.Unknown = ReadUInt32BE(br);
            h.DataStartOffset = ReadUInt32BE(br);

            h.Unknown2 = new uint[4];
            for (int i = 0; i < 4; i++)
                h.Unknown2[i] = ReadUInt32BE(br);

            h.NumNodes = ReadUInt32BE(br);

            h.Unknown3 = new uint[2];
            h.Unknown3[0] = ReadUInt32BE(br);
            h.Unknown3[1] = ReadUInt32BE(br);

            h.FileEntriesOffset = ReadUInt32BE(br);
            h.Unknown4 = ReadUInt32BE(br);
            h.StringTableOffset = ReadUInt32BE(br);

            h.Unknown5 = new uint[2];
            h.Unknown5[0] = ReadUInt32BE(br);
            h.Unknown5[1] = ReadUInt32BE(br);

            Node root = GetNode(0, br);

            DumpNode(root, h, br, directory);
        }

        static ushort ReadUInt16BE(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(2);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt16(bytes, 0);
        }

        static uint ReadUInt32BE(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string DumpArchive(string args)
        {
            if (args.Length < 1 || !File.Exists(args))
            {
                return "";
            }

            using (FileStream fs = new FileStream(args, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                args = args.Replace(".rarc", "").Replace(".arc", "");
                ReadFile(br, args);
            }

            string subfolder = PatchFunctions.AfterLast(args, '/');

            return args + "\\" + subfolder;
        }
    }
}
