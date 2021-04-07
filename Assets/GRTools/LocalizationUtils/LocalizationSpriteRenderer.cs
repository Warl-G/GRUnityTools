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
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (spriteRenderer != null)
            {
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
                            spriteRenderer.sprite = sprite;
                        }
                    });
            }
        }
    }
}