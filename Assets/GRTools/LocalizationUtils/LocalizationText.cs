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
            if (text == null)
            {
                text = GetComponent<Text>();
            }
            if (text != null)
            {
                string value = LocalizationManager.Singleton.GetLocalizedText(localizationKey);
                if (value == null)
                {
                    value = defaultValue;
                }

                text.text = value;
            }
        }
    }
}

