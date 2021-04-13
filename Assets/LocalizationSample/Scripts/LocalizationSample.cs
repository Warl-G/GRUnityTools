
using System.IO;
using GRTools.Localization;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationSample : MonoBehaviour
{
    [SerializeField] private Text languageList;
    [SerializeField] private LoaderType loaderType;

    private LocalizationLoader _loader;
    public enum LoaderType
    {
        Resources,
        AssetBundle,
        Addressable
    }
    
    private int index = 0;

    private void Awake()
    {
        if (loaderType == LoaderType.Resources)
        {
            _loader = new LocalizationResourcesLoader(Path.Combine("Localizations", "TxtLocalizationManifest"));
        }
        else if (loaderType == LoaderType.AssetBundle)
        {
            _loader = new LocalizationAssetBundleLoader();
            // loader.RootPath =  Path.Combine(Application.streamingAssetsPath, "Localizations");
            // loader.ManifestPath = "TxtLocalizationManifest";
        }
        else if (loaderType == LoaderType.Addressable)
        {
            _loader = new LocalizationAddressablesLoader();
        }
        // var loader = new LocalizationResourcesLoader();
        
        LocalizationManager.LocalizationChangeEvent += OnLocalizationChanged;
        LocalizationManager.Init(_loader, LocalizationFileType.Txt);
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
        if (_loader is LocalizationResourcesLoader resourcesLoader)
        {
            resourcesLoader.ManifestPath = Path.Combine("Localizations", "CsvLocalizationManifest");
        }
        else if (_loader is LocalizationAssetBundleLoader assetBundleLoader)
        {
            
        }
        else if (_loader is LocalizationAddressablesLoader addressablesLoader)
        {
            
        }
        LocalizationManager.Singleton.RefreshInfoList();
    }
    
    public void ChangeToJson()
    {
        ((LocalizationDefaultParser) LocalizationManager.Singleton.Parser).ParseType = LocalizationFileType.Json;
        if (_loader is LocalizationResourcesLoader resourcesLoader)
        {
            resourcesLoader.ManifestPath = Path.Combine("Localizations", "JsonLocalizationManifest");
        }
        else if (_loader is LocalizationAssetBundleLoader assetBundleLoader)
        {
            
        }
        else if (_loader is LocalizationAddressablesLoader addressablesLoader)
        {
            
        }
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
