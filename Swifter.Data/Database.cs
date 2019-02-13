using Swifter.RW;
using Swifter.Tools;
using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Swifter.Data.Sql;
using static Swifter.Data.DbHelper;

namespace Swifter.Data
{
    /// <summary>
    /// 提供数据库操作的工具方法。
    /// </summary>
    public sealed class Database
    {
        /// <summary>
        /// 默认命令超时时间。
        /// </summary>
        internal const int CommandTimeout = 30;
        
        readonly SqlBuilder SqlBuilder;

        /// <summary>
        /// 数据库供应者工厂。
        /// </summary>
        public readonly DbProviderFactory DbProviderFactory;

        /// <summary>
        /// 数据库连接字符串。
        /// </summary>
        public readonly string DbConnectionString;

        /// <summary>
        /// 初始化数据库连接字符串。
        /// </summary>
        /// <param name="providerName">数据库供应者的包名</param>
        /// <param name="dbConnectionString">数据库连接字符串</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public Database(string providerName, string dbConnectionString)
        {
            DbProviderFactory = GetFactory(providerName);

            DbConnectionString = dbConnectionString;

            SqlBuilder = GetSQLBuilder(providerName);
        }

        /// <summary>
        /// 初始化数据库连接字符串。
        /// </summary>
        /// <param name="configName">数据库连接的配置项名称</param>
        public Database(string configName)
        {
            var config = System.Configuration.ConfigurationManager.ConnectionStrings[configName];

            var providerName = config.ProviderName;
            var connectionString = config.ConnectionString;

            DbProviderFactory = GetFactory(providerName);

            DbConnectionString = connectionString;

            SqlBuilder = GetSQLBuilder(providerName);
        }

        /// <summary>
        /// 打开一个新的数据库连接。
        /// </summary>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DbConnection OpenConnection()
        {
            var dbConnection = DbProviderFactory.CreateConnection();

            dbConnection.ConnectionString = DbConnectionString;

            dbConnection.Open();

            return dbConnection;
        }
        
        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个命令</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DbCommand CreateCommand(string sql, object parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbConnection = dbTransaction?.Connection;

            if (dbConnection == null)
            {
                dbConnection = OpenConnection();
            }

            var dbCommand = dbConnection.CreateCommand();

            dbCommand.CommandText = sql;

            dbCommand.CommandTimeout = commandTimeout;

            dbCommand.CommandType = commandType;

            dbCommand.Transaction = dbTransaction;

            if (parameters != null)
            {
                dbCommand.SetParameters(parameters);
            }

            return dbCommand;
        }

        private static readonly NameCache<string[]> nameCache = new NameCache<string[]>();

        /// <summary>
        /// 创建一个命令
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个命令</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DbCommand CreateCommand(string sql, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            return CreateCommand(sql, null, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 将数据读取器的当前结果集映射为指定类的实例。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="dbDataReader">数据读取器</param>
        /// <returns>返回该类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static T ReadScalar<T>(DbDataReader dbDataReader)
        {
            return ValueInterface<T>.Content.ReadValue(new ReadScalarReader(dbDataReader));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static (T1 Result1, T2 Result2) ReadScalar<T1, T2>(DbDataReader dbDataReader)
        {
            var item1 = ReadScalar<T1>(dbDataReader);

            dbDataReader.NextResult();

            var item2 = ReadScalar<T2>(dbDataReader);

            return (item1, item2);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static (T1 Result1, T2 Result2, T3 Result3) ReadScalar<T1, T2, T3>(DbDataReader dbDataReader)
        {
            var item1 = ReadScalar<T1>(dbDataReader);

            dbDataReader.NextResult();

            var item2 = ReadScalar<T2>(dbDataReader);

            dbDataReader.NextResult();

            var item3 = ReadScalar<T3>(dbDataReader);

            return (item1, item2, item3);
        }

        /// <summary>
        /// 执行一条查询命令。
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个数据读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DbDataReader ExecuteReader(string sql, object parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

            if (dbTransaction != null)
            {
                return dbCommand.ExecuteReader();
            }

            return new ResultDbDataReader(dbCommand, dbCommand.ExecuteReader());
        }

        /// <summary>
        /// 执行一条查询命令。
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个数据读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DbDataReader ExecuteReader(string sql, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            return ExecuteReader(sql, (object)null, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行一条查询语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T ExecuteScalar<T>(string sql, object parameters = null, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

            if (dbTransaction != null)
            {
                using (var dataReader = dbCommand.ExecuteReader())
                {
                    return ReadScalar<T>(dataReader);
                }
            }

            using (dbCommand.Connection)
            {
                using (dbCommand)
                {
                    using (var dataReader = dbCommand.ExecuteReader())
                    {
                        return ReadScalar<T>(dataReader);
                    }
                }
            }
        }

        /// <summary>
        /// 执行一个 T-SQL 代码，并返回两个表的返回值。
        /// </summary>
        /// <typeparam name="T1">表 1 返回值类型</typeparam>
        /// <typeparam name="T2">表 2 返回值类型</typeparam>
        /// <param name="sql">T-SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个元组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public (T1 Result1, T2 Result2) ExecuteScalar<T1, T2>(string sql, object parameters = null, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

            if (dbTransaction != null)
            {
                using (var dataReader = dbCommand.ExecuteReader())
                {
                    return ReadScalar<T1, T2>(dataReader);
                }
            }

            using (dbCommand.Connection)
            {
                using (dbCommand)
                {
                    using (var dataReader = dbCommand.ExecuteReader())
                    {
                        return ReadScalar<T1, T2>(dataReader);
                    }
                }
            }
        }

        /// <summary>
        /// 执行一个 T-SQL 代码，并返回两个表的返回值。
        /// </summary>
        /// <typeparam name="T1">表 1 返回值类型</typeparam>
        /// <typeparam name="T2">表 2 返回值类型</typeparam>
        /// <typeparam name="T3">表 3 返回值类型</typeparam>
        /// <param name="sql">T-SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个元组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public (T1 Result1, T2 Result2, T3 Result3) ExecuteScalar<T1, T2, T3>(string sql, object parameters = null, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

            if (dbTransaction != null)
            {
                using (var dataReader = dbCommand.ExecuteReader())
                {
                    return ReadScalar<T1, T2, T3>(dataReader);
                }
            }

            using (dbCommand.Connection)
            {
                using (dbCommand)
                {
                    using (var dataReader = dbCommand.ExecuteReader())
                    {
                        return ReadScalar<T1, T2, T3>(dataReader);
                    }
                }
            }
        }
        
        /// <summary>
        /// 执行一条非查询语句。
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ExecuteNonQuery(string sql, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            return ExecuteNonQuery(sql, (object)null, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行一条非查询语句。
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ExecuteNonQuery(string sql, object parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

            return dbCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// 异步执行一条查询语句。
        /// </summary>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ExecuteReaderAsync(string sql, Action<Exception, DbDataReader> asyncCallback, object parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            AsyncTask(() =>
            {
                DbCommand dbCommand = null;
                Exception exception = null;
                DbDataReader dbDataReader = null;

                try
                {
                    dbCommand = CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

                    dbDataReader = dbCommand.ExecuteReader();

                    throw new Exception();
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
        /// 异步执行一条查询语句。
        /// </summary>
        /// <param name="sql">SQL 语句</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ExecuteReaderAsync(string sql, Action<Exception, DbDataReader> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            ExecuteReaderAsync(sql, asyncCallback, (object)null, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 异步执行一条查询语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ExecuteScalarAsync<T>(string sql, Action<Exception, T> asyncCallback, object parameters = null, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            AsyncTask(() =>
            {
                Exception exception = null;
                T result = default(T);

                try
                {
                    result = ExecuteScalar<T>(sql, parameters, dbTransaction, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    asyncCallback(exception, result);
                }
            });
        }

        /// <summary>
        /// 异步执行一个 T-SQL 代码，并返回两个表的返回值。
        /// </summary>
        /// <typeparam name="T1">表 1 返回值类型</typeparam>
        /// <typeparam name="T2">表 2 返回值类型</typeparam>
        /// <param name="sql">T-SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ExecuteScalarAsync<T1, T2>(string sql, Action<Exception, T1, T2> asyncCallback, object parameters = null, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            AsyncTask(() =>
            {
                Exception exception = null;

                var tuple = new ValueTuple<T1, T2>(default(T1), default(T2));

                try
                {
                    tuple = ExecuteScalar<T1, T2>(sql, parameters, dbTransaction, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    asyncCallback(exception, tuple.Item1, tuple.Item2);
                }
            });
        }

        /// <summary>
        /// 异步执行一个 T-SQL 代码，并返回两个表的返回值。
        /// </summary>
        /// <typeparam name="T1">表 1 返回值类型</typeparam>
        /// <typeparam name="T2">表 2 返回值类型</typeparam>
        /// <typeparam name="T3">表 3 返回值类型</typeparam>
        /// <param name="sql">T-SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个元组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ExecuteScalarAsync<T1, T2, T3>(string sql, Action<Exception, T1, T2, T3> asyncCallback, object parameters = null, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            AsyncTask(() =>
            {
                Exception exception = null;

                var tuple = new ValueTuple<T1, T2, T3>(default(T1), default(T2), default(T3));

                try
                {
                    tuple = ExecuteScalar<T1, T2, T3>(sql, parameters, dbTransaction, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    asyncCallback(exception, tuple.Item1, tuple.Item2, tuple.Item3);
                }
            });
        }

        /// <summary>
        /// 异步执行一条非查询语句。
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ExecuteNonQueryAsync(string sql, Action<Exception, int> asyncCallback, object parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            AsyncTask(() =>
            {
                Exception exception = null;
                int rows = 0;

                try
                {
                    rows = ExecuteNonQuery(sql, parameters, dbTransaction, commandTimeout, commandType);
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

        /// <summary>
        /// 异步执行一条非查询语句。
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ExecuteNonQueryAsync(string sql, Action<Exception, int> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            ExecuteNonQueryAsync(sql, asyncCallback, (object)null, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 获取当前供应商的 T-SQL 生成器。
        /// </summary>
        /// <returns>返回 T-SQL 生成器</returns>
        /// <exception cref="NotSupportedException">当不支持该供应商时发生。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public SqlBuilder CreateSQLBuilder()
        {
            if (SqlBuilder == null)
            {
                throw new NotSupportedException("T-SQL builder does not support the provider.");
            }

            return SqlBuilder.CreateInstance();
        }
    }
}