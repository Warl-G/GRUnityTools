using System;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public class LocalizationAddressablesLoader : LocalizationLoader
    {
        public string ManifestAddress;
        public LocalizationAddressablesLoader(string manifestAddress = "LocalizationManifest")
        {
            ManifestAddress = manifestAddress;
        }
        
        public override void LoadManifestAsync(Action<bool ,LocalizationInfo[]> completed)
        {
            if (completed != null)
            {
                Addressables.LoadAssetAsync<LocalizationManifest>(ManifestAddress).Completed += handle =>
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
                            completed(true, newInfoList);
                        }
                    }
                    else
                    {
                        completed(false, new LocalizationInfo[0]);
                    }
                };
            }
        }

        public override void LoadLocalizationTextAsset(LocalizationInfo info, Action<Object> completed)
        {
            LoadAssetAsync(info, info.TextAssetPath, completed);
        }

        public override void LoadAssetAsync<TAsset>(LocalizationInfo info, string assetName, Action<TAsset> completed)
        {
            if (!string.IsNullOrEmpty(assetName) && completed != null)
            {
                Addressables.LoadAssetAsync<TAsset>(Path.Combine(assetName)).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        completed(handle.Result);
                    }
                    else
                    {
                        completed(null);
                    }
                };
            }
        }
    }
}