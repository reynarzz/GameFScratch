using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{

    internal class TextAssetProcessor : IAssetProcessor
    {
        byte[] IAssetProcessor.Process(string path)
        {
            using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            return Encoding.UTF8.GetBytes(reader.ReadToEnd());
        }
    }
}
