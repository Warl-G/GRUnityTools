
using GRTools.Localization;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationSample : MonoBehaviour
{
    [SerializeField] private Text languageList;
    [SerializeField] private LoaderType loaderType;
    public enum LoaderType
    {
        Resources,
        AssetBundle,
        Addressable
    }
    
    private int index = 0;

    private void Awake()
    {
        LocalizationLoader loader = null;
        if (loaderType == LoaderType.Resources)
        {
            loader = new LocalizationResourcesLoader();
            loader.RootPath = "Localizations";
            loader.ManifestPath = "TxtLocalizationManifest";
        }
        else if (loaderType == LoaderType.AssetBundle)
        {
            loader = new LocalizationAssetBundleLoader();
        }
        else if (loaderType == LoaderType.Addressable)
        {
            loader = new LocalizationAddressablesLoader();
        }
        // var loader = new LocalizationResourcesLoader();
        
        LocalizationManager.LocalizationChangeEvent += OnLocalizationChanged;
        LocalizationManager.Init(loader, LocalizationFileType.Txt);
    }

    private void OnLocalizationChanged(LocalizationInfo localizationfile)
    {
        index = LocalizationManager.Singleton.CurrentLanguageIndex;
        UpdateList();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateList();
    }

    public void ChangeLanguage()
    {
        index++;
        index = index >= LocalizationManager.Singleton.InfoList.Length ? 0 : index;
        LocalizationManager.Singleton.ChangeToLanguage(index, null);
    }

    public void ChangeToCsv()
    {
        ((LocalizationDefaultParser) LocalizationManager.Singleton.Parser).ParseType = LocalizationFileType.Csv;
        ((LocalizationLoader) LocalizationManager.Singleton.Loader).ManifestPath = "CsvLocalizationManifest"; 
        LocalizationManager.Singleton.RefreshInfoList();
    }
    
    public void ChangeToJson()
    {
        ((LocalizationDefaultParser) LocalizationManager.Singleton.Parser).ParseType = LocalizationFileType.Json;
        ((LocalizationLoader) LocalizationManager.Singleton.Loader).ManifestPath = "JsonLocalizationManifest"; 
        LocalizationManager.Singleton.RefreshInfoList();
    }

    private void UpdateList()
    {
        var str = "";
        for (int i = 0; i < LocalizationManager.Singleton.InfoList.Length; i++)
        {
            LocalizationInfo file = LocalizationManager.Singleton.InfoList[i];
            if (LocalizationManager.Singleton.CurrentLanguageType == file.LanguageType)
            {
                str += "<color=#FF0000FF>" + file.TextAssetPath + "</color>";
            }
            else
            {
                str += file.TextAssetPath;
            }
            str += "\n";
        }

        languageList.text = str;
    }
}
