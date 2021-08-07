## GRTools.Localization  

version 1.1.0

本地化工具，分为 LocalizationManager 、LocalizationComponent 以及文本解析接口 ILocalizationParser 和 资源加载接口 ILocalizationLoader

本工具依赖于 UnityEngine.SystemLanguage 枚举类型，可参考该枚举值确认语言所包含语言  

目前已通过 ILocalizationParser 扩展支持三种文本格式： txt、csv、json，其中 txt 为本项目自定义的规范，如需自定义文本格式规范可通过实现该接口进行扩展  

LocalizationManager 负责统一管理，切换文本，通知多语言状态

LocalizationComponent 负责对控件的自动化统一更新，目前支持Text、Text Mesh、Image 和 SpriteRenderer 

ILocalizationParser 负责解析本地化文本文件

ILocalizationLoader 负责加载本地化资源

目前已添加了 Resources、AssetBundle 和 Addressable 三种本地化资源管理方式的支持，需将资源按规范存放  

样例可在 GRTools/LocalizationSample 查看

### 工具目录  

```
|-- GRTools.Localization 本地化核心库
	|-- Core	核心库
	|-- Base	基础支持库
|-- GRTools.Localization.Utils 提供组件工具库
|-- GRTools.Localization.Addressables Addressables 支持
|-- GRTools.Localization.TMP	TextMeshPro 支持
```

### 使用说明    

#### 资源格式   

根据需求选定本地化文本文件格式，可使用项目提供的 LocalizationDefaultParser 支持的三种格式，也可实现 ILocalizationParser 自定义格式解析

#### 资源路径  

确认本地化资源加载方式，可通过实现 ILocalizationLoader 自定义资源管理方式，也可使用默认提供的三种加载器，依据规范存放、处理资源，可在下方见详细资源管理规范

#### 初始化  

代码中，使用 LocalizationManager.Init 方法初始化单例，传入使用的加载器，解析器

#### 使用  

`LocalizationChangeEvent` 监听语言切换事件  

`RefreshInfoList`更换加载器解析器并重新读取语言配置列表  

`ChangeToLanguage`  切换语言

`GetLocalizedText`获取本地化文本字段

若配合 LocalizationComponent 使用，文件中值可为本地化文案，也可为本地化图片路径（Resources 下路径）

### LocalizationManager

* LocalizationChangeEvent  

  本地化语言更改事件   

* InfoList  

  本地化资源配置信息列表

* WarnMissedValue

  是否 Log 警告键值或图片缺失，默认false，随时开关  

* CurrentLanguage  

  当前语言名，依据 UnityEngine.SystemLanguage 枚举值名称    

* CurrentLanguageIndex  

  当前语言在 FileList 中 Index

* SystemLanguage  

  系统语言，由 Application.systemLanguage 转换成 string  

* CurrentLocalizationInfo  

  当前选择语言文件

* Init(ILocalizationLoader assetLoader, ILocalizationParser assetParser, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)

  必须调用，否则无法生成单例，默认开启跟随系统语言 (followSystem)，默认使用英语

* RefreshInfoList(ILocalizationLoader assetLoader = null,
            ILocalizationParser assetParser = null, Action<bool> completed = null)
  
* ClearLanguageSelection()  

  清除语言选择记录

* ChangeToLanguage(int index, Action<bool> success) / ChangeToLanguage(SystemLanguage language, Action<bool> success)  

  根据 FileList index 或 SystemLanguage 更改语言，返回是否成功 

* GetLocalizedText(string key, string defaultText = "")  

  由键获取文案或相应的资源路径，可设置默认文本  

### LocalizationExtensions  

- Init(bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)

  使用默认`LocalizationResourcesLoader`加载器和 `LocalizationDefaultParser` 解析器

- Init(ILocalizationLoader assetLoader, LocalizationTextType textType = LocalizationTextType.Csv, bool followSystem = true, SystemLanguage defaultLanguage = SystemLanguage.English)

  使用自定义加载器和默认解析器

- LoadLocalizationAssetAsync<TAsset>(string assetPath, Action<TAsset> callback) where TAsset : Object

  异步通过传入的资源路径获取资源

### LocalizationInfo  

多语言资源配置信息，用于 ILocalizationLoader 读取，可继承复写用于自定义加载器使用

* LanguageType  

  语言类型

* TextAssetPath  

  多语言文本文件路径

* AssetsPath  

  资源文件路径

### LocalizationManifest  

用于保存 `LocalizationInfo` 列表的 ScriptableObject，配合默认加载器使用，可自定义其他配置文件

### LocalizationComponentList  

用于挂载 Monobehavior 对象，自动执行控件更新，可增加 LocalizationComponentItem 数组内容配置相应键值

* LocalizationComponentItem

  * component  

    需更新的组件，目前支持  Text、Text Mesh、Image 与 SpriteRenderer   

  * localizationKey  

    本地化键，用于获取本地化文本或作为图片名获取本地化图片

  * defaultValue  

    默认文本（图片名），当使用 localizationKey 获取的内容为空时，则使用 defaultValue 直接展示或获取默认图片  

  * setNativeSize  

    更新图片时是否使用图片原始尺寸，否则使用配置尺寸

### ILocalizationLoader   

本地化资源加载器接口，需实现以下两方法提供给 LocalizationManager 使用  

* LoadManifestAsync(Action<bool, LocalizationInfo[]> completed)

  加载语言文本文件列表

* LoadLocalizationTextAsset(LocalizationInfo info, Action<Object> completed)

  加载本地化文本资源

* LoadAssetAsync<TAsset>(LocalizationInfo info, string assetName, Action<TAsset> completed) where TAsset : Object  

  加载本地化其他资源   

***其中 UnityEngine.SystemLanguage.Chinese 会转换为 ChineseSimplified 类型***

首个语言会作为无目标语言情况下的默认语言   

本项目提供了三种预设资源管理方式，均配合 LocalizationManifest 加载多语言配置信息

* LocalizationResourcesLoader
* LocalizationAssetBundleLoader
* LocalizationAddressablesLoader

#### LocalizationResourcesLoader  

使用 Resources 加载本地化资源（Unity 官方并不建议使用 Resources 动态加载资源）  

ResourcesLoader 通过 ManifestPath 路径变量加载配置文件， 详细使用方式可见项目中示例

例：

English.txt

```
Image_Key=Localizations/ENAssets/Sample_Image_English
Common_Image_key=Localizations/Common/Sample_Image2
```

ChineseSimplified.txt

```
Image_Key=Localizations/CNAssets/Sample_Image_Chinese
Common_Image_key=Localizations/Common/Sample_Image
```

<img src="Images/Localization01.png" style="zoom:50%;" />

#### LocalizationAssetBundleLoader  

LocalizationAssetBundleLoader 使用 AssetBundle 管理本地化资源，该 Loader 提供针对本地化资源打包 AssetBundle 的快捷指令  

* 右击需打包资源目录 > GRTools > Localization > Build AssetBundles
* LocalizationAssetBundleLoader.BuildLocalizationAssets(string localizationFilePath, BuildTarget target)  

使用上述两种方式打包，需将本地化资源放入统一路径，脚本会按目录中子目录名称分包

快捷打包会使用文件夹名称分别打包成每种语言的 AssetBundle，会依据选择的目录最后一级目录名称，存于 StreamingAssets 目录下，加载器会依据 LocalizationManifest 中配置的路径名称加载对应 AssetBundle

同时还提供公共资源路径属性`CommonAssetsPath`，默认值为`Common`，即与语言目录同级的`Common`目录用于打包多语言通用资源  

`CommonAssetsPrefix` 公共资源前置路径，带有该前置字符的路径会从 Common AssetsBundle 加载

UnloadLastLocalizationBundle 属性，是否在加载新的语言 AssetBundle 时卸载上一个

若使用 `LocalizationComponent`自动更新，因 AssetBundle 不区分路径，文本中配置资源名即可

例选择 Localization 目录作为资源目录进行打包，则 AssetBundle 会打包在 StreamingAssets下的 Localization 目录  

<img src="Images/Localization03.png" style="zoom:50%;" />

#### LocalizationAddressableLoader  

另外扩展了 Addressable 的支持，为避免引用问题，`LocalizationAddressableLoader`的需从 `GRTools/LocalizationExtra`中手动下载导入，或使用 UPM 导入`"com.warlg.grtools.localization.addressables": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Localization.Addressables"`  

若使用 `LocalizationComponent`自动更新，文本中配置资源 Address 即可

<img src="Images/Localization04.png" style="zoom:50%;" />

### ILocalizationParser   

本地化文本解析器接口，需实现以下方法提供给 LocalizationManager 使用  

* Dictionary<string, string> Parse(string text)

  将文本解析为字典   

本项目提供了一套默认解析器，支持解析 txt（自定义解析规则）、csv、json

#### LocalizationDefaultParser  

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

