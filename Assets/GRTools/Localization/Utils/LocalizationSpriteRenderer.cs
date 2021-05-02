using UnityEngine;
using UnityEngine.UI;

namespace GRTools.Localization
{
    public class LocalizationSpriteRender : MonoBehaviour
    {
        [Tooltip("本地化键")] public string localizationKey;
        [Tooltip("本地化默认图片加载路径")] public string defaultValue;
        
        [SerializeField] private SpriteRenderer spriteRenderer;

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
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            string value = LocalizationManager.Singleton.GetLocalizedText(localizationKey, defaultValue);
            if (spriteRenderer != null && !string.IsNullOrEmpty(value))
            {
                LocalizationManager.Singleton.LoadLocalizationAssetAsync<Sprite>(value, sprite =>
                {
                    if (sprite == null && !string.IsNullOrEmpty(defaultValue) && value != defaultValue)
                    {
                        LocalizationManager.Singleton.LoadLocalizationAssetAsync<Sprite>(defaultValue,
                            defaultSprite =>
                            {
                                spriteRenderer.sprite = defaultSprite;
                            });
                    }
                    else
                    {
                        spriteRenderer.sprite = sprite;
                    }
                });
            }
        }
    }
}