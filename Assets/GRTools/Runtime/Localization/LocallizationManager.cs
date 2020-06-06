using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GRTools.Localization
{
    public struct LocalizationFile
    {
        public int Index { get; }
        public SystemLanguage Type { get; }
        public string Name  { get; }
        public string FileName  { get; }

        public LocalizationFile(SystemLanguage type = SystemLanguage.Unknown)
        {
            Index = -1;
            Index = -1;
            Type = type;
            Name = type.ToString();
            FileName = null;
        }
        public LocalizationFile(string fileName = null)
        {
            Index = -1;
            Type = SystemLanguage.Unknown;
            Name = "Unknown";
            FileName = null;
            
            if (!string.IsNullOrEmpty(fileName))
            {
                Name = fileName;
                FileName = fileName;
                
                string[] lan = fileName.Split('.');
                bool success;
                if (lan.Length > 1)
                {
                    int index;
                    success = Int32.TryParse(lan[0], out index);
                    Index = success ? index : -1;
                    Name = lan[1];
                }
                
                SystemLanguage lanuageType;
                success = Enum.TryParse(Name, true, out lanuageType);
                Type = success ? lanuageType : SystemLanguage.Unknown;
            }
        }
    }
    
    //现语言配置文件从Resource/Localization目录下获取文件
    /****语言配置txt文件名参考枚举类型 SystemLanguage 并附带序号用于展示排序***
     0.English.txt
     1.ChineseSimplified.txt
     2.ChineseTraditional.txt
     */
    //
    /****语言配置txt文件参考格式***
    TestKey1=TextValue1
    TestKey2=TextValue2
    TestKey3=TextValue3
    ****语言配置txt文件参考格式***/

    public class LocalizationManager
    {
        private const string KLocalizeKey = "GR_Localization_Current_Language";
        public delegate void LocalizationChangeHandler(LocalizationFile localizationFile);
        
        /// <summary>
        /// 更改语言事件，初始化时会调用一次
        /// </summary>
        public static event LocalizationChangeHandler LocalizationChangeEvent;
        public static LocalizationManager Singleton => _singleton;
        
        /// <summary>
        /// 本地化文件列表
        /// </summary>
        public LocalizationFile[] FileList { private set; get; }

        /// <summary>
        /// 多语言资源加载器
        /// </summary>
        public ILocalizationLoader loader;

        /// <summary>
        /// 多语言文本文件解析器
        /// </summary>
        public ILocalizationParser parser;
        
        /// <summary>
        /// 是否正在读取多语言文本文件
        /// </summary>
        public bool IsLoading { private set; get; }

        /// <summary>
        /// 警告缺失键值
        /// </summary>
        public bool WarnMissedValue = false;
        
        /// <summary>
        /// 当前语言类型（SystemLanguage.Chinese 会转换为 SystemLanguage.ChineseSimplified 类型）
        /// </summary>
        public SystemLanguage CurrentLanguageType => CurrentLocalizationFile.Type;

        public int CurrentLanguageIndex => IndexOfLanguage(CurrentLanguageType);

        /// <summary>
        /// 系统语言类型（SystemLanguage.Chinese 会转换为 SystemLanguage.ChineseSimplified 类型）
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
        /// 当前多语言文件信息
        /// </summary>
        public LocalizationFile CurrentLocalizationFile
        {
            get => _currentFile;
            private set
            {
                _currentFile = value;
                PlayerPrefs.SetInt(KLocalizeKey, (int) value.Type);
                PlayerPrefs.Save();
            }
        }

        private static LocalizationManager _singleton;
        
        private bool _followSystem = true;

        private SystemLanguage _defaultLanguage = SystemLanguage.English;

        private LocalizationFile _currentFile;

        private Dictionary<string, string> _localDict;

        /// <summary>
        /// 初始化单例
        /// </summary>
        /// <param name="followSystem">无选择语言时，是否跟随系统语言</param>
        /// <param name="defaultLanguage">无选择语言，且不跟随系统时默认语言</param>
        /// <param name="filesPath">多语言文件目录，默认 'Localization'</param>
        /// <param name="fileType">多语言文件类型，默认 CSV</param>
        public static void Init(ILocalizationLoader fileLoader, ILocalizationParser fileParser, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            if (_singleton == null)
            {
                _singleton = new LocalizationManager(fileLoader, fileParser, followSystem, defaultLanguage);
            }
        }
        
        public static void Init(ILocalizationLoader fileLoader, LocalizationFileType fileType = LocalizationFileType.Csv, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            if (_singleton == null)
            {
                ILocalizationParser parser = new LocalizationDefaultParser(fileType);
                _singleton = new LocalizationManager(fileLoader, parser, followSystem, defaultLanguage);
            }
        }
        public static void Init(LocalizationFileType fileType = LocalizationFileType.Csv, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            if (_singleton == null)
            {
                ILocalizationParser parser = new LocalizationDefaultParser(fileType);
                _singleton = new LocalizationManager(null, parser, followSystem, defaultLanguage);
            }
        }
        
        public static void Init(bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            if (_singleton == null)
            {
                _singleton = new LocalizationManager(null, null, followSystem, defaultLanguage);
            }
        }

        private LocalizationManager(ILocalizationLoader fileLoader, ILocalizationParser fileParser, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            _followSystem = followSystem;
            _defaultLanguage = defaultLanguage;
            loader = fileLoader;
            
            if (fileLoader == null)
            {
                loader = new LocalizationAssetBundleLoader();
            }

            if (fileParser == null)
            {
                parser = new LocalizationDefaultParser();
            }

            //获取语言列表
            LoadAllLocalizationFilesData(null, fileParser);
        }

        /// <summary>
        /// 获取目标路径下语言文件列表，可修改加载器和解析器
        /// </summary>
        /// <param name="fileLoader"></param>
        /// <param name="fileParser"></param>
        public void LoadAllLocalizationFilesData(ILocalizationLoader fileLoader = null, ILocalizationParser fileParser = null)
        {
            if (fileLoader != null)
            {
                loader = fileLoader;
            }
            
            if (fileParser != null)
            {
                parser = fileParser;
            }
            
            SystemLanguage savedLanguageType = (SystemLanguage)PlayerPrefs.GetInt(KLocalizeKey, _followSystem ? (int)SystemLanguageType : (int)_defaultLanguage);
            
            _currentFile = new LocalizationFile(savedLanguageType);
            FileList = new LocalizationFile[0];
            
            loader.LoadAllFileListAsync(files =>
            {
                FileList = files;

                if (files.Length > 0)
                {
                    int defaultIndex = -1;
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (_currentFile.FileName == null && files[i].Type == savedLanguageType)
                        {
                            _currentFile = files[i];
                        }

                        if (defaultIndex == -1 && files[i].Type == _defaultLanguage)
                        {
                            defaultIndex = i;
                        }
                    }
                    //若无选中语言则依据系统语言，若无系统语言则默认第一个
                    if (_currentFile.FileName == null)
                    {
                        if (defaultIndex > -1)
                        {
                            _currentFile = files[defaultIndex];
                        }
                        else
                        {
                            _currentFile = FileList[0];
                        }
                    }

                    LoadLocalizationDict(_currentFile.FileName, null);
                }
            });
        }

        /// <summary>
        /// 加载并解析语言文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="complete">加载成功回调</param>
        private void LoadLocalizationDict(string fileName, Action<bool> success)
        {
            IsLoading = true;
            loader.LoadAssetAsync<TextAsset>(fileName,fileName, false,textAsset =>
            {
                if (textAsset == null)
                {
                    Debug.LogError("Localization: no localizefile " + fileName);
                    loader.LoadAssetAsync<TextAsset>(FileList[0].FileName, FileList[0].FileName, false,defaultTextAsset =>
                    {
                        if (defaultTextAsset != null)
                        {
                            Parse(defaultTextAsset.text);
                        }
                        else if (success != null)
                        {
                            success(false);
                        }
                    });
                }
                else
                {
                    Parse(textAsset.text);
                }
            });

            void Parse(string text)
            {
                Dictionary<string, string> dict = parser.Parse(text);
                if (dict != null)
                {
                    if (_localDict != null)
                    {
                        _localDict.Clear();
                        _localDict = null;
                    }
                    _localDict = dict;
                    
                    if (success != null)
                    {
                        success(true);
                    }
                    
                    if (LocalizationChangeEvent != null)
                    {
                        LocalizationChangeEvent(CurrentLocalizationFile);
                    }
                    
                }
                else
                {
                    success?.Invoke(false);
                }

                IsLoading = false;
            }
        }

        /// <summary>
        /// 语言类型在语言表的index
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        private int IndexOfLanguage(SystemLanguage language)
        {
            for (int i = 0; i < FileList.Length; i++)
            {
                if (FileList[i].Type == language)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 清除语言选择记录
        /// </summary>
        public void ClearLanguageSelection()
        {
            PlayerPrefs.DeleteKey(KLocalizeKey);
            PlayerPrefs.Save();
        }


        /// <summary>
        /// 根据 LocalizationFileList index 更换语言
        /// </summary>
        /// <param name="index"></param>
        /// <param name="success">切换成功回调</param>
        public void ChangeToLanguage(int index, Action<bool> success)
        {
            if (FileList.Length > 0 && FileList.Length > index && index >= 0)
            {
                CurrentLocalizationFile = FileList[index];
                LoadLocalizationDict(CurrentLocalizationFile.FileName, success);
            }
            else if (success != null)
            {
                success(true);
            }
        }

        /// <summary>
        /// 根据语言类型更换语言
        /// </summary>
        /// <param name="language"></param>
        /// <param name="success">切换成功回调</param>
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
        /// </summary>
        /// <param name="key">本地化键</param>
        /// <returns></returns>
        public string GetLocalizedText(string key)
        {
            if (_localDict == null || !_localDict.ContainsKey(key))
            {
                if (WarnMissedValue)
                {
                    Debug.LogWarning("Localization: Miss localized text key: " + key);
                }

                return null;
            }

            return _localDict[key];
        }

        public void LoadLocalizationAssetAsync<T>(string assetPath, string defaultAssetPath, Action<T> callback) where T : UnityEngine.Object
        {
            loader.LoadAssetAsync<T>(CurrentLocalizationFile.FileName, assetPath, false,asset =>
            {
                if (asset == null)
                {
                    if (WarnMissedValue)
                    {
                        Debug.LogWarning("Localization: Miss asset '" + assetPath + "'");
                    }

                    if (!string.IsNullOrEmpty(defaultAssetPath))
                    {
                        loader.LoadAssetAsync<T>(CurrentLocalizationFile.FileName, defaultAssetPath, true,defaultAsset =>
                        {
                            if (WarnMissedValue && defaultAsset == null)
                            {
                                Debug.LogWarning("Localization: Miss asset '" + defaultAssetPath + "'");
                            }
                            if (callback != null)
                            {
                                callback(defaultAsset);
                            }
                        });
                    }
                    else if (callback != null)
                    {
                        callback(asset);
                    }
                }
                else if (callback != null)
                {
                    callback(asset);
                }
            });
        }
    }
}