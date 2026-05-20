using System.Text.Json;
using System.Text.Json.Nodes;

public class BMG
{
    public BMG(List<BMGDataBlock> blocks)
    {
        dataBlocks = blocks;
    }

    List<BMGDataBlock> dataBlocks { get; set; }
}

// Note: While, yes technically the sections could be generalized, there's only two additional params for the FLW and FLI sections, so it's not worth dealing with at the moment imo. Perhaps as time progresses, things will change.
public class BMGDataBlock
{
    public BMGDataBlock(
        string id,
        string index,
        string attributes,
        List<string> text,
        string section,
        string data
    )
    {
        ID = id;
        Index = index;
        Attributes = attributes;
        Text = text;
        Section = section;
        Data = data;
    }

    public string ID { get; set; }
    public string Index { get; set; }
    public string Attributes { get; set; }
    public List<string> Text { get; set; }
    public string Section { get; set; }
    public string Data { get; set; }
}

public class BMGPatch
{
    public string FilePath { get; set; }
    public List<BMGChanges> Changes { get; set; }
}

public class BMGChanges
{
    public string Operation { get; set; }
    public string Index { get; set; }
    public string Section { get; set; }
    public JsonElement ChangeData { get; set; }
}
