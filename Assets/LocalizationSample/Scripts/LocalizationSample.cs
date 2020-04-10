using System;
using System.Collections;
using System.Collections.Generic;
using GRTools.Localization;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationSample : MonoBehaviour
{
    [SerializeField] private Text languageList;
    
    private int index = 0;

    private void Awake()
    {
        LocalizationManager.Init(true, SystemLanguage.ChineseSimplified);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(LocalizationManager.Singleton.CurrentLocalizationFile.FileName);
        Debug.Log(LocalizationManager.Singleton.CurrentLocalizationFile.Type);
        Debug.Log(LocalizationManager.Singleton.CurrentLocalizationFile.Index);
        Debug.Log(LocalizationManager.Singleton.CurrentLocalizationFile.Name);
        Debug.Log(LocalizationManager.Singleton.CurrentLanguageType);
        UpdateList();
    }

    public void ChangeLanguage()
    {
        index++;
        index = index >= LocalizationManager.Singleton.LocalizationFileList.Length ? 0 : index;
        LocalizationManager.Singleton.ChangeToLanguage(index);
        UpdateList();

    }

    private void UpdateList()
    {
        var str = "";
        for (int i = 0; i < LocalizationManager.Singleton.LocalizationFileList.Length; i++)
        {
            LocalizationFile file = LocalizationManager.Singleton.LocalizationFileList[i];
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
