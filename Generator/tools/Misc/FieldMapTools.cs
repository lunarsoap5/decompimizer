using System.ComponentModel.DataAnnotations.Schema;
using RarcTools;

public class dMenu_Fmap_portal_data_c
{
    public struct data
    {
        public byte mSelectWarpPt;
        public byte mRegionNo;
        public string mStageName;
        public byte mRoomNo;
        public sbyte mWarpPlayerNo; // adjusted per note above
        public ushort mMessageID;
        public byte mStageNo;
        public byte mSwitchNo;
        public float xPosition;
        public float yPosition;
        public float zPosition;

        public data(
            byte selectWarpPt,
            byte regionNo,
            string stageName,
            byte roomNo,
            sbyte warpPlayerNo,
            ushort messageId,
            byte stageNo,
            byte switchNo,
            float x,
            float y,
            float z
        )
        {
            mSelectWarpPt = selectWarpPt;
            mRegionNo = regionNo;
            mStageName = stageName;
            mRoomNo = roomNo;
            mWarpPlayerNo = warpPlayerNo;
            mMessageID = messageId;
            mStageNo = stageNo;
            mSwitchNo = switchNo;
            xPosition = x;
            yPosition = y;
            zPosition = z;
        }
    }

    /* 0x0 */public uint version;

    /* 0x4 */public uint fileSize;

    /* 0x8 */public byte mCount;

    /* 0xC */public List<data> mData;
}

public static class FieldMapTools
{
    public static void generatePortalData()
    {
        dMenu_Fmap_portal_data_c portalData = new();
        portalData.version = 8;
        List<dMenu_Fmap_portal_data_c.data> rawData =
            new()
            {
                new(0, 1, "F_SP104", 1, -2, 0x05DB, 0, 0x34, -17.6484375f, 6230.0f, 156389.141f), // Ordon Portal
                new(1, 2, "F_SP108", 0, -2, 0x05DA, 2, 0x47, 1855.0f, 1856.81128f, 134791.0f), // South Faron Portal
                new(2, 2, "F_SP108", 6, -2, 0x05D9, 2, 0x2, -16950.0f, 1875.0f, 118800.0f), // North Faron Portal
                new(3, 3, "F_SP121", 3, 1, 0x05D3, 6, 0x15, 41894.2422f, -1200.0f, 57291.7266f), // Kak Gorge Portal
                new(4, 3, "F_SP109", 0, 0xE, 0x05D8, 3, 0x1F, 91115.0f, 6000.0f, 61754.0f), // Kak Village Portal
                new(5, 3, "F_SP110", 3, 2, 0x05D7, 3, 0x15, 113800.0f, 5000.0f, 1600.0f), // Death Mountain Portal
                new(6, 3, "F_SP121", 0, 3, 0x05D2, 6, 0x63, 84800.0f, 5700.0f, -26735.0f), // Eldin Field Portal
                new(7, 4, "F_SP122", 8, -2, 0x05D6, 6, 0x3, -20000.0f, 8340.0f, 10662.0f), // West CT Portal
                new(8, 4, "F_SP115", 0, -2, 0x05D4, 4, 0xA, -51516.2109f, -2470.0f, 53531.9336f), // Lake Hylia Portal
                new(9, 4, "F_SP113", 0, -2, 0x05D5, 4, 0x2, 4976.0f, 6000.0f, -110750.0f), // ZD Portal
                new(10, 4, "F_SP126", 0, 5, 0x05CF, 4, 0x15, 39074.0f, 6670.0f, -91401.8781f), // UZR Portal
                new(11, 5, "F_SP125", 4, 5, 0x05CD, 10, 0x28, -134200.156f, 15000.0f, 5708.23047f), // Mirror Chamber Portal
                new(12, 6, "F_SP114", 1, 2, 0x05D1, 8, 0x15, -52424.1289f, 6779.820f, -94607.63f), // Snowpeak Portal
                new(13, 2, "F_SP117", 1, -2, 0x05CE, 7, 0x64, -35000.0f, 7000.0f, 110065.0f), // Sacred Grove Portal
                new(14, 5, "F_SP124", 0, 3, 0x05D0, 10, 0x15, -135884.859f, 8000.0f, 80633.7578f), // Gerudo Desert Portal
                new(15, 6, "F_SP114", 1, 7, 0x06D0, 8, 0x15, -102424.125f, 6780.0f, -20670.6328f) // Custom - SPR Portal
            };
        List<byte> portalDataBytes = new();

        portalData.mCount = (byte)rawData.Count;

        portalDataBytes.AddRange(Tools.Converter.GcBytes((UInt32)portalData.version));
        int fileSize = (0xC + (portalData.mCount * 0x1C));
        portalDataBytes.AddRange(Tools.Converter.GcBytes((UInt32)fileSize));
        portalDataBytes.Add(Tools.Converter.GcByte(portalData.mCount));
        portalDataBytes.Add(Tools.Converter.GcByte(0)); // Padding
        portalDataBytes.Add(Tools.Converter.GcByte(0)); // Padding
        portalDataBytes.Add(Tools.Converter.GcByte(0)); // Padding
        foreach (dMenu_Fmap_portal_data_c.data portalEntry in rawData)
        {
            portalDataBytes.Add(Tools.Converter.GcByte(portalEntry.mSelectWarpPt));
            portalDataBytes.Add(Tools.Converter.GcByte(portalEntry.mRegionNo));
            portalDataBytes.AddRange(Tools.Converter.StringBytes(portalEntry.mStageName, 8));
            portalDataBytes.Add(Tools.Converter.GcByte(portalEntry.mRoomNo));
            portalDataBytes.Add(Tools.Converter.GcByte(portalEntry.mWarpPlayerNo));
            portalDataBytes.AddRange(Tools.Converter.GcBytes((UInt16)portalEntry.mMessageID));
            portalDataBytes.Add(Tools.Converter.GcByte(portalEntry.mStageNo));
            portalDataBytes.Add(Tools.Converter.GcByte(portalEntry.mSwitchNo));
            portalDataBytes.AddRange(Tools.Converter.GcBytes((float)portalEntry.xPosition));
            portalDataBytes.AddRange(Tools.Converter.GcBytes((float)portalEntry.yPosition));
            portalDataBytes.AddRange(Tools.Converter.GcBytes((float)portalEntry.zPosition));
        }

        string portalDirectory = @"extractedISO/root/res/FieldMap/Field0";
        string archiveDirectory = RARCDump.DumpArchive(
            Yaz0dec.InitYaz0Decode(portalDirectory + @".arc")
        );

        string portalFileDir = Path.Combine(archiveDirectory, @"dat/portal.dat");

        File.WriteAllBytes(portalFileDir, portalDataBytes.ToArray());

        RARCPacker.PackArchive(portalDirectory, portalDirectory + ".arc", "archive", true, true);

        return;
    }
}
