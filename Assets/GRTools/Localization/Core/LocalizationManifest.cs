using System;
using UnityEngine;

namespace GRTools.Localization
{
    [CreateAssetMenu(fileName = "LocalizationManifest", menuName = "GRTools/Localization/Create Manifest Asset")]
    [Serializable]
    public class LocalizationManifest : ScriptableObject
    {
        public LocalizationInfo[] InfoList;
    }
}
