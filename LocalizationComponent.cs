using System;
using UnityEngine;
using UnityEngine.UI;

namespace GRTools.Localization
{
    [Serializable]
    public class LocalizationComponentItem
    {
        [Tooltip("本地化控件")] [SerializeField] internal Component component;
        [Tooltip("本地化键")] public string localizationKey;
        [Tooltip("本地化默认值（文本，图片名称）")] public string defaultValue;
        [Tooltip("是否设置图片原始尺寸")] public bool setNativeSize;
        internal Vector2 originalImageSize = Vector2.zero;
    }
    
    public class LocalizationComponent : MonoBehaviour
    {
        public LocalizationComponentItem[] items; 
        
        private SystemLanguage _currentLanguage;

        private void Start()
        {
            if (LocalizationManager.Singleton != null)
            {
                OnLocalizationChanged(LocalizationManager.Singleton.CurrentLocalizationFile);
            }
            LocalizationManager.LocalizationChangeEvent += OnLocalizationChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.LocalizationChangeEvent -= OnLocalizationChanged;
        }

        private void OnLocalizationChanged(LocalizationFile localizationFile)
        {
            _currentLanguage = localizationFile.Type;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.localizationKey) && item.component != null)
                {
                    string value = LocalizationManager.Singleton.GetLocalizedText(item.localizationKey);
                    if (value == null)
                    {
                        value = item.defaultValue;
                    }

                    if (item.component is Text text)
                    {
                        text.text = value;
                    }
                    else if (item.component is TextMesh mesh)
                    {
                        mesh.text = value;
                    }
                    else
                    {
                        Image image = item.component as Image;
                        SpriteRenderer spriteRenderer = item.component as SpriteRenderer;

                        if (image != null)
                        {
                            if (item.originalImageSize == Vector2.zero)
                            {
                                item.originalImageSize = image.rectTransform.sizeDelta;
                            }
                        }

                        if (image != null || spriteRenderer != null)
                        {
                            //替换图片
                            LocalizationManager.Singleton.LoadLocalizationAssetAsync(value, item.defaultValue,
                                delegate(Sprite sprite)
                                {
                                    if (sprite != null)
                                    {
                                        if (image != null)
                                        {
                                            image.sprite = sprite;
                                            if (item.setNativeSize)
                                            {
                                                image.SetNativeSize();
                                            }
                                            else
                                            {
                                                image.rectTransform.sizeDelta = item.originalImageSize;
                                            }
                                        }

                                        if (spriteRenderer != null)
                                        {
                                            spriteRenderer.sprite = sprite;
                                        }
                                    }
                                });
                        }
                    }
                }
            }
        }
    }
}