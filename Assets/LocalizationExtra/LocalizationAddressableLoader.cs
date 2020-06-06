using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GRTools.Localization
{
    public class LocalizationAddressableLoader : LocalizationLoader
    {
        public override void LoadAllFileListAsync(Action<LocalizationFile[]> complete)
        {
            if (complete != null)
            {
                Addressables.LoadResourceLocationsAsync(KLocalizeFolder).Completed += handle =>
                {
                    List<LocalizationFile> fileList = new List<LocalizationFile>();
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        foreach (var file in handle.Result)
                        {
                            LocalizationFile data = new LocalizationFile(file.PrimaryKey);
                            fileList.Add(data);
                        }
                    }
                    complete(fileList.ToArray());
                };
            }
        }

        public override void LoadAssetAsync<T>(string localizationFileName, string assetPath, bool defaultAsset, Action<T> loadComplete)
        {
            if (!string.IsNullOrEmpty(assetPath) && loadComplete != null)
            {
                Addressables.LoadAssetAsync<T>(assetPath).Completed += handle =>
                {
                    loadComplete(handle.Result);
                };
            }
        }
    }
}