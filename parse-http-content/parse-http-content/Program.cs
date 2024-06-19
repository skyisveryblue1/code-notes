using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parse_http_content
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "sample_http_stream.bin";
            string savePath = "D:\\";

            try
            {
                // Read the file content as byte array
                byte[] fileContent = ReadFileAsByteArray(filePath);
                if (fileContent == null)
                    return;

                int offset = 0;
                while (true)
                {
                    // Get the file name
                    string fileName = getBetweenString(fileContent, ref offset, "Content-Disposition: form-data; name=\"", "filename=\"", 256);
                    if (offset == -1)
                        break;

                    if (fileName == "")
                    {
                        offset = offset + 20;
                        continue;
                    }

                    fileName = fileName.Substring(0, fileName.Length - 3);

                    // Get the content type
                    string mimeType = getBetweenString(fileContent, ref offset, "Content-Type: ", "\r\n", 256);
                    if (mimeType == "")
                    {
                        offset = offset + 10;
                        continue;
                    }

                    // Get the content length
                    string fileLength = getBetweenString(fileContent, ref offset, "Content-Length: ", "\r\n", 256);
                    if (fileLength == "")
                    {
                        offset = offset + 10;
                        continue;
                    }

                    int nFileLength = Int32.Parse(fileLength);

                    // Find the start position of image data
                    int start = FindStringInByteArray(fileContent, offset, "\r\n\r\n");

                    // Extract the file
                    byte[] imageData = GetSubArray(fileContent, start + 4, nFileLength);
                    File.WriteAllBytes(savePath + fileName + GetExtensionFromMime(mimeType), imageData);

                    offset = start + 4 + nFileLength;

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        public static string getBetweenString(byte[] byteArray, ref int offset, string startMark, string endMark, int limitDistance)
        {
            int start = FindStringInByteArray(byteArray, offset, startMark);
            if (start == -1)
            {
                offset = -1;
                return "";
            }

            int startLen = Encoding.UTF8.GetBytes(startMark).Length;
            int end = FindStringInByteArray(byteArray, start + startLen, endMark);
            if (end == -1)
            {
                offset = -1;
                return "";
            }
                        
            int len = end - start - startLen;
            if (len > limitDistance)
            {
                offset = start + startLen;
                return "";
            }

            offset = end + Encoding.UTF8.GetBytes(endMark).Length;
            byte[] results = GetSubArray(byteArray, start + startLen, end - start - startLen);
            return Encoding.UTF8.GetString(results);
        }

        public static byte[] ReadFileAsByteArray(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"The file '{filePath}' does not exist.");
                return null;
            }

            return File.ReadAllBytes(filePath);
        }

        public static int FindStringInByteArray(byte[] byteArray, int offset, string searchString)
        {
            // Convert the search string to a byte array
            byte[] searchBytes = Encoding.UTF8.GetBytes(searchString);

            // Get the length of the byte arrays
            int byteArrayLength = byteArray.Length;
            int searchBytesLength = searchBytes.Length;

            // Loop through the byte array to find the position of the search string
            for (int i = offset; i <= byteArrayLength - searchBytesLength; i++)
            {
                bool found = true;
                for (int j = 0; j < searchBytesLength; j++)
                {
                    if (byteArray[i + j] != searchBytes[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    return i; // Return the starting position of the found string
                }
            }

            return -1; // Return -1 if the string is not found
        }

        public static byte[] GetSubArray(byte[] data, int index, int length)
        {
            // Validate the parameters
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (index < 0 || index >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (length < 0 || index + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            // Create a new byte array to hold the subarray
            byte[] result = new byte[length];

            // Copy the specified range of bytes from the original array to the new array
            Array.Copy(data, index, result, 0, length);

            return result;
        }

        private static readonly Dictionary<string, string> MimeToExtensionMap = new Dictionary<string, string>
        {
            { "image/jpeg", ".jpg" },
            { "image/png", ".png" },
            { "image/gif", ".gif" },
            { "image/bmp", ".bmp" },
            { "image/tiff", ".tiff" },
            // Add more MIME types and their corresponding extensions as needed
        };

        public static string GetExtensionFromMime(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentNullException(nameof(mimeType));
            }

            if (MimeToExtensionMap.TryGetValue(mimeType.ToLower(), out string extension))
            {
                return extension;
            }

            return string.Empty; // Return empty string if extension is not found
        }
    }
}
