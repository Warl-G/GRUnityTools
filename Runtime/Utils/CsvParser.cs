using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GRTools.Utils
{
    public class CsvParser
    {
        private const string COLUMN_SPLIT_PATTERN = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private const string ROW_SPLIT_PATTERN = @"\r\n|\n\r|\n|\r";
        private const string QUOTE_PATTERN = @"(?<=^"")(.|\n)*?(?=""$)";

        private static object ParseSingleCell(string cell)
        {
            object value = "";
            if (!string.IsNullOrEmpty(cell))
            {
                int intValue;
                float floatValue;
                if(int.TryParse(cell, out intValue)) {
                    value = intValue;
                } 
                else if (float.TryParse(cell, out floatValue)) 
                {
                    value = floatValue;
                }
                else
                {
                    cell = Regex.Unescape(cell);
                    var match = Regex.Match(cell, QUOTE_PATTERN);

                    if (match.Success)
                    {
                        cell = match.Value;
                    }
                                
                    value = cell.Replace(@"""""", @"""");
                }
            }

            return value;
        }
        
        /// <summary>
        /// 完全解析 csv 行列，类型为int, float, string, 保留空字段为空字符
        /// </summary>
        /// <param name="csvString"></param>
        /// <returns></returns>
        public static List<List<object>> Parse(string csvString)
        {
            if (!string.IsNullOrEmpty(csvString))
            {
                var rows = Regex.Split(csvString, ROW_SPLIT_PATTERN);
                
                List<List<object>> rowList = new List<List<object>>();

                for (int i = 0; i < rows.Length; i++)
                {
                    var columns = Regex.Split(rows[i], COLUMN_SPLIT_PATTERN);
                    List<object> columnList = new List<object>();
                    
                    for (int j = 0; j < columns.Length; j++)
                    {
                        object value = ParseSingleCell(columns[j]);

                        columnList.Add(value);
                    }
                    
                    rowList.Add(columnList);
                }
                return rowList;
            }
            
            return null;
        }

        /// <summary>
        /// 解析行，以每行第一个字段为 key，其余解析为 list 
        /// </summary>
        /// <param name="csvString"></param>
        /// <returns></returns>
        public static Dictionary<string, List<object>> ParseRowToDictionary(string csvString)
        {
            if (!string.IsNullOrEmpty(csvString))
            {
                var rows = Regex.Split(csvString, ROW_SPLIT_PATTERN);

                Dictionary<string, List<object>> rowDict = new Dictionary<string, List<object>>();

                for (int i = 0; i < rows.Length; i++)
                {
                    var columns = Regex.Split(rows[i], COLUMN_SPLIT_PATTERN);
                    
                    //第一列为 key
                    if (columns.Length > 0)
                    {
                        string rowKey = ParseSingleCell(columns[0]).ToString();
                        if (!string.IsNullOrEmpty(rowKey))
                        {
                            List<object> columnList = new List<object>();
                            for (int j = 1; j < columns.Length; j++)
                            {
                                object value = ParseSingleCell(columns[j]);
                            
                                columnList.Add(value);
                            }
                            rowDict.Add(rowKey, columnList);
                        }
                    }

                }

                return rowDict;
            }

            return null; 
        }
        
        /// <summary>
        /// 解析列，以第一行字段为key，将其余每行数据解析为字典，组成list
        /// </summary>
        /// <param name="csvString"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ParseColumnToDictionary(string csvString)
        {
            if (!string.IsNullOrEmpty(csvString))
            {
                var rows = Regex.Split(csvString, ROW_SPLIT_PATTERN);

                if (rows.Length == 0)
                {
                    return null;
                }
                
                //将第一行字段解析为 key
                string[] firstRow = Regex.Split(rows[0], COLUMN_SPLIT_PATTERN);

                for (int i = 0; i < firstRow.Length; i++)
                {
                    firstRow[i] = ParseSingleCell(firstRow[i]).ToString();
                }
                
                List<Dictionary<string, object>> rowList = new List<Dictionary<string, object>>();

                //第二行开始分配字段值
                for (int i = 1; i < rows.Length; i++)
                {
                    var columns = Regex.Split(rows[i], COLUMN_SPLIT_PATTERN);
                    Dictionary<string, object> columnDict = new Dictionary<string, object>();

                    for (int j = 0; j < columns.Length && j < firstRow.Length; j++)
                    {
                        //字段不为空则记录该字段
                        if (!string.IsNullOrEmpty(firstRow[j]))
                        {
                            object value = ParseSingleCell(columns[j]);
                            columnDict.Add(firstRow[j], value);
                        }
                    }
                    
                    rowList.Add(columnDict);
                }

                return rowList;
            }
            return null;
        }
        
        /// <summary>
        /// 解析行列，每行第一个
        /// </summary>
        /// <param name="csvString"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, object>> ParseRowAndColumnToDictionary(string csvString)
        {
            if (!string.IsNullOrEmpty(csvString))
            {
                var rows = Regex.Split(csvString, ROW_SPLIT_PATTERN);

                if (rows.Length == 0)
                {
                    return null;
                }
                
                //将第一行字段解析为 key
                string[] firstRow = Regex.Split(rows[0], COLUMN_SPLIT_PATTERN);

                for (int i = 0; i < firstRow.Length; i++)
                {
                    firstRow[i] = ParseSingleCell(firstRow[i]).ToString();
                }
                
                Dictionary<string, Dictionary<string, object>> rowDict = new Dictionary<string, Dictionary<string, object>>();

                //第二行开始解析值
                for (int i = 1; i < rows.Length; i++)
                {
                    var columns = Regex.Split(rows[i], COLUMN_SPLIT_PATTERN);

                    //第一列不为空
                    if (columns.Length > 0)
                    {
                        string rowKey = ParseSingleCell(columns[0]).ToString();
                        if (!string.IsNullOrEmpty(rowKey))
                        {
                            Dictionary<string, object> columnDict = new Dictionary<string, object>();

                            for (int j = 1; j < columns.Length && j < firstRow.Length; j++)
                            {
                                //字段不为空则记录该字段
                                if (!string.IsNullOrEmpty(firstRow[j]))
                                {
                                    object value = ParseSingleCell(columns[j]);
                                    columnDict.Add(firstRow[j], value);
                                }
                            }

                            rowDict.Add(rowKey, columnDict);
                        }
                    }
                }

                return rowDict;
            }
            return null;
        }
    
    }
}

