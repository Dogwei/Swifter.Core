using Swifter.Data.Sql;
using Swifter.Data.Sql.MsSql;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Swifter.Data.Database;

namespace Swifter.Data
{
    /// <summary>
    /// 提供数据库操作的工具方法
    /// </summary>
    public static class DbHelper
    {
        private const string ProviderFactoryInstance = "Instance";

        private static readonly NameCache<DbProviderFactory> ProviderTypesCache;
        private static readonly NameCache<SqlBuilder> SQLBuilderTypesCache;

        static DbHelper()
        {
            ProviderTypesCache = new NameCache<DbProviderFactory>();
            SQLBuilderTypesCache = new NameCache<SqlBuilder>();

            RegisterSQLBuilder(new MsSqlBuilder());
        }

        /// <summary>
        /// 创建数据库连接。
        /// </summary>
        /// <param name="providerName">供应商的包名</param>
        /// <returns>返回一个数据库连接</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbConnection CreateConnection(string providerName)
        {
            return GetFactory(providerName).CreateConnection();
        }

        /// <summary>
        /// 获取数据库工厂实例。
        /// </summary>
        /// <param name="providerName">数据库供应者的包名</param>
        /// <returns>返回数据库工厂实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbProviderFactory GetFactory(string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException(nameof(providerName));
            }

            if (ProviderTypesCache.TryGetValue(providerName, out var value))
            {
                return value;
            }

            return ProviderTypesCache.LockGetOrAdd(providerName, name => SearchFactory(name));
        }

        /// <summary>
        /// 注册数据库供应者工厂。
        /// </summary>
        /// <param name="providerName">数据库供应者的包名</param>
        /// <param name="dbProviderFactory">数据库供应者工厂实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void RegisterFactory(string providerName, DbProviderFactory dbProviderFactory)
        {
            if (dbProviderFactory == null)
            {
                throw new ArgumentNullException(nameof(dbProviderFactory));
            }

            if (dbProviderFactory == null)
            {
                throw new ArgumentNullException(nameof(dbProviderFactory));
            }

            ProviderTypesCache.LockAdd(providerName, dbProviderFactory);
        }

        /// <summary>
        /// 注册数据库的 T-SQL 生成器。
        /// </summary>
        /// <param name="sqlBuilder">T-SQL 生成器实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void RegisterSQLBuilder(SqlBuilder sqlBuilder)
        {
            if (sqlBuilder == null)
            {
                throw new ArgumentNullException(nameof(sqlBuilder));
            }

            SQLBuilderTypesCache.LockAdd(sqlBuilder.ProviderName, sqlBuilder);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static DbProviderFactory GetInstance(Type type)
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

            return null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static DbProviderFactory TestType(ProviderClasses providerClasses, Type type)
        {
            if (typeof(DbProviderFactory).IsAssignableFrom(type))
            {
                var result = GetInstance(type);

                if (result != null)
                {
                    return result;
                }
            }
            else if (typeof(IDbConnection).IsAssignableFrom(type))
            {
                providerClasses.tConnection = type;
            }
            else if (typeof(IDbCommand).IsAssignableFrom(type))
            {
                providerClasses.tCommand = type;
            }
            else if (typeof(IDataAdapter).IsAssignableFrom(type))
            {
                providerClasses.tDataAdapter = type;
            }
            else if (typeof(IDataReader).IsAssignableFrom(type))
            {
                providerClasses.tDataReader = type;
            }
            else if (typeof(IDataParameter).IsAssignableFrom(type))
            {
                providerClasses.tParameter = type;
            }
            else if (typeof(IDataParameterCollection).IsAssignableFrom(type))
            {
                providerClasses.tParameterCollection = type;
            }
            else if (typeof(IDbTransaction).IsAssignableFrom(type))
            {
                providerClasses.tTransaction = type;
            }

            return null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
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

            var providerClasses = new ProviderClasses(providerName);

            var assemblyName = providerName;

        Loop:

            try
            {
                /* 尝试加载供应商程序集 */
                var asembly = Assembly.Load(assemblyName);

                if (asembly != null)
                {
                    foreach (var type in asembly.GetTypes())
                    {
                        var result = TestType(providerClasses, type);

                        if (result != null)
                        {
                            return result;
                        }
                    }

                    if (providerClasses.tConnection != null)
                    {
                        var result = GetInstance(providerClasses.GetDynamicProviderFactoryType());

                        if (result != null)
                        {
                            return result;
                        }
                    }
                }


            }
            catch (Exception)
            {
            }

            var dotIndex = assemblyName.LastIndexOf('.');

            if (dotIndex != -1)
            {
                assemblyName = assemblyName.Substring(0, dotIndex);

                goto Loop;
            }

            /* 尝试查找已加载的程序集 */
            foreach (var asembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asembly.GetTypes())
                {
                    if (type.Namespace == providerName)
                    {
                        var result = TestType(providerClasses, type);

                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            if (providerClasses.tConnection != null)
            {
                var result = GetInstance(providerClasses.GetDynamicProviderFactoryType());

                if (result != null)
                {
                    return result;
                }
            }

            throw new ArgumentException($"Data provider not found in .net assemblies -- \"{providerName}\".");
        }

        /// <summary>
        /// 创建数据库连接。
        /// </summary>
        /// <param name="providerName">数据库连接的包名</param>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>返回一个数据库连接</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
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
        [MethodImpl(VersionDifferences.AggressiveInlining)]
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
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbParameter AddParameter(this DbCommand dbCommand, string name, object value)
        {
            var dbParameter = dbCommand.CreateParameter();

            dbParameter.ParameterName = name;
            dbParameter.Value = value;

            dbCommand.Parameters.Add(dbParameter);

            return dbParameter;
        }

        /// <summary>
        /// 获取指定供应商名称的 T-SQL 生成器。
        /// </summary>
        /// <param name="providerName">供应商名称</param>
        /// <returns>返回 T-SQL 生成器</returns>
        /// <exception cref="NotSupportedException">当不支持该供应商时发生。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static SqlBuilder CreateSqlBuilder(string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException(nameof(providerName));
            }

            if (SQLBuilderTypesCache.TryGetValue(providerName, out var value))
            {
                return value.CreateInstance();
            }

            throw new NotSupportedException("T-SQL builder does not support the provider.");
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static Sql.SqlBuilder GetSQLBuilder(string providerName)
        {
            SQLBuilderTypesCache.TryGetValue(providerName, out var value);

            return value;
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void AsyncTask(System.Threading.ThreadStart action)
        {
            // 使用线程提高并发
            // Used Thread Concurrency Higher.
            new System.Threading.Thread(action).Start();
        }



        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="database">数据库操作实例</param>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个命令</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbCommand CreateCommand<T>(this Database database, string sql, T parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = database.CreateCommand(sql, dbTransaction, commandTimeout, commandType);

            if (parameters != null)
            {
                dbCommand.SetParameters(parameters);
            }

            return dbCommand;
        }

        /// <summary>
        /// 执行一条查询命令。
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="database">数据库操作实例</param>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个数据读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbDataReader ExecuteReader<T>(this Database database, string sql, T parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(database, sql, parameters, dbTransaction, commandTimeout, commandType);

            if (dbTransaction != null)
            {
                return dbCommand.ExecuteReader();
            }

            return new ResultDbDataReader(dbCommand, dbCommand.ExecuteReader());
        }

        /// <summary>
        /// 执行一条非查询语句。
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="database">数据库操作实例</param>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ExecuteNonQuery<T>(this Database database, string sql, T parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = database.CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

            return dbCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// 异步执行一条查询语句。
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="database">数据库操作实例</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ExecuteReaderAsync<T>(this Database database, string sql, Action<Exception, DbDataReader> asyncCallback, T parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            AsyncTask(() =>
            {
                DbCommand dbCommand = null;
                Exception exception = null;
                DbDataReader dbDataReader = null;

                try
                {
                    dbDataReader = ExecuteReader(database, sql, parameters, dbTransaction, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    asyncCallback(exception, dbDataReader);

                    dbDataReader?.Close();

                    if (dbTransaction == null)
                    {
                        dbCommand?.Dispose();
                        dbCommand?.Connection.Close();
                    }
                }
            });
        }

        /// <summary>
        /// 异步执行一条非查询语句。
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="database">数据库操作实例</param>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ExecuteNonQueryAsync<T>(this Database database, string sql, Action<Exception, int> asyncCallback, T parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            AsyncTask(() =>
            {
                Exception exception = null;
                int rows = 0;

                try
                {
                    rows = ExecuteNonQuery(database, sql, parameters, dbTransaction, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    asyncCallback(exception, rows);
                }
            });
        }
    }
}