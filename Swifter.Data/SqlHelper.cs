using Swifter.Data.Sql;
using Swifter.Tools;
using System;
using System.Collections;
using System.Data.Common;
using System.Runtime.CompilerServices;
using static Swifter.Data.Database;

namespace Swifter.Data
{
    /// <summary>
    /// 提供 T-SQL 生成的工具方法。
    /// </summary>
    public static class SqlHelper
    {
        /// <summary>
        /// 执行动态生成的 Update 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Update 的表</param>
        /// <param name="action">初始化 Update 语句回调</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ExecuteUpdateBuild(this Database database, Table table, Action<UpdateStatement> action, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout)
        {
            var builder = database.CreateSQLBuilder();

            var update = new UpdateStatement(table);

            action(update);

            builder.BuildUpdateStatement(update);

            return database.ExecuteNonQuery(builder.ToSQL(), dbTransaction, commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Insert 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Insert 的表</param>
        /// <param name="action">初始化 Insert 语句回调</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ExecuteInsertBuild(this Database database, Table table, Action<InsertStatement> action, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout)
        {
            var builder = database.CreateSQLBuilder();

            var insert = new InsertStatement(table);

            action(insert);

            builder.BuildInsertStatement(insert);

            return database.ExecuteNonQuery(builder.ToSQL(), dbTransaction, commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Delete 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Delete 的表</param>
        /// <param name="action">初始化 Delete 语句回调</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ExecuteDeleteBuild(this Database database, Table table, Action<DeleteStatement> action, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout)
        {
            var builder = database.CreateSQLBuilder();

            var delete = new DeleteStatement(table);

            action(delete);

            builder.BuildDeleteStatement(delete);

            return database.ExecuteNonQuery(builder.ToSQL(), dbTransaction, commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <returns>返回一个数据读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbDataReader ExecuteSelectBuild(this Database database, Table table, Action<SelectStatement> action, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout)
        {
            var builder = database.CreateSQLBuilder();

            var select = new SelectStatement(table);

            action(select);

            builder.BuildSelectStatement(select);

            return database.ExecuteReader(builder.ToSQL(), dbTransaction, commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T ExecuteScalarSelectBuild<T>(this Database database, Table table, Action<SelectStatement> action, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout)
        {
            var builder = database.CreateSQLBuilder();

            var select = new SelectStatement(table);

            action(select);

            builder.BuildSelectStatement(select);

            return database.ExecuteScalar<T>(builder.ToSQL(), null, dbTransaction, commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句，并获取该查询条件下的数据总数。
        /// </summary>
        /// <typeparam name="TData">表格数据类型</typeparam>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbTransaction">事务</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <returns>返回指定类型的值和数据总数</returns>
        public static (TData Data, long Total) ExecutePagingSelectBuild<TData>(this Database database, Table table, Action<SelectStatement> action, DbTransaction dbTransaction = null, int commandTimeout = CommandTimeout)
        {
            var builder = database.CreateSQLBuilder();

            var select = new SelectStatement(table);

            action(select);

            builder.BuildSelectStatement(select);

            select.Offset = null;
            select.Limit = null;

            select.Columns.Clear();
            select.OrderBys.Clear();

            select.CountOfColumn("1", "Total");

            builder.BuildSelectStatement(select);

            return database.ExecuteScalar<TData, long>(builder.ToSQL(), null, dbTransaction, commandTimeout);
        }

        /// <summary>
        /// 为 Insert 语句设置一个赋值参数。
        /// </summary>
        /// <param name="insertStatement">Insert 语句</param>
        /// <param name="columnName">要赋值的列</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static InsertStatement Set(this InsertStatement insertStatement, string columnName, IValue value)
        {
            insertStatement.Values.Add(new AssignValue(new Column(insertStatement.Table, columnName), value));

            return insertStatement;
        }

        /// <summary>
        /// 为 Delete 语句设置一个 AND 条件。
        /// </summary>
        /// <param name="deleteStatement">Delete 语句</param>
        /// <param name="columnName">要比较的列</param>
        /// <param name="comparison">比较符</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static DeleteStatement Where(this DeleteStatement deleteStatement, string columnName, Comparisons comparison, IValue value)
        {
            deleteStatement.Where.Add(new Condition(ConditionTypes.And, comparison, new Column(deleteStatement.Table, columnName), value));

            return deleteStatement;
        }

        /// <summary>
        /// 为 Update 语句设置一个赋值参数。
        /// </summary>
        /// <param name="updateStatement">Update 语句</param>
        /// <param name="columnName">要赋值的列</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static UpdateStatement Set(this UpdateStatement updateStatement, string columnName, IValue value)
        {
            updateStatement.Values.Add(new AssignValue(new Column(updateStatement.Table, columnName), value));

            return updateStatement;
        }

        /// <summary>
        /// 为 Update 语句设置一个 AND 条件。
        /// </summary>
        /// <param name="updateStatement">Update 语句</param>
        /// <param name="columnName">要比较的列</param>
        /// <param name="comparison">比较符</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static UpdateStatement Where(this UpdateStatement updateStatement, string columnName, Comparisons comparison, IValue value)
        {
            updateStatement.Where.Add(new Condition(ConditionTypes.And, comparison, new Column(updateStatement.Table, columnName), value));

            return updateStatement;
        }

        /// <summary>
        /// 为 Select 语句的主表添加一个查询列。
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <param name="columnName">主表列</param>
        /// <param name="alias">别名</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement MainColumn(this SelectStatement selectStatement, string columnName, string alias = null)
        {
            selectStatement.Columns.Add(new SelectColumn(new Column(selectStatement.MainTable, columnName), alias));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句的主表设置一个 AND 条件。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要比较的主表列</param>
        /// <param name="comparison">比较符</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement MainWhere(this SelectStatement selectStatement, string columnName, Comparisons comparison, IValue value)
        {
            selectStatement.Where.Add(new Condition(ConditionTypes.And, comparison, new Column(selectStatement.MainTable, columnName), value));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句的主表添加一个排序。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要排序的主表列</param>
        /// <param name="direction">排序方向</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement MainOrderBy(this SelectStatement selectStatement, string columnName, OrderByDirections direction)
        {
            selectStatement.OrderBys.Add(new OrderBy(new Column(selectStatement.MainTable, columnName), direction));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句的主表添加一个分组。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要分组的主表列</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement MainGroupBy(this SelectStatement selectStatement, string columnName)
        {
            selectStatement.GroupBys.Add(new Column(selectStatement.MainTable, columnName));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句添加一个查询列。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">表</param>
        /// <param name="columnName">主表列</param>
        /// <param name="alias">别名</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Column(this SelectStatement selectStatement, ITable table, string columnName, string alias = null)
        {
            selectStatement.Columns.Add(new SelectColumn(new Column(table, columnName), alias));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句添加一个查询列。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="selectColumn">查询列信息</param>
        /// <returns></returns>
        public static SelectStatement Column(this SelectStatement selectStatement, ISelectColumn selectColumn)
        {
            selectStatement.Columns.Add(selectColumn);

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句设置一个 AND 条件。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">表</param>
        /// <param name="columnName">要比较的列</param>
        /// <param name="comparison">比较符</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Where(this SelectStatement selectStatement, ITable table, string columnName, Comparisons comparison, IValue value)
        {
            selectStatement.Where.Add(new Condition(ConditionTypes.And, comparison, new Column(table, columnName), value));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句添加一个排序。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">表</param>
        /// <param name="columnName">要排序的列</param>
        /// <param name="direction">排序方向</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement OrderBy(this SelectStatement selectStatement, ITable table, string columnName, OrderByDirections direction)
        {
            selectStatement.OrderBys.Add(new OrderBy(new Column(selectStatement.MainTable, columnName), direction));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句添加一个分组。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">表</param>
        /// <param name="columnName">要分组的列</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement GroupBy(this SelectStatement selectStatement, ITable table, string columnName)
        {
            selectStatement.GroupBys.Add(new Column(selectStatement.MainTable, columnName));

            return selectStatement;
        }

        /// <summary>
        /// 创建一个 Count 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="column">要聚合的列</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement CountOfColumn(this SelectStatement selectStatement, Column column, string alias = null) => selectStatement.Column(new CountFunction(column, alias));

        /// <summary>
        /// 创建一个 SumOf 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="column">要聚合的列</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SumOfColumn(this SelectStatement selectStatement, Column column, string alias = null) => selectStatement.Column(new CountFunction(column, alias));

        /// <summary>
        /// 创建一个 Max 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="column">要聚合的列</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement MaxOfColumn(this SelectStatement selectStatement, Column column, string alias = null) => selectStatement.Column(new CountFunction(column, alias));

        /// <summary>
        /// 创建一个 Min 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="column">要聚合的列</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement MinOfColumn(this SelectStatement selectStatement, Column column, string alias = null) => selectStatement.Column(new CountFunction(column, alias));

        /// <summary>
        /// 为 Select 语句添加一个表左关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement LeftJoin(this SelectStatement selectStatement, Table table, Action<Join> action) => Join(selectStatement, JoinDirections.Left, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表右关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement RightJoin(this SelectStatement selectStatement, Table table, Action<Join> action) => Join(selectStatement, JoinDirections.Right, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表内关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement InnerJoin(this SelectStatement selectStatement, Table table, Action<Join> action) => Join(selectStatement, JoinDirections.Inner, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表外关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement FullJoin(this SelectStatement selectStatement, Table table, Action<Join> action) => Join(selectStatement, JoinDirections.Full, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表左关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement LeftJoin(this SelectStatement selectStatement, ITable table, Action<Join> action) => Join(selectStatement, JoinDirections.Left, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表右关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement RightJoin(this SelectStatement selectStatement, ITable table, Action<Join> action) => Join(selectStatement, JoinDirections.Right, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表内关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement InnerJoin(this SelectStatement selectStatement, ITable table, Action<Join> action) => Join(selectStatement, JoinDirections.Inner, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表外关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement FullJoin(this SelectStatement selectStatement, ITable table, Action<Join> action) => Join(selectStatement, JoinDirections.Full, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表连接。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="direction">表连接方向</param>
        /// <param name="table">要连接的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        private static SelectStatement Join(SelectStatement selectStatement, JoinDirections direction, ITable table, Action<Join> action)
        {
            var join = new Join(direction, table);

            selectStatement.Joins.Add(join);

            action(join);

            return selectStatement;
        }

        /// <summary>
        /// 为条件集合添加一个 And 条件。
        /// </summary>
        /// <param name="conditions">条件集合</param>
        /// <param name="before">前置值</param>
        /// <param name="comparisons">比较符</param>
        /// <param name="after">后置值</param>
        /// <returns>返回当前实例</returns>
        public static Conditions And(this Conditions conditions, IValue before, Comparisons comparisons, IValue after)
        {
            conditions.Add(new Condition(ConditionTypes.And, comparisons, before, after));

            return conditions;
        }

        /// <summary>
        /// 为条件集合添加一个 Or 条件。
        /// </summary>
        /// <param name="conditions">条件集合</param>
        /// <param name="before">前置值</param>
        /// <param name="comparisons">比较符</param>
        /// <param name="after">后置值</param>
        /// <returns>返回当前实例</returns>
        public static Conditions Or(this Conditions conditions, IValue before, Comparisons comparisons, IValue after)
        {
            conditions.Add(new Condition(ConditionTypes.Or, comparisons, before, after));

            return conditions;
        }

        /// <summary>
        /// 为表连接信息添加一个条件。
        /// </summary>
        /// <param name="join">表连接信息</param>
        /// <param name="conditionExpression">"Regular Format :\[(?&lt;Index&gt;[0-9a-fA-F]+)?(?&lt;Type&gt;&amp;|\||And|Or)?(?&lt;Operator&gt;[A-Za-z!=&gt;&lt;]+)\](?&lt;Name&gt;[A-Za-z_0-9]+)"</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static Join On(this Join join, string conditionExpression, IValue value)
        {
            var condition = DecodeConditionExpression(join.Table, conditionExpression);

            if (condition == null)
            {
                throw new ArgumentException("Regular Format : \"\\[(?<Index>[0-9]+)?(?<Type>&|\\||And|Or)?(?<Operator>[A-Za-z!=><]+)\\](?<Name>[A-Za-z_0-9]+)\"", nameof(conditionExpression));
            }

            condition.After = value;

            join.On.Add(condition);

            return join;
        }
        
        /// <summary>
        /// 为 Select 语句的主表设置一个 AND 条件。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="conditionExpression">"Regular Format :\[(?&lt;Index&gt;[0-9]+)?(?&lt;Type&gt;&amp;|\||And|Or)?(?&lt;Operator&gt;[A-Za-z!=&gt;&lt;]+)\](?&lt;Name&gt;[A-Za-z_0-9]+)"</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement MainWhere(this SelectStatement selectStatement, string conditionExpression, IValue value)
        {
            var condition = DecodeConditionExpression(selectStatement.MainTable, conditionExpression);

            if (condition == null)
            {
                throw new ArgumentException("Regular Format : \"\\[(?<Index>[0-9]+)?(?<Type>&|\\||And|Or)?(?<Operator>[A-Za-z!=><]+)\\](?<Name>[A-Za-z_0-9]+)\"", nameof(conditionExpression));
            }

            condition.After = value;

            selectStatement.Where.Add(condition);

            return selectStatement;
        }

        private unsafe static Condition DecodeConditionExpression(ITable table, string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return null;
            }

            if (expression[0] != '[')
            {
                return null;
            }

            var length = expression.Length;

            fixed (char* pExpression = expression)
            {
                var indexLength = 0;

                var i = 1;

                for (; i < length; i++)
                {
                    var c = pExpression[i];

                    if (c >= '0' && c <= '9')
                    {
                        ++indexLength;
                    }
                    else
                    {
                        break;
                    }
                }

                string index = null;

                if (indexLength != 0)
                {
                    index = expression.Substring(1, indexLength);
                }

                var conditionType = ConditionTypes.And;

                if (i + 3 >= length)
                {
                    return null;
                }

                switch (pExpression[i])
                {
                    case '&':
                        ++i;
                        break;
                    case '|':
                        ++i;
                        conditionType = ConditionTypes.Or;
                        break;
                    case 'a':
                    case 'A':
                        if (StringHelper.IgnoreCaseEquals(pExpression, i, expression.Length, "AND"))
                        {
                            i += 3;
                        }
                        break;
                    case 'o':
                    case 'O':
                        if (StringHelper.IgnoreCaseEquals(pExpression, i, expression.Length, "OR"))
                        {
                            i += 2;
                            conditionType = ConditionTypes.Or;
                        }
                        break;
                    case 'n':
                    case 'N':
                        if (indexLength != 0 && StringHelper.ToUpper(index[indexLength]) == 'A' && StringHelper.IgnoreCaseEquals(pExpression, i, expression.Length, "ND"))
                        {
                            i += 2;

                            --indexLength;

                            index = indexLength == 0 ? null : index.Substring(0, indexLength);
                        }
                        break;
                }

                var comparisonStart = i;

                for (; i < length; i++)
                {
                    if (pExpression[i] == ']')
                    {
                        break;
                    }
                }

                var comparisonName = expression.Substring(comparisonStart, i - comparisonStart);

                var comparison = ComparisonAttribute.GetComparison(comparisonName);

                ++i;

                var columnName = expression.Substring(i);

                return new Condition(index ?? "0", conditionType, comparison, new Column(table, columnName), null);
            }
        }

        /// <summary>
        /// 为 Select 语句设置分页参数。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="offset">偏移行数</param>
        /// <param name="limit">行数数量</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Paging(this SelectStatement selectStatement, int offset, int limit)
        {
            selectStatement.Offset = offset;
            selectStatement.Limit = limit;

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句设置一个子查询列。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">子查询表</param>
        /// <param name="action">子查询初始化回调</param>
        /// <param name="alias">子查询列别名</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement SubQueryColumn(this SelectStatement selectStatement, Table table, Action<SelectStatement> action, string alias = null)
        {
            var subQuery = new SelectStatement(table);

            selectStatement.Columns.Add(new SubQueryColumn(subQuery, alias));

            action(subQuery);

            return selectStatement;
        }

        static IValue BoxValue<T>(T value) => new ConstantValue<T>(value);

        /// <summary>
        /// 创建一个 String 值。
        /// </summary>
        /// <param name="value">String</param>
        /// <returns>返回值</returns>
        public static IValue ValueOf(string value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Boolean 值。
        /// </summary>
        /// <param name="value">Boolean</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(bool value) => BoxValue(value);

        /// <summary>
        /// 创建一个 SByte 值。
        /// </summary>
        /// <param name="value">SByte</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(sbyte value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Int16 值。
        /// </summary>
        /// <param name="value">Int16</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(short value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Int32 值。
        /// </summary>
        /// <param name="value">Int32</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(int value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Int64 值。
        /// </summary>
        /// <param name="value">Int64</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(long value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Byte 值。
        /// </summary>
        /// <param name="value">Byte</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(byte value) => BoxValue(value);

        /// <summary>
        /// 创建一个 UInt16 值。
        /// </summary>
        /// <param name="value">UInt16</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(ushort value) => BoxValue(value);

        /// <summary>
        /// 创建一个 UInt32 值。
        /// </summary>
        /// <param name="value">UInt32</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(uint value) => BoxValue(value);

        /// <summary>
        /// 创建一个 UInt64 值。
        /// </summary>
        /// <param name="value">UInt64</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(ulong value) => BoxValue(value);

        /// <summary>
        /// 创建一个 String 值。
        /// </summary>
        /// <param name="value">String</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(char value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Single 值。
        /// </summary>
        /// <param name="value">Single</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(float value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Double 值。
        /// </summary>
        /// <param name="value">Double</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(double value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Decimal 值。
        /// </summary>
        /// <param name="value">Decimal</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(decimal value) => BoxValue(value);

        /// <summary>
        /// 创建一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(DateTime value) => BoxValue(value);

        /// <summary>
        /// 创建一个 DateTimeOffset 值。
        /// </summary>
        /// <param name="value">DateTimeOffset</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(DateTimeOffset value) => BoxValue(value);

        /// <summary>
        /// 创建一个 Guid 值。
        /// </summary>
        /// <param name="value">Guid</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(Guid value) => BoxValue(value);

        /// <summary>
        /// 创建一个可空值类型的值。
        /// </summary>
        /// <param name="value">可空值类型</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf<T>(T? value) where T : struct
        {
            if (value == null)
                return BoxValue(DBNull.Value);

            switch (TypeInfo<T>.TypeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return BoxValue(value.Value);
            }

            return ValueOf(value.Value);
        }

        /// <summary>
        /// 创建一个对象值。
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(object value)
        {
            if (value == null)
                return BoxValue(DBNull.Value);

            if (value is IValue @as)
                return @as;

            if (value is IConvertible convertible)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return BoxValue((bool)value);
                    case TypeCode.Char:
                        return BoxValue((char)value);
                    case TypeCode.SByte:
                        return BoxValue((byte)value);
                    case TypeCode.Byte:
                        return BoxValue((byte)value);
                    case TypeCode.Int16:
                        return BoxValue((short)value);
                    case TypeCode.UInt16:
                        return BoxValue((ushort)value);
                    case TypeCode.Int32:
                        return BoxValue((int)value);
                    case TypeCode.UInt32:
                        return BoxValue((uint)value);
                    case TypeCode.Int64:
                        return BoxValue((long)value);
                    case TypeCode.UInt64:
                        return BoxValue((ulong)value);
                    case TypeCode.Single:
                        return BoxValue((float)value);
                    case TypeCode.Double:
                        return BoxValue((double)value);
                    case TypeCode.Decimal:
                        return BoxValue((decimal)value);
                    case TypeCode.DateTime:
                        return BoxValue((DateTime)value);
                    case TypeCode.String:
                        return BoxValue((string)value);
                }
            }

            if (value is DateTimeOffset dateTimeOffset)
                return BoxValue(dateTimeOffset);

            if (value is Guid guid)
                return BoxValue(guid);

            if (value is ICollection collection)
            {
                var array = new IValue[collection.Count];

                var index = 0;

                foreach (var item in collection)
                {
                    array[index] = ValueOf(item);

                    ++index;
                }

                return BoxValue(array);
            }

            return BoxValue(value.ToString());
        }

        /// <summary>
        /// 创建一个数组值。
        /// </summary>
        /// <param name="value">数组</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(object[] value)
        {
            if (value == null)
                return BoxValue(DBNull.Value);

            var array = new IValue[value.Length];

            for (int i = value.Length - 1; i >= 0; --i)
            {
                array[i] = ValueOf(value[i]);
            }

            return BoxValue(array);
        }

        /// <summary>
        /// 创建一个列信息。
        /// </summary>
        /// <param name="table">列所在表</param>
        /// <param name="columnName">列名</param>
        /// <returns>返回一个列信息</returns>
        public static Column ColumnOf(ITable table, string columnName) => new Column(table, columnName);

        /// <summary>
        /// 为 Select 语句的主表创建一个列信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">列名</param>
        /// <returns>返回一个列信息</returns>
        public static Column ColumnOfMain(this SelectStatement selectStatement, string columnName) => new Column(selectStatement.MainTable, columnName);

        /// <summary>
        /// 为 Join 信息的主表创建一个列信息。
        /// </summary>
        /// <param name="join">Select 语句</param>
        /// <param name="columnName">列名</param>
        /// <returns>返回一个列信息</returns>
        public static Column ColumnOf(this Join join, string columnName) => new Column(join.Table, columnName);
    }
}