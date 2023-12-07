using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NPinyin;


namespace MetaTable
{
    public static class StringExtension
    {
        private static readonly CultureInfo CULTURE = CultureInfo.InvariantCulture;
        private const NumberStyles HEX = NumberStyles.HexNumber;

        private static readonly TextInfo TXT_INFO = CultureInfo.InvariantCulture.TextInfo;
        public static List<int> ToListInt(this string s, string split = "|")
        {
            if (string.IsNullOrEmpty(s))
            {
                return new List<int>();
            }
            return s.Split(split).Select(o => int.Parse(o)).ToList();
        }

        public static int[] ToArrInt(this string str, string split = "|")
        {
            List<int> ret = new List<int>();
            if (str.IsNullOrWhiteSpace())
            {
                return new int[0];
            }

            var strs = str.Split(split);

            for (int i = 0; i < strs.Length; i++)
            {
                if (int.TryParse(strs[i], out int value))
                {
                    ret.Add(value);
                }
            }
            return ret.ToArray();
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            if (str == null || (str != null && str.Trim() == string.Empty))
            {
                return true;
            }
            return false;
        }


        public static string Humanize(this string source)
        {
            if (source == null)
            {
                return null;
            }

#if UNITY_EDITOR
            source = UnityEditor.ObjectNames.NicifyVariableName(source);
#endif

            source = source.Replace('-', ' ').Replace('_', ' ');
            source = TXT_INFO.ToTitleCase(source);
            return source;
        }


        public static Color ToColor(this string input)
        {
            if (input.Length > 9 || input.IsNullOrWhiteSpace()) return default;


            if (input[0] == '#') input = input[1..];
            int inputLength = input.Length;

            if (inputLength != 6 && inputLength != 8) return default;
            int r = int.Parse($"{input[0]}{input[1]}", HEX, CULTURE);
            int g = int.Parse($"{input[2]}{input[3]}", HEX, CULTURE);
            int b = int.Parse($"{input[4]}{input[5]}", HEX, CULTURE);

            int a = inputLength == 8
                ? int.Parse($"{input[6]}{input[7]}", HEX, CULTURE)
                : 255;

            return new Color(
                r / 255f,
                g / 255f,
                b / 255f,
                a / 255f
            );
        }

        public static T ToEnum<T>(this string str, T defaultVal = default(T))
        {
            if (str == null)
            {
                return defaultVal;
            }
            try
            {
                return (T)Enum.Parse(typeof(T), str);
            }
            catch
            {
                return defaultVal;
            }
        }


        public static string ToPascal(this string str)
        {
            char[] chars = str.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            for (int i = 1; i < chars.Length; i++)
            {
                chars[i] = char.ToLower(chars[i]);
            }
            return new string(chars);
        }

        public static bool ContainsChinese(this string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                return false;
            }
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        public static bool ContainsChinese(this char c)
        {
            return Regex.IsMatch(c.ToString(), @"[\u4e00-\u9fa5]");
        }

        public static string ToPinyin(this string str)
        {
            Debug.Log($"在toPin中：{str}");
            if (str.IsNullOrWhiteSpace())
            {
                return str;
            }


            if (!str.ContainsChinese())
            {
                return str.Replace("-", "_");
            }

            var chars = str.ToCharArray();
            StringBuilder sb = new StringBuilder();
            foreach (var c in chars)
            {
                if (c.ContainsChinese())
                {
                    sb.Append(Pinyin.GetPinyin(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Replace("-", "_");
        }

        public static string PathReplace(this string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                return str;
            }
            return str.Replace("\\", "/");
        }


    }
}
