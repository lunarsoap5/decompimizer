using System.Text.Json;
using System.Text.Json.Nodes;

public class AssetPatch
{
    public string? FileName { get; set; }
    public string? Operation { get; set; }
    public string? Directory { get; set; }
}
