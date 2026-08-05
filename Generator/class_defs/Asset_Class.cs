using System.Text.Json;
using System.Text.Json.Nodes;

public class AssetPatch
{
    public string? Directory { get; set; }
    public List<AssetFiles>? Files { get; set; }
}

public class AssetFiles
{
    public string? FileName { get; set; }
    public string? Operation { get; set; }
    public string? Subdirectory { get; set; }
}
