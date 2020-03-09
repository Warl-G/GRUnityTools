using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace GRTools.SqliteHelper
{
        public static class SqliteConnectionExtensionBase
        {
            /// <summary>
            /// 执行事务
            /// </summary>
            /// <param name="connection"></param>
            /// <param name="sqlAction">返回是否回滚</param>
            public static void Transaction(this SqliteConnection connection,
                Func<SqliteConnection, SqliteTransaction, bool> sqlAction)
            {
                using (var transaction = connection.BeginTransaction())
                {
                    bool shouldRollback = sqlAction(connection, transaction);
                    if (shouldRollback)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                }
            }
        }

        public static class SqliteCommandExtensionBase
        {

            /// <summary>
            /// 非查询带参语句执行，如INSERT, UPDATE, DELETE 以及 SELECT 为 0
            /// example sql: "INSERT INTO table (column) VALUES (@value_key)"
            /// example param: {"@value_key" : value}
            /// </summary>
            /// <param name="command"></param>
            /// <param name="sql">正常sql语句，需将value值替换为带有 @、$或：前缀的key，配合param，如：@value_key</param>
            /// <param name="param">与sql中的 @value_key 配合，key 为 sql 语句中为 value 设置的 key 而非列名，为空则正常执行</param>
            /// <returns>执行条数，除 INSERT, UPDATE, DELETE 外，其他返回值均为0</returns>
            public static int ExecuteSqlNonQuery(this SqliteCommand command, string sql,
                Dictionary<string, object> param = null)
            {
                if (string.IsNullOrEmpty(sql)) return 0;

                command.Parameters.Clear();
                if (param != null && param.Count > 0)
                {
                    foreach (KeyValuePair<string, object> p in param)
                    {
                        command.Parameters.AddWithValue(p.Key, p.Value);
                    }
                }

                command.CommandText = sql;
                int result = command.ExecuteNonQuery();
                command.Parameters.Clear();
                return result;
            }

            /// <summary>
            /// 执行带参语句
            /// example sql: "INSERT INTO table (column) VALUES (@value_key)"
            /// example param: {"@value_key" : value}
            /// </summary>
            /// <param name="command"></param>
            /// <param name="sql">正常sql语句，需将value值替换为带有 @、$或：前缀的key，配合param，如：@value_key</param>
            /// <param name="param">与sql中的 @value_key 配合，key 为 sql 语句中为 value 设置的 key 而非列名，为空则正常执行</param>
            /// <returns></returns>
            public static SqliteDataReader ExecuteSql(this SqliteCommand command, string sql,
                Dictionary<string, object> param = null)
            {
                if (string.IsNullOrEmpty(sql)) return null;

                command.Parameters.Clear();
                if (param != null && param.Count > 0)
                {
                    foreach (KeyValuePair<string, object> p in param)
                    {
                        command.Parameters.AddWithValue(p.Key, p.Value);
                    }
                }

                command.CommandText = sql;
                SqliteDataReader reader = command.ExecuteReader();
                command.Parameters.Clear();
                return reader;
            }

            /// <summary>
            /// 返回执行 Bool 结果
            /// </summary>
            /// <param name="command"></param>
            /// <param name="sql"></param>
            /// <returns>是否执行成功</returns>
            public static bool ExecuteSqlWithBool(this SqliteCommand command, string sql)
            {
                try
                {
                    SqliteDataReader reader = command.ExecuteSql(sql);
                    if (reader != null)
                    {
                        reader.Close();
                        return true;
                    }

                    return false;
                }
                catch (SqliteException e)
                {
                    return false;
                }
            }
        }

        public static class SqliteCommandExtensionCommon
        {
            /// <summary>
            /// 表是否存在
            /// </summary>
            /// <param name="table">表名</param>
            /// <returns></returns>
            public static bool TableExists(this SqliteCommand command, string table)
            {
                SqliteDataReader reader =
                    command.ExecuteSql(
                        $"SELECT COUNT(*) AS cnt FROM sqlite_master WHERE type = 'table' AND name = '{table}'");

                if (reader != null)
                {
                    bool result = Convert.ToInt32(reader["cnt"]) > 0;
                    reader.Close();
                    return result;
                }

                return false;
            }

            /// <summary>
            /// 删除表
            /// </summary>
            /// <param name="table"></param>
            /// <returns></returns>
            public static void DropTable(this SqliteCommand command, string table)
            {
                command.ExecuteSqlNonQuery($"DROP TABLE IF EXISTS {table}");
            }

            /// <summary>
            /// 表现有行数
            /// </summary>
            /// <param name="table"></param>
            /// <returns></returns>
            public static int RowsOfTable(this SqliteCommand command, string table)
            {
                SqliteDataReader reader = command.ExecuteSql($"SELECT COUNT(*) AS cnt FROM '{table}'");
                if (reader != null)
                {
                    int count = Convert.ToInt32(reader["cnt"]);
                    reader.Close();
                    return count;
                }

                return 0;
            }

            /// <summary>
            /// 插入操作
            /// "INSERT INTO {table} ({cols}) VALUES ({values})"
            /// </summary>
            /// <param name="table">表名</param>
            /// <param name="param">列名和值组成的字典</param>
            /// <returns></returns>
            public static bool Insert(this SqliteCommand command, string table, Dictionary<string, object> param)
            {
                string cols = "";
                string valuesKey = "";
                int count = 0;
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> p in param)
                {
                    if (count > 0)
                    {
                        cols += ",";
                        valuesKey += ",";
                    }

                    count++;
                    cols += p.Key;
                    string key = "@" + p.Key;
                    valuesKey += key;
                    values.Add(key, p.Value);
                }

                string sql = $"INSERT INTO {table} ({cols}) VALUES ({valuesKey})";
                int result = command.ExecuteSqlNonQuery(sql, values);
                if (result > 0)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 删除操作
            /// DELETE FROM {table} WHERE {condition}
            /// </summary>
            /// <param name="table"></param>
            /// <param name="condition">选择条件, 可为 null 或 "" 即代表无条件</param>
            /// <returns>删除条数</returns>
            public static int Delete(this SqliteCommand command, string table, string condition = null)
            {
                string sql = $"DELETE FROM {table}";
                if (!string.IsNullOrEmpty(condition))
                {
                    sql += " WHERE " + condition;
                }

                return command.ExecuteSqlNonQuery(sql);
            }

            /// <summary>
            /// "SELECT {cols} FROM {table} WHERE {condition}";
            /// </summary>
            /// <param name="table"></param>
            /// <param name="cols">列名，可为 {*}、空 或 null 即代表选择全部列</param>
            /// <param name="condition">选择条件, 即 WHERE 后语句，可为 null 或 "" 即代表无条件</param>
            /// <returns></returns>
            public static SqliteDataReader Select(this SqliteCommand command, string table, string[] cols = null,
                string condition = null)
            {
                string sql = "SELECT ";
                if (cols == null || cols.Length == 0 || cols[0] == "*")
                {
                    sql += "*";
                }
                else
                {
                    sql += String.Join(", ", cols);
                }

                sql += " FROM " + table;
                if (!string.IsNullOrEmpty(condition))
                {
                    sql += " WHERE " + condition;
                }

                return command.ExecuteSql(sql);
            }

            /// <summary>
            /// "SELECT {cols} FROM {table} WHERE {condition}"
            /// 同Select方法，返回Dictionary[]结果，key为列名
            /// </summary>
            /// <param name="table"></param>
            /// <param name="cols">列名，可为 {*}、空 或 null 即代表选择全部列</param>
            /// <param name="conditions">选择条件, 即 WHERE 后语句，可为 null 或 "" 即代表无条件</param>
            /// <returns></returns>
            public static Dictionary<string, object>[] SelectDictionaries(this SqliteCommand command, string table,
                string[] cols = null, string conditions = null)
            {
                SqliteDataReader reader = command.Select(table, cols, conditions);
                var result = reader.ResultsInDictionaries();
                reader.Close();
                return result;
            }

            /// <summary>
            /// 更新操作
            /// "UPDATE {table} SET {param.keys = param.values} WHERE {condition}"
            /// </summary>
            /// <param name="table"></param>
            /// <param name="param">列名</param>
            /// <param name="condition">条件, 即 WHERE 后语句</param>
            /// <returns>执行条数</returns>
            public static int Update(this SqliteCommand command, string table, Dictionary<string, object> param,
                string condition)
            {
                string sql = $"UPDATE {table} SET ";
                int count = 0;
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> p in param)
                {
                    if (count > 0)
                    {
                        sql += ",";
                    }

                    count++;
                    string key = "@" + p.Key;
                    sql += p.Key + " = " + key;
                    values.Add(key, p.Value);
                }

                sql += " WHERE " + condition;

                return command.ExecuteSqlNonQuery(sql, values);
            }

            /// <summary>
            /// 执行事务
            /// </summary>
            /// <param name="connection"></param>
            /// <param name="sqlAction">返回是否回滚</param>
            public static void Transaction(this SqliteCommand command,
                Func<SqliteConnection, SqliteTransaction, bool> sqlAction)
            {
                command.Connection.Transaction(sqlAction);
            }

            /// <summary>
            /// 执行事务
            /// </summary>
            /// <param name="command"></param>
            /// <param name="sqlAction">返回是否回滚</param>
            public static void Transaction(this SqliteCommand command,
                Func<SqliteCommand, SqliteTransaction, bool> sqlAction)
            {
                using (var transaction = command.Connection.BeginTransaction())
                {
                    bool shouldRollback = sqlAction(command, transaction);
                    if (shouldRollback)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                }
            }
        }

        public static class SqliteDataReaderExtensionCommon
        {
            /// <summary>
            /// 将 reader 整合为字典 
            /// </summary>
            /// <param name="reader"></param>
            /// <returns></returns>
            public static Dictionary<string, object>[] ResultsInDictionaries(this SqliteDataReader reader)
            {
                List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                while (reader.Read())
                {
                    int colsCount = reader.FieldCount;
                    for (int i = 0; i < colsCount; i++)
                    {
                        string name = reader.GetName(i);
                        object value = reader.GetValue(i);
                        Dictionary<string, object> dict = new Dictionary<string, object>() {{name, value}};
                        results.Add(dict);
                    }
                }

                return results.ToArray();
            }
        }
    }