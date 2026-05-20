using System.Text.Json;
using System.Text.Json.Nodes;

public class DZX
{
    public DZX(List<DZXDataBlock> blocks)
    {
        dataBlocks = blocks;
    }

    List<DZXDataBlock> dataBlocks { get; set; }

    static readonly Dictionary<string, Type> TagTypes =
        new()
        {
            ["SCLS"] = typeof(SCLSEntry),
            // add as many as needed
        };
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

public class SCLSEntry
{
    public SCLSEntry(string stg, int start, int room, int fieldA, int fieldB, int mWipe)
    {
        Stage = stg;
        Start = start;
        Room = room;
        field_0xa = fieldA;
        field_0xb = fieldB;
        Wipe = mWipe;
    }

    public string Stage { get; set; }
    public int Start { get; set; }
    public int Room { get; set; }
    public int field_0xa { get; set; }
    public int field_0xb { get; set; }
    public int Wipe { get; set; }
}

public class DZXPatch
{
    public string FilePath { get; set; }
    public List<DZXChanges> Changes { get; set; }
}

public class DZXChanges
{
    public string Operation { get; set; }
    public string DataBlock { get; set; }
    public string ID { get; set; }
    public JsonElement Data { get; set; }
}

public static class PatchFunctions
{
    static T ApplyChanges<T>(T target, JsonElement changes)
    {
        // Serialize the target to a JsonObject so we can mutate it
        var node = JsonSerializer.SerializeToNode(target).AsObject();

        // Overwrite matching properties from changes
        foreach (var prop in changes.EnumerateObject())
        {
            node[prop.Name] = prop.Value.Deserialize<JsonNode>();
        }

        return node.Deserialize<T>();
    }
}
