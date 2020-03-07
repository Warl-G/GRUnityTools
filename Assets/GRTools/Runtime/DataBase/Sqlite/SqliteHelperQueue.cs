using System;
using System.Collections.Generic;
using GRTools.Multithreading;
using Mono.Data.Sqlite;
using GRTools.SqliteHelper.Extension;

namespace GRTools.SqliteHelper.Queue
{
    public class SqliteHelperQueue
    {
        private TaskQueue _queue;
        private SqliteHelper _sqliteHelper;

        public SqliteConnection Connection
        {
            get { return _sqliteHelper.Connection; }
        }

        public SqliteCommand Command
        {
            get { return _sqliteHelper.Command; }
        }

        public string CurrentDbPath
        {
            get { return _sqliteHelper.CurrentDbPath; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbPath">数据库路径</param>
        public SqliteHelperQueue(string dbPath)
        {
            _queue = TaskQueue.CreateSerialQueue();
            _sqliteHelper = new SqliteHelper(dbPath);

        }

        /// <summary>
        /// 从默认路径读取数据库
        /// </summary>
        /// <param name="dbName">数据库名</param>
        /// <returns></returns>
        public static SqliteHelperQueue DbInDefaultPath(string dbName)
        {
            string path = SqliteHelper.DefaultPathForDb(dbName);
            return new SqliteHelperQueue(path);
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="dbPath">数据库路径</param>
        public void Open(string dbPath)
        {
            if (_sqliteHelper.Connection != null)
            {
                _sqliteHelper.Close();
                _sqliteHelper.Open(dbPath);
            }
        }

        /// <summary>
        /// 断开数据库
        /// </summary>
        public void Close()
        {
            _sqliteHelper.Close();
        }

        /// <summary>
        /// 队列中执行操作
        /// </summary>
        /// <param name="sqlAction"></param>
        public void Execute(Action<SqliteCommand> sqlAction)
        {
            _queue.RunSync(() => { sqlAction(Command); });
        }

        /// <summary>
        /// 队列中执行SQL带参语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public SqliteDataReader ExecuteSql(string sql, Dictionary<string, object> param = null)
        {
            SqliteDataReader reader = null;
            _queue.RunSync(() => { reader = Command.ExecuteSql(sql, param); });
            return reader;
        }

        /// <summary>
        /// 队列中执行事务，
        /// </summary>
        /// <param name="sqlAction">返回是否回滚，回滚true，提交false</param>
        public void Transaction(Func<SqliteCommand, SqliteTransaction, bool> sqlAction)
        {
            _queue.RunSync(() => { Command.Transaction(sqlAction); });
        }
    }
}