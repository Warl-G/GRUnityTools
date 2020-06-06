# GRUnityTools
A Unity Tools Package ver.1.3.0

GRTools 是一个集成多种工具的 Unity Package，持续扩充中  

### 集成方式

1. 直接下载源码放入项目中使用
2. 本项目支持upm，可在 Packages/manifest.json 中 添加 `"com.warlg.grtools": "https://github.com/Warl-G/GRUnityTools.git#1.3.0"`集成

### 文档  

各个工具的详细文档可在 Asset/GRTools/Documents 下查看  

### TaskQueue   

[文档](Documents/TaskQueue.md)，[编写教程](https://warl.top/posts/unity-taskqueue/)   

一个实现了同步串行、异步串行、同步并行、异步并行的多线程任务队列工具  

### Localization

[文档](Documents/Localization.md)

自己写的多语言工具，无需修改代码即可扩充支持语言，有极强的可扩展性，可自定义文件加载、解析方式  

支持文本与资源自动切换，支持 Text、Text Mesh、Image、Sprite 和 SpriteRender 的自动更新脚本

已扩展 Resources、AssetBundle 和 Addressable 三种本地化资源管理方式

已扩展 txt（自定义解析规则）、csv 和 json 三种文本解析格式

### SqliteHelper  

[文档](Documents/SqliteHelper.md)

使用 Mono.Sqlite.Data 和 TaskQueue 制作的数据库快捷操作和操作队列工具