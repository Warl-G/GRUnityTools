using System;
using System.Collections.Generic;
using UnityEngine;

namespace GRTools.Localization
{
    public class LocalizationResourcesLoader : LocalizationLoader
    {
        public override void LoadAllFileListAsync(Action<LocalizationFile[]> complete)
        {
            if (complete != null)
            {
                TextAsset[] res = Resources.LoadAll<TextAsset>(FilesPath);
                List<LocalizationFile> fileList = new List<LocalizationFile>();

                for (int i = 0; i < res.Length; i++)
                {
                    TextAsset asset = res[i];
                    LocalizationFile data = new LocalizationFile(asset.name);
                    fileList.Add(data);
                    Resources.UnloadAsset(asset);
                }
                complete(fileList.ToArray());
            }
        }

        public override void LoadAssetAsync<T>(string localizationFileName, string assetPath, bool defaultAsset, Action<T> complete)
        {
            if (!string.IsNullOrEmpty(assetPath) && complete != null)
            {
                var request = Resources.LoadAsync<T>(FilesPath + "/" + assetPath);
                request.completed += operation =>
                {
                    complete(request.asset as T);
                };
            }
        }
    }
}
