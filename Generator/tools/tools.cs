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

    public static void CleanUpExtractedArchive(string extractedDirectory)
    {
        // If there is only one item and it is a directory,
        // treat it as an archive wrapper folder
        string[] contents = Directory.GetDirectories(extractedDirectory);

        // Only flatten if there is exactly one directory
        if (contents.Length == 1 && Directory.GetFileSystemEntries(extractedDirectory).Length == 1)
        {
            string wrapperFolder = contents[0];

            // Temporarily rename wrapper folder to avoid name collisions
            string tempWrapper = Path.Combine(
                extractedDirectory,
                "_temp_" + Guid.NewGuid().ToString()
            );

            Directory.Move(wrapperFolder, tempWrapper);

            // Move contents up
            foreach (string item in Directory.GetFileSystemEntries(tempWrapper))
            {
                string destination = Path.Combine(extractedDirectory, Path.GetFileName(item));

                if (File.Exists(item))
                {
                    File.Move(item, destination);
                }
                else if (Directory.Exists(item))
                {
                    Directory.Move(item, destination);
                }
            }

            // Remove empty temporary wrapper
            Directory.Delete(tempWrapper);
        }
    }
}
