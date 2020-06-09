# GRUnityTools
~~A Unity Tools Package ver.1.3.0~~  

GRTools 是一个集成多种工具的 Unity Package，持续扩充中    

2020.6.10 更新：

由于各个工具功能比较独立，版本更新也不同步，现已将 GRUnityTools 拆分，现可根据需求选择性引用各模块，GRUnityTools 统一版本号作废，各模块独立拥有版本号，从1.0.0 开始

## 集成方式

1. 直接下载源码放入项目中使用，注意模块间的引用
2. 使用 upm 导入项目，upm格式：`"com.warlg.grtools.[模块名]": "https://github.com/Warl-G/GRUnityTools.git#[模块名]@[版本号]"`，@[版本号] 为可选字段

## 文档  

各个工具的详细文档可在 Assets/GRTools/*/Documents~ 下查看   

## 模块

### GRTools.Utils  

Ver.1.0.0 | `"com.warlg.grtools.utils": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Utils@1.0.0"`

GRTools 的通用工具包，部分模块引用该包体内容

1. newtonsoft-json  

   第三方 Json 解析工具，Unity 配置了官方引用包，Utils 在此直接引用作为桥接，详见[官网](https://www.newtonsoft.com/json)

2. CsvParser  

   csv 解析工具，可以四种模式解析 csv，详见[文档](Assets/GRTools/Utils/Documentation~/Utils.md)

### GRTools.Thread   

Ver.1.0.0 | `"com.warlg.grtools.thread": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Thread@1.0.0"`

GRTools 的线程工具包  

1. Loom

   网上流传比较广泛的 Unity 主线程回归方案

2. TaskQueue

   一个实现了同步串行、异步串行、同步并行、异步并行的多线程任务队列工具，详见[文档](Assets/GRTools/Thread/Documentation~/TaskQueue.md)，[编写教程](https://warl-g.github.io/posts/unity-taskqueue/)     

### GRTools.Localization  

Ver.1.0.0 | `"com.warlg.grtools.localization": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Localization@1.0.0"`

GRTools 的本地化工具包

[文档](Assets/GRTools/Localization/Documentation~/Localization.md)、[编写教程](https://warl-g.github.io/posts/Unity-Localization/)

多语言工具，无需修改代码即可扩充支持语言，有极强的可扩展性，可自定义文件加载、解析方式  

支持文本与资源自动切换，支持 `Text`、`Text Mesh`、`Image`、`Sprite` 和 `SpriteRender` 的自动更新脚本

已扩展 `Resources`、`AssetBundle` 和 `Addressable` 三种本地化资源管理方式

已扩展 txt（自定义解析规则）、csv 和 json 三种文本解析格式  

### GRTools.Localization.Addressable   

Ver.1.0.0 | `"com.warlg.grtools.localization.addressable": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Localization.Addressable@1.0.0"`

GRTools 的本地化工具的扩展包  

为本地化资源加载扩展 Addressable 支持，详见`GRTools.Localization`

### GRTools.Sqlite  

Ver.1.0.0 | `"com.warlg.grtools.sqlite": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Sqlite@1.0.0"`

[文档](Assets/GRTools/DataBase/Sqlite/Documentation~/SqliteHelper.md)

对 `Mono.Sqlite.Data` 的二次封装和配合 `TaskQueue` 制作的数据库快捷操作和操作队列工具