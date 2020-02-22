## LocalizeUtil

本地化工具，仅有 LocalizeUtil.cs 一个代码文件，其中包含2个类，LocalizationManager 和 LocalizeUtil，目录结构需遵守规范名称与层级

### 资源规范

语言配置文件txt需 与 UnityEngine.SystemLanguage 枚举值名称一致，并带序号

如```0.English.txt```   ```1.ChineseSimplified.txt```  

其中0序号语言会作为无目标语言情况下的默认语言

txt内容格式，换行分隔，若文本内容包括换行则使用转义字符 \n  

```
TextKey1=TextValue1
TextKey2=TextValue2 \n New line Text
TextKey3=TextValue3
```



#### 目录结构

*  Resources  
  * Localization  
    * 0.English.txt
    * 1.ChineseSimplified.txt
    * 2.......txt
    * Images
      * English
        * Image0.png
        * Image1.png
        * Image2.png
        * ...
      * ChineseSimplified
        * Image0.png
        * Image1.png
        * Image2.png  
        * ...

### LocalizationManager

* localizationChangeEvent  

  本地化语言更改事件   

* warnMissedText

  是否在内容缺失时警告，默认false，随时开关  

* SystemLanguage  

  系统语言，由 Application.systemLanguage 转换成string  

* CurrentLanguage  

  当前语言名，名称依据 UnityEngine.SystemLanguage 枚举值名称  

* LanguageList  

  语言资源列表

* Init(bool followSystem = true, bool warnMissedValue = false)  

  初始化最早时机调用（Awake），默认开启跟随系统语言（followSystem），不警告缺失文本或图片（warnMissedValue）  

* ChangeToLanguage  

  根据 LanguageList 顺序更改语言  

* GetLocalizedText  

  由键值获取语言  

* GetLocalizedImageByNameAsync  

  协程，根据传入的图片名异步返回加载的本地化图片

### LocalizeUtil  

用于挂载 Text、Image 与 SpriteRenderer 的脚本工具，负责自动本地化内容更新

* LocalizationType  

  GVLocalizationType 枚举值，用于刷新时区分上述控件类型，

* localizationKey  

  本地化键，用于获取本地化文本或作为图片名获取本地化图片

* defaultValue  

  默认文本（图片名），当使用 localizationKey 获取的内容为空时，则使用 defaultValue直接展示  

* setNativeSize  

  是否使用图片原始尺寸

