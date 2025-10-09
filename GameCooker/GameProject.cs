using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    public class GameProject
    {
        public void Initialize(ProjectConfig config)
        {
            if (string.IsNullOrEmpty(config.ProjectFolderRoot) || !Directory.Exists(config.ProjectFolderRoot))
            {
                Console.WriteLine("Wrong root path");
                return;
            }

            InitializeProjectDirectories(config.ProjectFolderRoot);
        }

        private void InitializeProjectDirectories(string projectRoot)
        {
            Paths.ProjectRootFolder = projectRoot;
            foreach (var dir in new string[]
            {
                Paths.GetAssetsFolderPath(),
                Paths.GetLibraryFolderPath(),
                Paths.GetProjectSettingsFolder(),
                Paths.GetAssetDatabaseFolder(),
                Paths.GetBuildTempFolderPath(),
            })
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}