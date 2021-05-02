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

    public class LocalizationComponentList : MonoBehaviour
    {
        public LocalizationComponentItem[] items;

        private SystemLanguage _currentLanguage;

        private void Start()
        {
            if (LocalizationManager.Singleton != null && LocalizationManager.Singleton.CurrentLocalizationInfo != null)
            {
                OnLocalizationChanged(LocalizationManager.Singleton.CurrentLocalizationInfo);
            }

            LocalizationManager.LocalizationChangeEvent += OnLocalizationChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.LocalizationChangeEvent -= OnLocalizationChanged;
        }

        private void OnLocalizationChanged(LocalizationInfo localizationInfo)
        {
            _currentLanguage = localizationInfo.LanguageType;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.localizationKey) && item.component != null)
                {
                    string value = LocalizationManager.Singleton.GetLocalizedText(item.localizationKey, item.defaultValue);
                    ChangeText(item.component, value);
                    ChangeSprite(item, value);
                }
            }
        }

        private void ChangeText(Component component, string value)
        {
            if (component is Text text)
            {
                text.text = value;
            }
            else if (component is TextMesh mesh)
            {
                mesh.text = value;
            }
        }

        private void ChangeSprite(LocalizationComponentItem item, string value)
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
                LocalizationManager.Singleton.LoadLocalizationAssetAsync<Sprite>(value, sprite =>
                {
                    if (sprite == null && !string.IsNullOrEmpty(item.defaultValue) && value != item.defaultValue)
                    {
                        LocalizationManager.Singleton.LoadLocalizationAssetAsync<Sprite>(item.defaultValue,
                            SpriteLoaded);
                    }
                    else
                    {
                        SpriteLoaded(sprite);
                    }
                });

                void SpriteLoaded(Sprite sprite)
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
            }
        }

        private void ChangeTexture(LocalizationComponentItem item, string value)
        {
            RawImage image = item.component as RawImage;

            if (image != null && !string.IsNullOrEmpty(value))
            {
                if (item.originalImageSize == Vector2.zero)
                {
                    item.originalImageSize = image.rectTransform.sizeDelta;
                }
                
                //替换图片
                LocalizationManager.Singleton.LoadLocalizationAssetAsync<Texture>(value, texture =>
                {
                    if (texture == null && !string.IsNullOrEmpty(item.defaultValue) && value != item.defaultValue)
                    {
                        LocalizationManager.Singleton.LoadLocalizationAssetAsync<Texture>(item.defaultValue,
                            TextureLoaded);
                    }
                    else
                    {
                        TextureLoaded(texture);
                    }
                });

                void TextureLoaded(Texture texture)
                {
                    if (image != null)
                    {
                        image.texture = texture;
                        if (item.setNativeSize)
                        {
                            image.SetNativeSize();
                        }
                        else
                        {
                            image.rectTransform.sizeDelta = item.originalImageSize;
                        }
                    }
                }
            }
        }
    }
}