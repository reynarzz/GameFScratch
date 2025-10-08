using System.IO;
using K4os.Compression.LZ4.Streams;

namespace GameCooker
{

    public static class AssetCompressor
    {
        public static void CompressFile(string inputFile, string outputFile)
        {
            using var input = File.OpenRead(inputFile);
            using var output = File.Create(outputFile);
            CompressStreamInternal(input, output);
        }

        public static void DecompressFile(string inputFile, string outputFile)
        {
            using var input = File.OpenRead(inputFile);
            using var output = File.Create(outputFile);
            DecompressStreamInternal(input, output);
        }

        public static MemoryStream CompressStream(Stream input)
        {
            var output = new MemoryStream();
            CompressStreamInternal(input, output);
            output.Position = 0;
            return output;
        }

        public static MemoryStream DecompressStream(Stream input)
        {
            var output = new MemoryStream();
            DecompressStreamInternal(input, output);
            output.Position = 0;
            return output;
        }

        public static byte[] CompressBytes(byte[] input)
        {
            using var inputStream = new MemoryStream(input);
            using var compressedStream = CompressStream(inputStream);
            return compressedStream.ToArray();
        }

        public static byte[] DecompressBytes(byte[] input)
        {
            using var inputStream = new MemoryStream(input);
            using var decompressedStream = DecompressStream(inputStream);
            return decompressedStream.ToArray();
        }


        private static void CompressStreamInternal(Stream input, Stream output)
        {
            using var lz4 = LZ4Stream.Encode(output, leaveOpen: true);
            input.CopyTo(lz4);
        }

        private static void DecompressStreamInternal(Stream input, Stream output)
        {
            using var lz4 = LZ4Stream.Decode(input, leaveOpen: true);
            lz4.CopyTo(output);
        }

    }
}
