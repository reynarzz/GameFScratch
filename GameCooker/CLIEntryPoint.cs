using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    public static class CLIEntryPoint
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Asset cooker");
            Console.WriteLine("Arg 0: entry folder Path");
            Console.WriteLine("Arg 1: output folder Path");
            Console.WriteLine("Arg 2: 0 = Monolith, 1 = Separated files");
            Console.WriteLine("Ex: Path/To/OutputFolder 1");

            if (args.Length != 3)
            {
                Console.WriteLine("Error, invalid args");
                return;
            }

            new AssetsCooker().CookAll(new CookOptions()
            {
                Type = (CookingType)int.Parse(args[2]),
                AssetsFolderPath = args[0],
                ExportFolderPath = args[1],
                FileOptions = new CookFileOptions()
                {
                    CompressAllFiles = true,
                    EncryptAllFiles = true,
                    EncryptFilesPath = false
                }
            });
        }
    }
}