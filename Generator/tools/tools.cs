using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    internal class Converter
    {
        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte GcByte(int x)
        {
            return (byte)x;
        }

        /// <summary>
        /// Returns x as BigEndian (GC).
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a Big Endian byte. </returns>
        public static byte[] GcBytes(UInt64 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte[] GcBytes(UInt32 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte[] GcBytes(UInt16 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte[] GcBytes(Int32 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte[] GcBytes(Int16 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        public static byte[] GcBytes(float x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Get bytes from text (without null terminator).
        /// </summary>
        /// <param name="text"> The ASCII text you want to convert.</param>
        /// <param name="desiredLength"> The length of the string in bytes. If
        /// not specified, returned array will match the length of the provided
        /// text.</param>
        /// <returns>Array of Bytes processed.</returns>
        public static byte[] StringBytes(string text, int desiredLength = -1)
        {
            List<byte> textData = new();

            if (desiredLength == 0 || text == null)
            {
                return new byte[0];
            }

            if (desiredLength < 0)
            {
                desiredLength = text.Length;
            }

            if (text.Length > desiredLength)
            {
                textData.AddRange(Encoding.ASCII.GetBytes(text.Substring(0, desiredLength)));
            }
            else
            {
                textData.AddRange(Encoding.ASCII.GetBytes(text));
            }

            // Account for padding
            while (textData.Count < desiredLength)
            {
                textData.Add(0);
            }

            return textData.ToArray<byte>();
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="text">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte StringBytes(char text)
        {
            return (byte)text;
        }
    }
}
