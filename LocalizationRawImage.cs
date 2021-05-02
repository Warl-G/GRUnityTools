using System;
using UnityEngine;
using UnityEngine.UI;

namespace GRTools.Localization
{
    public class LocalizationRawImage : MonoBehaviour
    {
        [Tooltip("本地化键")] public string localizationKey;
        [Tooltip("本地化默认图片加载路径")] public string defaultValue;
        [Tooltip("是否设置图片原始尺寸")] public bool setNativeSize;
        
        [SerializeField] private RawImage image;
        
        private Vector2 _originalImageSize = Vector2.zero;
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
            if (image == null)
            {
                image = GetComponent<RawImage>();
            }
            string value = LocalizationManager.Singleton.GetLocalizedText(localizationKey, defaultValue);
            
            if (image != null && !string.IsNullOrEmpty(value))
            {
                if (_originalImageSize == Vector2.zero)
                {
                    _originalImageSize = image.rectTransform.sizeDelta;
                }

                LocalizationManager.Singleton.LoadLocalizationAssetAsync<Texture>(value, texture =>
                {
                    if (texture == null && !string.IsNullOrEmpty(defaultValue) && value != defaultValue)
                    {
                        LocalizationManager.Singleton.LoadLocalizationAssetAsync<Texture>(defaultValue,TextureLoaded);
                    }
                    else
                    {
                        TextureLoaded(texture);
                    }
                });
                
                void TextureLoaded(Texture texture)
                {
                    image.texture = texture;
                    if (setNativeSize)
                    {
                        image.SetNativeSize();
                    }
                    else
                    {
                        image.rectTransform.sizeDelta = _originalImageSize;
                    }
                }
            }
        }
    }
}