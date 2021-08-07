# GRTools.GitPackageResolver  

`"com.warlg.grtools.gitpackageresolver": "https://github.com/Warl-G/GRUnityTools.git#GRTools.GitPackageResolver"`

由 [GitDependencyResolverForUnity](https://github.com/mob-sakai/GitDependencyResolverForUnity) 改写，为 Unity Package 添加 Git Package 依赖解析扩展   

由于 Unity Package Manager 仅支持 Unity Registry 和 Scoped Registry 的 Package 依赖解析，而`GitDependencyResolverForUnity`是直接导入 Git 项目，均不能直接支持在 Git 上的自定义的 Package  

因此使用`GitDependencyResolverForUnity`中的解析方法，再为 PackageManager 添加 UI 扩展，实现 Git Unity Package 的解析与展示，并可一键手动导入

## Pacakge 制作者  

想让`GRTools.GitPackageResolver`解析出 Git 上的Unity Package 依赖，需在自定义的 Package 的 package.json 添加 gitPackages 字段，并添加对应的 Pakcage Git 地址，如：

```json
"gitPackages": {
		"com.warlg.grtools.utils": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Utils",
		"com.warlg.grtools.thread": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Thread",
		"com.warlg.grtools.sqlite": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Sqlite",
		"com.warlg.grtools.addressables": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Addressables",
		"com.warlg.grtools.localization": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Localization",
		"com.warlg.grtools.localization.tmp": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Localization.TextMeshPro",
		"com.warlg.grtools.localization.addressables": "https://github.com/Warl-G/GRUnityTools.git#GRTools.Localization.Addressables"
	}
```

可参考本工具的 pakcage.json

`#`前为 Git 地址，后部分可为分支名或标签名，可通过为 Package 不同版本打不同版本标签实现不同版本的依赖，但当前不支持处理版本冲突，需使用者手动选择版本  

## Pacakge 使用者

通过 Unity Package Manager 导入`GRTools.GitPackageResolver`后，所有支持的 Package 会在 Unity Package 详情页看到依赖的 Git Unity Package，并可一键导入，如图：  

<img src="Doc01.png" alt="Doc01" style="zoom:33%;" />