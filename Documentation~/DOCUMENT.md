# GRTools.Addressables  

## PacakScripts  

Addressables 内置资源独立打包脚本，由 Addressables 1.16.16 `BuildScriptPackedMode`脚本改写，由于不同版本 Addressables API 不同，使用不同版本 Addressables 导入后可能会产生编译错误，建议可以自行参考[Unity 内置资源独立打包](https://warl-g.github.io/posts/Unity-BuiltIn-Pack/)，自己根据当前使用版本改写

1. 集成后 ContentBuilder 会增加`Builtin ResS Build Script`   

<img src="Images/doc01.png" alt="doc01" style="zoom:30%;" />

2. 创建 Builder 配置后，打开 Addressables -> Settings，添加新生成的配置  

   <img src="/Users/guoru/Developer/GRUnityTools/Assets/GRTools/Addressables/Documentation~/Images/doc02.png" alt="doc02" style="zoom:33%;" />

3. 在 Addressables Groups 面板下即可使用该脚本打包资源，实现自动内置资源独立分包   

   <img src="/Users/guoru/Developer/GRUnityTools/Assets/GRTools/Addressables/Documentation~/Images/doc03.png" alt="doc03" style="zoom:35%;" />