using System;
using UnityEngine;
using UnityEngine.UI;

namespace GRTools.Localization
{
    public class LocalizationImage : MonoBehaviour
    {
        [Tooltip("本地化键")] public string localizationKey;
        [Tooltip("本地化默认图片加载路径")] public string defaultValue;
        [Tooltip("是否设置图片原始尺寸")] public bool setNativeSize;
        
        [SerializeField] private Image image;
        
        private Vector2 _originalImageSize = Vector2.zero;

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
            if (image == null)
            {
                image = GetComponent<Image>();
            }
            if (image != null)
            {
                if (_originalImageSize == Vector2.zero)
                {
                    _originalImageSize = image.rectTransform.sizeDelta;
                }
                string value = LocalizationManager.Singleton.GetLocalizedText(localizationKey);
                if (value == null)
                {
                    value = defaultValue;
                }

                LocalizationManager.Singleton.LoadLocalizationAssetAsync(value, defaultValue,
                    delegate(Sprite sprite)
                    {
                        if (sprite != null)
                        {
                            image.sprite = sprite;
                            if (setNativeSize)
                            {
                                image.SetNativeSize();
                            }
                            else
                            {
                                image.rectTransform.sizeDelta = _originalImageSize;
                            }
                        }
                    });
            }
        }
    }
}