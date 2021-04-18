using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public class LocalizationResourcesLoader : LocalizationLoader
    {
        public string ManifestPath;

        public LocalizationResourcesLoader(string manifestPath = "LocalizationManifest")
        {
            ManifestPath = manifestPath;
        }

        public override void LoadManifestAsync(Action<bool, LocalizationInfo[]> completed)
        {
            if (completed != null)
            {
                var request = Resources.LoadAsync<LocalizationManifest>(ManifestPath);
                request.completed += operation =>
                {
                    if (operation.isDone)
                    {
                        LocalizationInfo[] infoList = (request.asset as LocalizationManifest)?.InfoList;
                        if (infoList != null)
                        {
                            LocalizationInfo[] newInfoList = new LocalizationInfo[infoList.Length];
                            for (int i = 0; i < infoList.Length; i++)
                            {
                                newInfoList[i] = infoList[i];
                            }
                            completed(true, newInfoList);
                        }
                        else
                        {
                            completed(false, new LocalizationInfo[0]);
                        }
                        Resources.UnloadAsset(request.asset);
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
                var request = Resources.LoadAsync<TAsset>(Path.Combine(assetName));
                request.completed += operation =>
                {
                    if (operation.isDone)
                    {
                        completed(request.asset as TAsset);
                    }
                };
            }
        }
    }
}
