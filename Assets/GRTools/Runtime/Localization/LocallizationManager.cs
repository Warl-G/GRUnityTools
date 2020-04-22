using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace GRTools.Localization
{
    
    public enum LocalizationFileType
    {
        Txt,
        Csv,
        Json,
        Asset
    }

    public struct LocalizationFile
    { 
        public int Index { private set; get; }
        public SystemLanguage Type { private set; get; }
        public string Name  { private set; get; }
        public string FileName  { private set; get; }

        public LocalizationFile(SystemLanguage type = SystemLanguage.Unknown)
        {
            Index = -1;
            Type = type;
            Name = type.ToString();
            FileName = null;
        }
        public LocalizationFile(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Index = -1;
                Type = SystemLanguage.Unknown;
                Name = "Unknown";
                FileName = null;
            }
            else
            {
                string[] lan = fileName.Split('.');
                Index = Int32.Parse(lan[0]);
                Name = lan[1];
                FileName = fileName;
                SystemLanguage lanuageType;
                bool success = Enum.TryParse<SystemLanguage>(Name, out lanuageType);
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
        private const string KLocalizeFolder = "Localization";
        private const string KLocalizeImagesPath = "/Sprites";
        private const string KLocalizeDefaultImagesPath = "/Sprites/_Default";
        
        public delegate void LocalizationChangeHandler(LocalizationFile localizationFile);
        
        /// <summary>
        /// 更改语言事件，初始化时会调用一次
        /// </summary>
        public static event LocalizationChangeHandler LocalizationChangeEvent;
        public static LocalizationManager Singleton => _singleton;
        
        /// <summary>
        /// 本地化文件列表
        /// </summary>
        public LocalizationFile[] LocalizationFileList { private set; get; }
        
        /// <summary>
        /// 本地化文件类型
        /// </summary>
        public LocalizationFileType FileType { private set; get; }
        
        /// <summary>
        /// 本地化文件路径
        /// </summary>
        public string LocalizationFilePath { private set; get; }
        
        /// <summary>
        /// 本地化默认图片路径，默认 'Localization/Sprites/_Default' 或 '初始化配置目录/Sprites/_Default'
        /// </summary>
        public string LocalizationDefaultImagePath;
        
        /// <summary>
        /// 本地化图片路径，默认 'Localization/Sprites' 或 '初始化配置目录/Sprites'
        /// </summary>
        public string LocalizationImagePath;

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
            get => _currentLocalizationFile;
            private set
            {
                _currentLocalizationFile = value;
                PlayerPrefs.SetInt(KLocalizeKey, (int) value.Type);
                PlayerPrefs.Save();
            }
        }

        private static LocalizationManager _singleton;
        
        private bool _followSystem = true;

        private SystemLanguage _defaultLanguage = SystemLanguage.English;

        private LocalizationFile _currentLocalizationFile;

        private Dictionary<string, string> _localDict;

        /// <summary>
        /// 初始化单例
        /// </summary>
        /// <param name="followSystem">无选择语言时，是否跟随系统语言</param>
        /// <param name="defaultLanguage">无选择语言，且不跟随系统时默认语言</param>
        /// <param name="filesPath">多语言文件目录，默认 'Localization'</param>
        /// <param name="fileType">多语言文件类型，默认 CSV</param>
        public static void Init(bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English, string filesPath = KLocalizeFolder, LocalizationFileType fileType = LocalizationFileType.Csv)
        {
            if (_singleton == null)
            {
                _singleton = new LocalizationManager(followSystem, defaultLanguage, filesPath, fileType);
            }
        }

        private LocalizationManager(bool followSystem, SystemLanguage defaultLanguage, string filesPath = KLocalizeFolder, LocalizationFileType fileType = LocalizationFileType.Csv)
        {
            _followSystem = followSystem;
            _defaultLanguage = defaultLanguage;

            if (string.IsNullOrEmpty(filesPath))
            {
                filesPath = KLocalizeFolder;
            }
            else if (filesPath.EndsWith("/"))
            {
                filesPath = filesPath.Remove(filesPath.Length - 1);
            }

            LocalizationImagePath = filesPath + KLocalizeImagesPath;

            LocalizationDefaultImagePath = filesPath + KLocalizeDefaultImagesPath;
            //获取语言列表
            LoadAllLocalizationFilesData(filesPath, fileType);
        }

        /// <summary>
        /// 获取目标路径下语言文件列表，图片资源路径不会更改
        /// </summary>
        /// <param name="filesPath">存储语言文件的路径</param>
        /// <param name="fileType">语言文件类型</param>
        public void LoadAllLocalizationFilesData(string filesPath, LocalizationFileType fileType = LocalizationFileType.Csv)
        {
            LocalizationFilePath = filesPath;
            FileType = fileType;
            SystemLanguage savedLanguageType = (SystemLanguage)PlayerPrefs.GetInt(KLocalizeKey, _followSystem ? (int)SystemLanguageType : (int)_defaultLanguage);
            
            TextAsset[] res = Resources.LoadAll<TextAsset>(LocalizationFilePath);
            
            if (res.Length > 0)
            {
                int defaultIndex = -1;
                LocalizationFile[] fileList = new LocalizationFile[res.Length];
                for (int i = 0; i < res.Length; i++)
                {
                    TextAsset asset = res[i];
                    LocalizationFile data = new LocalizationFile(asset.name);
                    fileList[i] = data;
                    if (_currentLocalizationFile.FileName == null && data.Type == savedLanguageType)
                    {
                        _currentLocalizationFile = data;
                    }
        
                    if (defaultIndex == -1 && data.Type == _defaultLanguage)
                    {
                        defaultIndex = i;
                    }
                    
                    Resources.UnloadAsset(asset);
                }
        
                LocalizationFileList = fileList;
        
                //若无选中语言则依据系统语言，若无系统语言则默认第一个
                if (_currentLocalizationFile.FileName == null)
                {
                    if (defaultIndex > -1)
                    {
                        _currentLocalizationFile = LocalizationFileList[defaultIndex];
                    }
                    else
                    {
                        _currentLocalizationFile = LocalizationFileList[0];
                    }
                }
        
                LoadLocalizationDict(_currentLocalizationFile.FileName);
                
                if (LocalizationChangeEvent != null)
                {
                    LocalizationChangeEvent(CurrentLocalizationFile);
                }
            }
            else
            {
                _currentLocalizationFile = new LocalizationFile(savedLanguageType);
                LocalizationFileList = new LocalizationFile[0];
            }
        }

        /// <summary>
        /// 加载并解析语言文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        private void LoadLocalizationDict(string fileName)
        {
            TextAsset asset = Resources.Load<TextAsset>(LocalizationFilePath + "/" + fileName);
            if (asset == null)
            {
                Debug.LogError("Localization: no localizefile " + fileName);
                asset = Resources.Load<TextAsset>(LocalizationFilePath + "/" + LocalizationFileList[0].FileName);
            }

            Dictionary<string, string> dict = LocalizationParser.Parse(asset.text, FileType);
            
            Resources.UnloadAsset(asset);

            if (_localDict != null)
            {
                _localDict.Clear();
                _localDict = null;
            }
            
            _localDict = dict;
        }

        /// <summary>
        /// 语言类型在语言表的index
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        private int IndexOfLanguage(SystemLanguage language)
        {
            for (int i = 0; i < LocalizationFileList.Length; i++)
            {
                if (LocalizationFileList[i].Type == language)
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
        /// <returns>是否替换成功</returns>
        public bool ChangeToLanguage(int index)
        {
            if (LocalizationFileList.Length > 0 && LocalizationFileList.Length > index && index >= 0)
            {
                CurrentLocalizationFile = LocalizationFileList[index];
                LoadLocalizationDict(CurrentLocalizationFile.FileName);
                if (LocalizationChangeEvent != null)
                {
                    LocalizationChangeEvent(CurrentLocalizationFile);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据语言类型更换语言
        /// </summary>
        /// <param name="language"></param>
        /// <returns>是否替换成功</returns>
        public bool ChangeToLanguage(SystemLanguage language)
        {
            if (language == SystemLanguage.Chinese)
            {
                language = SystemLanguage.ChineseSimplified;
            }
                
            int index = IndexOfLanguage(language);
            return ChangeToLanguage(index);
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

        /// <summary>
        /// 使用协程异步加载当前语言图片路径或默认路径下的图片，回调传回
        /// </summary>
        /// <param name="imageName">图片名称</param>
        /// <param name="defaultImageName">默认图片名称</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetLocalizedSpriteByNameAsync(string spriteName, string defaultSpriteName, Action<Sprite> callback)
        {
            ResourceRequest request =
                Resources.LoadAsync<Sprite>(LocalizationImagePath + "/" + CurrentLocalizationFile.Name + "/" + spriteName);
            yield return request;
            Sprite image = request.asset as Sprite;
            if (image == null)
            {
                if (WarnMissedValue)
                {
                    Debug.LogWarning("Localization: Miss localized image: " + spriteName);
                }

                request =
                    Resources.LoadAsync<Sprite>(LocalizationDefaultImagePath + "/" + defaultSpriteName);
                yield return request;
                image = request.asset as Sprite;
                
                if (WarnMissedValue)
                {
                    Debug.LogWarning("Localization: Miss localized default image: " + defaultSpriteName);
                }
            }

            if (callback != null)
            {
                callback(image);
            }
        }
    }
}