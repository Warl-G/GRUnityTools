using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GRTools
{
    public class LocalizeUtil : MonoBehaviour
    {
        public enum LocalizationType
        {
            Text,
            Image,
            SpriteRender
        }

        [Tooltip("本地化类型")] public LocalizationType localizationType;
        [Tooltip("本地化键")] public string localizationKey;
        [Tooltip("本地化默认值（文本，图片名称）")] public string defaultValue;
        [Tooltip("是否设置图片原始尺寸")] public bool setNativeSize;
        private string _currentLanguage;

        private Text _text;
        private Image _image;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            if (localizationKey != null)
            {
                if (localizationType == LocalizationType.Text)
                {
                    _text = GetComponent<Text>();
                    if (string.IsNullOrEmpty(defaultValue) && _text != null)
                    {
                        defaultValue = _text.text;
                    }
                }
                else if (localizationType == LocalizationType.Image)
                {
                    _image = GetComponent<Image>();
                }
                else if (localizationType == LocalizationType.SpriteRender)
                {
                    _spriteRenderer = GetComponent<SpriteRenderer>();
                }
            }
            LocalizationManager.Singleton.localizationChangeEvent += LocalizationChanged;
        }

        private void Start()
        {
            LocalizationChanged(LocalizationManager.Singleton.CurrentLanguage);
        }

        private void LocalizationChanged(string localization)
        {
            _currentLanguage = localization;
            if (localizationKey != null)
            {
                string value = LocalizationManager.Singleton.GetLocalizedText(localizationKey);
                if (value == null)
                {
                    value = defaultValue;
                }

                if (_text != null)
                {
                    _text.text = value;
                }

                //替换图片
                StartCoroutine(LocalizationManager.Singleton.GetLocalizedImageByNameAsync(localizationKey,
                    delegate(Sprite image)
                    {
                        if (image != null)
                        {
                            if (_image != null)
                            {
                                _image.sprite = image;
                                if (setNativeSize)
                                {
                                    _image.SetNativeSize();
                                }
                            }

                            if (_spriteRenderer != null)
                            {
                                _spriteRenderer.sprite = image;
                            }
                        }
                    }));
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
        private const string KLocalizeImagesFolder = "Localization/Images";

        private static LocalizationManager _singleton;

        private string _currentLanguage;
        
        private string firstLanguageFileName;

        private Dictionary<string, string> localDict;
        
        private bool followSystem = true;

        public delegate void LocalizationChangeHandler(string localization);

        public event LocalizationChangeHandler localizationChangeEvent;

        public bool warnMissedValue = false;

        public static LocalizationManager Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new LocalizationManager();
                }

                return _singleton;
            }
        }

        public string SystemLanguage
        {
            get
            {
                string lan = Application.systemLanguage.ToString();
                if (Application.systemLanguage == UnityEngine.SystemLanguage.Chinese)
                {
                    lan = UnityEngine.SystemLanguage.ChineseSimplified.ToString();
                }

                return lan;
            }
        }

        public string CurrentLanguage
        {
            private set
            {
                _currentLanguage = value;
                PlayerPrefs.SetString(KLocalizeKey, _currentLanguage);
            }
            get { return _currentLanguage; }
        }

        public string[] LanguageList { private set; get; }


        /// <summary>
        /// 为保证语言刷新逻辑 Awake 调用
        /// </summary>
        /// <param name="followSystem">无已选语言时是否跟随系统语言</param>
        /// <param name="warnMissedText">警告缺失字段</param>
        public void Init(bool followSystem = true, bool warnMissedValue = false)
        {
            Singleton.followSystem = followSystem;
            Singleton.warnMissedValue = warnMissedValue;

            _currentLanguage = PlayerPrefs.GetString(KLocalizeKey, followSystem ? SystemLanguage : "");
            string index = "0";
            bool containLanguage = false;
            //获取语言列表
            TextAsset[] res = Resources.LoadAll<TextAsset>(KLocalizeFolder);
            if (res.Length > 0)
            {
                firstLanguageFileName = res[0].name;
                LanguageList = new string[res.Length];
                for (int i = 0; i < res.Length; i++)
                {
                    string[] lan = res[i].name.Split('.');
                    string languageName = lan[1];
                    LanguageList[i] = languageName;
                    if (string.Equals(languageName, _currentLanguage))
                    {
                        containLanguage = true;
                        index = lan[0];
                    }
                }

                //若无选中语言则依据系统语言，若无系统语言则默认英语
                if (!containLanguage)
                {
                    _currentLanguage = LanguageList[0];
                    index = res[0].name.Split('.')[0];
                }

                string fileName = index + "." + CurrentLanguage;
                ParseLocalizationFile(fileName);
            }
        }

        private 
            LocalizationManager()
        {
        }

        private void ParseLocalizationFile(string fileName)
        {
            TextAsset asset = Resources.Load<TextAsset>(KLocalizeFolder + "/" + fileName);
            if (asset == null)
            {
                Debug.LogError("no localizefile " + fileName);
                asset = Resources.Load<TextAsset>(KLocalizeFolder + "/" + firstLanguageFileName);
            }

            if (asset != null)
            {
                string[] lines = asset.text.Split('\n');
                localDict = new Dictionary<string, string>();
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] keyAndValue = line.Split('=');
                        localDict.Add(keyAndValue[0], keyAndValue[1]);
                    }
                }
            }
        }

        /// <summary>
        /// 根据LanguageList index更换语言
        /// </summary>
        /// <param name="index"></param>
        public void ChangeToLanguage(int index)
        {
            if (LanguageList == null || index >= LanguageList.Length)
            {
                return;
            }

            CurrentLanguage = LanguageList[index];
            string fileName = index + "." + CurrentLanguage;
            ParseLocalizationFile(fileName);
            if (localizationChangeEvent != null)
            {
                localizationChangeEvent(CurrentLanguage);
            }
        }

        public void ChangeToLanguage(SystemLanguage language)
        {
            if (LanguageList != null && LanguageList.Length > 0)
            {
                string lan = language.ToString();
                if (language == UnityEngine.SystemLanguage.Chinese)
                {
                    lan = UnityEngine.SystemLanguage.ChineseSimplified.ToString();
                }
                int index = LanguageList.ToList().IndexOf(lan);
                ChangeToLanguage(index);
            }
        }
        
        public void ChangeToLanguage(string language)
        {
            if (LanguageList != null && LanguageList.Length > 0)
            {
                string lan = language;
                if (UnityEngine.SystemLanguage.Chinese.ToString().Equals(language))
                {
                    lan = UnityEngine.SystemLanguage.ChineseSimplified.ToString();
                }
                int index = LanguageList.ToList().IndexOf(lan); 
                ChangeToLanguage(index);
            }
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

            return localDict[key].Replace("\\n", "\n");
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
        public IEnumerator GetLocalizedImageByNameAsync(string imageName, Action<Sprite> callback)
        {
            ResourceRequest request =
                Resources.LoadAsync<Sprite>(KLocalizeImagesFolder + "/" + _currentLanguage + "/" + imageName);
            yield return request;
            Sprite image = request.asset as Sprite;
            if (image == null)
            {
                if (warnMissedValue)
                {
                    Debug.LogWarning("Miss localized Image: " + imageName);
                }
                request =
                    Resources.LoadAsync<Sprite>(KLocalizeImagesFolder + "/" + LanguageList[0] + "/" + imageName);
                yield return request;
                image = request.asset as Sprite;
            }

            if (callback != null)
            {
                callback(image);
            }
        }
    }
}