using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

// --------------------
// Based off of tools/libstage/libstage.py, by jdflyer
// --------------------

internal class LibStage
{
    public class FloatConverter : JsonConverter<float>
    {
        public override float Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            return reader.GetSingle();
        }

        public override void Write(
            Utf8JsonWriter writer,
            float value,
            JsonSerializerOptions options
        )
        {
            writer.WriteRawValue(value.ToString("G9"));
        }
    }

    // ----------------------------------------------------------
    // Big-endian helpers (Python's struct ">" prefix)
    // ----------------------------------------------------------
    static byte ReadU8(byte[] d, int o) => d[o];

    static sbyte ReadS8(byte[] d, int o) => (sbyte)d[o];

    static ushort ReadU16(byte[] d, int o) => (ushort)((d[o] << 8) | d[o + 1]);

    static short ReadS16(byte[] d, int o) => (short)((d[o] << 8) | d[o + 1]);

    static uint ReadU32(byte[] d, int o) =>
        (uint)((d[o] << 24) | (d[o + 1] << 16) | (d[o + 2] << 8) | d[o + 3]);

    static int ReadS32(byte[] d, int o) => (int)ReadU32(d, o);

    static float ReadF32(byte[] d, int o)
    {
        byte[] b = { d[o + 3], d[o + 2], d[o + 1], d[o] };
        return BitConverter.ToSingle(b, 0);
    }

    static string ReadAscii(byte[] d, int o, int len)
    {
        var s = Encoding.ASCII.GetString(d, o, len);
        return s.TrimEnd('\0');
    }

    static void WriteU8(List<byte> buf, byte v) => buf.Add(v);

    static void WriteS8(List<byte> buf, sbyte v) => buf.Add((byte)v);

    static void WriteU16(List<byte> buf, ushort v)
    {
        buf.Add((byte)(v >> 8));
        buf.Add((byte)v);
    }

    static void WriteS16(List<byte> buf, short v) => WriteU16(buf, (ushort)v);

    static void WriteU32(List<byte> buf, uint v)
    {
        buf.Add((byte)(v >> 24));
        buf.Add((byte)(v >> 16));
        buf.Add((byte)(v >> 8));
        buf.Add((byte)v);
    }

    static void WriteS32(List<byte> buf, int v) => WriteU32(buf, (uint)v);

    static void WriteF32(List<byte> buf, float v)
    {
        byte[] b = BitConverter.GetBytes(v);
        buf.Add(b[3]);
        buf.Add(b[2]);
        buf.Add(b[1]);
        buf.Add(b[0]);
    }

    static void WriteAscii(List<byte> buf, string s, int len)
    {
        byte[] b = new byte[len];
        var encoded = Encoding.ASCII.GetBytes(s);
        Array.Copy(encoded, b, Math.Min(encoded.Length, len));
        buf.AddRange(b);
    }

    // ----------------------------------------------------------
    // RGB helper
    // ----------------------------------------------------------
    static JsonArray ReadRGB(byte[] d, int o) =>
        new JsonArray(
            JsonValue.Create(d[o]),
            JsonValue.Create(d[o + 1]),
            JsonValue.Create(d[o + 2])
        );

    static void WriteRGB(List<byte> buf, JsonArray arr)
    {
        buf.Add((byte)arr[0]!.GetValue<int>());
        buf.Add((byte)arr[1]!.GetValue<int>());
        buf.Add((byte)arr[2]!.GetValue<int>());
    }

    // ----------------------------------------------------------
    // EXTRACT functions
    // ----------------------------------------------------------

    // EVLY – layer table (15 bytes each)
    static JsonArray ExtractEVLY(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            var tbl = new JsonArray();
            for (int j = 0; j < 15; j++)
                tbl.Add(d[offset + j]);
            entries.Add(new JsonObject { ["layerTable"] = tbl });
            offset += 15;
        }
        return entries;
    }

    // RPPN / PPNT – path points
    static JsonArray ExtractRPPN(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["field_0x0"] = d[offset],
                    ["field_0x1"] = d[offset + 1],
                    ["field_0x2"] = d[offset + 2],
                    ["field_0x3"] = d[offset + 3],
                    ["Position_X"] = ReadF32(d, offset + 4),
                    ["Position_Y"] = ReadF32(d, offset + 8),
                    ["Position_Z"] = ReadF32(d, offset + 12),
                }
            );
            offset += 0x10;
        }
        return entries;
    }

    // RPAT – path headers
    static JsonArray ExtractRPAT(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            ushort numPoints = ReadU16(d, offset);
            short pathIndex = ReadS16(d, offset + 2);
            byte f4 = d[offset + 4];
            byte isLoop = d[offset + 5];
            ushort f6 = ReadU16(d, offset + 6);
            uint firstOffset = ReadU32(d, offset + 8);
            entries.Add(
                new JsonObject
                {
                    ["Number_of_Points"] = numPoints,
                    ["Path_Index"] = pathIndex,
                    ["field_0x4"] = f4,
                    ["Looped"] = isLoop,
                    ["field_0x6"] = f6,
                    ["RPPN_Entry_Index"] = (int)(firstOffset / 0x10),
                }
            );
            offset += 12;
        }
        return entries;
    }

    // MULT
    static JsonArray ExtractMULT(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["x"] = ReadF32(d, offset),
                    ["y"] = ReadF32(d, offset + 4),
                    ["Angle"] = ReadS16(d, offset + 8),
                    ["roomNo"] = d[offset + 10],
                    ["field_0xb"] = d[offset + 11],
                }
            );
            offset += 12;
        }
        return entries;
    }

    // ACTR / PLYR / TGOB
    static JsonArray ExtractACTR(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["Name"] = ReadAscii(d, offset, 8),
                    ["param"] = (long)ReadU32(d, offset + 8),
                    ["x"] = ReadF32(d, offset + 12),
                    ["y"] = ReadF32(d, offset + 16),
                    ["z"] = ReadF32(d, offset + 20),
                    ["Angle_X"] = ReadS16(d, offset + 24),
                    ["Angle_Y"] = ReadS16(d, offset + 26),
                    ["Angle_Z"] = ReadS16(d, offset + 28),
                    ["EnemyNo"] = ReadS16(d, offset + 30),
                }
            );
            offset += 0x20;
        }
        return entries;
    }

    // CAMR / RCAM
    static JsonArray ExtractCAM(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["Camera_Type"] = ReadAscii(d, offset, 16),
                    ["field_0x10"] = d[offset + 16],
                    ["field_0x11"] = d[offset + 17],
                    ["field_0x12"] = d[offset + 18],
                    ["field_0x13"] = d[offset + 19],
                    ["field_0x14"] = ReadU16(d, offset + 20),
                    ["field_0x16"] = ReadU16(d, offset + 22),
                }
            );
            offset += 0x18;
        }
        return entries;
    }

    // RTBL
    static JsonArray ExtractRTBL(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            int roomDataOffset = (int)ReadU32(d, offset);
            offset += 4;
            int infoLen = d[roomDataOffset];
            byte f1 = d[roomDataOffset + 1];
            byte f2 = d[roomDataOffset + 2];
            // byte pad = d[roomDataOffset + 3];
            int infoTblOffset = (int)ReadU32(d, roomDataOffset + 4);
            var tbl = new JsonArray();
            for (int j = 0; j < infoLen; j++)
                tbl.Add(d[infoTblOffset + j]);
            entries.Add(
                new JsonObject
                {
                    ["field_0x1"] = f1,
                    ["field_0x2"] = f2,
                    ["Table"] = tbl
                }
            );
        }
        return entries;
    }

    // RARO / AROB
    static JsonArray ExtractRARO(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["x"] = ReadF32(d, offset),
                    ["y"] = ReadF32(d, offset + 4),
                    ["z"] = ReadF32(d, offset + 8),
                    ["Angle_X"] = ReadS16(d, offset + 12),
                    ["Angle_Y"] = ReadS16(d, offset + 14),
                    ["Angle_Z"] = ReadS16(d, offset + 16),
                    ["field_0x12"] = d[offset + 18],
                    ["field_0x13"] = d[offset + 19],
                }
            );
            offset += 0x14;
        }
        return entries;
    }

    // SCLS
    static JsonArray ExtractSCLS(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["Stage"] = ReadAscii(d, offset, 8),
                    ["Start"] = d[offset + 8],
                    ["Room"] = (sbyte)d[offset + 9],
                    ["field_0xa"] = d[offset + 10],
                    ["field_0xb"] = d[offset + 11],
                    ["Wipe"] = (sbyte)d[offset + 12],
                }
            );
            offset += 0xD;
        }
        return entries;
    }

    // TGSC / SCOB / TGDR / Door / Doo0 / SCO0
    static JsonArray ExtractTGSC(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["Name"] = ReadAscii(d, offset, 8),
                    ["param"] = (long)ReadU32(d, offset + 8),
                    ["x"] = ReadF32(d, offset + 12),
                    ["y"] = ReadF32(d, offset + 16),
                    ["z"] = ReadF32(d, offset + 20),
                    ["Angle_X"] = ReadS16(d, offset + 24),
                    ["Angle_Y"] = ReadS16(d, offset + 26),
                    ["Angle_Z"] = ReadS16(d, offset + 28),
                    ["EnemyNo"] = ReadS16(d, offset + 30),
                    ["Scale_X"] = d[offset + 32],
                    ["Scale_Y"] = d[offset + 33],
                    ["Scale_Z"] = d[offset + 34],
                    ["field_0x23"] = d[offset + 35],
                }
            );
            offset += 0x24;
        }
        return entries;
    }

    // FILI (stage file)
    static JsonArray ExtractFILI(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            var tbl = new JsonArray();
            for (int j = 0; j < 10; j++)
                tbl.Add(d[offset + 0x10 + j]);
            entries.Add(
                new JsonObject
                {
                    ["Parameters"] = (long)ReadU32(d, offset),
                    ["Sea_Level"] = ReadF32(d, offset + 4),
                    ["field_0x8"] = ReadF32(d, offset + 8),
                    ["field_0xc"] = ReadF32(d, offset + 12),
                    ["field_0x10"] = tbl,
                    ["Default_Camera"] = d[offset + 0x1A],
                    ["Bit_Sw"] = d[offset + 0x1B],
                    ["Msg"] = ReadU16(d, offset + 0x1C),
                }
            );
            offset += 0x20;
        }
        return entries;
    }

    // FILI (room stage variant)
    static JsonArray ExtractFILI2(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["Left_Room_X"] = ReadF32(d, offset),
                    ["Inner_Room_Z"] = ReadF32(d, offset + 4),
                    ["Right_Room_X"] = ReadF32(d, offset + 8),
                    ["Front_Room_Z"] = ReadF32(d, offset + 12),
                    ["Min_Floor_No"] = d[offset + 16],
                    ["Max_Floor_No"] = d[offset + 17],
                    ["field_0x12"] = d[offset + 18],
                    ["field_0x13"] = d[offset + 19],
                    ["field_0x14"] = ReadF32(d, offset + 20),
                    ["field_0x18"] = ReadF32(d, offset + 24),
                    ["field_0x1c"] = ReadS16(d, offset + 28),
                }
            );
            offset += 0x20;
        }
        return entries;
    }

    // REVT
    static JsonArray ExtractREVT(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            JsonNode nameNode;
            try
            {
                nameNode = JsonValue.Create(ReadAscii(d, offset + 0xD, 0xB))!;
            }
            catch
            {
                var arr = new JsonArray();
                for (int j = 0; j < 0xB; j++)
                    arr.Add(d[offset + 0xD + j]);
                nameNode = arr;
            }
            entries.Add(
                new JsonObject
                {
                    ["Type"] = d[offset],
                    ["field_0x1"] = d[offset + 1],
                    ["field_0x2"] = d[offset + 2],
                    ["field_0x3"] = d[offset + 3],
                    ["field_0x4"] = d[offset + 4],
                    ["field_0x5"] = d[offset + 5],
                    ["Priority"] = d[offset + 6],
                    ["field_0x7"] = d[offset + 7],
                    ["field_0x8"] = d[offset + 8],
                    ["field_0x9"] = d[offset + 9],
                    ["field_0xa"] = d[offset + 10],
                    ["field_0xb"] = d[offset + 11],
                    ["field_0xc"] = d[offset + 12],
                    ["Name"] = nameNode,
                    ["seType"] = d[offset + 0x18],
                    ["field_0x1a"] = d[offset + 0x19],
                    ["field_0x1b"] = d[offset + 0x1A],
                    ["switch"] = d[offset + 0x1B],
                }
            );
            offset += 0x1C;
        }
        return entries;
    }

    // SOND / SON0
    static JsonArray ExtractSOND(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["Name"] = ReadAscii(d, offset, 8),
                    ["x"] = ReadF32(d, offset + 8),
                    ["y"] = ReadF32(d, offset + 12),
                    ["z"] = ReadF32(d, offset + 16),
                    ["field_0x14"] = d[offset + 20],
                    ["field_0x15"] = d[offset + 21],
                    ["field_0x16"] = d[offset + 22],
                    ["field_0x17"] = d[offset + 23],
                    ["field_0x18"] = d[offset + 24],
                    ["field_0x19"] = d[offset + 25],
                    ["field_0x1a"] = d[offset + 26],
                    ["field_0x1b"] = d[offset + 27],
                }
            );
            offset += 0x1C;
        }
        return entries;
    }

    // LGTV / LGT0
    static JsonArray ExtractLGTV(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["x"] = ReadF32(d, offset),
                    ["y"] = ReadF32(d, offset + 4),
                    ["z"] = ReadF32(d, offset + 8),
                    ["Radius"] = ReadF32(d, offset + 12),
                    ["Direction_X"] = ReadF32(d, offset + 16),
                    ["Direction_Y"] = ReadF32(d, offset + 20),
                    ["Spotlight_Cutoff"] = ReadF32(d, offset + 24),
                    ["field_0x1c"] = d[offset + 28],
                    ["field_0x1d"] = d[offset + 29],
                    ["field_0x1e"] = d[offset + 30],
                    ["field_0x1f"] = d[offset + 31],
                }
            );
            offset += 0x20;
        }
        return entries;
    }

    // ENVR / Env0
    static JsonArray ExtractENVR(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            var tbl = new JsonArray();
            for (int j = 0; j < 65; j++)
                tbl.Add(d[offset + j]);
            entries.Add(new JsonObject { ["Pselect_ID_Table"] = tbl });
            offset += 0x41;
        }
        return entries;
    }

    // Col0
    static JsonArray ExtractCol(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            var palIds = new JsonArray();
            for (int j = 0; j < 8; j++)
                palIds.Add(d[offset + j]);
            entries.Add(
                new JsonObject
                {
                    ["Palette_Ids"] = palIds,
                    ["Change_Rate"] = ReadF32(d, offset + 8),
                }
            );
            offset += 0xC;
        }
        return entries;
    }

    // PAL0
    static JsonArray ExtractPAL(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            var actorAmb = ReadRGB(d, offset);
            offset += 3;
            var bgAmb = new JsonArray();
            for (int j = 0; j < 4; j++)
            {
                bgAmb.Add(ReadRGB(d, offset));
                offset += 3;
            }
            var plight = new JsonArray();
            for (int j = 0; j < 6; j++)
            {
                plight.Add(ReadRGB(d, offset));
                offset += 3;
            }
            var fogColor = ReadRGB(d, offset);
            offset += 3;
            entries.Add(
                new JsonObject
                {
                    ["Actor_Ambient_Color"] = actorAmb,
                    ["BG_Ambient_Colors"] = bgAmb,
                    ["P_Light_Colors"] = plight,
                    ["Fog_Color"] = fogColor,
                    ["Fog_Start_Z"] = ReadF32(d, offset),
                    ["Fog_End_Z"] = ReadF32(d, offset + 4),
                    ["Virt_Idx"] = d[offset + 8],
                    ["Terrain_Light_Influence"] = d[offset + 9],
                    ["Cloud_Shadow_Density"] = d[offset + 10],
                    ["field_0x2f"] = d[offset + 11],
                    ["Bloom_Table_Idx"] = d[offset + 12],
                    ["BG_Ambient_Color_1a"] = d[offset + 13],
                    ["BG_Ambient_Color_2a"] = d[offset + 14],
                    ["BG_Ambient_Color_3a"] = d[offset + 15],
                }
            );
            offset += 16;
        }
        return entries;
    }

    // VRB0
    static JsonArray ExtractVRB(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            var others = new JsonArray();
            for (int j = 0; j < 5; j++)
                others.Add(ReadRGB(d, offset + 6 + j * 3));
            entries.Add(
                new JsonObject
                {
                    ["Sky_Color"] = ReadRGB(d, offset),
                    ["Cloud_Color"] = ReadRGB(d, offset + 3),
                    ["Other_Colors"] = others,
                }
            );
            offset += 0x15;
        }
        return entries;
    }

    // LBNK
    static JsonArray ExtractLBNK(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["field_0x0"] = d[offset],
                    ["field_0x1"] = d[offset + 1],
                    ["field_0x2"] = d[offset + 2]
                }
            );
            offset += 3;
        }
        return entries;
    }

    // TRES
    static JsonArray ExtractTRES(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["Name"] = ReadAscii(d, offset, 8),
                    ["field_0x8"] = d[offset + 8],
                    ["Type_Flag"] = d[offset + 9],
                    ["field_0xa"] = d[offset + 10],
                    ["Appear_Type"] = d[offset + 11],
                    ["Position_X"] = ReadF32(d, offset + 12),
                    ["Position_Y"] = ReadF32(d, offset + 16),
                    ["Position_Z"] = ReadF32(d, offset + 20),
                    ["Room_No"] = ReadS16(d, offset + 24),
                    ["Rotation"] = ReadS16(d, offset + 26),
                    ["Item"] = d[offset + 28],
                    ["Flag_ID"] = d[offset + 29],
                    ["field_0x1e"] = d[offset + 30],
                    ["field_0x1f"] = d[offset + 31],
                }
            );
            offset += 0x20;
        }
        return entries;
    }

    // TRES (room/stage variant)
    static JsonArray ExtractStageTRES(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject
                {
                    ["Number"] = d[offset],
                    ["Room_Number"] = (sbyte)d[offset + 1],
                    ["Status"] = d[offset + 2],
                    ["Argument_1"] = d[offset + 3],
                    ["Position_X"] = ReadF32(d, offset + 4),
                    ["Position_Y"] = ReadF32(d, offset + 8),
                    ["Position_Z"] = ReadF32(d, offset + 12),
                    ["Sw_Bit"] = d[offset + 16],
                    ["Type"] = d[offset + 17],
                    ["Argument_2"] = d[offset + 18],
                    ["Angle_Y"] = (sbyte)d[offset + 19],
                }
            );
            offset += 0x14;
        }
        return entries;
    }

    // STAG
    static JsonArray ExtractSTAG(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            var f14 = new JsonArray();
            for (int j = 0; j < 6; j++)
                f14.Add(d[offset + 0x14 + j]);
            var particle = new JsonArray();
            for (int j = 0; j < 16; j++)
                particle.Add(d[offset + 0x2C + j]);
            entries.Add(
                new JsonObject
                {
                    ["field_0x0"] = ReadF32(d, offset),
                    ["field_0x4"] = ReadF32(d, offset + 4),
                    ["Camera_Type"] = d[offset + 8],
                    ["field_0x9"] = d[offset + 9],
                    ["field_0xa"] = ReadU16(d, offset + 10),
                    ["field_0xc"] = (long)ReadU32(d, offset + 12),
                    ["field_0x10"] = (long)ReadU32(d, offset + 16),
                    ["field_0x14"] = f14,
                    ["Gap_Level"] = ReadS16(d, offset + 0x1A),
                    ["Range_Up"] = ReadS16(d, offset + 0x1C),
                    ["Range_Down"] = ReadS16(d, offset + 0x1E),
                    ["field_0x20"] = ReadF32(d, offset + 0x20),
                    ["field_0x24"] = ReadF32(d, offset + 0x24),
                    ["Msg_Group"] = d[offset + 0x28],
                    ["field_0x29"] = d[offset + 0x29],
                    ["Stage_Title_No"] = ReadU16(d, offset + 0x2A),
                    ["Particle_No"] = particle,
                }
            );
            offset += 0x3C;
        }
        return entries;
    }

    // MEMA / MEM0
    static JsonArray ExtractMEMA(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add((long)ReadU32(d, offset));
            offset += 4;
        }
        return entries;
    }

    // MECO / MEC0
    static JsonArray ExtractMECO(int count, int offset, byte[] d)
    {
        var entries = new JsonArray();
        for (int i = 0; i < count; i++)
        {
            entries.Add(
                new JsonObject { ["Room_Number"] = d[offset], ["Block_Id"] = d[offset + 1] }
            );
            offset += 2;
        }
        return entries;
    }

    // MPAT / MPA0 (complex nested structure)
    static JsonObject ExtractMPAT(int count, int offset, byte[] d)
    {
        int baseOffset = offset;
        byte floorEntries = d[offset];
        byte f1 = d[offset + 1];
        ushort xyEntries = ReadU16(d, offset + 2);
        int floorOffset = (int)ReadU32(d, offset + 4) + baseOffset;
        int floatOffset = (int)ReadU32(d, offset + 8) + baseOffset;

        var vertices = new JsonArray();
        for (int j = 0; j < xyEntries; j++)
        {
            vertices.Add(
                new JsonObject
                {
                    ["x"] = ReadF32(d, floatOffset),
                    ["z"] = ReadF32(d, floatOffset + 4)
                }
            );
            floatOffset += 8;
        }

        var floors = new JsonArray();
        for (int j = 0; j < floorEntries; j++)
        {
            byte floorId = d[floorOffset];
            byte groupCount = d[floorOffset + 1];
            byte ff2 = d[floorOffset + 2];
            byte ff3 = d[floorOffset + 3];
            int groupOffset = (int)ReadU32(d, floorOffset + 4) + baseOffset;

            var groups = new JsonArray();
            for (int k = 0; k < groupCount; k++)
            {
                byte g0 = d[groupOffset];
                byte g1 = d[groupOffset + 1];
                byte lineCount = d[groupOffset + 2];
                byte g3 = d[groupOffset + 3];
                byte polyCount = d[groupOffset + 4];
                byte g5 = d[groupOffset + 5];
                byte g6 = d[groupOffset + 6];
                byte g7 = d[groupOffset + 7];
                int lineOffset = (int)ReadU32(d, groupOffset + 8) + baseOffset;
                byte gc = d[groupOffset + 12];
                byte gd = d[groupOffset + 13];
                byte ge = d[groupOffset + 14];
                byte gf = d[groupOffset + 15];
                int polyOffset = (int)ReadU32(d, groupOffset + 16) + baseOffset;

                var lines = new JsonArray();
                for (int l = 0; l < lineCount; l++)
                {
                    byte l0 = d[lineOffset];
                    byte l1 = d[lineOffset + 1];
                    byte dataCount = d[lineOffset + 2];
                    byte l3 = d[lineOffset + 3];
                    int dataOffset = (int)ReadU32(d, lineOffset + 4) + baseOffset;
                    var verts = new JsonArray();
                    for (int m = 0; m < dataCount; m++)
                        verts.Add(ReadU16(d, dataOffset + m * 2));
                    lines.Add(
                        new JsonObject
                        {
                            ["field_0x0"] = l0,
                            ["field_0x1"] = l1,
                            ["field_0x3"] = l3,
                            ["Vertex_Indexes"] = verts
                        }
                    );
                    lineOffset += 8;
                }
                var polygons = new JsonArray();
                for (int l = 0; l < polyCount; l++)
                {
                    byte p0 = d[polyOffset];
                    byte dataCount = d[polyOffset + 1];
                    byte p2 = d[polyOffset + 2];
                    byte p3 = d[polyOffset + 3];
                    int dataOffset = (int)ReadU32(d, polyOffset + 4) + baseOffset;
                    var verts = new JsonArray();
                    for (int m = 0; m < dataCount; m++)
                        verts.Add(ReadU16(d, dataOffset + m * 2));
                    polygons.Add(
                        new JsonObject
                        {
                            ["field_0x0"] = p0,
                            ["field_0x2"] = p2,
                            ["field_0x3"] = p3,
                            ["Vertex_Indexes"] = verts
                        }
                    );
                    polyOffset += 8;
                }
                groups.Add(
                    new JsonObject
                    {
                        ["field_0x0"] = g0,
                        ["field_0x1"] = g1,
                        ["field_0x3"] = g3,
                        ["field_0x5"] = g5,
                        ["field_0x6"] = g6,
                        ["field_0x7"] = g7,
                        ["field_0xc"] = gc,
                        ["field_0xd"] = gd,
                        ["field_0xe"] = ge,
                        ["field_0xf"] = gf,
                        ["Lines"] = lines,
                        ["Polygons"] = polygons,
                    }
                );
                groupOffset += 0x14;
            }
            floors.Add(
                new JsonObject
                {
                    ["Id"] = floorId,
                    ["field_0x2"] = ff2,
                    ["field_0x3"] = ff3,
                    ["Groups"] = groups
                }
            );
            floorOffset += 8;
        }
        return new JsonObject
        {
            ["field_0x1"] = f1,
            ["Entry_Num"] = count,
            ["Vertices"] = vertices,
            ["Floors"] = floors
        };
    }

    // ----------------------------------------------------------
    // PACKAGE functions
    // ----------------------------------------------------------

    static byte[] PackageSTAG(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteF32(buf, e!["field_0x0"]!.GetValue<float>());
            WriteF32(buf, e["field_0x4"]!.GetValue<float>());
            WriteU8(buf, (byte)e["Camera_Type"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x9"]!.GetValue<int>());
            WriteU16(buf, (ushort)e["field_0xa"]!.GetValue<int>());
            WriteU32(buf, (uint)e["field_0xc"]!.GetValue<long>());
            WriteU32(buf, (uint)e["field_0x10"]!.GetValue<long>());
            foreach (var v in e["field_0x14"]!.AsArray())
                buf.Add((byte)v!.GetValue<int>());
            WriteS16(buf, (short)e["Gap_Level"]!.GetValue<int>());
            WriteS16(buf, (short)e["Range_Up"]!.GetValue<int>());
            WriteS16(buf, (short)e["Range_Down"]!.GetValue<int>());
            WriteF32(buf, e["field_0x20"]!.GetValue<float>());
            WriteF32(buf, e["field_0x24"]!.GetValue<float>());
            WriteU8(buf, (byte)e["Msg_Group"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x29"]!.GetValue<int>());
            WriteU16(buf, (ushort)e["Stage_Title_No"]!.GetValue<int>());
            foreach (var v in e["Particle_No"]!.AsArray())
                buf.Add((byte)v!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageRTBL(JsonNode entries, int offset)
    {
        var arr = entries.AsArray();
        var data = new List<byte>();
        var middledata = new List<byte>();
        var enddata = new List<byte>();
        int middleOffset = offset + 4 * arr.Count;
        int endOffset = middleOffset + arr.Count * 8;
        for (int i = 0; i < arr.Count; i++)
        {
            var e = arr[i]!;
            var tbl = e["Table"]!.AsArray();
            WriteU32(data, (uint)(middleOffset + i * 8));
            WriteU8(middledata, (byte)tbl.Count);
            WriteU8(middledata, (byte)e["field_0x1"]!.GetValue<int>());
            WriteU8(middledata, (byte)e["field_0x2"]!.GetValue<int>());
            WriteU8(middledata, 0);
            WriteU32(middledata, (uint)(endOffset + enddata.Count));
            foreach (var b in tbl)
                enddata.Add((byte)b!.GetValue<int>());
        }
        data.AddRange(middledata);
        data.AddRange(enddata);
        return data.ToArray();
    }

    static byte[] PackageMULT(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteF32(buf, e!["x"]!.GetValue<float>());
            WriteF32(buf, e["y"]!.GetValue<float>());
            WriteS16(buf, (short)e["Angle"]!.GetValue<int>());
            WriteU8(buf, (byte)e["roomNo"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0xb"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageCAM(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteAscii(buf, e!["Camera_Type"]!.GetValue<string>(), 16);
            WriteU8(buf, (byte)e["field_0x10"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x11"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x12"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x13"]!.GetValue<int>());
            WriteU16(buf, (ushort)e["field_0x14"]!.GetValue<int>());
            WriteU16(buf, (ushort)e["field_0x16"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageRARO(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteF32(buf, e!["x"]!.GetValue<float>());
            WriteF32(buf, e["y"]!.GetValue<float>());
            WriteF32(buf, e["z"]!.GetValue<float>());
            WriteS16(buf, (short)e["Angle_X"]!.GetValue<int>());
            WriteS16(buf, (short)e["Angle_Y"]!.GetValue<int>());
            WriteS16(buf, (short)e["Angle_Z"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x12"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x13"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageEVLY(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
            foreach (var v in e!["layerTable"]!.AsArray())
                buf.Add((byte)v!.GetValue<int>());
        return buf.ToArray();
    }

    static byte[] PackageVRB(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteRGB(buf, e!["Sky_Color"]!.AsArray());
            WriteRGB(buf, e["Cloud_Color"]!.AsArray());
            foreach (var col in e["Other_Colors"]!.AsArray())
                WriteRGB(buf, col!.AsArray());
        }
        return buf.ToArray();
    }

    static byte[] PackageEnv(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
            foreach (var v in e!["Pselect_ID_Table"]!.AsArray())
                buf.Add((byte)v!.GetValue<int>());
        return buf.ToArray();
    }

    static byte[] PackageCol(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            foreach (var v in e!["Palette_Ids"]!.AsArray())
                buf.Add((byte)v!.GetValue<int>());
            WriteF32(buf, e["Change_Rate"]!.GetValue<float>());
        }
        return buf.ToArray();
    }

    static byte[] PackagePAL(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteRGB(buf, e!["Actor_Ambient_Color"]!.AsArray());
            foreach (var col in e["BG_Ambient_Colors"]!.AsArray())
                WriteRGB(buf, col!.AsArray());
            foreach (var col in e["P_Light_Colors"]!.AsArray())
                WriteRGB(buf, col!.AsArray());
            WriteRGB(buf, e["Fog_Color"]!.AsArray());
            WriteF32(buf, e["Fog_Start_Z"]!.GetValue<float>());
            WriteF32(buf, e["Fog_End_Z"]!.GetValue<float>());
            WriteU8(buf, (byte)e["Virt_Idx"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Terrain_Light_Influence"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Cloud_Shadow_Density"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x2f"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Bloom_Table_Idx"]!.GetValue<int>());
            WriteU8(buf, (byte)e["BG_Ambient_Color_1a"]!.GetValue<int>());
            WriteU8(buf, (byte)e["BG_Ambient_Color_2a"]!.GetValue<int>());
            WriteU8(buf, (byte)e["BG_Ambient_Color_3a"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageFILI(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteU32(buf, (uint)e!["Parameters"]!.GetValue<long>());
            WriteF32(buf, e["Sea_Level"]!.GetValue<float>());
            WriteF32(buf, e["field_0x8"]!.GetValue<float>());
            WriteF32(buf, e["field_0xc"]!.GetValue<float>());
            foreach (var v in e["field_0x10"]!.AsArray())
                buf.Add((byte)v!.GetValue<int>());
            WriteU8(buf, (byte)e["Default_Camera"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Bit_Sw"]!.GetValue<int>());
            WriteU16(buf, (ushort)e["Msg"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageFILI2(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteF32(buf, e!["Left_Room_X"]!.GetValue<float>());
            WriteF32(buf, e["Inner_Room_Z"]!.GetValue<float>());
            WriteF32(buf, e["Right_Room_X"]!.GetValue<float>());
            WriteF32(buf, e["Front_Room_Z"]!.GetValue<float>());
            WriteU8(buf, (byte)e["Min_Floor_No"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Max_Floor_No"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x12"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x13"]!.GetValue<int>());
            WriteF32(buf, e["field_0x14"]!.GetValue<float>());
            WriteF32(buf, e["field_0x18"]!.GetValue<float>());
            WriteS16(buf, (short)e["field_0x1c"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageSCLS(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteAscii(buf, e!["Stage"]!.GetValue<string>(), 8);
            WriteU8(buf, (byte)e["Start"]!.GetValue<int>());
            WriteS8(buf, (sbyte)e["Room"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0xa"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0xb"]!.GetValue<int>());
            WriteS8(buf, (sbyte)e["Wipe"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageLBNK(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteU8(buf, (byte)e!["field_0x0"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x2"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageREVT(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteU8(buf, (byte)e!["Type"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x2"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x3"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x4"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x5"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Priority"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x7"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x8"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x9"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0xa"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0xb"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0xc"]!.GetValue<int>());
            var name = e["Name"];
            if (name is JsonValue)
                WriteAscii(buf, name.GetValue<string>(), 11);
            else
                foreach (var v in name!.AsArray())
                    buf.Add((byte)v!.GetValue<int>());
            WriteU8(buf, (byte)e["seType"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1a"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1b"]!.GetValue<int>());
            WriteU8(buf, (byte)e["switch"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageACTR(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteAscii(buf, e!["Name"]!.GetValue<string>(), 8);
            WriteU32(buf, (uint)e["param"]!.GetValue<long>());
            WriteF32(buf, e["x"]!.GetValue<float>());
            WriteF32(buf, e["y"]!.GetValue<float>());
            WriteF32(buf, e["z"]!.GetValue<float>());
            WriteS16(buf, (short)e["Angle_X"]!.GetValue<int>());
            WriteS16(buf, (short)e["Angle_Y"]!.GetValue<int>());
            WriteS16(buf, (short)e["Angle_Z"]!.GetValue<int>());
            WriteS16(buf, (short)e["EnemyNo"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageLGT(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteF32(buf, e!["x"]!.GetValue<float>());
            WriteF32(buf, e["y"]!.GetValue<float>());
            WriteF32(buf, e["z"]!.GetValue<float>());
            WriteF32(buf, e["Radius"]!.GetValue<float>());
            WriteF32(buf, e["Direction_X"]!.GetValue<float>());
            WriteF32(buf, e["Direction_Y"]!.GetValue<float>());
            WriteF32(buf, e["Spotlight_Cutoff"]!.GetValue<float>());
            WriteU8(buf, (byte)e["field_0x1c"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1d"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1e"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1f"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageTRES(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteAscii(buf, e!["Name"]!.GetValue<string>(), 8);
            WriteU8(buf, (byte)e["field_0x8"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Type_Flag"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0xa"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Appear_Type"]!.GetValue<int>());
            WriteF32(buf, e["Position_X"]!.GetValue<float>());
            WriteF32(buf, e["Position_Y"]!.GetValue<float>());
            WriteF32(buf, e["Position_Z"]!.GetValue<float>());
            WriteS16(buf, (short)e["Room_No"]!.GetValue<int>());
            WriteS16(buf, (short)e["Rotation"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Item"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Flag_ID"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1e"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1f"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageStageTRES(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteU8(buf, (byte)e!["Number"]!.GetValue<int>());
            WriteS8(buf, (sbyte)e["Room_Number"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Status"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Argument_1"]!.GetValue<int>());
            WriteF32(buf, e["Position_X"]!.GetValue<float>());
            WriteF32(buf, e["Position_Y"]!.GetValue<float>());
            WriteF32(buf, e["Position_Z"]!.GetValue<float>());
            WriteU8(buf, (byte)e["Sw_Bit"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Type"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Argument_2"]!.GetValue<int>());
            WriteS8(buf, (sbyte)e["Angle_Y"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageRPAT(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteU16(buf, (ushort)e!["Number_of_Points"]!.GetValue<int>());
            WriteS16(buf, (short)e["Path_Index"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x4"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Looped"]!.GetValue<int>());
            WriteU16(buf, (ushort)e["field_0x6"]!.GetValue<int>());
            WriteU32(buf, (uint)(e["RPPN_Entry_Index"]!.GetValue<int>() * 0x10));
        }
        return buf.ToArray();
    }

    static byte[] PackageRPPN(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteU8(buf, (byte)e!["field_0x0"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x2"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x3"]!.GetValue<int>());
            WriteF32(buf, e["Position_X"]!.GetValue<float>());
            WriteF32(buf, e["Position_Y"]!.GetValue<float>());
            WriteF32(buf, e["Position_Z"]!.GetValue<float>());
        }
        return buf.ToArray();
    }

    static byte[] PackageTGSC(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteAscii(buf, e!["Name"]!.GetValue<string>(), 8);
            WriteU32(buf, (uint)e["param"]!.GetValue<long>());
            WriteF32(buf, e["x"]!.GetValue<float>());
            WriteF32(buf, e["y"]!.GetValue<float>());
            WriteF32(buf, e["z"]!.GetValue<float>());
            WriteS16(buf, (short)e["Angle_X"]!.GetValue<int>());
            WriteS16(buf, (short)e["Angle_Y"]!.GetValue<int>());
            WriteS16(buf, (short)e["Angle_Z"]!.GetValue<int>());
            WriteS16(buf, (short)e["EnemyNo"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Scale_X"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Scale_Y"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Scale_Z"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x23"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageSOND(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteAscii(buf, e!["Name"]!.GetValue<string>(), 8);
            WriteF32(buf, e["x"]!.GetValue<float>());
            WriteF32(buf, e["y"]!.GetValue<float>());
            WriteF32(buf, e["z"]!.GetValue<float>());
            WriteU8(buf, (byte)e["field_0x14"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x15"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x16"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x17"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x18"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x19"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1a"]!.GetValue<int>());
            WriteU8(buf, (byte)e["field_0x1b"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageMECO(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
        {
            WriteU8(buf, (byte)e!["Room_Number"]!.GetValue<int>());
            WriteU8(buf, (byte)e["Block_Id"]!.GetValue<int>());
        }
        return buf.ToArray();
    }

    static byte[] PackageMEMO(JsonNode entries, int offset)
    {
        var buf = new List<byte>();
        foreach (var e in entries.AsArray())
            WriteU32(buf, (uint)e!.GetValue<long>());
        return buf.ToArray();
    }

    static byte[] PackageMPAT(JsonNode entryNode, int offset)
    {
        var entry = entryNode.AsObject();
        var floors = entry["Floors"]!.AsArray();
        var vertices = entry["Vertices"]!.AsArray();

        var headerBuf = new List<byte>();
        var floatData = new List<byte>();
        var groupData = new List<byte>();
        var linePolyData = new List<byte>();
        var indexData = new List<byte>();

        int o = 12 + floors.Count * 8;
        WriteU8(headerBuf, (byte)floors.Count);
        WriteU8(headerBuf, (byte)entry["field_0x1"]!.GetValue<int>());
        WriteU16(headerBuf, (ushort)vertices.Count);
        WriteU32(headerBuf, 12);
        WriteU32(headerBuf, (uint)o);
        o += 8 * vertices.Count;

        int groupLength = 0,
            linePolyLength = 0;
        foreach (var floor in floors)
            foreach (var g in floor!["Groups"]!.AsArray())
            {
                groupLength++;
                linePolyLength += g!["Lines"]!.AsArray().Count + g["Polygons"]!.AsArray().Count;
            }

        int polyLineOffset = o + groupLength * 20;
        int indexDataOffset = polyLineOffset + linePolyLength * 8;

        foreach (var v in vertices)
        {
            WriteF32(floatData, v!["x"]!.GetValue<float>());
            WriteF32(floatData, v["z"]!.GetValue<float>());
        }

        foreach (var floor in floors)
        {
            WriteU8(headerBuf, (byte)floor!["Id"]!.GetValue<int>());
            WriteU8(headerBuf, (byte)floor["Groups"]!.AsArray().Count);
            WriteU8(headerBuf, (byte)floor["field_0x2"]!.GetValue<int>());
            WriteU8(headerBuf, (byte)floor["field_0x3"]!.GetValue<int>());
            WriteU32(headerBuf, (uint)(o + groupData.Count));

            foreach (var g in floor["Groups"]!.AsArray())
            {
                int lineOff = polyLineOffset + linePolyData.Count;
                int polyOff = lineOff + g!["Lines"]!.AsArray().Count * 8;
                WriteU8(groupData, (byte)g["field_0x0"]!.GetValue<int>());
                WriteU8(groupData, (byte)g["field_0x1"]!.GetValue<int>());
                WriteU8(groupData, (byte)g["Lines"]!.AsArray().Count);
                WriteU8(groupData, (byte)g["field_0x3"]!.GetValue<int>());
                WriteU8(groupData, (byte)g["Polygons"]!.AsArray().Count);
                WriteU8(groupData, (byte)g["field_0x5"]!.GetValue<int>());
                WriteU8(groupData, (byte)g["field_0x6"]!.GetValue<int>());
                WriteU8(groupData, (byte)g["field_0x7"]!.GetValue<int>());
                WriteU32(groupData, (uint)lineOff);
                WriteU8(groupData, (byte)g["field_0xc"]!.GetValue<int>());
                WriteU8(groupData, (byte)g["field_0xd"]!.GetValue<int>());
                WriteU8(groupData, (byte)g["field_0xe"]!.GetValue<int>());
                WriteU8(groupData, (byte)g["field_0xf"]!.GetValue<int>());
                WriteU32(groupData, (uint)polyOff);

                foreach (var l in g["Lines"]!.AsArray())
                {
                    var verts = l!["Vertex_Indexes"]!.AsArray();
                    WriteU8(linePolyData, (byte)l["field_0x0"]!.GetValue<int>());
                    WriteU8(linePolyData, (byte)l["field_0x1"]!.GetValue<int>());
                    WriteU8(linePolyData, (byte)verts.Count);
                    WriteU8(linePolyData, (byte)l["field_0x3"]!.GetValue<int>());
                    WriteU32(linePolyData, (uint)(indexDataOffset + indexData.Count));
                    foreach (var i in verts)
                        WriteU16(indexData, (ushort)i!.GetValue<int>());
                }
                foreach (var p in g["Polygons"]!.AsArray())
                {
                    var verts = p!["Vertex_Indexes"]!.AsArray();
                    WriteU8(linePolyData, (byte)p["field_0x0"]!.GetValue<int>());
                    WriteU8(linePolyData, (byte)verts.Count);
                    WriteU8(linePolyData, (byte)p["field_0x2"]!.GetValue<int>());
                    WriteU8(linePolyData, (byte)p["field_0x3"]!.GetValue<int>());
                    WriteU32(linePolyData, (uint)(indexDataOffset + indexData.Count));
                    foreach (var i in verts)
                        WriteU16(indexData, (ushort)i!.GetValue<int>());
                }
            }
        }
        var result = new List<byte>();
        result.AddRange(headerBuf);
        result.AddRange(floatData);
        result.AddRange(groupData);
        result.AddRange(linePolyData);
        result.AddRange(indexData);
        return result.ToArray();
    }

    // ----------------------------------------------------------
    // Dispatch tables
    // ----------------------------------------------------------
    static bool IsLayerChar(char c) => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'e');

    delegate JsonNode ExtractFunc(int count, int offset, byte[] data);
    delegate byte[] PackageFunc(JsonNode entries, int offset);

    static Dictionary<string, ExtractFunc> BuildDecodeTbl(bool roomStage) =>
        new()
        {
            ["STAG"] = (c, o, d) => ExtractSTAG(c, o, d),
            ["EVLY"] = (c, o, d) => ExtractEVLY(c, o, d),
            ["RPPN"] = (c, o, d) => ExtractRPPN(c, o, d),
            ["RPAT"] = (c, o, d) => ExtractRPAT(c, o, d),
            ["MULT"] = (c, o, d) => ExtractMULT(c, o, d),
            ["PLYR"] = (c, o, d) => ExtractACTR(c, o, d),
            ["CAMR"] = (c, o, d) => ExtractCAM(c, o, d),
            ["RCAM"] = (c, o, d) => ExtractCAM(c, o, d),
            ["ACTR"] = (c, o, d) => ExtractACTR(c, o, d),
            ["TGOB"] = (c, o, d) => ExtractACTR(c, o, d),
            ["RTBL"] = (c, o, d) => ExtractRTBL(c, o, d),
            ["AROB"] = (c, o, d) => ExtractRARO(c, o, d),
            ["RARO"] = (c, o, d) => ExtractRARO(c, o, d),
            ["SCLS"] = (c, o, d) => ExtractSCLS(c, o, d),
            ["TGSC"] = (c, o, d) => ExtractTGSC(c, o, d),
            ["PPNT"] = (c, o, d) => ExtractRPPN(c, o, d),
            ["SCOB"] = (c, o, d) => ExtractTGSC(c, o, d),
            ["FILI"] = roomStage
                ? (ExtractFunc)((c, o, d) => ExtractFILI2(c, o, d))
                : (c, o, d) => ExtractFILI(c, o, d),
            ["Door"] = (c, o, d) => ExtractTGSC(c, o, d),
            ["TGDR"] = (c, o, d) => ExtractTGSC(c, o, d),
            ["REVT"] = (c, o, d) => ExtractREVT(c, o, d),
            ["SOND"] = (c, o, d) => ExtractSOND(c, o, d),
            ["SON0"] = (c, o, d) => ExtractSOND(c, o, d),
            ["LGT0"] = (c, o, d) => ExtractLGTV(c, o, d),
            ["LGTV"] = (c, o, d) => ExtractLGTV(c, o, d),
            ["Env0"] = (c, o, d) => ExtractENVR(c, o, d),
            ["Col0"] = (c, o, d) => ExtractCol(c, o, d),
            ["PAL0"] = (c, o, d) => ExtractPAL(c, o, d),
            ["VRB0"] = (c, o, d) => ExtractVRB(c, o, d),
            ["Doo0"] = (c, o, d) => ExtractTGSC(c, o, d),
            ["SCO0"] = (c, o, d) => ExtractTGSC(c, o, d),
            ["ACT0"] = (c, o, d) => ExtractACTR(c, o, d),
            ["TRE0"] = roomStage
                ? (ExtractFunc)((c, o, d) => ExtractStageTRES(c, o, d))
                : (c, o, d) => ExtractACTR(c, o, d),
            ["TRES"] = roomStage
                ? (ExtractFunc)((c, o, d) => ExtractStageTRES(c, o, d))
                : (c, o, d) => ExtractTRES(c, o, d),
            ["LBNK"] = (c, o, d) => ExtractLBNK(c, o, d),
            ["MEM0"] = (c, o, d) => ExtractMEMA(c, o, d),
            ["MEC0"] = (c, o, d) => ExtractMECO(c, o, d),
            ["MPAT"] = (c, o, d) => ExtractMPAT(c, o, d),
            ["MPA0"] = (c, o, d) => ExtractMPAT(c, o, d),
        };

    static Dictionary<string, PackageFunc> BuildEncodeTbl(bool roomStage) =>
        new()
        {
            ["STAG"] = PackageSTAG,
            ["RTBL"] = PackageRTBL,
            ["MULT"] = PackageMULT,
            ["RCAM"] = PackageCAM,
            ["RARO"] = PackageRARO,
            ["AROB"] = PackageRARO,
            ["EVLY"] = PackageEVLY,
            ["VRB0"] = PackageVRB,
            ["Env0"] = PackageEnv,
            ["Col0"] = PackageCol,
            ["PAL0"] = PackagePAL,
            ["SCLS"] = PackageSCLS,
            ["FILI"] = roomStage ? (PackageFunc)PackageFILI2 : PackageFILI,
            ["LBNK"] = PackageLBNK,
            ["REVT"] = PackageREVT,
            ["PLYR"] = PackageACTR,
            ["ACTR"] = PackageACTR,
            ["TGOB"] = PackageACTR,
            ["TRES"] = roomStage ? (PackageFunc)PackageStageTRES : PackageTRES,
            ["ACT0"] = PackageACTR,
            ["TRE0"] = roomStage ? (PackageFunc)PackageStageTRES : PackageACTR,
            ["LGT0"] = PackageLGT,
            ["LGTV"] = PackageLGT,
            ["RPAT"] = PackageRPAT,
            ["RPPN"] = PackageRPPN,
            ["SCOB"] = PackageTGSC,
            ["TGSC"] = PackageTGSC,
            ["TGDR"] = PackageTGSC,
            ["SCO0"] = PackageTGSC,
            ["MPAT"] = (e, o) => PackageMPAT(e, o),
            ["MPA0"] = (e, o) => PackageMPAT(e, o),
            ["SOND"] = PackageSOND,
            ["SON0"] = PackageSOND,
            ["MEC0"] = PackageMECO,
            ["MEM0"] = PackageMEMO,
            ["Door"] = PackageTGSC,
            ["Doo0"] = PackageTGSC,
        };

    // ----------------------------------------------------------
    // Main Extract / Package
    // ----------------------------------------------------------

    static string NormalizeLayerTag(string tag)
    {
        if (tag.Length > 0 && IsLayerChar(tag[tag.Length - 1]))
            return tag.Substring(0, tag.Length - 1) + "0";
        return tag;
    }

    public static string Extract(byte[] data, bool roomStage, bool roomFile)
    {
        int chunkCount = (int)ReadU32(data, 0);
        int offset = 4;
        var tbl = BuildDecodeTbl(roomStage);

        var chunkTable = new List<(string realTag, int dataOffset, JsonNode entries)>();

        for (int i = 0; i < chunkCount; i++)
        {
            string realTag = ReadAscii(data, offset, 4);
            int entryNum = ReadS32(data, offset + 4);
            int dataOffset = (int)ReadU32(data, offset + 8);
            offset += 12;

            string lookupTag = realTag;
            if (!tbl.ContainsKey(lookupTag))
            {
                lookupTag = NormalizeLayerTag(realTag);
                if (!tbl.ContainsKey(lookupTag))
                {
                    Console.WriteLine($"Unknown tag: {realTag}");
                    continue;
                }
            }

            JsonNode entries;
            if (lookupTag == "MPAT" || lookupTag == "MPA0")
                entries = ExtractMPAT(entryNum, dataOffset, data);
            else
                entries = tbl[lookupTag](entryNum, dataOffset, data);

            chunkTable.Add((realTag, dataOffset, entries));
        }

        chunkTable.Sort((a, b) => a.dataOffset.CompareTo(b.dataOffset));

        var result = new JsonArray();
        foreach (var (tag, _, entries) in chunkTable)
            result.Add(new JsonObject { ["Tag"] = tag, ["Entries"] = entries });

        var options = new JsonSerializerOptions { WriteIndented = true };

        options.Converters.Add(new FloatConverter());

        string json = JsonSerializer.Serialize(result, options);

        return json;
    }

    static readonly string[] PackageOrder =
    {
        "STAG",
        "RTBL",
        "SCLS",
        "REVT",
        "MULT",
        "FILI",
        "LBNK",
        "RPAT",
        "RPPN",
        "RCAM",
        "RARO",
        "EVLY",
        "PLYR",
        "ACTR",
        "SCOB",
        "SOND",
        "LGTV",
        "TRES",
        "MPAT",
        "Door"
    };
    static readonly string[] LayerOrder =
    {
        "ACT",
        "SCO",
        "SON",
        "MEC",
        "MEM",
        "LGT",
        "MPA",
        "VRB",
        "Env",
        "Col",
        "PAL",
        "Doo",
        "TRE"
    };

    static List<string> BuildFullOrder()
    {
        var order = new List<string>(PackageOrder);
        for (int i = 0; i < 15; i++)
            foreach (var prefix in LayerOrder)
                order.Add(prefix + i.ToString("x"));
        return order;
    }

    public static byte[] Package(JsonArray chunkTable, bool roomStage, bool roomFile)
    {
        var tbl = BuildEncodeTbl(roomStage);

        var data = new List<byte>();
        var headerTable = new List<(string tag, int dataOffset, int numEntries)>();
        int headerSize = 4 + chunkTable.Count * 12;

        foreach (var chunkNode in chunkTable)
        {
            string realTag = chunkNode!["Tag"]!.GetValue<string>();
            string lookupTag = realTag;
            if (!tbl.ContainsKey(lookupTag))
            {
                lookupTag = NormalizeLayerTag(realTag);
                if (!tbl.ContainsKey(lookupTag))
                {
                    Console.WriteLine($"Unknown tag: {realTag}");
                    continue;
                }
            }

            int dataOffset = headerSize + data.Count;
            var entries = chunkNode["Entries"]!;
            int numEntries = entries is JsonArray arr
                ? arr.Count
                : entries["Entry Num"]!.GetValue<int>();

            byte[] chunkData = tbl[lookupTag](entries, dataOffset);
            data.AddRange(chunkData);
            if (data.Count % 4 != 0)
                data.AddRange(new byte[4 - data.Count % 4].Select(b => (byte)0xFF));

            headerTable.Add((realTag, dataOffset, numEntries));
        }

        var fullOrder = BuildFullOrder();
        var tagIndex = new Dictionary<string, int>();
        for (int i = 0; i < fullOrder.Count; i++)
            tagIndex[fullOrder[i]] = i;
        headerTable.Sort(
            (a, b) =>
                tagIndex
                    .GetValueOrDefault(a.tag, 9999)
                    .CompareTo(tagIndex.GetValueOrDefault(b.tag, 9999))
        );

        var headerData = new List<byte>();
        WriteU32(headerData, (uint)headerTable.Count);
        foreach (var (tag, dataOffset, numEntries) in headerTable)
        {
            while (tag.Length < 4) { } // tags are always 4 chars from the file
            headerData.Add((byte)tag[0]);
            headerData.Add((byte)tag[1]);
            headerData.Add(tag.Length > 2 ? (byte)tag[2] : (byte)0);
            headerData.Add(tag.Length > 3 ? (byte)tag[3] : (byte)0);
            WriteS32(headerData, numEntries);
            WriteU32(headerData, (uint)dataOffset);
        }

        var result = new List<byte>();
        result.AddRange(headerData);
        result.AddRange(data);

        if (roomFile)
        {
            uint[] trailer = { 0, 0x10, 0x10, 0x10, 0, 0x10, 0x10, 0x10 };
            foreach (var v in trailer)
                WriteU32(result, v);
        }
        if (result.Count % 0x20 != 0)
            result.AddRange(Enumerable.Repeat((byte)0xFF, 0x20 - result.Count % 0x20));

        return result.ToArray();
    }

    // ----------------------------------------------------------
    // Public convenience wrappers (mirrors Python's top-level fns)
    // ----------------------------------------------------------

    public static string ExtractToJson(string path, byte[] data)
    {
        bool roomFile = path.EndsWith(".dzr", StringComparison.OrdinalIgnoreCase);
        bool roomStage = !roomFile && path.Contains("room", StringComparison.OrdinalIgnoreCase);
        return Extract(data, roomStage, roomFile);
    }

    public static byte[] PackageFromJson(string path, string json)
    {
        bool roomFile = path.EndsWith(".dzr", StringComparison.OrdinalIgnoreCase);
        bool roomStage = !roomFile && path.Contains("room", StringComparison.OrdinalIgnoreCase);
        var chunkTable = JsonNode.Parse(json)!.AsArray();
        return Package(chunkTable, roomStage, roomFile);
    }

    public static void ExtractDZX(string dzxFile, string jsonFile)
    {
        // Binary → JSON
        byte[] data = File.ReadAllBytes(dzxFile);
        bool roomFile = dzxFile.EndsWith(".dzr", StringComparison.OrdinalIgnoreCase);
        bool roomStage = !roomFile && dzxFile.Contains("room", StringComparison.OrdinalIgnoreCase);
        string json = Extract(data, roomStage, roomFile);
        File.WriteAllText(jsonFile, json);
    }

    // Overload. If no json file param is passed, return the contents of the file.
    public static string ExtractDZX(string dzxFile)
    {
        // Binary → JSON
        byte[] data = File.ReadAllBytes(dzxFile);
        bool roomFile = dzxFile.EndsWith(".dzr", StringComparison.OrdinalIgnoreCase);
        bool roomStage = !roomFile && dzxFile.Contains("room", StringComparison.OrdinalIgnoreCase);
        string json = Extract(data, roomStage, roomFile);
        return json;
    }

    public static void PackageDZX(string jsonFile, string dzxFile, bool readDirectory)
    {
        string json = "";
        // JSON → binary
        if (readDirectory)
        {
            json = File.ReadAllText(jsonFile);
        }
        else
        {
            json = jsonFile;
        }
        var chunkTable = JsonNode.Parse(json)!.AsArray();
        bool roomFile = dzxFile.EndsWith(".dzr", StringComparison.OrdinalIgnoreCase);
        bool roomStage = !roomFile && dzxFile.Contains("room", StringComparison.OrdinalIgnoreCase);
        byte[] output = Package(chunkTable, roomStage, roomFile);
        File.WriteAllBytes(dzxFile, output);
    }
}
