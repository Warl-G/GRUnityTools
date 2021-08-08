using UnityEngine;
using TMPro;

namespace GRTools.Localization
{
    public class LocalizationTMPUI : MonoBehaviour
    {
        [Tooltip("本地化键")] public string localizationKey;
        [Tooltip("本地化默认文本")] public string defaultValue;
        
        [SerializeField] private TextMeshProUGUI text;

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

        private void OnLocalizationChanged(LocalizationInfo localizationFile)
        {
            if (text == null)
            {
                text = GetComponent<TextMeshProUGUI>();
            }
            if (text != null)
            {
                string value = LocalizationManager.Singleton.GetLocalizedText(localizationKey, defaultValue);
                text.text = value;
            }
        }
    }
}