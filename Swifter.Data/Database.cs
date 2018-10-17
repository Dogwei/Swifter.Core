using Swifter.Readers;
using Swifter.RW;
using Swifter.Writers;
using System;
using System.Data;
using System.Data.Common;

namespace Swifter.Data
{
    /// <summary>
    /// 提供数据库操作的实例方法。
    /// </summary>
    public sealed class Database
    {
        /// <summary>
        /// 默认命令超时时间。
        /// </summary>
        public const int CommandTimeout = 30;



#if NET20 || NET30 || NET35 || NET40
        private static void StartTask(System.Threading.ThreadStart action)
        {
            new System.Threading.Thread(action).Start();
        }
#else
        private static void StartTask(Action action)
        {
            new System.Threading.Tasks.Task(action).Start();
        }
#endif

        /// <summary>
        /// 数据库工厂实例。
        /// </summary>
        public readonly DbProviderFactory DbProviderFactory;
        /// <summary>
        /// 数据库连接字符串。
        /// </summary>
        public readonly string DbConnectionString;

        /// <summary>
        /// 初始化数据库连接字符串。
        /// </summary>
        /// <param name="providerName">数据库工厂名称</param>
        /// <param name="dbConnectionString">数据库连接字符串</param>
        public Database(string providerName, string dbConnectionString)
        {
            DbProviderFactory = DbHelper.GetProviderFactory(providerName);

            this.DbConnectionString = dbConnectionString;
        }

        /// <summary>
        /// 打开一个新的数据库连接。
        /// </summary>
        /// <returns></returns>
        public DbConnection OpenConnection()
        {
            var dbConnection = DbProviderFactory.CreateConnection();

            dbConnection.ConnectionString = DbConnectionString;

            dbConnection.Open();

            return dbConnection;
        }

        /// <summary>
        /// 打开一个新的数据库连接，并开启事务。
        /// </summary>
        /// <returns></returns>
        public DbTransaction BeginTransaction()
        {
            return OpenConnection().BeginTransaction();
        }

        private DbCommand CreateCommand<T>(string sql, T parameters, DbTransaction dbTransaction, int commandTimeout, CommandType commandType)
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
                DbHelper.SetParameters(dbCommand, parameters);
            }

            return dbCommand;
        }

        private T ReadScalar<T>(DbDataReader dbDataReader)
        {
            using (dbDataReader)
            {
                if (typeof(T).IsAssignableFrom(dbDataReader.GetFieldType(0)))
                {
                    if (dbDataReader.Read())
                    {
                        return (T)dbDataReader[0];
                    }

                    return default(T);
                }

                var overrideDbDataReader = new OverrideDbDataReader(dbDataReader);

                var dataWriter = RWHelper.CreateWriter<T>();

                dataWriter.Initialize();

                if (dataWriter is IDataWriter<int>)
                {
                    RWHelper.Copy(overrideDbDataReader, dataWriter);

                    return RWHelper.GetContent<T>(dataWriter);
                }

                if (overrideDbDataReader.Read())
                {
                    RWHelper.Copy((IDataReader<string>)overrideDbDataReader, dataWriter);

                    return RWHelper.GetContent<T>(dataWriter);
                }

                return default(T);
            }
        }

        /// <summary>
        /// 执行一条查询命令。
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个数据读取器</returns>
        public DbDataReader ExecuteReader<T>(string sql, T parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
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
        /// <typeparam name="TParameters">参数类型</typeparam>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回指定类型的值</returns>
        public T ExecuteScalar<T, TParameters>(string sql, TParameters parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

            if (dbTransaction != null)
            {
                return ReadScalar<T>(dbCommand.ExecuteReader());
            }

            using (dbCommand.Connection)
            {
                using (dbCommand)
                {
                    return ReadScalar<T>(dbCommand.ExecuteReader());
                }
            }
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
        public T ExecuteScalar<T>(string sql, object parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalar<T, object>(sql, parameters, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行一条查询语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">SQL 代码</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回指定类型的值</returns>
        public T ExecuteScalar<T>(string sql, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalar<T, object>(sql, null, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行一条非查询语句。
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        public int ExecuteNonQuery<T>(string sql, T parameters, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbTransaction, commandTimeout, commandType);

            return dbCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行一条非查询语句。
        /// </summary>
        /// <param name="sql">SQL 代码</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        public int ExecuteNonQuery(string sql, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            return ExecuteNonQuery(sql, (object)null, dbTransaction, commandTimeout, commandType);
        }
        
        /// <summary>
        /// 异步执行一条查询语句。
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        public void ExecuteReaderAsync<T>(string sql, T parameters, Action<Exception, DbDataReader> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            StartTask(() =>
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
        public void ExecuteReaderAsync(string sql, Action<Exception, DbDataReader> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            ExecuteReaderAsync(sql,(object)null, asyncCallback, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行一条查询语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <typeparam name="TParameters">参数类型</typeparam>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        public void ExecuteScalarAsync<T, TParameters>(string sql, TParameters parameters, Action<Exception, T> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            StartTask(() =>
            {
                Exception exception = null;
                T result = default(T);

                try
                {
                    result = ExecuteScalar<T, TParameters>(sql, parameters, dbTransaction, commandTimeout, commandType);
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
        public void ExecuteScalarAsync<T>(string sql, object parameters, Action<Exception, T> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            ExecuteScalarAsync<T, object>(sql, parameters, asyncCallback, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 异步执行一条查询语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">SQL 语句</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        public void ExecuteScalarAsync<T>(string sql, Action<Exception, T> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            ExecuteScalarAsync<T, object>(sql, null, asyncCallback, dbTransaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 异步执行一条非查询语句。
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="sql">SQL 代码</param>
        /// <param name="parameters">参数</param>
        /// <param name="asyncCallback">回调函数</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        public void ExecuteNonQueryAsync<T>(string sql, T parameters, Action<Exception, int> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            StartTask(() =>
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
        public void ExecuteNonQueryAsync(string sql, Action<Exception, int> asyncCallback, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout, CommandType commandType = CommandType.Text)
        {
            ExecuteNonQueryAsync(sql, (object)null, asyncCallback, dbTransaction, commandTimeout, commandType);
        }
    }
}