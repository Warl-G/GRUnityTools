using System.Collections.Generic;
using UnityEngine;

namespace GRTools.Localization
{
    public interface ILocalizationParser
    {
        /// <summary>
        /// 文本解析
        /// Parse localized data
        /// </summary>
        /// <param name="localizedAsset">localized asset</param>
        /// <returns></returns>
        Dictionary<string, string> Parse(Object localizedAsset);
    }
}

