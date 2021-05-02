
using System;
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
            _loader = new LocalizationAssetBundleLoader("LocalManifestsForAB/TxtLocalizationManifest");
        }
        else if (loaderType == LoaderType.Addressable)
        {
            _loader = new LocalizationAddressablesLoader("Localizations/TxtLocalizationManifest.asset");
        }
        
        LocalizationManager.LocalizationChangeEvent += OnLocalizationChanged;
        LocalizationManager.Init(_loader, LocalizationTextType.Txt);
    }

    private void OnLocalizationChanged(LocalizationInfo localizationInfo)
    {
        index = LocalizationManager.Singleton.CurrentLanguageIndex;
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
        ((LocalizationDefaultParser) LocalizationManager.Singleton.Parser).ParseType = LocalizationTextType.Csv;
        if (_loader is LocalizationResourcesLoader resourcesLoader)
        {
            resourcesLoader.ManifestPath = Path.Combine("Localizations", "CsvLocalizationManifest");
        }
        else if (_loader is LocalizationAssetBundleLoader assetBundleLoader)
        {
            assetBundleLoader.ManifestPath = "LocalManifestsForAB/CsvLocalizationManifest";
        }
        else if (_loader is LocalizationAddressablesLoader addressablesLoader)
        {
            addressablesLoader.ManifestAddress = "Localizations/CsvLocalizationManifest.asset";
        }
        LocalizationManager.Singleton.RefreshInfoList();
    }
    
    public void ChangeToJson()
    {
        ((LocalizationDefaultParser) LocalizationManager.Singleton.Parser).ParseType = LocalizationTextType.Json;
        if (_loader is LocalizationResourcesLoader resourcesLoader)
        {
            resourcesLoader.ManifestPath = Path.Combine("Localizations", "JsonLocalizationManifest");
        }
        else if (_loader is LocalizationAssetBundleLoader assetBundleLoader)
        {
            assetBundleLoader.ManifestPath = "LocalManifestsForAB/JsonLocalizationManifest";
        }
        else if (_loader is LocalizationAddressablesLoader addressablesLoader)
        {
            addressablesLoader.ManifestAddress = "Localizations/JsonLocalizationManifest.asset";
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
