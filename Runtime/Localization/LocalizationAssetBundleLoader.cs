using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GRTools.Localization
{
    public class LocalizationAssetBundleLoader : LocalizationLoader
    {
        /// <summary>
        /// 公共语言资源Bundle名称
        /// </summary>
        public string CommonAssetsPath = "Common";
        
        /// <summary>
        /// 是否卸载上一个本地化 AssetBundle
        /// </summary>
        public bool UnloadLastLocalizationBundle = true;

        private AssetBundle _assetBundle;
        private AssetBundle _commonBundle;

        public override void LoadAllFileListAsync(Action<LocalizationFile[]> complete)
        {
            if (complete != null)
            {
                string bundlePath = Path.Combine(Application.streamingAssetsPath, FilesPath, FilesPath);

                var loadrequest = AssetBundle.LoadFromFileAsync(bundlePath);
                loadrequest.completed += operation =>
                {
                    AssetBundleManifest manifest =
                        loadrequest.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    var files = manifest.GetAllAssetBundles();

                    List<LocalizationFile> fileList = new List<LocalizationFile>();
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (!files[i].Equals(CommonAssetsPath.ToLower()))
                        {
                            LocalizationFile data = new LocalizationFile(files[i]);
                            fileList.Add(data);
                        }
                    }

                    loadrequest.assetBundle.Unload(manifest);
                    complete(fileList.ToArray());
                };
            }
        }

        public override void LoadAssetAsync<T>(string localizationFileName, string assetPath, bool defaultAsset,
            Action<T> complete)
        {
            if (!string.IsNullOrEmpty(localizationFileName) && !string.IsNullOrEmpty(assetPath) && complete != null)
            {
                if (defaultAsset)
                {
                    LoadCommonBundle(assetPath, complete);
                }
                else
                {
                    LoadLocalizationBundle(localizationFileName, assetPath, complete);
                }
            }
        }

        private void LoadLocalizationBundle<T>(string localizationFileName, string assetPath, Action<T> complete)
            where T : UnityEngine.Object
        {
            void LoadAsset()
            {
                if (_assetBundle != null)
                {
                    var request = _assetBundle.LoadAssetAsync<T>(assetPath);
                    request.completed += asyncOperation => { complete(request.asset as T); };
                }
            }

            AssetBundleCreateRequest loadrequest = null;
            if (_assetBundle == null)
            {
                loadrequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, FilesPath,
                    localizationFileName));
            }
            else if (!_assetBundle.name.Equals(localizationFileName))
            {
                //加载新语言包，则卸载旧语言包，可能导致部分资源缺失
                if (UnloadLastLocalizationBundle)
                {
                    _assetBundle.Unload(true);
                    _assetBundle = null;
                }
                
                loadrequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, FilesPath,
                    localizationFileName));
            }

            if (loadrequest != null)
            {
                loadrequest.completed += operation =>
                {
                    _assetBundle = loadrequest.assetBundle;
                    LoadAsset();
                };
            }
            else
            {
                LoadAsset();
            }
        }

        private void LoadCommonBundle<T>(string assetPath, Action<T> complete) where T : UnityEngine.Object
        {
            void LoadAsset()
            {
                if (_commonBundle != null)
                {
                    var request = _commonBundle.LoadAssetAsync<T>(assetPath);
                    request.completed += asyncOperation => { complete(request.asset as T); };
                }
            }

            if (_commonBundle == null)
            {
                var request =
                    AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, FilesPath,
                        CommonAssetsPath));
                request.completed += operation =>
                {
                    _commonBundle = request.assetBundle;
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
                if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
                {
                    AssetDatabase.CreateFolder("Assets", "StreamingAssets");
                }

                if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/" + KLocalizeFolder))
                {
                    AssetDatabase.CreateFolder("Assets/StreamingAssets", KLocalizeFolder);
                }

                //TODO: 区分平台
                BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/" + KLocalizeFolder, buildMap,
                    BuildAssetBundleOptions.ChunkBasedCompression, target);
            }
        }

        [MenuItem("GRTools/Localization/Build AssetBundles")]
        public static void BuildABsMenu()
        {
            string key = "LocalizationFilePath";
            string localizationFilePath =
                EditorPrefs.GetString(key, "");

            void ShowDialog()
            {
                int index = EditorUtility.DisplayDialogComplex("LocalizationFilePath", localizationFilePath, "Build",
                    "Cancel",
                    "Select Folder");
                if (index == 2)
                {
                    ShowFolderSelector();
                }
                else if (index == 0)
                {
                    BuildBundle();
                }
            }

            void ShowFolderSelector()
            {
                localizationFilePath =
                    EditorUtility.OpenFolderPanel("LocalizationFilePath", localizationFilePath, "Localization");
                ShowDialog();
            }

            ShowDialog();

            void BuildBundle()
            {
                if (!Directory.Exists(localizationFilePath))
                {
                    EditorUtility.DisplayDialog("", "Invalid path", "OK");
                    ShowDialog();
                    return;
                }

                EditorPrefs.SetString(key, localizationFilePath);

                BuildLocalizationAssets(localizationFilePath, EditorUserBuildSettings.activeBuildTarget);
            }
        }
#endif
    }
}