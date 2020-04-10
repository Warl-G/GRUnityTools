using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GRTools.Localization
{
    public class LocalizationParser
    {
        public enum LocalizationFileType
        {
            Txt,
            Csv,
            Asset
        }

        public Dictionary<string, string> ParseFile(string filePath, LocalizationFileType type = LocalizationFileType.Txt)
        {
            if (type == LocalizationFileType.Txt)
            {
                return ParseTxtFile(filePath);
            }

            return null;
        }
        
        private Dictionary<string, string> ParseTxtFile(string filePath)
        {
            TextAsset asset = Resources.Load<TextAsset>(filePath);
            if (asset == null)
            {
                Debug.LogError("no localizefile " + filePath);
            }
            else
            {
                Dictionary<string, string> dict = ParseTxt(asset.text);
                Resources.UnloadAsset(asset);
                return dict;
            }

            return null;
        }
    
        public Dictionary<string, string> ParseTxt(string txt)
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

    }
}

