public class Tools
{
    public static string GetSubstringFromMarker(string input, string marker)
    {
        int index = input.IndexOf(marker);
        return index >= 0 ? input.Substring(0, index + marker.Length) : input;
    }

    public static string GetSuperstringAfterMarker(string input, string marker)
    {
        int index = input.IndexOf(marker);
        return index >= 0 ? input.Substring(index + marker.Length) : input;
    }
}
