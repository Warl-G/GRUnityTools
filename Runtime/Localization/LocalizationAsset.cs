using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GRTools.Localization
{
    [Serializable]
    public struct LocalizationAssetItem
    {
        public string key;
        public string text;
        public Sprite sprite;
    }
    
    [CreateAssetMenu]
    public class LocalizationAsset : ScriptableObject
    {
        public List<LocalizationAssetItem> localization;
    }
}
