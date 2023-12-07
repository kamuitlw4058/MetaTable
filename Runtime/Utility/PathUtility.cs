using System;
using System.IO;

namespace MetaTable
{

    public static partial class PathUtility
    {
        public static string GetDirectoryName(string path)
        {
            var ret = Path.GetDirectoryName(path);
            return ret.Replace("\\", "/");
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public static string Join(string path, string subpath)
        {
            return Path.Join(path, subpath).Replace("\\", "/");
        }


        public static string MatchParentDir(string path, string parentDir)
        {
            var filename = Path.GetFileName(path);
            if (filename.Equals(parentDir))
            {
                return Path.GetDirectoryName(path);
            }
            return null;
        }

        public static string MatchParentDirs(string path, string[] parentDirs)
        {
            if (parentDirs == null || (parentDirs != null && parentDirs.Length == 0)) return null;

            string parentDir = path;

            for (int i = 0; i < parentDirs.Length; i++)
            {
                parentDir = MatchParentDir(parentDir, parentDirs[i]);
                if (parentDir == null)
                {
                    return null;
                }
            }

            return parentDir;
        }
    }
}