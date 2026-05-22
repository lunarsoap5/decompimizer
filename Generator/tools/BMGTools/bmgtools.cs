using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class BmgTools
{
    // --------------------
    // Based off of pikminBMGtool.py, a code for dumping BMG
    // Credit to Yoshi2 and all the amazing work they put in.
    // [https://github.com/RenolY2/pikminBMG]
    // --------------------

    public static string PrettyHex(byte[] data)
    {
        return string.Join(" ", data.Select(b => b.ToString("X2")));
    }

    public static string PrettyHexNoSpace(byte[] data)
    {
        return string.Concat(data.Select(b => b.ToString("X2")));
    }

    public static uint ReadUInt32BE(BinaryReader br)
    {
        byte[] bytes = br.ReadBytes(4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    public static ushort ReadUInt16BE(BinaryReader br)
    {
        byte[] bytes = br.ReadBytes(2);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToUInt16(bytes, 0);
    }

    public static byte ReadUInt8(BinaryReader br)
    {
        return br.ReadByte();
    }

    public static uint ReadUInt24BE(BinaryReader br)
    {
        byte upperVal = ReadUInt8(br);
        ushort lowerVal = ReadUInt16BE(br);

        return (uint)((upperVal << 16) | lowerVal);
    }

    public static void WriteUInt32BE(BinaryWriter bw, uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        bw.Write(bytes);
    }

    public static void WriteUInt16BE(BinaryWriter bw, ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        bw.Write(bytes);
    }

    public static void WriteUInt8(BinaryWriter bw, byte value)
    {
        bw.Write(value);
    }

    public static void WriteUInt24BE(BinaryWriter bw, uint value)
    {
        ushort upper = (ushort)(value >> 8);
        byte lower = (byte)(value & 0xFF);

        byte[] upperBytes = BitConverter.GetBytes(upper);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(upperBytes);

        bw.Write(upperBytes);
        bw.Write(lower);
    }

    public class Message
    {
        public string Attributes { get; set; } = "";
        public List<byte[]> MessageParts { get; set; } = new List<byte[]>();
        public Tuple<uint, byte>? MsgId { get; set; }

        public List<string> AsStringNewline(Encoding encoding)
        {
            string msg = "";

            foreach (var part in MessageParts)
            {
                if (part.Length == 0)
                {
                    continue;
                }
                else if (part[0] == 0x1A)
                {
                    msg += "{" + PrettyHexNoSpace(part) + "}";
                }
                else
                {
                    string decoded = encoding.GetString(part);
                    decoded = decoded.Replace("{", "\\{");
                    decoded = decoded.Replace("}", "\\}");
                    msg += decoded;
                }
            }

            return msg.Split('\n').ToList();
        }
    }

    public class Section
    {
        public byte[] Magic { get; set; }
        public MemoryStream Data { get; set; }

        public Section(byte[] magic)
        {
            Magic = magic;
            Data = new MemoryStream();
        }

        public void WriteSection(BinaryWriter bw, bool pad = true)
        {
            byte[] dataBytes = Data.ToArray();

            bw.Write(Magic);

            long sizePos = bw.BaseStream.Position;

            WriteUInt32BE(bw, 0xFF00FF00);

            bw.Write(dataBytes);

            int padding = 0;

            if (pad)
            {
                long pos = bw.BaseStream.Position;

                if (pos % 32 != 0)
                {
                    padding = (int)(32 - (pos % 32));
                }
                else
                {
                    padding = 0;
                }

                bw.Write(new byte[padding]);
            }

            long now = bw.BaseStream.Position;

            bw.BaseStream.Seek(sizePos, SeekOrigin.Begin);
            WriteUInt32BE(bw, (uint)(8 + dataBytes.Length + padding));

            bw.BaseStream.Seek(now, SeekOrigin.Begin);
        }
    }

    public class JsonMessage
    {
        [JsonPropertyName("ID")]
        public string? ID { get; set; }

        [JsonPropertyName("index")]
        public string? Index { get; set; }

        [JsonPropertyName("attributes")]
        public string? Attributes { get; set; }

        [JsonPropertyName("text")]
        public List<string>? Text { get; set; }

        [JsonPropertyName("Section")]
        public string? Section { get; set; }

        [JsonPropertyName("Data")]
        public string? Data { get; set; }

        [JsonPropertyName("Attribute_Length")]
        public int? AttributeLength { get; set; }

        [JsonPropertyName("Unknown_MID1_Value")]
        public string? UnknownMID1Value { get; set; }
    }

    public static Encoding GetEncodingFromBMG(uint encodingValue)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (encodingValue == 0x03000000)
        {
            Console.WriteLine($"Got encoding value {encodingValue:x}, assuming Shift-JIS encoding");
            return Encoding.GetEncoding("shift-jis");
        }
        else
        {
            Console.WriteLine($"Got encoding value {encodingValue:x}, assuming latin-1 encoding");
            return Encoding.GetEncoding("iso-8859-1");
        }
    }

    public static string DumpBmgToJsonTxt(string input, string output)
    {
        using FileStream inputBMG = new FileStream(input, FileMode.Open, FileAccess.Read);

        using BinaryReader br = new BinaryReader(inputBMG);

        byte[] magic = br.ReadBytes(8);

        if (Encoding.ASCII.GetString(magic) != "MESGbmg1")
        {
            throw new RuntimeException(
                $"Input file not a BMG file. Encountered magic {Encoding.ASCII.GetString(magic)}"
            );
        }

        uint fileSize = ReadUInt32BE(br);
        uint sectionCount = ReadUInt32BE(br);
        uint encodingVal = ReadUInt32BE(br);

        Encoding encoding = GetEncodingFromBMG(encodingVal);

        byte[] padding = br.ReadBytes(0x0C);

        Console.WriteLine(Encoding.ASCII.GetString(magic));
        Console.WriteLine("filesize: " + fileSize.ToString("X"));
        Console.WriteLine("sections: " + sectionCount);

        List<(long Start, byte[] Magic, uint Size, byte[] Data)> sections =
            new List<(long, byte[], uint, byte[])>();

        for (int i = 0; i < sectionCount; i++)
        {
            long sectionStart = br.BaseStream.Position;

            byte[] sectionMagic = br.ReadBytes(4);
            uint sectionSize = ReadUInt32BE(br);

            Console.WriteLine(
                $"found section {Encoding.ASCII.GetString(sectionMagic)} with size 0x{sectionSize:X}"
            );

            byte[] data = br.ReadBytes((int)sectionSize - 8);

            sections.Add((sectionStart, sectionMagic, sectionSize, data));
        }

        Console.WriteLine("reached end of file");
        Console.WriteLine("0x" + br.BaseStream.Position.ToString("X"));

        var infSection = sections[0];

        if (Encoding.ASCII.GetString(infSection.Magic) != "INF1")
        {
            throw new Exception("Expected INF1 section");
        }

        br.BaseStream.Seek(infSection.Start + 8, SeekOrigin.Begin);

        ushort messageCount = ReadUInt16BE(br);
        ushort itemLength = ReadUInt16BE(br);

        br.ReadBytes(4);

        List<(uint Offset, byte[] Attributes)> infItems = new List<(uint, byte[])>();

        for (int i = 0; i < messageCount; i++)
        {
            uint dat1Offset = ReadUInt32BE(br);
            byte[] attributes = br.ReadBytes(itemLength - 4);

            //Console.WriteLine($"0x{i:X} {PrettyHex(attributes)}");

            infItems.Add((dat1Offset, attributes));
        }

        //Console.WriteLine($"{messageCount} entries in inf1 read");
        //Console.WriteLine(
        //    $"0x{br.BaseStream.Position:X} 0x{(infSection.Start + infSection.Size):X}"
        //);

        List<Message> messages = new List<Message>();

        var datSection = sections[1];
        var midSection = sections[2];

        if (Encoding.ASCII.GetString(datSection.Magic) != "DAT1")
            throw new Exception("Expected DAT1 section");

        if (Encoding.ASCII.GetString(midSection.Magic) != "MID1")
            throw new Exception("Expected MID1 section");

        List<(long Start, byte[] Magic, uint Size, byte[] Data)> additionalSections =
            new List<(long, byte[], uint, byte[])>();

        if (sections.Count > 3)
        {
            for (int i = 3; i < sections.Count; i++)
            {
                additionalSections.Add(sections[i]);
            }
        }

        int msgIndex = 0;

        foreach (var item in infItems)
        {
            br.BaseStream.Seek(datSection.Start + item.Offset + 8, SeekOrigin.Begin);

            byte currentChar = br.ReadByte();

            List<byte[]> text = new List<byte[]>();
            List<byte> outText = new List<byte>();

            while (currentChar != 0x00)
            {
                if (currentChar == 0x1A)
                {
                    text.Add(outText.ToArray());

                    byte argLen = br.ReadByte();

                    int argLenVal = argLen;

                    List<byte> escapeSequence = new List<byte>();
                    escapeSequence.Add(currentChar);
                    escapeSequence.Add(argLen);

                    escapeSequence.AddRange(br.ReadBytes(argLenVal - 2));

                    text.Add(escapeSequence.ToArray());

                    outText.Clear();
                }
                else
                {
                    outText.Add(currentChar);
                }

                currentChar = br.ReadByte();
            }

            text.Add(outText.ToArray());

            Message msgObj = new Message();

            msgObj.Attributes = PrettyHexNoSpace(item.Attributes);
            msgObj.MessageParts = text;

            br.BaseStream.Seek(midSection.Start + 0x10 + msgIndex * 4, SeekOrigin.Begin);

            uint msgId = ReadUInt24BE(br);
            byte subId = ReadUInt8(br);

            msgObj.MsgId = Tuple.Create(msgId, subId);

            messages.Add(msgObj);

            msgIndex++;
        }

        br.BaseStream.Seek(midSection.Start + 0xA, SeekOrigin.Begin);

        ushort unknownMidValue = ReadUInt16BE(br);

        List<object> messagesJson = new List<object>();

        messagesJson.Add(
            new Dictionary<string, object>
            {
                { "Attribute_Length", itemLength },
                { "Unknown_MID1_Value", unknownMidValue.ToString("x") }
            }
        );

        for (int i = 0; i < messages.Count; i++)
        {
            Message msg = messages[i];

            messagesJson.Add(
                new Dictionary<string, object>
                {
                    { "ID", $"{msg.MsgId!.Item1}, {msg.MsgId.Item2}" },
                    { "index", $"0x{i:X}" },
                    { "attributes", msg.Attributes },
                    { "text", msg.AsStringNewline(encoding) }
                }
            );
        }

        foreach (var section in additionalSections)
        {
            messagesJson.Add(
                new Dictionary<string, object>
                {
                    { "Section", Encoding.ASCII.GetString(section.Magic) },
                    { "Data", PrettyHexNoSpace(section.Data) }
                }
            );
        }

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        string json = JsonSerializer.Serialize(messagesJson, options);
        if (output != "")
        {
            using FileStream outputJson = new FileStream(output, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new StreamWriter(outputJson, new UTF8Encoding(false));
            sw.Write(json);
            return "";
        }
        else
        {
            return json;
        }
    }

    public static void PackJsonToBmg(
        Stream inputJsonFile,
        Stream outputBmg,
        string encodingName = "shift-jis"
    )
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string jsonText;

        using (StreamReader sr = new StreamReader(inputJsonFile))
        {
            jsonText = sr.ReadToEnd();
        }

        JsonDocument doc = JsonDocument.Parse(jsonText);

        List<JsonElement> messages = doc.RootElement.EnumerateArray().ToList();

        Section infSection = new Section(Encoding.ASCII.GetBytes("INF1"));
        Section datSection = new Section(Encoding.ASCII.GetBytes("DAT1"));
        Section midSection = new Section(Encoding.ASCII.GetBytes("MID1"));

        List<Section> additionalSections = new List<Section>();

        ushort unkMid1Val = 0x1001;
        ushort attrLength = 8;

        if (messages.Count > 0)
        {
            JsonElement first = messages[0];

            if (first.TryGetProperty("Attribute_Length", out JsonElement attrLenElem))
            {
                attrLength = (ushort)attrLenElem.GetInt32();

                if (first.TryGetProperty("Unknown_MID1_Value", out JsonElement unknownElem))
                {
                    unkMid1Val = ushort.Parse(unknownElem.GetString()!, NumberStyles.HexNumber);
                }

                messages.RemoveAt(0);
            }
        }

        List<JsonElement> tempMessages = new List<JsonElement>();

        foreach (JsonElement message in messages)
        {
            if (!message.TryGetProperty("Section", out JsonElement sectionElement))
            {
                tempMessages.Add(message);
            }
            else
            {
                string sectionName = sectionElement.GetString()!;
                string dataHex = message.GetProperty("Data").GetString()!;

                Section section = new Section(Encoding.ASCII.GetBytes(sectionName));

                byte[] data = Enumerable
                    .Range(0, dataHex.Length / 2)
                    .Select(i => Convert.ToByte(dataHex.Substring(i * 2, 2), 16))
                    .ToArray();

                section.Data.Write(data, 0, data.Length);

                additionalSections.Add(section);
            }
        }

        messages = tempMessages;

        using BinaryWriter infWriter = new BinaryWriter(infSection.Data, Encoding.UTF8, true);
        using BinaryWriter datWriter = new BinaryWriter(datSection.Data, Encoding.UTF8, true);
        using BinaryWriter midWriter = new BinaryWriter(midSection.Data, Encoding.UTF8, true);

        WriteUInt16BE(infWriter, (ushort)messages.Count);
        WriteUInt16BE(infWriter, attrLength);
        WriteUInt32BE(infWriter, 0x00000000);

        datWriter.Write((byte)0x00);

        WriteUInt16BE(midWriter, (ushort)messages.Count);
        WriteUInt16BE(midWriter, unkMid1Val);
        WriteUInt32BE(midWriter, 0x00000000);

        int written = 1;

        Encoding encoding;

        if (encodingName == "shift-jis")
        {
            encoding = Encoding.GetEncoding("shift-jis");
        }
        else if (encodingName == "iso-8859-1")
        {
            encoding = Encoding.GetEncoding("iso-8859-1");
        }
        else
        {
            throw new Exception($"unknown encoding: {encodingName}");
        }

        foreach (JsonElement msg in messages)
        {
            string attributesHex = msg.GetProperty("attributes").GetString()!;

            byte[] attributes = Enumerable
                .Range(0, attributesHex.Length / 2)
                .Select(i => Convert.ToByte(attributesHex.Substring(i * 2, 2), 16))
                .ToArray();

            JsonElement.ArrayEnumerator textEnumerator = msg.GetProperty("text").EnumerateArray();
            List<string> textList = textEnumerator.Select(x => x.GetString()!).ToList();

            int offset = written;

            WriteUInt32BE(infWriter, (uint)offset);
            infWriter.Write(attributes);

            if (offset > 0)
            {
                string text = string.Join("\n", textList);

                long start = datSection.Data.Position;

                int i = 0;

                while (i < text.Length)
                {
                    char letter = text[i];

                    if (letter == '\\')
                    {
                        if (i + 1 < text.Length)
                        {
                            char next = text[i + 1];

                            if (next == '{')
                            {
                                datWriter.Write((byte)'{');
                                i += 2;
                                continue;
                            }
                            else if (next == '}')
                            {
                                datWriter.Write((byte)'}');
                                i += 2;
                                continue;
                            }
                        }

                        datWriter.Write((byte)'\\');
                        i++;
                    }
                    else if (letter == '{')
                    {
                        int endIndex = text.IndexOf('}', i);

                        if (endIndex == -1)
                        {
                            throw new Exception(
                                $"Hit end of string while reading command sequence in message ID {msg.GetProperty("ID").GetString()}"
                            );
                        }

                        string hexData = text.Substring(i + 1, endIndex - i - 1);

                        for (int h = 0; h < hexData.Length; h += 2)
                        {
                            datWriter.Write(Convert.ToByte(hexData.Substring(h, 2), 16));
                        }

                        i = endIndex + 1;
                    }
                    else
                    {
                        string charString = letter.ToString();

                        byte[] encodedLetter;

                        try
                        {
                            encodedLetter = encoding.GetBytes(charString);
                        }
                        catch
                        {
                            encodedLetter = encoding.GetBytes("?");

                            Console.WriteLine(
                                $"Warning for Message ID {msg.GetProperty("ID").GetString()}: Unsupported character '{letter}' replaced with '?'"
                            );
                        }

                        datWriter.Write(encodedLetter);
                        i++;
                    }
                }

                datWriter.Write((byte)0x00);

                written += (int)(datSection.Data.Position - start);
            }

            string[] idParts = msg.GetProperty("ID").GetString()!.Split(',');

            uint id = uint.Parse(idParts[0].Trim());
            byte num = byte.Parse(idParts[1].Trim());

            WriteUInt24BE(midWriter, id);
            WriteUInt8(midWriter, num);
        }

        using BinaryWriter bw = new BinaryWriter(outputBmg);

        bw.Write(Encoding.ASCII.GetBytes("MESGbmg1"));

        WriteUInt32BE(bw, 0xFF00FF00);
        WriteUInt32BE(bw, (uint)(3 + additionalSections.Count));

        if (encodingName == "shift-jis")
        {
            WriteUInt32BE(bw, 0x03000000);
        }
        else if (encodingName == "iso-8859-1")
        {
            WriteUInt32BE(bw, 0x01000000);
        }
        else
        {
            throw new Exception($"unknown encoding: {encodingName}");
        }

        bw.Write(new byte[12]);

        infSection.WriteSection(bw);
        datSection.WriteSection(bw);
        midSection.WriteSection(bw);

        long end = bw.BaseStream.Position;

        foreach (Section section in additionalSections)
        {
            section.WriteSection(bw);
        }

        bw.BaseStream.Seek(0x08, SeekOrigin.Begin);

        WriteUInt32BE(bw, (uint)end);
    }

    public static void PackJsonToBmg(
        string jsonText,
        Stream outputBmg,
        string encodingName = "shift-jis"
    )
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        JsonDocument doc = JsonDocument.Parse(jsonText);

        List<JsonElement> messages = doc.RootElement.EnumerateArray().ToList();

        Section infSection = new Section(Encoding.ASCII.GetBytes("INF1"));
        Section datSection = new Section(Encoding.ASCII.GetBytes("DAT1"));
        Section midSection = new Section(Encoding.ASCII.GetBytes("MID1"));

        List<Section> additionalSections = new List<Section>();

        ushort unkMid1Val = 0x1001;
        ushort attrLength = 8;

        if (messages.Count > 0)
        {
            JsonElement first = messages[0];

            if (first.TryGetProperty("Attribute_Length", out JsonElement attrLenElem))
            {
                attrLength = (ushort)attrLenElem.GetInt32();

                if (first.TryGetProperty("Unknown_MID1_Value", out JsonElement unknownElem))
                {
                    unkMid1Val = ushort.Parse(unknownElem.GetString()!, NumberStyles.HexNumber);
                }

                messages.RemoveAt(0);
            }
        }

        List<JsonElement> tempMessages = new List<JsonElement>();

        foreach (JsonElement message in messages)
        {
            if (!message.TryGetProperty("Section", out JsonElement sectionElement))
            {
                tempMessages.Add(message);
            }
            else
            {
                string sectionName = sectionElement.GetString()!;
                string dataHex = message.GetProperty("Data").GetString()!;

                Section section = new Section(Encoding.ASCII.GetBytes(sectionName));

                byte[] data = Enumerable
                    .Range(0, dataHex.Length / 2)
                    .Select(i => Convert.ToByte(dataHex.Substring(i * 2, 2), 16))
                    .ToArray();

                section.Data.Write(data, 0, data.Length);

                additionalSections.Add(section);
            }
        }

        messages = tempMessages;

        using BinaryWriter infWriter = new BinaryWriter(infSection.Data, Encoding.UTF8, true);
        using BinaryWriter datWriter = new BinaryWriter(datSection.Data, Encoding.UTF8, true);
        using BinaryWriter midWriter = new BinaryWriter(midSection.Data, Encoding.UTF8, true);

        WriteUInt16BE(infWriter, (ushort)messages.Count);
        WriteUInt16BE(infWriter, attrLength);
        WriteUInt32BE(infWriter, 0x00000000);

        datWriter.Write((byte)0x00);

        WriteUInt16BE(midWriter, (ushort)messages.Count);
        WriteUInt16BE(midWriter, unkMid1Val);
        WriteUInt32BE(midWriter, 0x00000000);

        int written = 1;

        Encoding encoding;

        if (encodingName == "shift-jis")
        {
            encoding = Encoding.GetEncoding("shift-jis");
        }
        else if (encodingName == "iso-8859-1")
        {
            encoding = Encoding.GetEncoding("iso-8859-1");
        }
        else
        {
            throw new Exception($"unknown encoding: {encodingName}");
        }

        foreach (JsonElement msg in messages)
        {
            string attributesHex = msg.GetProperty("attributes").GetString()!;

            byte[] attributes = Enumerable
                .Range(0, attributesHex.Length / 2)
                .Select(i => Convert.ToByte(attributesHex.Substring(i * 2, 2), 16))
                .ToArray();

            JsonElement.ArrayEnumerator textEnumerator = msg.GetProperty("text").EnumerateArray();
            List<string> textList = textEnumerator.Select(x => x.GetString()!).ToList();

            int offset = written;

            WriteUInt32BE(infWriter, (uint)offset);
            infWriter.Write(attributes);

            if (offset > 0)
            {
                string text = string.Join("\n", textList);

                long start = datSection.Data.Position;

                int i = 0;

                while (i < text.Length)
                {
                    char letter = text[i];

                    if (letter == '\\')
                    {
                        if (i + 1 < text.Length)
                        {
                            char next = text[i + 1];

                            if (next == '{')
                            {
                                datWriter.Write((byte)'{');
                                i += 2;
                                continue;
                            }
                            else if (next == '}')
                            {
                                datWriter.Write((byte)'}');
                                i += 2;
                                continue;
                            }
                        }

                        datWriter.Write((byte)'\\');
                        i++;
                    }
                    else if (letter == '{')
                    {
                        int endIndex = text.IndexOf('}', i);

                        if (endIndex == -1)
                        {
                            throw new Exception(
                                $"Hit end of string while reading command sequence in message ID {msg.GetProperty("ID").GetString()}"
                            );
                        }

                        string hexData = text.Substring(i + 1, endIndex - i - 1);

                        for (int h = 0; h < hexData.Length; h += 2)
                        {
                            datWriter.Write(Convert.ToByte(hexData.Substring(h, 2), 16));
                        }

                        i = endIndex + 1;
                    }
                    else
                    {
                        string charString = letter.ToString();

                        byte[] encodedLetter;

                        try
                        {
                            encodedLetter = encoding.GetBytes(charString);
                        }
                        catch
                        {
                            encodedLetter = encoding.GetBytes("?");

                            Console.WriteLine(
                                $"Warning for Message ID {msg.GetProperty("ID").GetString()}: Unsupported character '{letter}' replaced with '?'"
                            );
                        }

                        datWriter.Write(encodedLetter);
                        i++;
                    }
                }

                datWriter.Write((byte)0x00);

                written += (int)(datSection.Data.Position - start);
            }

            string[] idParts = msg.GetProperty("ID").GetString()!.Split(',');

            uint id = uint.Parse(idParts[0].Trim());
            byte num = byte.Parse(idParts[1].Trim());

            WriteUInt24BE(midWriter, id);
            WriteUInt8(midWriter, num);
        }

        using BinaryWriter bw = new BinaryWriter(outputBmg);

        bw.Write(Encoding.ASCII.GetBytes("MESGbmg1"));

        WriteUInt32BE(bw, 0xFF00FF00);
        WriteUInt32BE(bw, (uint)(3 + additionalSections.Count));

        if (encodingName == "shift-jis")
        {
            WriteUInt32BE(bw, 0x03000000);
        }
        else if (encodingName == "iso-8859-1")
        {
            WriteUInt32BE(bw, 0x01000000);
        }
        else
        {
            throw new Exception($"unknown encoding: {encodingName}");
        }

        bw.Write(new byte[12]);

        infSection.WriteSection(bw);
        datSection.WriteSection(bw);
        midSection.WriteSection(bw);

        long end = bw.BaseStream.Position;

        foreach (Section section in additionalSections)
        {
            section.WriteSection(bw);
        }

        bw.BaseStream.Seek(0x08, SeekOrigin.Begin);

        WriteUInt32BE(bw, (uint)end);
    }

    public static void DumpBmg(string input, string output)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (output == null)
        {
            output = input + ".json";
        }

        //Console.WriteLine("input: " + input);
        Console.WriteLine("output: " + output);

        DumpBmgToJsonTxt(input, output);

        Console.WriteLine("json-formatted txt file created");
    }

    public static string DumpBmg(string input)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //Console.WriteLine("input: " + input);

        return DumpBmgToJsonTxt(input, "");
    }

    public static void PackBmg(string input, string output, string encoding, bool useFileStream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (output == null)
        {
            output = input + ".bmg";
        }

        //Console.WriteLine("input: " + input);
        Console.WriteLine("output: " + output);
        Console.WriteLine("encoding: " + encoding);
        if (useFileStream)
        {
            using FileStream inputFile = new FileStream(input, FileMode.Open, FileAccess.Read);

            byte[] bom = new byte[4];
            inputFile.Read(bom, 0, 4);

            string detectedEncoding;

            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
            {
                detectedEncoding = "utf-8";
            }
            else if (
                (bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
                || (bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE && bom[3] == 0xFF)
            )
            {
                detectedEncoding = "utf-32";
            }
            else if ((bom[0] == 0xFF && bom[1] == 0xFE) || (bom[0] == 0xFE && bom[1] == 0xFF))
            {
                detectedEncoding = "utf-16";
            }
            else
            {
                detectedEncoding = "utf-8";
            }

            Console.WriteLine("Assuming encoding of input file: " + detectedEncoding);

            inputFile.Seek(0, SeekOrigin.Begin);

            using FileStream bmgFile = new FileStream(output, FileMode.Create, FileAccess.Write);

            PackJsonToBmg(inputFile, bmgFile, encoding);
        }
        else
        {
            using FileStream bmgFile = new FileStream(output, FileMode.Create, FileAccess.Write);

            PackJsonToBmg(input, bmgFile, encoding);
        }

        Console.WriteLine("bmg file created");
    }
}

public class RuntimeException : Exception
{
    public RuntimeException(string message) : base(message) { }
}
