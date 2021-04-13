using System;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public interface ILocalizationLoader<TLocalizationInfo> where TLocalizationInfo : LocalizationInfo
    { 
        /// <summary>
        /// 加载支持语言清单
        /// Load manifest of supported languages
        /// </summary>
        /// <param name="completed"></param>
        void LoadManifestAsync(Action<TLocalizationInfo[]> completed);

        /// <summary>
        /// 加载语言文本文件
        /// Load language asset of localizationFile
        /// </summary>
        /// <param name="info"></param>
        /// <param name="completed"></param>
        /// <typeparam name="TAsset"></typeparam>
        void LoadLocalizationTextAsset(TLocalizationInfo info, Action<Object> completed);

        /// <summary>
        /// 加载多语言资源，包括但不限于 TextAsset/Sprite/Texture 的 UnityEngine.Object 对象
        /// Local assets for localization, UnityEngine.Object 
        /// </summary>
        /// <param name="info">LocalizationFile type instance</param>
        /// <param name="assetName"></param>
        /// <param name="defaultAsset">是否为默认资源 use as default asset</param>
        /// <param name="completed">加载完成回调 completed callback</param>
        /// <typeparam name="TAsset"></typeparam>
        void LoadAssetAsync<TAsset>(TLocalizationInfo info, string assetName, bool defaultAsset, Action<TAsset> completed) where TAsset : Object;
    }

    public abstract class LocalizationLoader :  ILocalizationLoader<LocalizationInfo>
    {
        public abstract void LoadManifestAsync(Action<LocalizationInfo[]> completed);

        public abstract void LoadLocalizationTextAsset(LocalizationInfo info,
            Action<Object> completed);

        public abstract void LoadAssetAsync<TAsset>(LocalizationInfo info, string assetName, bool defaultAsset,
            Action<TAsset> completed) where TAsset : Object;
    }
}