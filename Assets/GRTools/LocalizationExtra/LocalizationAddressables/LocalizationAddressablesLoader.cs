using System;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public class LocalizationAddressablesLoader : LocalizationLoader
    {
        public string RootPath;
        public string ManifestPath;
        public override void LoadManifestAsync(Action<LocalizationInfo[]> completed)
        {
            if (completed != null)
            {
                Addressables.LoadAssetAsync<LocalizationManifest>(Path.Combine(RootPath, ManifestPath)).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        LocalizationInfo[] infoList = handle.Result.InfoList;
                        if (infoList != null)
                        {
                            LocalizationInfo[] newInfoList = new LocalizationInfo[infoList.Length];
                            for (int i = 0; i < infoList.Length; i++)
                            {
                                newInfoList[i] = infoList[i];
                            }
                            completed(newInfoList);
                        }
                    }
                };
            }
        }

        public override void LoadLocalizationTextAsset(LocalizationInfo info, Action<Object> completed)
        {
            LoadAssetAsync(info, info.TextAssetPath, false, completed);
        }

        public override void LoadAssetAsync<TAsset>(LocalizationInfo info, string assetName, bool defaultAsset, Action<TAsset> completed)
        {
            if (!string.IsNullOrEmpty(assetName) && completed != null)
            {
                Addressables.LoadAssetAsync<TAsset>(Path.Combine(info.AssetsPath, assetName)).Completed += handle =>
                {
                    completed(handle.Result);
                };
            }
        }
    }
}