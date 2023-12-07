using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MetaTable
{
#if UNITY_EDITOR
    public class DirectoryUtility
    {
        public static void ExistsOrCreate(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent_path = PathUtility.GetDirectoryName(path);
                var dir_name = PathUtility.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent_path))
                {
                    ExistsOrCreate(parent_path);
                }
                AssetDatabase.CreateFolder(parent_path, dir_name);
            }
        }

        public static bool CopyDirectory(string sourceFolder, string destFolder, bool IsImporterAsset = false)
        {

            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }

                //得到原文件根目录下的所有文件
                string[] files = Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    File.Copy(file, dest);//复制文件

#if UNITY_EDITOR
                    if (IsImporterAsset)
                    {
                        AssetDatabase.ImportAsset(dest);
                    }

#endif
                }

                //得到原文件根目录下的所有文件夹
                string[] folders = Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Path.Combine(destFolder, name);
                    CopyDirectory(folder, dest);//构建目标路径,递归复制文件
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
#endif
}