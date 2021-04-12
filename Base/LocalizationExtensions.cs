using UnityEngine;

namespace GRTools.Localization
{
    public partial class LocalizationManager
    {
        public static void Init(bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            Init(new LocalizationResourcesLoader(), new LocalizationDefaultParser(), followSystem, defaultLanguage);
        }

        public static void Init(ILocalizationLoader<LocalizationInfo> fileLoader, LocalizationFileType fileType = LocalizationFileType.Csv, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)
        {
            Init(fileLoader, new LocalizationDefaultParser(fileType), followSystem, defaultLanguage);
        }
    }
}
