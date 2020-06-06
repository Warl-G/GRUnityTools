
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
        AssetBudnle,
        Addressable
    }
    
    private int index = 0;

    private void Awake()
    {
        ILocalizationLoader loader = null;
        if (loaderType == LoaderType.Resources)
        {
            loader = new LocalizationResourcesLoader();
        }
        else if (loaderType == LoaderType.AssetBudnle)
        {
            loader = new LocalizationAssetBundleLoader();
        }
        else if (loaderType == LoaderType.Addressable)
        {
            loader = new LocalizationAddressableLoader();
        }
        // var loader = new LocalizationResourcesLoader();
        
        LocalizationManager.Init(loader, LocalizationFileType.Txt);
        index = LocalizationManager.Singleton.CurrentLanguageIndex;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateList();
    }

    public void ChangeLanguage()
    {
        index++;
        index = index >= LocalizationManager.Singleton.FileList.Length ? 0 : index;
        LocalizationManager.Singleton.ChangeToLanguage(index, null);
        UpdateList();
    }

    public void ChangeToCsv()
    {
        // LocalizationManager.Singleton.LoadAllLocalizationFilesData("Csv");
    }
    
    public void ChangeToJson()
    {
        // LocalizationManager.Singleton.LoadAllLocalizationFilesData("Json", LocalizationFileType.Json);
    }

    private void UpdateList()
    {
        var str = "";
        for (int i = 0; i < LocalizationManager.Singleton.FileList.Length; i++)
        {
            LocalizationFile file = LocalizationManager.Singleton.FileList[i];
            if (LocalizationManager.Singleton.CurrentLanguageType == file.Type)
            {
                str += "<color=#FF0000FF>" + file.FileName + "</color>";
            }
            else
            {
                str += file.FileName;
            }
            str += "\n";
        }

        languageList.text = str;
    }
}
