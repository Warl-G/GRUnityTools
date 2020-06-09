using System;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public interface ILocalizationLoader
    { 
        /// <summary>
        /// 加载支持语言列表
        /// </summary>
        /// <param name="complete"></param>
        void LoadAllFileListAsync(Action<LocalizationFile[]> complete);

        /// <summary>
        /// 加载多语言资源，包括但不限于 TextAsset/Sprite/Texture 的 UnityEngine.Object 对象
        /// </summary>
        /// <param name="localizationFileName">语言文件名称，如0.English</param>
        /// <param name="assetPath">资源路径或名称</param>
        /// <param name="defaultAsset">是否为默认资源</param>
        /// <param name="complete">加载完成回调</param>
        /// <typeparam name="T"></typeparam>
        void LoadAssetAsync<T>(string localizationFileName, string assetPath, bool defaultAsset, Action<T> complete) where T : Object;
    }

    public abstract class LocalizationLoader :  ILocalizationLoader
    {
        protected const string KLocalizeFolder = "Localization";

        public string FilesPath;

        public LocalizationLoader(string filesPath = KLocalizeFolder)
        {
            if (string.IsNullOrEmpty(filesPath))
            {
                filesPath = KLocalizeFolder;
            }
            else if (filesPath.EndsWith("/"))
            {
                filesPath = filesPath.Remove(filesPath.Length - 1);
            }

            FilesPath = filesPath;
        }

        public abstract void LoadAllFileListAsync(Action<LocalizationFile[]> complete);

        public abstract void LoadAssetAsync<T>(string localizationFileName, string assetPath, bool defaultAsset,
            Action<T> complete) where T : Object;
    }
}