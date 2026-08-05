using System.Text.Json;
using System.Text.Json.Nodes;

public class DZX
{
    public DZX(List<DZXDataBlock> blocks)
    {
        dataBlocks = blocks;
    }

    List<DZXDataBlock> dataBlocks { get; set; }

    public static readonly Dictionary<string, Type> TagTypes =
        new()
        {
            ["SCLS"] = typeof(SCLS),
            ["FILI"] = typeof(FILI),
            ["PLYR"] = typeof(PLYR),
            ["Doo"] = typeof(Door), // Matches Door, Doo0, Doo1, Doo2, etc.
            ["ACT"] = typeof(ACTR), // Matches ACTR, ACT0, ACT1, etc.
            ["SCO"] = typeof(SCOB), // Matches SCOB, SCO0, SCO1, etc.
            ["LGT"] = typeof(LGT0), // Matches LGT0, LGT1, LGT2, etc.
            ["TRES"] = typeof(TRES), // Matches TRES, TRE0, TRE1, etc.
            // add as many as needed
        };

    public static Type GetEntryType(string tag)
    {
        // Try exact match first
        if (TagTypes.TryGetValue(tag, out Type? type))
            return type;

        // Fall back to prefix match
        var prefix = TagTypes.Keys.FirstOrDefault(k => tag.StartsWith(k));
        return prefix != null ? TagTypes[prefix] : null;
    }
}

public class DZXDataBlock
{
    public DZXDataBlock(string tag, List<JsonElement> entries)
    {
        Tag = tag;
        Entries = entries;
    }

    public string Tag { get; set; }
    public List<JsonElement> Entries { get; set; }
}

public class SCLS
{
    public string? Stage { get; set; }
    public int Start { get; set; }
    public int Room { get; set; }
    public int field_0xa { get; set; }
    public int field_0xb { get; set; }
    public int Wipe { get; set; }
}

public class FILI
{
    public uint Parameters { get; set; }
    public int Sea_Level { get; set; }
    public int field_0x8 { get; set; }
    public int field_0xc { get; set; }
    public int[]? field_0x10 { get; set; }
    public int Default_Camera { get; set; }
    public int BitSw { get; set; }
    public int Msg { get; set; }
}

public class PLYR
{
    public string? Name { get; set; }
    public byte param_0 { get; set; }
    public byte param_1 { get; set; }
    public byte param_2 { get; set; }
    public byte param_3 { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public short Angle_X { get; set; }
    public short Angle_Y { get; set; }
    public short Spawn_ID { get; set; }
    public int EnemyNo { get; set; }
}

public class Door
{
    public string? Name { get; set; }
    public uint param { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public short Angle_X { get; set; }
    public short Angle_Y { get; set; }
    public short Angle_Z { get; set; }
    public int EnemyNo { get; set; }
    public int Scale_X { get; set; }
    public int Scale_Y { get; set; }
    public int Scale_Z { get; set; }
    public int field_0x23 { get; set; }
}

public class SCOB
{
    public string? Name { get; set; }
    public uint param { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public short Angle_X { get; set; }
    public short Angle_Y { get; set; }
    public short Angle_Z { get; set; }
    public int EnemyNo { get; set; }
    public int Scale_X { get; set; }
    public int Scale_Y { get; set; }
    public int Scale_Z { get; set; }
    public int field_0x23 { get; set; }
}

public class ACTR
{
    public string? Name { get; set; }
    public byte param_0 { get; set; }
    public byte param_1 { get; set; }
    public byte param_2 { get; set; }
    public byte param_3 { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public short Angle_X { get; set; }
    public short Angle_Y { get; set; }
    public short Angle_Z { get; set; }
    public int EnemyNo { get; set; }
}

public class LGT0
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public float Radius { get; set; }
    public float Direction_X { get; set; }
    public float Direction_Y { get; set; }
    public float Spotlight_Cutoff { get; set; }
    public int field_0x1c { get; set; }
    public int field_0x1d { get; set; }
    public int field_0x1e { get; set; }
    public int field_0x1f { get; set; }
}

public class TRES
{
    public string Name { get; set; }
    public int field_0x8 { get; set; }
    public int Type_Flag { get; set; }
    public int field_0xa { get; set; }
    public int Appear_Type { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public short Room_No { get; set; }
    public short Rotation { get; set; }
    public int Item { get; set; }
    public int Flag_ID { get; set; }
    public int field_0x1e { get; set; }
    public int field_0x1f { get; set; }
}

public class DZXPatch
{
    public string? FilePath { get; set; }
    public List<DZXChange>? Changes { get; set; }
}

public class DZXChange
{
    public string? Operation { get; set; }
    public string? DataBlock { get; set; }
    public string? ID { get; set; }
    public JsonElement Data { get; set; }
}

public static class PatchFunctions
{
    public static T ApplyChanges<T>(T target, JsonElement changes)
    {
        // Serialize the target to a JsonObject so we can mutate it
        var node = JsonSerializer.SerializeToNode(target)?.AsObject();

        // Overwrite matching properties from changes
        foreach (var prop in changes.EnumerateObject())
        {
            if (!node.ContainsKey(prop.Name))
                throw new InvalidOperationException(
                    $"Property '{prop.Name}' does not exist on target type '{typeof(T).Name}'."
                );
            node[prop.Name] = prop.Value.Deserialize<JsonNode>();
        }

        return node.Deserialize<T>();
    }

    public static string AfterLast(string s, char c)
    {
        int i = s.LastIndexOf(c);
        return i >= 0 ? s[(i + 1)..] : s;
    }

    public static string GetEntryID(JsonElement e) =>
        $"{e.GetProperty("Name").GetString()}@"
        + $"{(int)e.GetProperty("x").GetSingle()},"
        + $"{(int)e.GetProperty("y").GetSingle()},"
        + $"{(int)e.GetProperty("z").GetSingle()}";

    public static string GetEntryParamID(JsonElement e) =>
        $"{e.GetProperty("Name").GetString()}@"
        + $"{(int)e.GetProperty("x").GetSingle()},"
        + $"{(int)e.GetProperty("y").GetSingle()},"
        + $"{(int)e.GetProperty("z").GetSingle()},"
        + $"{e.GetProperty("param_0").GetUInt32() + e.GetProperty("param_1").GetUInt32() + e.GetProperty("param_2").GetUInt32() + e.GetProperty("param_3").GetUInt32()}";
}
