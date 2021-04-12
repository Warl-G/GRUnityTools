using System;
using UnityEngine;

namespace GRTools.Localization
{
    [CreateAssetMenu(fileName = "LocalizationManifest", menuName = "GRTools/Localization/Create Manifest Asset")]
    [Serializable]
    public class LocalizationManifest : ScriptableObject
    {
        // public LocalizationManifest(LocalizationInfo[] infoList)
        // {
        //     InfoList = infoList;
        // }
        //
        public LocalizationInfo[] InfoList; //{ get; }
    }
}
