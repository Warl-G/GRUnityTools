using System.Collections.Generic;
using System.Text.RegularExpressions;
using GRTools.Utils;
using Newtonsoft.Json;

namespace GRTools.Localization
{
    public class LocalizationParser
    {
        /// <summary>
        /// 解析本地化文本
        /// </summary>
        /// <param name="text">本地化文本</param>
        /// <param name="type">本地化文件类型</param>
        /// <returns></returns>
        public static Dictionary<string, string> Parse(string text, LocalizationFileType type = LocalizationFileType.Csv)
        {
            if (type == LocalizationFileType.Csv)
            {
                return ParseCsv(text);
            }
            
            if (type == LocalizationFileType.Txt)
            {
                return ParseTxt(text);
            }

            if (type == LocalizationFileType.Json)
            {
                return ParseJson(text);
            }

            return null;
        }
    
        /// <summary>
        /// 解析本地化 txt 文本
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseTxt(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return null;
            }
            string[] lines = txt.Split('\n');
            Dictionary<string, string> localDict = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    string[] keyAndValue = line.Split(new[] {'='}, 2);
                    if (keyAndValue.Length == 2)
                    {
                        string value = Regex.Unescape(keyAndValue[1]);
                        localDict.Add(keyAndValue[0], value);
                    }
                }
            }

            return localDict;
        }

        /// <summary>
        /// 解析本地化 csv 文本
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseCsv(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return null;
            }
            Dictionary<string, string> localDict = new Dictionary<string, string>();
            var dict = CsvParser.ParseRowToDictionary(csv);
            foreach (var key in dict.Keys)
            {
                var values = dict[key];
                if (!string.IsNullOrEmpty(key) && values.Count > 0)
                {
                    localDict.Add(key, values[0].ToString());
                }
            }

            return localDict;
        }

        public static Dictionary<string, string> ParseJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            Dictionary<string, string> localDict = new Dictionary<string, string>();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            foreach (var key in dict.Keys)
            {
                string value = Regex.Unescape(dict[key].ToString());
                localDict.Add(key, value);
            }
            
            return localDict;
        }
    }
}

