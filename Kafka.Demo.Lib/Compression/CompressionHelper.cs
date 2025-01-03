using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Kafka.Demo.Lib.Compression
{
    public static class CompressionHelper
    {
        public static string Compress(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(bytes, 0, bytes.Length);
                }

                var compressedBytes = outputStream.ToArray();
                return Convert.ToBase64String(compressedBytes);
            }
        }

        public static string Decompress(string compressedBase64)
        {
            var compressedBytes = Convert.FromBase64String(compressedBase64);
            using (var inputStream = new MemoryStream(compressedBytes))
            {
                using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(outputStream);
                        return Encoding.UTF8.GetString(outputStream.ToArray());
                    }
                }
            }
        }
    }
}