using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public static class Paths
    {
        public const string LIBRARY_FOLDER_NAME = "Library";
        public const string BUILD_TEMP_FOLDER_NAME = "BuildTemp";
        public const string ASSETS_FOLDER_NAME = "Assets";
        public const string PROJECT_CONFIG_FOLDER_NAME = "ProjectSettings";

        public const string ASSET_DATABASE_FOLDER_NAME = "AssetsDatabase";
        public const string ASSET_DATABASE_FILE_NAME = "AssetsDatabase.txt";
        public const string ASSET_DATABASE_BINARY_EXT_NAME = ".bin";
        public const string ASSET_META_EXT_NAME = ".mt";
        public const string ASSET_BUILD_DATA_EXT_NAME = ".gfs"; // "Game from scratch"
        public const string ASSET_BUILD_DATA_FILE_NAME = "data"; // "Game from scratch"

        private static string _projectRootFolder;
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

        public static string GetAssetBuildDataFilename()
        {
            return ASSET_BUILD_DATA_FILE_NAME + ASSET_BUILD_DATA_EXT_NAME;
        }


        public static string GetAssetDatabaseFolder(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), GetLibraryFolderPath(true), ASSET_DATABASE_FOLDER_NAME);
        }
        public static string GetProjectSettingsFolder(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), PROJECT_CONFIG_FOLDER_NAME);
        }
        public static string GetAssetDatabaseFilePath(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), GetAssetDatabaseFolder(true), ASSET_DATABASE_FILE_NAME);
        }

        public static string GetLibraryFolderPath(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), LIBRARY_FOLDER_NAME);
        }

        public static string GetAssetsFolderPath(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), ASSETS_FOLDER_NAME);
        }

        public static string CreateBinFilePath(string folderPath, string guid, bool isRelativePath = false)
        {
            return Path.Join(folderPath, guid + ASSET_DATABASE_BINARY_EXT_NAME);
        }

        public static string GetBuildTempFolderPath(bool isRelativePath = false)
        {
            return Path.Join(GetAbsolutePathFlag(isRelativePath), GetLibraryFolderPath(true), BUILD_TEMP_FOLDER_NAME);
        }

        public static string GetRelativeAssetPath(string absoluteAssetPath)
        {
            return absoluteAssetPath.Substring(absoluteAssetPath.IndexOf(ASSETS_FOLDER_NAME) + ASSETS_FOLDER_NAME.Length + 1);
        }

        public static string GetAbsoluteAssetPath(string relativeAssetPath)
        {
            return Path.Combine(ProjectRootFolder, ASSETS_FOLDER_NAME, relativeAssetPath);
        }

        private static string GetAbsolutePathFlag(bool isRelativePath)
        {
            return isRelativePath ? null : ProjectRootFolder;
        }
    }

}
