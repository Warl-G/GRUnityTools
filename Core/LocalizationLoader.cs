using System;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public interface ILocalizationLoader
    { 
        /// <summary>
        /// 加载支持语言清单
        /// Load manifest of supported languages
        /// </summary>
        /// <param name="completed"></param>
        void LoadManifestAsync(Action<bool, LocalizationInfo[]> completed);

        /// <summary>
        /// 加载语言文本文件
        /// Load language asset of localizationFile
        /// </summary>
        /// <param name="info"></param>
        /// <param name="completed"></param>
        void LoadLocalizationTextAsset(LocalizationInfo info, Action<Object> completed);

        /// <summary>
        /// 加载多语言资源，包括但不限于 TextAsset/Sprite/Texture 的 UnityEngine.Object 对象
        /// Local assets for localization, UnityEngine.Object 
        /// </summary>
        /// <param name="info">LocalizationFile type instance</param>
        /// <param name="assetName"></param>
        /// <param name="completed">加载完成回调 completed callback</param>
        /// <typeparam name="TAsset"></typeparam>
        void LoadAssetAsync<TAsset>(LocalizationInfo info, string assetName, Action<TAsset> completed) where TAsset : Object;
    }

    public abstract class LocalizationLoader :  ILocalizationLoader
    {
        public abstract void LoadManifestAsync(Action<bool, LocalizationInfo[]> completed);

        public abstract void LoadLocalizationTextAsset(LocalizationInfo info,
            Action<Object> completed);

        public abstract void LoadAssetAsync<TAsset>(LocalizationInfo info, string assetName,
            Action<TAsset> completed) where TAsset : Object;
    }
}