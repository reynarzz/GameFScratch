using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    public static class ProjectPaths
    {
        public const string LIBRARY_FOLDER_NAME = "Library";
        public const string BUILD_TEMP_FOLDER_NAME = "BuildTemp";
        public const string ASSETS_FOLDER_NAME = "Assets";
        public const string PROJECT_CONFIG_FOLDER_NAME = "ProjectSettings";

        public const string ASSET_DATABASE_FOLDER_NAME = "AssetsDatabase";
        public const string ASSET_DATABASE_FILE_NAME = "AssetsDatabase.txt";
        public const string ASSET_DATABASE_BINARY_EXT_NAME = ".bin";
        public const string ASSET_META_EXT_NAME = ".meta";
        public const string ASSET_BUILD_DATA_EXT_NAME = ".gfs"; // "Game from scratch"

        private static string _projectRootFolder; // Remove path
        public static string ProjectRootFolder
        {
            get => _projectRootFolder;
            set
            {
                if (!Path.IsPathRooted(value))
                {
                    throw new ArgumentException("ProjectRootFolder must be an absolute path.");
                }

                _projectRootFolder = Path.GetFullPath(value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }
        }

        public static string GetAssetDatabaseFolder(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), LIBRARY_FOLDER_NAME, ASSET_DATABASE_FOLDER_NAME);
        }
        public static string GetProjectSettingsFolder(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), PROJECT_CONFIG_FOLDER_NAME);
        }
        public static string GetAssetDatabaseFilePath(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), LIBRARY_FOLDER_NAME, ASSET_DATABASE_FOLDER_NAME, ASSET_DATABASE_FILE_NAME);
        }

        public static string GetLibraryFolderPath(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), LIBRARY_FOLDER_NAME);
        }

        public static string GetAssetsFolderPath(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), ASSETS_FOLDER_NAME);
        }

        public static string CreateAssetDatabaseBinFilePath(string guid, bool isRelativePath = false)
        {
            return Path.Join(GetAssetDatabaseFolder(isRelativePath), guid + ASSET_DATABASE_BINARY_EXT_NAME);
        }

        public static string GetBuildTempFolderPath(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), BUILD_TEMP_FOLDER_NAME);
        }

        public static string GetRelativeAssetPath(string absoluteAssetPath)
        {
            return absoluteAssetPath.Substring(absoluteAssetPath.IndexOf(ASSETS_FOLDER_NAME) + ASSETS_FOLDER_NAME.Length + 1);
        }

        private static string GetAbsolutePathFlag(bool isRelativePath)
        {
            return isRelativePath ? null : ProjectRootFolder;
        }
    }

}
