# GRTools.Sqlite  

***v1.0.3***

SqliteHelper  是对 Mono.Data.Sqlite 的封装扩展，分为 `SqliteHelper`、`SqliteHelperExtension`和`SqliteHelperQueue`三部分组成   

`SqliteHelper` 简单封装了开启数据库，快捷路径等操作

`SqliteHelperExtension` 中对 `SqliteConnection`、`SqliteCommand` 和 `SqliteDataReader` 进行扩展，提供一系列快捷方法    

`SqliteHelperQueue` 为使用 `TaskQueue` 和 `SqliteHelper` 封装的数据库操作队列，其中数据库操作方法均为串行队列同步执行

## SqliteHelper

### Property  

* SqliteConnection Connection  

  只读，SqliteHelper 使用的 SqliteConnection 对象

### Method  

* SqliteHelper(string dbPath)    

  构造方法，dbPath 为数据库路径  

* SqliteHelper DbInDefaultPath(string dbName)   

  静态方法，dbName 为数据库名称，返回默认路径下的数据库对象 

* void Open(string dbPath)   

  打开数据库，dbPath 为数据库路径  

* void Close()   

  关闭数据库

* string DefaultPathForDb(string dbName)    

  静态方法，dbName 为数据库名称，返回默认的数据库路径，`Application.persistentDataPath + "/.data/db/"+dbName `

```c#
//打开默认路径下的数据库
string path = SqliteHelper.DbInDefaultPath("Database.db");
SqliteHelper sqliteHelper = new SqliteHelper(path);

//切换其他数据库
path = "/Application/Data/Database.db";
sqliteHelper.Open(path);
```



## SqliteHelperExtension   

#### Method

##### SqliteConnectionExtensionBase  

* void Transaction(Func<SqliteConnection, SqliteTransaction, bool> sqlAction)  

  执行事务，sqlAction 中执行事务，sqlAction 需返回 bool 值以决定是否回滚操作

##### SqliteCommandExtensionBase  

* int ExecuteSqlNonQuery(string sql, Dictionary<string, object> param = null)   

  执行无结果语句

  sql 为执行的 SQL 语句，若带参则需将value值替换为带有 @、$或：前缀的 key，配合 param，如：@value_key

  param 与sql 中的 key ，如 @value_key ，配合，key 为 sql 语句中为 value 设置的 key 而非列名，为空则 sql 正常执行

  返回语句操作的数据行数，仅 INSERT, UPDATE, DELETE 有值，其他操作为0（官方文档说 SELECT 为 -1，实际测试为0） 

  ```c#
  //原 SQL 语句 "INSERT INTO table (col1,col2,col3) VALUES (111,'value222','333');"
  //一般执行方式
  string sql = "INSERT INTO table (col1,col2,col3) VALUES (111,'value222','333')";
  command.ExecuteSqlNonQuery(sql1);
  
  //带参方式
  sql = "INSERT INTO table (col1,col2,col3) VALUES ($key1,:key2,@key3)"; 
  Dictionary param = new Dictionary(){{"$key1",111},{":key2","value222"},{"@key3","333"}}
  command.ExecuteSqlNonQuery(sql,param);
  ```

* SqliteDataReader ExecuteSql(string sql, Dictionary<string, object> param = null)    

  使用方法同上，返回 `SqliteDataReader` 对象，使用完的 SqliteDataReader 最好 Close 否则影响后续同一 Command 操作结果

* bool ExecuteSqlWithBool(string sql)   

  执行语句，sql 为 SQL 语句，返回是否正确执行

##### SqliteCommandExtensionCommon  

* bool TableExists(string table)   

  表是否存在，table 为表名  

* void DropTable(string table)   

  删除表，table 为表名  

* int RowsOfTable(string table)   

  表中有多少行数据，table 为表名，返回行数  

* bool Insert(string table, Dictionary<string, object> param)    

  向表中插入数据，table 为表名，param 为列名与数据组成的字典

* int Delete(string table, string condition = null)   

  删除数据，table 为表名，condition 为匹配条件即 WHERE 之后的语句

* SqliteDataReader Select(this SqliteCommand command, string table, string[] cols = null, string condition = null)   

  查找数据，table 为表名，cols 为查找列名组成的数组，condition 为匹配条件即 WHERE 之后的语句

* Dictionary<string, object>[] SelectDictionaries(this SqliteCommand command, string table, string[] cols = null, string conditions = null)   

  同上，返回整合为字典数组的数据，一个数组元素为一行数据，一个键值对为该行对应列数据

* int Update(string table, Dictionary<string, object> param, string condition)   

  更新数据，table 为表名，param 为列名与数据组成的字典，condition 为匹配条件即 WHERE 之后的语句

* void Transaction(Func<SqliteConnection, SqliteTransaction, bool> sqlAction)    

  执行事务，sqlAction 中执行事务，sqlAction 需返回 bool 值以决定是否回滚操作

* void Transaction(Func<SqliteCommand, SqliteTransaction, bool> sqlAction)   

  执行事务，sqlAction 中执行事务，sqlAction 需返回 bool 值以决定是否回滚操作

##### SqliteDataReaderExtensionCommon

* Dictionary<string, object>[] ResultsInDictionaries()  

  SqliteDataReader 将数据整合为字典数组返回，一个数组元素为一行数据，一个键值对为该行对应列数据

## SqliteHelperQueue   

### Property  

* SqliteConnection Connection    

  只读属性

### Method  

* SqliteHelperQueue(string dbPath)   

  构造方法，dbPath 为数据库路径 

* SqliteHelperQueue DbInDefaultPath(string dbName)   

  静态方法，dbName 为数据库名称，返回数据库默认路径，同 SqliteHelper.DbInDefaultPath

* void Open(string dbPath)   

  打开数据库，dbPath 为数据库路径，该方法会关闭已打开数据库

* void Close()    

  关闭数据库

* void Execute(Action<SqliteCommand> sqlAction)   

  在串行队列中同步执行，在 sqlAction 中执行数据库操作

* void ExecuteSqlNonQuery(string sql, Dictionary<string, object> param = null)   

  在串行队列中同步执行，sql 为 SQL 语句，同  SqliteCommand.ExecuteSqlNonQuery

* SqliteDataReader ExecuteSql(string sql, Dictionary<string, object> param = null)  

  在串行队列中同步执行，sql 为 SQL 语句，返回结果 SqliteDataReader，使用完的 SqliteDataReader 需 Close 释放，同  SqliteCommand.ExecuteSql

* void Transaction(Func<SqliteConnection, SqliteTransaction, bool> sqlAction)  

  在串行队列中同步执行事务，sqlAction 中执行数据库操作，需返回 bool 值决定是否回滚操作  

  ```c#
  SqliteHelperQueue dbQueue = SqliteHelperQueue.DbInDefaultPath("Database.db");
  string sql = "INSERT INTO table (col1,col2,col3) VALUES ($key1,:key2,@key3)"; 
  Dictionary param = new Dictionary(){{"$key1",111},{":key2","value222"},{"@key3","333"}}
  
  //避免在 sqlAction 执行数据库以外的耗时操作，以免阻塞后续数据库操作
  dbQueue.Execute((SqliteConnection connection) =>
  {
    	using(var command = connection.CreateCommand())
      {
        	command.ExecuteSqlNonQuery(sql,param);
      }
  });
  
  SqliteDataReader reader = dbQueue.ExecuteSql(sql, param);
  //使用完的 SqliteDataReader 一定要 Close 释放 reader
  reader.Close();
  
  dbQueue.Transaction((SqliteConnection connection, SqliteTransaction transaction) =>
  {
    		//例如语句执行无异常就提交操作
    		try
        {
          	using(var command = connection.CreateCommand())
            {
              	command.Delete("table");
          			command.Insert("table", new Dictionary<string, object> { {"col1", 111},{"col2", "value222"}, {"col3", "333"}});
          			command.Insert("table", new Dictionary<string, object> { {"col1", 111},{"col2", "value222"}, {"col3", "333"}});
          			command.Insert("table", new Dictionary<string, object> { {"col1", 111},{"col2", "value222"}, {"col3", "333"}});
            }
          
          	//若无异常则无需回滚，提交操作
            return false;
        }
    		catch (SqliteException e)
    		{
          	//若有异常则回滚
          	return true;
    		}
  });
  ```

  