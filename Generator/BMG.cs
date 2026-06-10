using System.Text.Json;
using System.Text.Json.Nodes;

public class BMG
{
    public List<BMGDataBlock>? dataBlocks { get; set; }
}

// Note: While, yes technically the sections could be generalized, there's only two additional params for the FLW and FLI sections, so it's not worth dealing with at the moment imo. Perhaps as time progresses, things will change.
public class BMGDataBlock
{
    public string? ID { get; set; }
    public string? index { get; set; }
    public string? attributes { get; set; }
    public List<string>? text { get; set; }
    public string? Section { get; set; }
    public BMGData? Data { get; set; }
    public string[]? RawData { get; set; }
    public int Attribute_Length { get; set; }
    public string? Unknown_MID1_Value { get; set; }
}

public class BMGData
{
    public int flwTableCount { get; set; }
    public int flwIndexCount { get; set; }
    public List<string>? flwTable { get; set; }
    public List<string>? flwIndexTable { get; set; }
}

public class BMGPatch
{
    public string? FilePath { get; set; }
    public List<BMGChange>? Changes { get; set; }
}

public class BMGChange
{
    public string? Operation { get; set; }
    public string? Index { get; set; }
    public string? Section { get; set; }
    public JsonElement ChangeData { get; set; }
}
