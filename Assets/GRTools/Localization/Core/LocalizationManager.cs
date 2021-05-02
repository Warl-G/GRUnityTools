using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    [Serializable]
    public class LocalizationInfo
    {
        public SystemLanguage LanguageType;
        public string TextAssetPath;
        public string AssetsPath;
        
        public LocalizationInfo(SystemLanguage languageType = SystemLanguage.Unknown, string textAssetPath = null, string assetsPath = null)
        {
            LanguageType = languageType;
            TextAssetPath = textAssetPath ?? languageType.ToString();
            AssetsPath = assetsPath ?? "";
        }
    }

    public partial class LocalizationManager
    {
        private const string KLocalizeKey = "GR_Localization_Current_Language";
        public delegate void LocalizationChangeHandler(LocalizationInfo localizationInfo);
        
        /// <summary>
        /// 更改语言事件，初始化时会调用一次
        /// Language changed event, will be called when Init()
        /// </summary>
        public static event LocalizationChangeHandler LocalizationChangeEvent;
        public static LocalizationManager Singleton => _singleton;
        
        /// <summary>
        /// 本地化信息列表
        /// Localization info list
        /// </summary>
        public LocalizationInfo[] InfoList { private set; get; }

        /// <summary>
        /// 多语言资源加载器
        /// Localization asset loader
        /// </summary>
        public ILocalizationLoader Loader;

        /// <summary>
        /// 多语言文本文件解析器
        /// Localization text parser
        /// </summary>
        public ILocalizationParser Parser;
        
        /// <summary>
        /// 是否正在读取多语言文本文件
        /// If is loading text asset
        /// </summary>
        public bool IsLoading { private set; get; }

        /// <summary>
        /// 警告缺失键值
        /// Warn invalid key
        /// </summary>
        public bool WarnMissedValue = false;
        
        /// <summary>
        /// 当前语言类型（SystemLanguage.Chinese 会转换为 SystemLanguage.ChineseSimplified 类型）
        /// Current language type, SystemLanguage.Chinese will be considered as SystemLanguage.ChineseSimplified
        /// </summary>
        public SystemLanguage CurrentLanguageType => CurrentLocalizationInfo.LanguageType;

        public int CurrentLanguageIndex => IndexOfLanguage(CurrentLanguageType);

        /// <summary>
        /// 系统语言类型（SystemLanguage.Chinese 会转换为 SystemLanguage.ChineseSimplified 类型）
        /// System language type, SystemLanguage.Chinese will be considered as SystemLanguage.ChineseSimplified
        /// </summary>
        public SystemLanguage SystemLanguageType
        {
            get
            {
                SystemLanguage lan = Application.systemLanguage;
                if (Application.systemLanguage == SystemLanguage.Chinese)
                {
                    lan = SystemLanguage.ChineseSimplified;
                }

                return lan;
            }
        }

        /// <summary>
        /// 当前多语言配置信息
        /// Current localization info
        /// </summary>
        public LocalizationInfo CurrentLocalizationInfo
        {
            get => _currentInfo;
            private set
            {
                _currentInfo = value;
                PlayerPrefs.SetInt(KLocalizeKey, (int) value.LanguageType);
                PlayerPrefs.Save();
            }
        }

        private static LocalizationManager _singleton;
        
        private readonly bool _followSystem;

        private readonly SystemLanguage _defaultLanguage;

        private LocalizationInfo _currentInfo;

        private Dictionary<string, string> _localDict;
        
        public static void Init(ILocalizationLoader assetLoader, ILocalizationParser assetParser, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            if (_singleton == null)
            {
                _singleton = new LocalizationManager(assetLoader, assetParser, followSystem, defaultLanguage);
            }
        }

        private LocalizationManager(ILocalizationLoader assetLoader, ILocalizationParser assetParser, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            _followSystem = followSystem;
            _defaultLanguage = defaultLanguage;

            //获取语言列表
            RefreshInfoList(assetLoader, assetParser, null);
        }

        /// <summary>
        /// 获取目标路径下语言文件列表，可修改加载器和解析器
        /// </summary>
        /// <param name="assetLoader"></param>
        /// <param name="assetParser"></param>
        /// <param name="completed"></param>
        public void RefreshInfoList(ILocalizationLoader assetLoader = null,
            ILocalizationParser assetParser = null, Action<bool> completed = null)
        {
            if (assetLoader != null)
            {
                Loader = assetLoader;
            }
            
            if (assetParser != null)
            {
                Parser = assetParser;
            }
            
            SystemLanguage savedLanguageType = (SystemLanguage)PlayerPrefs.GetInt(KLocalizeKey, _followSystem ? (int)SystemLanguageType : (int)_defaultLanguage);

            Loader.LoadManifestAsync((success, infoList) =>
            {
                if (success)
                {
                    InfoList = infoList;
                    _currentInfo = null;

                    if (infoList.Length > 0)
                    {
                        int defaultIndex = -1;
                        for (int i = 0; i < infoList.Length; i++)
                        {
                            if (_currentInfo == null && infoList[i].LanguageType == savedLanguageType)
                            {
                                _currentInfo = infoList[i];
                            }

                            if (defaultIndex == -1 && infoList[i].LanguageType == _defaultLanguage)
                            {
                                defaultIndex = i;
                            }
                        }
                        //若无选中语言则依据系统语言，若无系统语言则默认第一个
                        if (_currentInfo == null)
                        {
                            if (defaultIndex > -1)
                            {
                                _currentInfo = infoList[defaultIndex];
                            }
                            else
                            {
                                _currentInfo = InfoList[0];
                            }
                        }
                        LoadLocalizationDict(_currentInfo, null);
                    }
                }
                completed?.Invoke(success);
            });
        }

        /// <summary>
        /// 加载并解析语言文件
        /// Load and parse localization text file
        /// </summary>
        /// <param name="info">
        /// 本地化信息
        /// Localizationinfo
        /// </param>
        /// <param name="completed">
        /// 加载成功回调
        /// success callback
        /// </param>
        private void LoadLocalizationDict(LocalizationInfo info, Action<bool> completed)
        {
            IsLoading = true;
            Loader.LoadLocalizationTextAsset(info, asset =>
            {
                if (asset == null)
                {
                    Debug.LogError("Localization: no localization text file " + info.TextAssetPath);
                    Loader.LoadLocalizationTextAsset(InfoList[0], defaultAsset =>
                    {
                        if (defaultAsset != null)
                        {
                            Parse(defaultAsset);
                        }
                        else
                        {
                            completed?.Invoke(false);
                        }
                    });
                }
                else
                {
                    Parse(asset);
                }
            });

            void Parse(Object asset)
            {
                Dictionary<string, string> dict = Parser.Parse(asset);
                if (dict != null)
                {
                    if (_localDict != null)
                    {
                        _localDict.Clear();
                        _localDict = null;
                    }
                    _localDict = dict;

                    completed?.Invoke(true);
                    LocalizationChangeEvent?.Invoke(CurrentLocalizationInfo);
                }
                else
                {
                    completed?.Invoke(false);
                }
                IsLoading = false;
            }
        }

        /// <summary>
        /// 语言类型在语言表的index
        /// Index of SystemLanguage in languageManifest
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        private int IndexOfLanguage(SystemLanguage language)
        {
            for (int i = 0; i < InfoList.Length; i++)
            {
                if (InfoList[i].LanguageType == language)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 清除语言选择记录
        /// Clear language selection
        /// </summary>
        public void ClearLanguageSelection()
        {
            PlayerPrefs.DeleteKey(KLocalizeKey);
            PlayerPrefs.Save();
        }


        /// <summary>
        /// 根据 LocalizationManifest index 更换语言
        /// Change language in LocalizationManifest
        /// </summary>
        /// <param name="index">语言序号 Index of language in LanguageManifest</param>
        /// <param name="success">成功回调 callback</param>
        public void ChangeToLanguage(int index, Action<bool> success)
        {
            if (InfoList.Length > 0 && InfoList.Length > index && index >= 0)
            {
                CurrentLocalizationInfo = InfoList[index];
                LoadLocalizationDict(CurrentLocalizationInfo, success);
            }
            else
            {
                success?.Invoke(false);
            }
        }

        /// <summary>
        /// 根据语言类型更换语言
        /// Change to language
        /// </summary>
        /// <param name="language"></param>
        /// <param name="success">成功回调 callback</param>
        public void ChangeToLanguage(SystemLanguage language, Action<bool> success)
        {
            if (language == SystemLanguage.Chinese)
            {
                language = SystemLanguage.ChineseSimplified;
            }
                
            int index = IndexOfLanguage(language); 
            ChangeToLanguage(index, success);
        }

        /// <summary>
        /// 通过键获取本地化文本
        /// Get localized text by key
        /// </summary>
        /// <param name="key">
        /// 本地化键
        /// Key for localized text
        /// </param>
        /// <param name="defaultText">
        /// 默认值
        /// Default localized text value
        /// </param>
        /// <returns></returns>
        public string GetLocalizedText(string key, string defaultText = "")
        {
            if (_localDict == null || !_localDict.ContainsKey(key))
            {
                if (WarnMissedValue)
                {
                    Debug.LogWarning("Localization: Localized text key is invalid: " + key);
                }

                return defaultText;
            }

            return _localDict[key];
        }
    }
}