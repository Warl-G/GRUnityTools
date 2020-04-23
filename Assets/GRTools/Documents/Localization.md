## GRTools.Localization  

version 1.1.0

本地化工具，分为 LocalizationManager 和 LocalizationComponent 以及静态工具类 LocalizationParser  

本工具依赖于 UnityEngine.SystemLanguage 枚举类型，可参考该枚举值确认语言所包含语言  

目前支持三种文件格式： txt、csv、json  

LocalizationManager 负责统一管理，加载，通知多语言状态

LocalizationComponent 负责对控件的统一修改，支持Text、Image、Text Mesh 和 SpriteRenderer 

LocalizationParser 负责解析本地化文件数据

目前仅支持图片资源通过 Resources 加载，后续会增加对 Assetbundle 等加载方式支持，或可通过自己实现代理方法自己加载替换

### 使用说明   

#### 文件路径   

本地化文件默认路径为 `Resources/Localization`

本地化图片默认路径为 `Resources/Localization/Sprite/(语言名与 SystemLanguage 一致)` 

默认图片路径  `Resources/Localization/Sprite/_Default`

例：

```
Resources/Localization/0.English.txt
Resources/Localization/1.ChineseSimplified.txt

Resources/Localization/Sprite/_Default
Resources/Localization/Sprites/English
Resources/Localization/Sprites/ChineseSimplified
```

可在 `LocalizationManager.Init` 和 `LocalizationManager.LoadAllLocalizationFilesData` 自定义路径和文件类型，但 `LoadAllLocalizationFilesData` 不会修改图片路径，需手动修改 `ImagesPath` 和 `DefaultImagesPath`

#### 文件名称  

语言配置文件名称需与 UnityEngine.SystemLanguage 枚举值名称一致，并加序号以排序

如```0.English.txt```   ```1.ChineseSimplified.txt```  

***其中 UnityEngine.SystemLanguage.Chinese 会转换为 ChineseSimplified 类型***

文件首个序号（可不为0）语言会作为无目标语言情况下的默认语言   

#### 文件格式

LocalizationManager 一次只能支持一种格式解析，需将目录下文件格式统一

若配合 LocalizationComponent 使用，文件中值可为本地化文案，也可为本地化图片路径（Resources 下路径）

##### txt 格式

txt内容格式，以每行第一个等号分隔键值对，换行分隔每段本地化内容，舍弃无等号行内容，支持转义字符，若文本内容包括换行则使用转义字符 '\n'   

```
TextKey1=TextValue1
TextKey2=TextValue2 \n New line Text  
comment(该行舍弃)
ImageKey3=English_Image0
```

##### csv 格式  

csv 文件仅适用前第一、二列数据，第一列为 key 第二列为 value，key为空舍弃，其他列内容舍弃  

|           |                             |                     |
| --------- | --------------------------- | ------------------- |
| TextKey1  | TextValue1                  | comment（该列舍弃） |
| TextKey2  | TextValue2 \n New line Text |                     |
|           | comment（该行舍弃）         |                     |
| ImageKey3 | English_Image0              |                     |

##### json 格式  

键值对转换为 json 即可

```
{
    "TextKey1": "TextValue1",
    "TextKey2": "TextValue2 \n New line Text",
    "ImageKey3": "English_Image0"
}
```

#### 默认目录结构

*  Resources  
  * Localization  
    * 0.English.txt
    * 1.ChineseSimplified.txt
    * 2.......txt
    * Sprites
      * _Default
        * PublicImage.png
      * English
        * English_Image0.png
        * English_Image1.png
        * English_Image2.png
        * ...
      * ChineseSimplified
        * Chinese_Image0.png
        * Chinese_Image1.png
        * Chinese_Image2.png  
        * ...

### LocalizationManager

* LocalizationChangeEvent  

  本地化语言更改事件   

* FileList  

  本地化资源目录下文件列表，可用作支持语言列表

* FileType  

  当前设置本地化文件类型，txt/csv/json

* FilePath  

  文件目录

* DefaultImagesPath  

  本地化默认图片目录

* ImagesPath  

  本地化图片目录

* WarnMissedValue

  是否 Log 警告键值或图片缺失，默认false，随时开关  

* CurrentLanguage  

  当前语言名，依据 UnityEngine.SystemLanguage 枚举值名称    

* CurrentLanguageIndex  

  当前语言在 FileList 中 Index

* SystemLanguage  

  系统语言，由 Application.systemLanguage 转换成 string  

* CurrentLocalizationFile  

  当前选择语言文件

* Init(bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English, string filesPath = "Localization", LocalizationFileType fileType = LocalizationFileType.Csv)  

  必须调用，负责无法生成单例，默认开启跟随系统语言 (followSystem)，找不到对应文件默认使用英语，默认路径`Localization`，默认文件类型 csv  

* LoadAllLocalizationFilesData(string filesPath, LocalizationFileType fileType = LocalizationFileType.Csv)  

  单例重新加载语言文件  

* ClearLanguageSelection()  

  清除语言选择记录

* ChangeToLanguage(int index) / ChangeToLanguage(SystemLanguage language)  

  根据 FileList index 或 SystemLanguage 更改语言，返回是否成功 

* GetLocalizedText(string key)  

  由键获取文案  

* GetLocalizedSpriteByNameAsync(string spriteName, string defaultSpriteName, Action<Sprite> callback)

  协程方法，根据传入的图片名异步加载并返回设定路径下的图片，若无图片则由 `DefaultImagesPath` 和 `defaultSpriteName` 加载默认图片

### LocalizationComponent  

用于挂载 Monobehavior 对象，自动执行控件更新，可增加 LocalizationComponentItem 数组内容配置相应键值

* LocalizationComponentItem

  * component  

    需更新的组件，目前支持  Text、Image 与 SpriteRenderer 和 Text Mesh 

  * localizationKey  

    本地化键，用于获取本地化文本或作为图片名获取本地化图片

  * defaultValue  

    默认文本（图片名），当使用 localizationKey 获取的内容为空时，则使用 defaultValue 直接展示或获取默认图片  

  * setNativeSize  

    更新图片时是否使用图片原始尺寸，否则使用配置尺寸

