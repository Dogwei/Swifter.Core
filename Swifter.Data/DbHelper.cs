using Swifter.RW;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Swifter.Data
{
    /// <summary>
    /// 提供数据库操作的工具方法
    /// </summary>
    public static class DbHelper
    {
        private static readonly List<KeyValuePair<string, DbProviderFactory>> providerTypesCache;
        private static readonly object providerTypesCacheLock;

        static DbHelper()
        {
            providerTypesCache = new List<KeyValuePair<string, DbProviderFactory>>();
            providerTypesCacheLock = new object();
        }

        /// <summary>
        /// 创建数据库连接。
        /// </summary>
        /// <param name="providerName">数据库连接的包名</param>
        /// <returns>返回一个数据库连接</returns>
        public static DbConnection CreateConnection(string providerName)
        {
            return GetProviderFactory(providerName).CreateConnection();
        }

        /// <summary>
        /// 获取数据库工厂实例。
        /// </summary>
        /// <param name="providerName">数据库工厂名称</param>
        /// <returns>返沪数据库工厂实例</returns>
        public static DbProviderFactory GetProviderFactory(string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException("providerName");
            }

            DbProviderFactory dbProviderFactory;

            if (!TryGetFactory(providerName, out dbProviderFactory))
            {
                lock (providerTypesCacheLock)
                {
                    if (!TryGetFactory(providerName, out dbProviderFactory))
                    {
                        dbProviderFactory = DbProviderFactories.GetFactory(providerName);
                    }
                }
            }
            
            return dbProviderFactory;
        }

        private static bool TryGetFactory(string providerName, out DbProviderFactory dbProviderFactory)
        {
            foreach (var item in providerTypesCache)
            {
                if (item.Key == providerName)
                {
                    dbProviderFactory = item.Value;

                    return true;
                }
            }

            dbProviderFactory = null;

            return false;
        }

        /// <summary>
        /// 创建数据库连接。
        /// </summary>
        /// <param name="providerName">数据库连接的包名</param>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>返回一个数据库连接</returns>
        public static DbConnection CreateConnection(string providerName, string connectionString)
        {
            var dbConnection = CreateConnection(providerName);

            dbConnection.ConnectionString = connectionString;

            return dbConnection;
        }

        /// <summary>
        /// 将指定对象作为参数集合传递给 T-SQL 命令。
        /// </summary>
        /// <typeparam name="T">参数集合类型</typeparam>
        /// <param name="dbCommand">T-SQL 命令</param>
        /// <param name="obj">参数集合</param>
        public static void SetParameters<T>(DbCommand dbCommand, T obj)
        {
            var dataReader = RWHelper.CreateReader(obj).As<string>();

            dataReader.OnReadAll(new DbCommandParametersAdder(dbCommand));
        }
    }
}
