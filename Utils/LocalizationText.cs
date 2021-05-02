using UnityEngine;
using UnityEngine.UI;

namespace GRTools.Localization
{
    public class LocalizationText : MonoBehaviour
    {
        [Tooltip("本地化键")] public string localizationKey;
        [Tooltip("本地化默认文本")] public string defaultValue;
        
        [SerializeField] private Text text;

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
            if (text == null)
            {
                text = GetComponent<Text>();
            }
            if (text != null)
            {
                string value = LocalizationManager.Singleton.GetLocalizedText(localizationKey, defaultValue);
                text.text = value;
            }
        }
    }
}

