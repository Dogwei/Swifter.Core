using Swifter.RW;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Swifter.Data
{
    /// <summary>
    /// 提供数据库操作的工具方法
    /// </summary>
    public static class DbHelper
    {
        private const string ProviderFactoryInstance = "Instance";

        private static readonly List<KeyValuePair<string, DbProviderFactory>> ProviderTypesCache;
        private static readonly object ProviderTypesCacheLock;

        static DbHelper()
        {
            ProviderTypesCache = new List<KeyValuePair<string, DbProviderFactory>>();
            ProviderTypesCacheLock = new object();
        }

        /// <summary>
        /// 创建数据库连接。
        /// </summary>
        /// <param name="providerName">供应商的包名</param>
        /// <returns>返回一个数据库连接</returns>
        public static DbConnection CreateConnection(string providerName)
        {
            return GetProviderFactory(providerName).CreateConnection();
        }

        /// <summary>
        /// 获取数据库工厂实例。
        /// </summary>
        /// <param name="providerName">供应商的包名</param>
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
                lock (ProviderTypesCacheLock)
                {
                    if (!TryGetFactory(providerName, out dbProviderFactory))
                    {
                        dbProviderFactory = SearchFactory(providerName);
                    }
                }
            }
            
            return dbProviderFactory;
        }

        private static DbProviderFactory SearchFactory(string providerName)
        {
#if NETFRAMEWORK || NETCOREAPP2_1
            try
            {
                var dbProviderFactory = DbProviderFactories.GetFactory(providerName);
                
                if (dbProviderFactory != null)
                {
                    return dbProviderFactory;
                }
            }
            catch (Exception)
            {
            }
#endif

            foreach (var asembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asembly.GetTypes())
                {
                    if (type.Namespace == providerName && typeof(DbProviderFactory).IsAssignableFrom(type))
                    {
                        var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

                        foreach (var item in members)
                        {
                            if (item.Name == ProviderFactoryInstance)
                            {
                                if (item is FieldInfo field && field.FieldType == type)
                                {
                                    var result = field.GetValue(null) as DbProviderFactory;

                                    if (result != null)
                                    {
                                        return result;
                                    }
                                }
                                else if (item is PropertyInfo property && property.PropertyType == type && property.GetIndexParameters().Length == 0)
                                {
                                    var result = property.GetValue(null, null) as DbProviderFactory;

                                    if (result != null)
                                    {
                                        return result;
                                    }
                                }
                            }
                        }

                        foreach (var item in members)
                        {
                            if (item is FieldInfo field && field.FieldType == type)
                            {
                                var result = field.GetValue(null) as DbProviderFactory;

                                if (result != null)
                                {
                                    return result;
                                }
                            }
                            else if (item is PropertyInfo property && property.PropertyType == type && property.GetIndexParameters().Length == 0)
                            {
                                var result = property.GetValue(null, null) as DbProviderFactory;

                                if (result != null)
                                {
                                    return result;
                                }
                            }
                        }
                    }
                }
            }

            throw new ArgumentException(".Net Framework Data Provider Not Found.");
        }

        private static bool TryGetFactory(string providerName, out DbProviderFactory dbProviderFactory)
        {
            foreach (var item in ProviderTypesCache)
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
        public static void SetParameters<T>(this DbCommand dbCommand, T obj)
        {
            var dataReader = RWHelper.CreateReader(obj).As<string>();

            dataReader.OnReadAll(new DbCommandParametersAdder(dbCommand));
        }

        /// <summary>
        /// 向 T-SQL 命令中添加具有指定名称和值的参数。
        /// </summary>
        /// <param name="dbCommand">T-SQL 命令</param>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>返回这个参数</returns>
        public static DbParameter AddParameter(this DbCommand dbCommand, string name, object value)
        {
            var dbParameter = dbCommand.CreateParameter();

            dbParameter.ParameterName = name;
            dbParameter.Value = value;

            dbCommand.Parameters.Add(dbParameter);

            return dbParameter;
        }
    }
}
