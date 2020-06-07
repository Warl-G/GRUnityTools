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
                LocalizationChanged(LocalizationManager.Singleton.CurrentLocalizationFile);
            }
            LocalizationManager.LocalizationChangeEvent += LocalizationChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.LocalizationChangeEvent -= LocalizationChanged;
        }

        private void LocalizationChanged(LocalizationFile localizationFile)
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
                        Image image = null;
                        SpriteRenderer spriteRenderer = null;

                        if (item.component is Image imageComponent)
                        {
                            image = imageComponent;
                            if (item.originalImageSize == Vector2.zero)
                            {
                                item.originalImageSize = image.rectTransform.sizeDelta;
                            }
                        }

                        if (item.component is SpriteRenderer rendererComponent)
                        {
                            spriteRenderer = rendererComponent;
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
                                            Image img = (Image) item.component;
                                            img.sprite = sprite;
                                            if (item.setNativeSize)
                                            {
                                                img.SetNativeSize();
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