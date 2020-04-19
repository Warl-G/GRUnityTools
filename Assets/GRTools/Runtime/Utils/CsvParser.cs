
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GRTools.Utils
{
    public class CsvParser
    {
        private const string COLUMN_SPLIT_PATTERN = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private const string ROW_SPLIT_PATTERN = @"\r\n|\n\r|\n|\r";
        private const string Quote_PATTERN = @"(?<=^"")(.|\n)*?(?=""$)";
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
                        object value = "";
                        if (!string.IsNullOrEmpty(columns[j]))
                        {
                            var column = columns[j];
                            
                            int intValue;
                            float floatValue;
                            if(int.TryParse(column, out intValue)) {
                                value = intValue;
                            } 
                            else if (float.TryParse(column, out floatValue)) 
                            {
                                value = floatValue;
                            }
                            else
                            {
                                var cil = column;
                                column = Regex.Unescape(column);
                                var match = Regex.Match(column, Quote_PATTERN);

                                if (match.Success)
                                {
                                    column = match.Value;
                                }
                                
                                value = column.Replace(@"""""", @"""");
                            }
                        }

                        columnList.Add(value);
                    }
                    
                    rowList.Add(columnList);
                }
                return rowList;
            }
            
            return null;
        }
    
    }
}

