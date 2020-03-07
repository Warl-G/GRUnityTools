using System.IO;
using UnityEngine;
using Mono.Data.Sqlite;
using Debug = UnityEngine.Debug;

namespace GRTools.SqliteHelper
{
    public class SqliteHelper
    {
        public SqliteConnection Connection
        {
            get { return _connection; }
        }

        public SqliteCommand Command
        {
            get
            {
                if (_command == null)
                {
                    _command = _connection.CreateCommand();
                }

                return _command;
            }
        }

        /// <summary>
        /// 数据库目录
        /// </summary>
        public string CurrentDbPath { private set; get; }

        private SqliteConnection _connection;
        private SqliteCommand _command;

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="dbPath">数据库地址</param>
        public SqliteHelper(string dbPath)
        {
            Open(dbPath);
        }

        /// <summary>
        /// 从默认路径Application.persistentDataPath + "/.data/db"连接数据库
        /// </summary>
        /// <param name="dbName">数据库名称</param>
        /// <returns></returns>
        public static SqliteHelper DbInDefaultPath(string dbName)
        {
            string path = DefaultPathForDb(dbName);
            return new SqliteHelper(path);
        }

        ~SqliteHelper()
        {
            Close();
        }

        public void Open(string dbPath)
        {
            try
            {
                _connection = new SqliteConnection("Data Source=" + dbPath);
                _connection.Open();
                CurrentDbPath = dbPath;
                Debug.Log("SqliteHelper: Connected to database");
            }
            catch (SqliteException e)
            {
                Debug.LogWarning("SqliteHelper: " + e.ToString());
            }
        }

        /// <summary>
        /// 断开数据库连接
        /// </summary>
        public void Close()
        {
            if (_command != null)
            {
                _command.Cancel();
                _command = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }

            Debug.Log("SqliteHelper: Disconnected from database.");
        }

        /// <summary>
        /// 默认数据库路径
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static string DefaultPathForDb(string dbName)
        {
            string path = Application.persistentDataPath + "/.data/db";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            return path + "/" + dbName;
        }
    }
}