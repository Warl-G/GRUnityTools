using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GRTools.Localization
{
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
        private const string KLocalizeKey = "GR_Current_Language";
        private const string KLocalizeFolder = "Localization";
        private const string KLocalizeImagesPath = "Localization/Sprites/";
        private const string KLocalizeDefaultImagesPath = "Localization/Sprites/_Default/";
        
        public bool warnMissedValue = false;
        
        private bool followSystem = true;

        private SystemLanguage defaultLanguage = SystemLanguage.English;

        private static LocalizationManager _singleton;

        public delegate void LocalizationChangeHandler(LocalizationFile localizationFile);

        public static event LocalizationChangeHandler LocalizationChangeEvent;
        public LocalizationFile[] LocalizationFileList { private set; get; }

        private LocalizationParser _parser;

        private LocalizationFile _currentLocalizationFile;

        private Dictionary<string, string> localDict;

        public static LocalizationManager Singleton
        {
            get
            {
                return _singleton;
            }
        }

        public SystemLanguage SystemLanguage
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

        public LocalizationFile CurrentLocalizationFile
        {
            get => _currentLocalizationFile;
            set
            {
                _currentLocalizationFile = value;
                PlayerPrefs.SetInt(KLocalizeKey, (int) value.Type);
                PlayerPrefs.Save();
            }
        }

        public SystemLanguage CurrentLanguageType
        {
            get
            {
                return CurrentLocalizationFile.Type;
            }
        }
        
        /// <summary>
        /// 初始化单例
        /// </summary>
        /// <param name="defaultLanguage">无选择语言，且不跟随系统时默认语言</param>
        /// <param name="followSystem">无选择语言时，是否跟随系统语言</param>
        public static void Init(bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            if (_singleton == null)
            {
                _singleton = new LocalizationManager(followSystem, defaultLanguage);
            }
        }

        private LocalizationManager(bool followSystem, SystemLanguage defaultLanguage)
        {
            this.followSystem = followSystem;
            this.defaultLanguage = defaultLanguage;
            
            _parser = new LocalizationParser();
            //获取语言列表
            LoadAllLocalizationFilesData();
        }

        private void LoadAllLocalizationFilesData()
        {
            SystemLanguage savedLanguageType = (SystemLanguage)PlayerPrefs.GetInt(KLocalizeKey, followSystem ? (int)SystemLanguage : (int)defaultLanguage);
            
            TextAsset[] res = Resources.LoadAll<TextAsset>(KLocalizeFolder);
            if (res.Length > 0)
            {
                int defaultIndex = -1;
                LocalizationFile[] fileList = new LocalizationFile[res.Length];
                for (int i = 0; i < res.Length; i++)
                {
                    string[] lan = res[i].name.Split('.');
                    string languageName = lan[1];
                    LocalizationFile data = new LocalizationFile(res[i].name);
                    fileList[i] = data;
                    if (_currentLocalizationFile.FileName == null && data.Type == savedLanguageType)
                    {
                        _currentLocalizationFile = data;
                    }

                    if (defaultIndex == -1 && data.Type == defaultLanguage)
                    {
                        defaultIndex = i;
                    }
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

                ParseLocalizationFile(_currentLocalizationFile.FileName);
                for (int i = 0; i < res.Length; i++)
                {
                    Resources.UnloadAsset(res[i]);
                }
                if (LocalizationChangeEvent != null)
                {
                    LocalizationChangeEvent(CurrentLocalizationFile);
                }
            }
            else
            {
                _currentLocalizationFile = new LocalizationFile((SystemLanguage)savedLanguageType);
                LocalizationFileList = new LocalizationFile[0];
            }
        }

        private void ParseLocalizationFile(string fileName)
        {
            var dict = _parser.ParseFile(KLocalizeFolder + "/" + fileName);
            if (dict == null)
            {
                dict = _parser.ParseFile(KLocalizeFolder + "/" + LocalizationFileList[0].FileName);
            }

            if (localDict != null)
            {
                localDict.Clear();
                localDict = null;
            }
            
            localDict = dict;
        }

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
                ParseLocalizationFile(CurrentLocalizationFile.FileName);
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
            if (localDict == null || !localDict.ContainsKey(key))
            {
                if (warnMissedValue)
                {
                    Debug.LogWarning("Miss localized text key: " + key);
                }

                return null;
            }

            return localDict[key];
        }

        //        public IEnumerator GetLocalizedImageByKeyAsync(string localizationkey, Action<Sprite> callback)
        //        {
        //            string imageName = GetLocalizedText(localizationkey);
        //            return GetLocalizedImageByNameAsync(imageName, callback);
        //        }

        /// <summary>
        /// 使用协程异步加载图片 回调传回
        /// </summary>
        /// <param name="imageName">图片名称</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetLocalizedSpriteByNameAsync(string imageName, string defaultImageName, Action<Sprite> callback)
        {
            ResourceRequest request =
                Resources.LoadAsync<Sprite>(KLocalizeImagesPath + CurrentLocalizationFile.Name + "/" + imageName);
            yield return request;
            Sprite image = request.asset as Sprite;
            if (image == null)
            {
                if (warnMissedValue)
                {
                    Debug.LogWarning("Miss localized image: " + imageName);
                }

                request =
                    Resources.LoadAsync<Sprite>(KLocalizeDefaultImagesPath + defaultImageName);
                yield return request;
                image = request.asset as Sprite;
                
                if (warnMissedValue)
                {
                    Debug.LogWarning("Miss localized default image: " + imageName);
                }
            }

            if (callback != null)
            {
                callback(image);
            }
        }
    }
}