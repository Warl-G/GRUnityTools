using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public class LocalizationAssetBundleLoader : LocalizationLoader
    {
        public string BundleRootPath;
        public string ManifestPath;
        /// <summary>
        /// 公共语言资源Bundle名称
        /// </summary>
        public string CommonBundlePath;

        public string CommonAssetsPrefix;
        
        /// <summary>
        /// 是否卸载上一个本地化 AssetBundle
        /// Unload last loaded localization assetbundle
        /// </summary>
        public bool UnloadLastLocalizationBundle = true;

        private string _loadingName;
        private AssetBundleCreateRequest _bundleCreateRequest;
        private AssetBundleCreateRequest _commonBundleRequest;
        private AssetBundle _assetBundle;
        private AssetBundle _commonBundle;

        public LocalizationAssetBundleLoader(string manifestPath = "LocalizationManifest", string bundleRootPath = "Localizations", string commonBundlePath = "Common", string commonAssetsPrefix = "Common/")
        {
            BundleRootPath = bundleRootPath;
            ManifestPath = manifestPath;
            CommonBundlePath = commonBundlePath;
            CommonAssetsPrefix = commonAssetsPrefix;
            LocalizationManager.LocalizationChangeEvent += OnLocalizationChangeEvent;
        }

        private void OnLocalizationChangeEvent(LocalizationInfo localizationinfo)
        {
            ReloadAssetBundle(localizationinfo);
        }

        private void ReloadAssetBundle(LocalizationInfo localizationinfo)
        {
            if (_assetBundle == null || _assetBundle.name != localizationinfo.AssetsPath)
            {
                if (_assetBundle != null && UnloadLastLocalizationBundle)
                {
                    _assetBundle.Unload(true);
                    _assetBundle = null;
                }
                if (localizationinfo.AssetsPath != _loadingName)
                {
                    _loadingName = localizationinfo.AssetsPath;
                    _bundleCreateRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, BundleRootPath, localizationinfo.AssetsPath));
                    _bundleCreateRequest.completed += BundleCreateRequestOncompleted;
                }
            }
        }

        private void BundleCreateRequestOncompleted(AsyncOperation obj)
        {
            _loadingName = null;
            if (_bundleCreateRequest.assetBundle && LocalizationManager.Singleton.CurrentLocalizationInfo.AssetsPath == _bundleCreateRequest.assetBundle.name )
            {
                _assetBundle = _bundleCreateRequest.assetBundle;
                _bundleCreateRequest = null;
            }
            else
            {
                _bundleCreateRequest.assetBundle.Unload(true);
            }
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
                        Resources.UnloadAsset(request.asset);
                    }
                };
            }
        }

        public override void LoadLocalizationTextAsset(LocalizationInfo info, Action<Object> completed)
        {
            LoadLocalizationBundle(info.AssetsPath, info.TextAssetPath, completed);
        }

        public override void LoadAssetAsync<TAsset>(LocalizationInfo info, string assetName, Action<TAsset> completed)
        {
            if (!string.IsNullOrEmpty(assetName) && completed != null)
            {
                if (assetName.StartsWith(CommonAssetsPrefix))
                {
                    LoadCommonBundle(assetName.Replace(CommonAssetsPrefix, ""), completed);
                }
                else
                {
                    LoadLocalizationBundle(info.AssetsPath, assetName, completed);
                }
            }
        }

        private void LoadLocalizationBundle<TAsset>(string assetsPath, string assetName, Action<TAsset> complete) where TAsset : Object
        {
            ReloadAssetBundle(LocalizationManager.Singleton.CurrentLocalizationInfo);
            if (_assetBundle == null)
            {
                var request = _bundleCreateRequest;
                request.completed += operation =>
                {
                    if (request.assetBundle && request.assetBundle.name == LocalizationManager.Singleton.CurrentLocalizationInfo.AssetsPath)
                    {
                        LoadAsset();
                    }
                };
            }
            else
            {
                LoadAsset();
            }
            
            void LoadAsset()
            {
                if (_assetBundle != null)
                {
                    var request = _assetBundle.LoadAssetAsync<TAsset>(assetName);
                    request.completed += asyncOperation => { complete(request.asset as TAsset); };
                }
            }
        }

        private void LoadCommonBundle<TAsset>(string assetPath, Action<TAsset> complete) where TAsset : Object
        {
            void LoadAsset()
            {
                if (_commonBundle != null)
                {
                    var request = _commonBundle.LoadAssetAsync<TAsset>(assetPath);
                    request.completed += asyncOperation => { complete(request.asset as TAsset); };
                }
            }

            if (_commonBundle == null)
            {
                if (_commonBundleRequest == null)
                {
                    _commonBundleRequest =
                        AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, BundleRootPath,
                            CommonBundlePath));
                }
                
                _commonBundleRequest.completed += operation =>
                {
                    if (_commonBundle == null && _commonBundleRequest != null)
                    {
                        _commonBundle = _commonBundleRequest.assetBundle;
                        _commonBundleRequest = null;
                    }
                    LoadAsset();
                };
            }
            else
            {
                LoadAsset();
            }
        }

#if UNITY_EDITOR
        public static void BuildLocalizationAssets(string localizationFilePath, BuildTarget target)
        {
            string[] paths = Directory.GetDirectories(localizationFilePath);
            AssetBundleBuild[] buildMap = new AssetBundleBuild[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                string bundleName = Path.GetFileName(paths[i]);
                buildMap[i].assetBundleName = bundleName;
                var files = Directory.GetFiles(paths[i], "*", SearchOption.AllDirectories);

                List<string> assets = new List<string>();
                foreach (var file in files)
                {
                    if (!file.EndsWith(".meta"))
                    {
                        var filePath = file.Replace(Application.dataPath, "Assets");
                        assets.Add(filePath);
                    }
                }

                buildMap[i].assetNames = assets.ToArray();
            }

            if (paths.Length > 0)
            {
                var parent = Path.GetFileName(localizationFilePath);
                
                if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
                {
                    AssetDatabase.CreateFolder("Assets", "StreamingAssets");
                }

                if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/" + parent))
                {
                    AssetDatabase.CreateFolder("Assets/StreamingAssets", parent);
                }
                
                BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/" + parent, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, target);
            }
        }
        
        [MenuItem("Assets/GRTools/Localization/Build AssetBundles")]
        public static void BuildABsMenu()
        {
            string localizationAssetsPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            if (!Directory.Exists(localizationAssetsPath))
            {
                EditorUtility.DisplayDialog("Invalid path", localizationAssetsPath, "OK");
            }
            else
            {
                bool yes = EditorUtility.DisplayDialog("Build Localization AssetBundle", localizationAssetsPath, "Build", "Cancel"); 
                if (yes)
                {
                    BuildLocalizationAssets(localizationAssetsPath, EditorUserBuildSettings.activeBuildTarget);
                }
            }
        }
#endif
    }
}