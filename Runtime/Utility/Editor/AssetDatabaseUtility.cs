using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MetaTable
{
    public static class AssetDatabaseUtility
    {
#if UNITY_EDITOR
        public static T LoadAssetAtPath<T>(string path = "Assets") where T : Object
        {
#if UNITY_5_3_OR_NEWER
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
#endif
        }


        public static IEnumerable<T> FindAsset<T>(string[] dirPath) where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", dirPath)
            .Select(x => AssetDatabase.GUIDToAssetPath(x))
            .Select(x => AssetDatabase.LoadAssetAtPath<T>(x));
        }

        public static IEnumerable<T> FindAsset<T>(string path = null) where T : Object
        {
            string[] searchFolder = null;
            if (path != null)
            {
                searchFolder = new string[] { path };
            }

            return FindAsset<T>(searchFolder);
        }

        public static T FindAssetFirst<T>(string path = null) where T : Object
        {
            var items = FindAsset<T>(path);
            Debug.Log(items.Count());
            if (items.Count() > 0)
            {
                return items.First();
            }
            return null;
        }

        /// <summary>
        /// 拼接字符串
        /// </summary>
        /// <param name="DirPath">字符串数组</param>
        /// <param name="separator">分隔符号</param>
        /// <returns></returns>
        public static string CombiningStrings(string[] DirPath, string separator)
        {
            string tmpStr = "";
            foreach (var str in DirPath)
            {
                if (tmpStr != "")
                {
                    tmpStr += separator;
                }

                if (str.Contains(","))
                {
                    tmpStr += $"\"{str}\"";
                }
                else
                {
                    tmpStr += str;
                }
            }
            return tmpStr;
        }


        public static void DeleteAsset(UnityEngine.Object Obj)
        {
            var path = AssetDatabase.GetAssetPath(Obj);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
#endif
    }
}