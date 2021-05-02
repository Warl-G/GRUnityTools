using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public partial class LocalizationManager
    {
        /// <summary>
        /// 用 LocalizationResourcesLoader 和 LocalizationDefaultParser 进行初始化
        /// Init with LocalizationResourcesLoader and LocalizationDefaultParser
        /// </summary>
        /// <param name="followSystem">
        /// 是否跟随系统语言
        /// If follow system language or not
        /// </param>
        /// <param name="defaultLanguage">
        /// 默认语言
        /// default language
        /// </param>
        public static void Init(bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            Init(new LocalizationResourcesLoader(), new LocalizationDefaultParser(), followSystem, defaultLanguage);
        }

        /// <summary>
        /// 自定义加载器，默认解析器初始化
        /// Init with custom assets loader and default parser 
        /// </summary>
        /// <param name="assetLoader">
        /// 自定义本地化资源加载器
        /// custom localization assets loader
        /// </param>
        /// <param name="textType">
        /// 默认解析器支持的文本文件类型
        /// which text file type that default parser supported
        /// </param>
        /// <param name="followSystem">
        /// 是否跟随系统语言
        /// If follow system language or not
        /// </param>
        /// <param name="defaultLanguage">
        /// 默认语言
        /// default language
        /// </param>
        public static void Init(ILocalizationLoader assetLoader, LocalizationTextType textType = LocalizationTextType.Csv, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            Init(assetLoader, new LocalizationDefaultParser(textType), followSystem, defaultLanguage);
        }
        
        /// <summary>
        /// 根据当前语言信息使用加载器加载资源
        /// Get asset with current language info by loader
        /// </summary>
        /// <param name="assetPath">
        /// 资源路径
        /// Asset Path
        /// </param>
        /// <param name="callback">
        /// 加载回调
        /// Asset loaded callback
        /// </param>
        /// <typeparam name="TAsset">
        /// 资源类型
        /// Asset type
        /// </typeparam>
        public void LoadLocalizationAssetAsync<TAsset>(string assetPath, Action<TAsset> callback) where TAsset : Object
        {
            Loader.LoadAssetAsync<TAsset>(CurrentLocalizationInfo, assetPath,asset =>
            {
                if (asset == null && WarnMissedValue)
                {
                    Debug.LogWarning($"Localization: Miss asset '{assetPath}'");
                }
                callback?.Invoke(asset);
            });
        }
    }
}
