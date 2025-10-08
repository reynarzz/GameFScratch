using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    [Serializable]
    public struct ProjectConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProjectFolderRoot { get; set; }
    }

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
            ProjectPaths.ProjectRootFolder = projectRoot;
            foreach (var dir in new string[]
            {
                ProjectPaths.GetAssetsFolderPath(),
                ProjectPaths.GetLibraryFolderPath(),
                ProjectPaths.GetProjectSettingsFolder(),
                ProjectPaths.GetAssetDatabaseFolder(),
                ProjectPaths.GetBuildTempFolderPath(),
            })
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
