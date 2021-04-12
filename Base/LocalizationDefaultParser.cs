using System.Collections.Generic;
using System.Text.RegularExpressions;
using GRTools.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace GRTools.Localization
{
    /// <summary>
    /// 可解析类型
    /// </summary>
    public enum LocalizationFileType
    {
        Txt,
        Csv,
        Json
    }
    public class LocalizationDefaultParser : ILocalizationParser
    {
        public LocalizationFileType ParseType;

        public LocalizationDefaultParser(LocalizationFileType type = LocalizationFileType.Csv)
        {
            ParseType = type;
        }
        /// <summary>
        /// 解析本地化文本
        /// </summary>
        /// <param name="textAsset">本地化文本</param>
        /// <returns></returns>
        public Dictionary<string, string> Parse(Object textAsset)
        {
            TextAsset asset = textAsset as TextAsset;
            if (asset)
            {
                if (ParseType == LocalizationFileType.Csv)
                {
                    return ParseCsv(asset.text);
                }
            
                if (ParseType == LocalizationFileType.Txt)
                {
                    return ParseTxt(asset.text);
                }

                if (ParseType == LocalizationFileType.Json)
                {
                    return ParseJson(asset.text);
                }
            }

            return null;
        }
    
        /// <summary>
        /// 解析本地化 txt 文本
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public Dictionary<string, string> ParseTxt(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return null;
            }
            
            string[] lines = txt.Replace("\r\n", "\n").Split('\n');
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
        public Dictionary<string, string> ParseCsv(string csv)
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

        /// <summary>
        /// 解析本地化 json 文本
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Dictionary<string, string> ParseJson(string json)
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

