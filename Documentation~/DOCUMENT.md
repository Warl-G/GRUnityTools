# GRTools.Utils

该包主要为其他 GRTools 提供辅助工具，也会扩充其他实用工具  

## CsvParser  

 csv 解析工具，将 csv  中内容解析为 int、float 或 string 类型

* List<List<object>> Parse(string csvString)  

  将行列解析为二维数组

* Dictionary<string, List<object>> ParseRowToDictionary(string csvString)  

  每行解析为一个键值对，每行第一个字段为 key，其余为 List

* List<Dictionary<string, object>> ParseColumnToDictionary(string csvString)

  每列解析为一个键值对，每列第一个字段为 key，其余为 List

* Dictionary<string, Dictionary<string, object>> ParseRowAndColumnToDictionary(string csvString)

  每行解析为一个字典，每行第一个字段为一级 key，其余每列第一个字段为二级 key

