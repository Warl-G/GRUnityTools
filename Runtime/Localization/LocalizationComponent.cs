﻿using System;
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

                    if (item.component.GetType() == typeof(Text))
                    {
                        ((Text)item.component).text = value;
                    }
                    else
                    {
                        Image imageComponent = null;
                        SpriteRenderer spriteRenderer = null;

                        if (item.component.GetType() == typeof(Image))
                        {
                            imageComponent = (Image) item.component;
                        }

                        if (item.component is SpriteRenderer)
                        {
                            spriteRenderer = (SpriteRenderer) item.component;
                        }

                        if (imageComponent != null || spriteRenderer != null)
                        {
                            //替换图片
                            StartCoroutine(LocalizationManager.Singleton.GetLocalizedSpriteByNameAsync(value, item.defaultValue,
                                delegate(Sprite sprite)
                                {
                                    if (sprite != null)
                                    {
                                        if (imageComponent != null)
                                        {
                                            Image img = ((Image) item.component);
                                            img.sprite = sprite;
                                            if (item.setNativeSize)
                                            {
                                                img.SetNativeSize();
                                            }
                                        }

                                        if (spriteRenderer != null)
                                        {
                                            spriteRenderer.sprite = sprite;
                                        }
                                    }
                                })
                            );
                        }
                    }
                }
            }
        }
    }
}