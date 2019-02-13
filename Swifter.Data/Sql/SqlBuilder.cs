using Swifter.Reflection;
using Swifter.Tools;
using System;
using System.Collections;
using System.Text;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// T-SQL 生成器父类。
    /// </summary>
    public abstract class SqlBuilder
    {
        #region - Constant -

        /// <summary>
        /// T
        /// </summary>
        protected const string Code_T = "T";
        /// <summary>
        /// O
        /// </summary>
        protected const string Code_O = "O";
        /// <summary>
        /// =
        /// </summary>
        protected const string Code_Equal = "=";
        /// <summary>
        /// &lt;&gt;
        /// </summary>
        protected const string Code_NotEqual = "<>";
        /// <summary>
        /// &gt;
        /// </summary>
        protected const string Code_GreaterThan = ">";
        /// <summary>
        /// &lt;
        /// </summary>
        protected const string Code_LessThan = "<";
        /// <summary>
        /// ||
        /// </summary>
        protected const string Code_DoubleVertical = "||";
        /// <summary>
        /// SELECT
        /// </summary>
        protected const string Code_Select = "SELECT";
        /// <summary>
        /// 1 = 1
        /// </summary>
        protected const string Code_True_Expression = "1 = 1";
        /// <summary>
        /// 1 &lt;&gt; 1
        /// </summary>
        protected const string Code_False_Expression = "1 <> 1";
        /// <summary>
        /// %
        /// </summary>
        protected const string Code_Percent = "%";
        /// <summary>
        /// NOT
        /// </summary>
        protected const string Code_Not = "NOT";
        /// <summary>
        /// IS
        /// </summary>
        protected const string Code_Is = "IS";
        /// <summary>
        /// AND
        /// </summary>
        protected const string Code_And = "AND";
        /// <summary>
        /// ORDER
        /// </summary>
        protected const string Code_Order = "ORDER";
        /// <summary>
        /// GROUP
        /// </summary>
        protected const string Code_Group = "GROUP";
        /// <summary>
        /// HAVING
        /// </summary>
        protected const string Code_Having = "HAVING";
        /// <summary>
        /// \r\n
        /// </summary>
        protected const string Code_WrapLine = "\r\n";
        /// <summary>
        /// LIKE
        /// </summary>
        protected const string Code_Like = "LIKE";
        /// <summary>
        /// BY
        /// </summary>
        protected const string Code_By = "BY";
        /// <summary>
        /// ASC
        /// </summary>
        protected const string Code_ASC = "ASC";
        /// <summary>
        /// DESC
        /// </summary>
        protected const string Code_DESC = "DESC";
        /// <summary>
        /// OR
        /// </summary>
        protected const string Code_Or = "OR";
        /// <summary>
        /// NULL
        /// </summary>
        protected const string Code_Null = "NULL";
        /// <summary>
        /// FROM
        /// </summary>
        protected const string Code_From = "FROM";
        /// <summary>
        /// WHERE
        /// </summary>
        protected const string Code_Where = "WHERE";
        /// <summary>
        /// LEFT
        /// </summary>
        protected const string Code_Left = "LEFT";
        /// <summary>
        /// RIGHT
        /// </summary>
        protected const string Code_Right = "RIGHT";
        /// <summary>
        /// INNER
        /// </summary>
        protected const string Code_Inner = "INNER";
        /// <summary>
        /// OUTER
        /// </summary>
        protected const string Code_Outer = "OUTER";
        /// <summary>
        /// Full
        /// </summary>
        protected const string Code_Full = "FULL";
        /// <summary>
        /// ALL
        /// </summary>
        protected const string Code_All = "ALL";
        /// <summary>
        /// JOIN
        /// </summary>
        protected const string Code_Join = "JOIN";
        /// <summary>
        /// ON
        /// </summary>
        protected const string Code_On = "ON";
        /// <summary>
        /// ON
        /// </summary>
        protected const string Code_Concat = "CONCAT";
        /// <summary>
        ///  
        /// </summary>
        protected const string Code_Space = " ";
        /// <summary>
        /// ,
        /// </summary>
        protected const string Code_Comma = ",";
        /// <summary>
        /// ;
        /// </summary>
        protected const string Code_Semicolon = ";";
        /// <summary>
        /// .
        /// </summary>
        protected const string Code_Dot = ".";
        /// <summary>
        /// *
        /// </summary>
        protected const string Code_Asterisk = "*";
        /// <summary>
        /// &amp;
        /// </summary>
        protected const string Code_Amp = "&";
        /// <summary>
        /// |
        /// </summary>
        protected const string Code_Vertical = "|";
        /// <summary>
        /// [
        /// </summary>
        protected const string Code_Square_Brackets_Begin = "[";
        /// <summary>
        /// ]
        /// </summary>
        protected const string Code_Square_Brackets_End = "]";
        /// <summary>
        /// (
        /// </summary>
        protected const string Code_Parenthesis_Bracket_Begin = "(";
        /// <summary>
        /// )
        /// </summary>
        protected const string Code_Parenthesis_Bracket_End = ")";
        /// <summary>
        /// {
        /// </summary>
        protected const string Code_Angle_Bracket_Begin = "{";
        /// <summary>
        /// }
        /// </summary>
        protected const string Code_Angle_Bracket_End = "}";
        /// <summary>
        /// OFFSET
        /// </summary>
        protected const string Code_Offset = "OFFSET";
        /// <summary>
        /// ROWS
        /// </summary>
        protected const string Code_Rows = "ROWS";
        /// <summary>
        /// FETCH
        /// </summary>
        protected const string Code_Fetch = "FETCH";
        /// <summary>
        /// NEXT
        /// </summary>
        protected const string Code_Next = "NEXT";
        /// <summary>
        /// ONLY
        /// </summary>
        protected const string Code_Only = "ONLY";
        /// <summary>
        /// IN
        /// </summary>
        protected const string Code_In = "IN";
        /// <summary>
        /// BETWEEN
        /// </summary>
        protected const string Code_Between = "BETWEEN";
        /// <summary>
        /// CREATE
        /// </summary>
        protected const string Code_Create = "CREATE";
        /// <summary>
        /// DELETE
        /// </summary>
        protected const string Code_Delete = "DELETE";
        /// <summary>
        /// UPDATE
        /// </summary>
        protected const string Code_Update = "UPDATE";
        /// <summary>
        /// SET
        /// </summary>
        protected const string Code_Set = "SET";
        /// <summary>
        /// INSERT
        /// </summary>
        protected const string Code_Insert = "INSERT";
        /// <summary>
        /// INTO
        /// </summary>
        protected const string Code_Into = "INTO";
        /// <summary>
        /// VALUES
        /// </summary>
        protected const string Code_Values = "VALUES";
        /// <summary>
        /// DROP
        /// </summary>
        protected const string Code_Drop = "DROP";
        /// <summary>
        /// TABLE
        /// </summary>
        protected const string Code_Table = "TABLE";
        /// <summary>
        /// INDEX
        /// </summary>
        protected const string Code_Index = "INDEX";
        /// <summary>
        /// WHEN
        /// </summary>
        protected const string Code_When = "WHEN";
        /// <summary>
        /// THEN
        /// </summary>
        protected const string Code_Then = "THEN";
        /// <summary>
        /// CASE
        /// </summary>
        protected const string Code_Case = "CASE";
        /// <summary>
        /// END
        /// </summary>
        protected const string Code_End = "END";
        /// <summary>
        /// TOP
        /// </summary>
        protected const string Code_Top = "TOP";
        /// <summary>
        /// 1
        /// </summary>
        protected const string Code_One = "1";
        /// <summary>
        /// 0
        /// </summary>
        protected const string Code_Zero = "0";
        /// <summary>
        /// SUM
        /// </summary>
        protected const string Code_Sum = "SUM";
        /// <summary>
        /// COUNT
        /// </summary>
        protected const string Code_Count = "COUNT";
        /// <summary>
        /// MAX
        /// </summary>
        protected const string Code_Max = "MAX";
        /// <summary>
        /// MIN
        /// </summary>
        protected const string Code_Min = "MIN";

        #endregion

        /// <summary>
        /// 十六进制进阶数字。
        /// </summary>
        protected static readonly int[] HexDigital = { 0xf, 0xf, 0xf0, 0xf00, 0xf000, 0xf0000, 0xf00000, 0xf000000 };

        /// <summary>
        /// %
        /// </summary>
        protected static readonly ConstantValue<string> Value_Percent = new ConstantValue<string>(Code_Percent);

        readonly ReferenceCache<string> AliasCache;
        readonly XTypeInfo XTypeInfo;
        readonly StringBuilder Builder;
        
        int aliasIndex;

        /// <summary>
        /// 初始化必要的构造参数。
        /// </summary>
        public SqlBuilder()
        {
            AliasCache = new ReferenceCache<string>();
            Builder = new StringBuilder();

            XTypeInfo = XTypeInfo.Create(GetType(), XBindingFlags.Default | XBindingFlags.NonPublic);

            aliasIndex = 0;
        }

        /// <summary>
        /// 分配对象别名。
        /// 注: 有别名的表中的列会自动附带表的别名。
        /// </summary>
        /// <param name="obj">对象</param>
        public void MakeAlias(object obj)
        {
            if (!AliasCache.TryGetValue(obj, out var alias))
            {
                var type = Code_O;

                if (obj is ITable)
                {
                    type = Code_T;
                }

                alias = $"{type}{++aliasIndex}";

                AliasCache.DirectAdd(obj, alias);
            }
        }

        /// <summary>
        /// 获取对象别名。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回该对象的别名或 Null</returns>
        public string GetAlias(object obj)
        {
            if (AliasCache.TryGetValue(obj, out var alias))
            {
                return alias;
            }

            return null;
        }

        /// <summary>
        /// 执行当前实例指定名称并且参数与之对应的实例方法。
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameter">参数</param>
        protected void Invoke(string methodName, object parameter)
        {
            var parameters = new object[] { parameter };

            var method = XTypeInfo.GetMethod(methodName, parameters);

            if (method == null)
            {
                throw new NotSupportedException();
            }

            method.Invoke(this, parameters);
        }

        /// <summary>
        /// 执行当前实例指定名称并且参数与之对应的实例方法。
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="before">参数 1</param>
        /// <param name="after">参数 2</param>
        protected void Invoke(string methodName, object before, object after)
        {
            var parameters = new object[] { before, after };

            var method = XTypeInfo.GetMethod(methodName, parameters);

            if (method == null)
            {
                throw new NotSupportedException();
            }

            method.Invoke(this, parameters);
        }
        
        /// <summary>
        /// 向后拼接 Code。
        /// </summary>
        /// <param name="code">Code</param>
        protected void BuildCode(string code)
        {
            Builder.Append(code);
        }

        /// <summary>
        /// 向后拼接一个名称。
        /// </summary>
        /// <param name="name">名称</param>
        protected abstract void BuildName(string name);

        /// <summary>
        /// 向后拼接一个语句结尾。
        /// </summary>
        protected abstract void BiildStatementEnd();

        /// <summary>
        /// 获取供应商名称
        /// </summary>
        public abstract string ProviderName { get; }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <returns></returns>
        protected internal abstract SqlBuilder CreateInstance();

        #region - Select -

        /// <summary>
        /// 向后拼接 Select 语句
        /// </summary>
        /// <param name="selectStatement">Select 语句信息</param>
        protected virtual void BuildSelect(SelectStatement selectStatement)
        {
            // Initialize
            if (selectStatement.Offset != null || selectStatement.Limit != null)
            {
                if (selectStatement.OrderBys != null && selectStatement.OrderBys.Count == 0)
                {
                    selectStatement.OrderBys.Add(GetDefaultOrderBy(selectStatement));
                }
            }
            if (selectStatement.Joins != null && selectStatement.Joins.Count != 0)
            {
                MakeAlias(selectStatement.MainTable);

                foreach (var item in selectStatement.Joins)
                {
                    MakeAlias(item.Table);
                }
            }

            // SELECT
            var isFirst = true;

            BuildCode(Code_Select);

            if (selectStatement.Columns != null && selectStatement.Columns.Count != 0)
            {
                foreach (var item in selectStatement.Columns)
                {
                    if (!isFirst)
                    {
                        BuildCode(Code_Comma);
                    }

                    BuildCode(Code_Space);

                    Invoke(nameof(BuildSelectColumn), item);

                    isFirst = false;
                }
            }
            else
            {
                BuildCode(Code_Space);
                BuildCode(Code_Asterisk);
            }

            if (selectStatement.MainTable != null)
            {
                // FROM
                BuildCode(Code_Space);

                BuildCode(Code_From);

                BuildCode(Code_Space);

                Invoke(nameof(BuildTable), selectStatement.MainTable);

                var alias = GetAlias(selectStatement.MainTable);

                if (alias != null)
                {
                    BuildCode(Code_Space);

                    BuildName(alias);
                }

                if (selectStatement.Joins != null && selectStatement.Joins.Count != 0)
                {
                    foreach (var join in selectStatement.Joins)
                    {
                        BuildJoin(join);
                    }
                }
            }

            // WHERE
            if (selectStatement.Where != null && selectStatement.Where.Count != 0)
            {
                BuildCode(Code_Space);

                BuildCode(Code_Where);

                BuildCode(Code_Space);

                BuildConditions(selectStatement.Where);
            }

            // GROUP BY
            if (selectStatement.GroupBys != null && selectStatement.GroupBys.Count != 0)
            {
                BuildCode(Code_Space);

                BuildCode(Code_Group);

                BuildCode(Code_Space);

                BuildCode(Code_By);

                isFirst = true;

                foreach (var item in selectStatement.GroupBys)
                {
                    if (!isFirst)
                    {
                        BuildCode(Code_Comma);
                    }

                    BuildCode(Code_Space);

                    BuildGroupBy(item);

                    isFirst = false;
                }

                // HAVING
                if (selectStatement.Having != null && selectStatement.Having.Count != 0)
                {
                    BuildCode(Code_Space);

                    BuildCode(Code_Having);

                    BuildCode(Code_Space);

                    BuildConditions(selectStatement.Having);
                }
            }

            var buildLimit = false;

            // ORDER BY
            if (selectStatement.OrderBys != null && selectStatement.OrderBys.Count != 0)
            {
                buildLimit = true;

                BuildCode(Code_Space);

                BuildCode(Code_Order);

                BuildCode(Code_Space);

                BuildCode(Code_By);

                isFirst = true;

                foreach (var item in selectStatement.OrderBys)
                {
                    if (!isFirst)
                    {
                        BuildCode(Code_Comma);
                    }

                    BuildCode(Code_Space);

                    BuildOrderBy(item);

                    isFirst = false;
                }
            }

            if (buildLimit && (selectStatement.Offset != null || selectStatement.Limit != null))
            {
                BuildSelectLimit(selectStatement.Offset, selectStatement.Limit);
            }
        }

        /// <summary>
        /// 向后拼接 Select 语句，并拼接语句结尾。
        /// </summary>
        /// <param name="selectStatement">Select 语句信息</param>
        public virtual void BuildSelectStatement(SelectStatement selectStatement)
        {
            BuildSelect(selectStatement);

            BiildStatementEnd();
        }

        /// <summary>
        /// 向后拼接 Limit 参数。
        /// </summary>
        /// <param name="offset">偏移行数</param>
        /// <param name="limit">行数数量</param>
        protected abstract void BuildSelectLimit(int? offset, int? limit);

        #endregion

        #region - Select Column -


        /// <summary>
        /// 向后拼接 Select 列。
        /// </summary>
        /// <param name="selectColumn">Select 列信息</param>
        protected virtual void BuildSelectColumn(SelectColumn selectColumn)
        {
            BuildColumn(selectColumn.Column);

            if (!string.IsNullOrEmpty(selectColumn.Alias))
            {
                BuildCode(Code_Space);

                BuildName(selectColumn.Alias);
            }
        }

        /// <summary>
        /// 向后拼接一个子查询列。
        /// </summary>
        /// <param name="subQueryColumn">子查询信息</param>
        protected virtual void BuildSelectColumn(SubQueryColumn subQueryColumn)
        {
            BuildCode(Code_Parenthesis_Bracket_Begin);

            BuildSelect(subQueryColumn.SubQuery);

            BuildCode(Code_Parenthesis_Bracket_End);

            if (!string.IsNullOrEmpty(subQueryColumn.Alias))
            {
                BuildCode(Code_Space);

                BuildName(subQueryColumn.Alias);
            }
        }

        /// <summary>
        /// 向后拼接一个 Count 聚合列。
        /// </summary>
        /// <param name="countFunction">Count 聚合信息</param>
        protected virtual void BuildSelectColumn(CountFunction countFunction)
        {
            BuildCode(Code_Count);

            BuildCode(Code_Parenthesis_Bracket_Begin);

            BuildColumn(countFunction.Column);

            BuildCode(Code_Parenthesis_Bracket_End);

            if (!string.IsNullOrEmpty(countFunction.Alias))
            {
                BuildCode(Code_Space);

                BuildName(countFunction.Alias);
            }
        }

        /// <summary>
        /// 向后拼接一个 Sum 聚合列。
        /// </summary>
        /// <param name="sumFunction">Sum 聚合信息</param>
        protected virtual void BuildSelectColumn(SumFunction sumFunction)
        {
            BuildCode(Code_Sum);

            BuildCode(Code_Parenthesis_Bracket_Begin);

            BuildColumn(sumFunction.Column);

            BuildCode(Code_Parenthesis_Bracket_End);

            if (!string.IsNullOrEmpty(sumFunction.Alias))
            {
                BuildCode(Code_Space);

                BuildName(sumFunction.Alias);
            }
        }

        /// <summary>
        /// 向后拼接一个 Max 聚合列。
        /// </summary>
        /// <param name="maxFunction">Max 聚合信息</param>
        protected virtual void BuildSelectColumn(MaxFunction maxFunction)
        {
            BuildCode(Code_Count);

            BuildCode(Code_Parenthesis_Bracket_Begin);

            BuildColumn(maxFunction.Column);

            BuildCode(Code_Parenthesis_Bracket_End);

            if (!string.IsNullOrEmpty(maxFunction.Alias))
            {
                BuildCode(Code_Space);

                BuildName(maxFunction.Alias);
            }
        }

        /// <summary>
        /// 向后拼接一个 Min 聚合列。
        /// </summary>
        /// <param name="minFunction">Min 聚合信息</param>
        protected virtual void BuildSelectColumn(MinFunction minFunction)
        {
            BuildCode(Code_Count);

            BuildCode(Code_Parenthesis_Bracket_Begin);

            BuildColumn(minFunction.Column);

            BuildCode(Code_Parenthesis_Bracket_End);

            if (!string.IsNullOrEmpty(minFunction.Alias))
            {
                BuildCode(Code_Space);

                BuildName(minFunction.Alias);
            }
        }

        #endregion

        #region - Insert -

        /// <summary>
        /// 向后拼接 Insert 语句。
        /// </summary>
        /// <param name="insertStatement">Insert 语句信息</param>
        public virtual void BuildInsertStatement(InsertStatement insertStatement)
        {
            if (insertStatement.Values != null && insertStatement.Values.Count != 0)
            {
                BuildCode(Code_Insert);

                BuildCode(Code_Space);

                BuildTable(insertStatement.Table);

                BuildCode(Code_Parenthesis_Bracket_Begin);

                var isFirst = true;

                foreach (var item in insertStatement.Values)
                {
                    if (!isFirst)
                    {
                        BuildCode(Code_Comma);

                        BuildCode(Code_Space);
                    }

                    BuildColumn(item.Column);

                    isFirst = false;
                }

                BuildCode(Code_Parenthesis_Bracket_End);
                
                BuildCode(Code_Space);

                BuildCode(Code_Values);

                BuildCode(Code_Parenthesis_Bracket_Begin);

                isFirst = true;

                foreach (var item in insertStatement.Values)
                {
                    if (!isFirst)
                    {
                        BuildCode(Code_Comma);

                        BuildCode(Code_Space);
                    }

                    Invoke(nameof(BuildValue), item.Value);

                    isFirst = false;
                }


                BuildCode(Code_Parenthesis_Bracket_End);

                if (insertStatement.GetIdentity)
                {
                    BiildStatementEnd();

                    BuildGetIdentity(insertStatement);
                }

                BiildStatementEnd();
            }
        }

        /// <summary>
        /// 向后拼接 Insert 语句之后的 GetIdentity 的语句。
        /// </summary>
        /// <param name="insertStatement">Insert 语句信息</param>
        public abstract void BuildGetIdentity(InsertStatement insertStatement);

        #endregion

        #region - Update -

        /// <summary>
        /// 向后拼接 Update 语句。
        /// </summary>
        /// <param name="updateStatement">Update 语句信息</param>
        public virtual void BuildUpdateStatement(UpdateStatement updateStatement)
        {
            if (updateStatement.Values != null && updateStatement.Values.Count != 0)
            {
                BuildCode(Code_Update);

                BuildCode(Code_Space);

                BuildTable(updateStatement.Table);

                BuildCode(Code_Space);

                BuildCode(Code_Set);

                BuildCode(Code_Space);

                var isFirst = true;

                foreach (var item in updateStatement.Values)
                {
                    if (!isFirst)
                    {
                        BuildCode(Code_Comma);

                        BuildCode(Code_Space);
                    }

                    BuildColumn(item.Column);

                    BuildCode(Code_Space);

                    BuildCode(Code_Equal);

                    BuildCode(Code_Space);

                    Invoke(nameof(BuildValue), item.Value);

                    isFirst = false;
                }

                if (updateStatement.Where != null && updateStatement.Where.Count != 0)
                {
                    BuildCode(Code_Space);

                    BuildCode(Code_Where);

                    BuildCode(Code_Space);

                    BuildConditions(updateStatement.Where);
                }

                BiildStatementEnd();
            }
        }

        #endregion

        #region - Delete -

        /// <summary>
        /// 向后拼接 Delete 语句。
        /// </summary>
        /// <param name="deleteStatement">Delete 语句信息</param>
        public virtual void BuildDeleteStatement(DeleteStatement deleteStatement)
        {
            BuildCode(Code_Delete);

            BuildCode(Code_Space);

            BuildTable(deleteStatement.Table);

            if (deleteStatement.Where != null && deleteStatement.Where.Count != 0)
            {
                BuildCode(Code_Space);

                BuildCode(Code_Where);

                BuildCode(Code_Space);

                BuildConditions(deleteStatement.Where);
            }

            BiildStatementEnd();
        }

        #endregion

        #region - Column -

        /// <summary>
        /// 向后拼接一个列。
        /// </summary>
        /// <param name="column">列信息</param>
        protected virtual void BuildColumn(Column column)
        {
            string alias;

            if (column.Table != null && (alias = GetAlias(column.Table)) != null)
            {
                BuildName(alias);

                BuildCode(Code_Dot);
            }

            BuildName(column.Name);
        }

        #endregion

        #region - Table -

        /// <summary>
        /// 向后拼接一个表。
        /// </summary>
        /// <param name="table">表信息</param>
        protected virtual void BuildTable(Table table)
        {
            BuildName(table.Name);
        }

        /// <summary>
        /// 向后拼接一个子查询的表。
        /// </summary>
        /// <param name="selectStatement">子查询信息</param>
        protected virtual void BuildTable(SelectStatement selectStatement)
        {
            BuildCode(Code_Parenthesis_Bracket_Begin);

            BuildSelect(selectStatement);

            BuildCode(Code_Parenthesis_Bracket_End);
        }

        #endregion

        #region - Value -

        /// <summary>
        /// 向后拼接一个 Int32 值。
        /// </summary>
        /// <param name="value">Int32</param>
        protected virtual void BuildValue(ConstantValue<int> value) => BuildCode(NumberHelper.Decimal.ToString(value.Value));

        /// <summary>
        /// 向后拼接一个 Int64 值。
        /// </summary>
        /// <param name="value">Int64</param>
        protected virtual void BuildValue(ConstantValue<long> value) => BuildCode(NumberHelper.Decimal.ToString(value.Value));

        /// <summary>
        /// 向后拼接一个 Float 值。
        /// </summary>
        /// <param name="value">Float</param>
        protected virtual void BuildValue(ConstantValue<float> value) => BuildCode(NumberHelper.Decimal.ToString(value.Value));

        /// <summary>
        /// 向后拼接一个 Double 值。
        /// </summary>
        /// <param name="value">Double</param>
        protected virtual void BuildValue(ConstantValue<double> value) => BuildCode(NumberHelper.Decimal.ToString(value.Value));

        /// <summary>
        /// 向后拼接一个 Boolean 值。
        /// </summary>
        /// <param name="value">Boolean</param>
        protected virtual void BuildValue(ConstantValue<bool> value) => BuildCode(value.Value ? "1" : "0");

        /// <summary>
        /// 向后拼接一个 Decimal 值。
        /// </summary>
        /// <param name="value">Decimal</param>
        protected virtual void BuildValue(ConstantValue<decimal> value) => BuildCode(NumberHelper.ToString(value.Value));

        /// <summary>
        /// 向后拼接一个 Int16 值。
        /// </summary>
        /// <param name="value">Int16</param>
        protected virtual void BuildValue(ConstantValue<short> value) => BuildCode(NumberHelper.Decimal.ToString(value.Value));

        /// <summary>
        /// 向后拼接一个 Byte 值。
        /// </summary>
        /// <param name="value">Byte</param>
        protected virtual void BuildValue(ConstantValue<byte> value) => BuildCode(NumberHelper.Decimal.ToString(value.Value));

        /// <summary>
        /// 向后拼接一个 String 值。
        /// </summary>
        /// <param name="value">String</param>
        protected abstract void BuildValue(ConstantValue<string> value);

        /// <summary>
        /// 向后拼接一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime</param>
        protected virtual void BuildValue(ConstantValue<DateTime> value) => BuildValue(new ConstantValue<string>(value.Value.ToString()));

        /// <summary>
        /// 向后拼接一个 DateTimeOffset 值。
        /// </summary>
        /// <param name="value">DateTimeOffset</param>
        protected virtual void BuildValue(ConstantValue<DateTimeOffset> value) => BuildValue(new ConstantValue<string>(value.Value.ToString()));

        /// <summary>
        /// 向后拼接一个 Guid 值。
        /// </summary>
        /// <param name="value">Guid</param>
        protected virtual void BuildValue(ConstantValue<Guid> value) => BuildValue(new ConstantValue<string>(NumberHelper.ToString(value.Value)));

        /// <summary>
        /// 向后拼接一个数组值。
        /// </summary>
        /// <param name="value">数组</param>
        protected virtual void BuildValue(ConstantValue<IValue[]> value)
        {
            if (value.Value != null && value.Value.Length != 0)
            {
                var isFirst = true;

                foreach (var item in value.Value)
                {
                    if (!isFirst)
                    {
                        BuildCode(Code_Comma);

                        BuildCode(Code_Space);
                    }

                    Invoke(nameof(BuildValue), item);

                    isFirst = false;
                }
            }
        }

        /// <summary>
        /// 向后拼接一个列值。
        /// </summary>
        /// <param name="column">列信息</param>
        protected virtual void BuildValue(Column column)
        {
            string alias;

            if (column.Table != null && (alias = GetAlias(column.Table)) != null)
            {
                BuildName(alias);

                BuildCode(Code_Dot);
            }

            BuildName(column.Name);
        }


        #endregion

        #region - Group By -

        /// <summary>
        /// 向后拼接一个分组列。
        /// </summary>
        /// <param name="column">列信息</param>
        protected virtual void BuildGroupBy(Column column)
        {
            string alias;

            if (column.Table != null && (alias = GetAlias(column.Table)) != null)
            {
                BuildName(alias);

                BuildCode(Code_Dot);
            }

            BuildName(column.Name);
        }

        #endregion

        #region - Order By -

        /// <summary>
        /// 当没有为查询语句设置排序列是获取它的默认排序列。
        /// </summary>
        /// <param name="selectStatement">查询语句信息</param>
        /// <returns>返回一个排序列信息</returns>
        protected abstract OrderBy GetDefaultOrderBy(SelectStatement selectStatement);

        /// <summary>
        /// 向后拼接一个排序列。
        /// </summary>
        /// <param name="orderBy">排序列信息</param>
        protected virtual void BuildOrderBy(OrderBy orderBy)
        {
            string alias;

            if (orderBy.Column.Table != null && (alias = GetAlias(orderBy.Column.Table)) != null)
            {
                BuildName(alias);

                BuildCode(Code_Dot);
            }

            BuildName(orderBy.Column.Name);

            BuildOrderByDirections(orderBy.Direction);
        }

        /// <summary>
        /// 向后拼接一个排序方法。
        /// </summary>
        /// <param name="orderByDirection">排序方向</param>
        protected virtual void BuildOrderByDirections(OrderByDirections orderByDirection)
        {
            switch (orderByDirection)
            {
                case OrderByDirections.None:
                    break;
                case OrderByDirections.ASC:
                    BuildCode(Code_Space);
                    BuildCode(Code_ASC);
                    break;
                case OrderByDirections.DESC:
                    BuildCode(Code_Space);
                    BuildCode(Code_DESC);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region - Comparison -

        void BuildComparison_(IValue before, IValue after)
        {
            throw new ArgumentException(nameof(Condition.Comparison));
        }

        /// <summary>
        /// 向后拼接一个等于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_Equal(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            if (after == null)
            {
                BuildCode(Code_Space);
                BuildCode(Code_Is);
                BuildCode(Code_Space);
                BuildCode(Code_Not);
                BuildCode(Code_Space);
                BuildCode(Code_Null);

                return;
            }

            BuildCode(Code_Space);
            BuildCode(Code_Equal);
            BuildCode(Code_Space);

            Invoke(nameof(BuildValue), after);
        }

        /// <summary>
        /// 向后拼接一个不等于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_NotEqual(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            if (after == null)
            {
                BuildCode(Code_Space);
                BuildCode(Code_Is);
                BuildCode(Code_Space);
                BuildCode(Code_Null);

                return;
            }

            BuildCode(Code_Space);
            BuildCode(Code_NotEqual);
            BuildCode(Code_Space);

            Invoke(nameof(BuildValue), after);
        }

        /// <summary>
        /// 向后拼接一个大于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_GreaterThan(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);
            
            BuildCode(Code_Space);
            BuildCode(Code_GreaterThan);
            BuildCode(Code_Space);

            Invoke(nameof(BuildValue), after);
        }

        /// <summary>
        /// 向后拼接一个大于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_LessThan(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_LessThan);
            BuildCode(Code_Space);

            Invoke(nameof(BuildValue), after);
        }

        /// <summary>
        /// 向后拼接一个大于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_GreaterEqual(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_GreaterThan);
            BuildCode(Code_Equal);
            BuildCode(Code_Space);

            Invoke(nameof(BuildValue), after);
        }

        /// <summary>
        /// 向后拼接一个小于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_LessEqual(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_LessThan);
            BuildCode(Code_Equal);
            BuildCode(Code_Space);

            Invoke(nameof(BuildValue), after);
        }

        /// <summary>
        /// 向后拼接一个包含该值的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_Fuzzy(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_Like);
            BuildCode(Code_Space);

            BuildCode(Code_Concat);
            BuildCode(Code_Parenthesis_Bracket_Begin);
            BuildValue(Value_Percent);
            BuildCode(Code_Comma);
            BuildCode(Code_Space);
            Invoke(nameof(BuildValue), after);
            BuildCode(Code_Comma);
            BuildCode(Code_Space);
            BuildValue(Value_Percent);
            BuildCode(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个在一些值中的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置数组</param>
        protected virtual void BuildComparison_In(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_In);
            BuildCode(Code_Space);

            BuildCode(Code_Parenthesis_Bracket_Begin);

            Invoke(nameof(BuildValue), after);

            BuildCode(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个在两个值之间的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置数组，必须两个长度</param>
        protected virtual void BuildComparison_BetweenAnd(IValue before, IValue after)
        {
            if (after is ConstantValue<IValue[]> array && array.Value.Length == 2)
            {
                Invoke(nameof(BuildValue), before);

                BuildCode(Code_Space);
                BuildCode(Code_Between);
                BuildCode(Code_Space);

                Invoke(nameof(BuildValue), array.Value[0]);

                BuildCode(Code_Space);
                BuildCode(Code_And);
                BuildCode(Code_Space);

                Invoke(nameof(BuildValue), array.Value[1]);

                return;
            }

            throw new NotSupportedException("Between And : After value must is a array and the length must be 2.");
        }

        /// <summary>
        /// 向后拼接一个开头与其相符的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_StartWith(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_Like);
            BuildCode(Code_Equal);
            BuildCode(Code_Space);

            BuildCode(Code_Concat);
            BuildCode(Code_Parenthesis_Bracket_Begin);
            BuildValue(Value_Percent);
            BuildCode(Code_Comma);
            BuildCode(Code_Space);
            Invoke(nameof(BuildValue), after);
            BuildCode(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个结尾与其相符的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_EndWith(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_Like);
            BuildCode(Code_Equal);
            BuildCode(Code_Space);

            BuildCode(Code_Concat);
            BuildCode(Code_Parenthesis_Bracket_Begin);
            Invoke(nameof(BuildValue), after);
            BuildCode(Code_Comma);
            BuildCode(Code_Space);
            BuildValue(Value_Percent);
            BuildCode(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个开头与其不相符的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_NotStartWith(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_Not);
            BuildCode(Code_Space);
            BuildCode(Code_Like);
            BuildCode(Code_Equal);
            BuildCode(Code_Space);

            BuildCode(Code_Concat);
            BuildCode(Code_Parenthesis_Bracket_Begin);
            BuildValue(Value_Percent);
            BuildCode(Code_Comma);
            BuildCode(Code_Space);
            Invoke(nameof(BuildValue), after);
            BuildCode(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个结尾与其不相符的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_NotEndWith(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_Not);
            BuildCode(Code_Space);
            BuildCode(Code_Like);
            BuildCode(Code_Equal);
            BuildCode(Code_Space);

            BuildCode(Code_Concat);
            BuildCode(Code_Parenthesis_Bracket_Begin);
            Invoke(nameof(BuildValue), after);
            BuildCode(Code_Comma);
            BuildCode(Code_Space);
            BuildValue(Value_Percent);
            BuildCode(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个按位与不为空的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        protected virtual void BuildComparison_BitAnd(IValue before, IValue after)
        {
            Invoke(nameof(BuildValue), before);

            BuildCode(Code_Space);
            BuildCode(Code_Amp);
            BuildCode(Code_Space);

            Invoke(nameof(BuildValue), after);

            BuildCode(Code_Space);
            BuildCode(Code_NotEqual);
            BuildCode(Code_Space);

            BuildCode(Code_Zero);
        }
        #endregion

        #region - Condition -

        /// <summary>
        /// 向后拼接一个条件。
        /// </summary>
        /// <param name="condition">条件信息</param>
        protected virtual void BuildCondition(Condition condition)
        {
            Invoke($"{nameof(BuildComparison_)}{Enum.GetName(typeof(Comparisons), condition.Comparison)}", condition.Before, condition.After);
        }

        /// <summary>
        /// 向后拼接一个条件集合。
        /// </summary>
        /// <param name="conditions">条件集合</param>
        protected virtual void BuildConditions(Conditions conditions)
        {
            int end = conditions.Count - 1;
            int depth = 0;

            for (int i = 0; i < conditions.Count; i++)
            {
                var prev = i == 0 ? Condition.Empty : conditions[i - 1];
                var next = i == end ? Condition.Empty : conditions[i + 1];
                var item = conditions[i];

                if (prev != Condition.Empty)
                {
                    if (prev.Index != item.Index)
                    {
                        while (depth != 0 && (prev.Index & HexDigital[depth]) != (item.Index & HexDigital[depth]))
                        {
                            BuildCode(Code_Parenthesis_Bracket_End);

                            --depth;
                        }
                    }

                    BuildCode(Code_Space);
                    BuildConditionType(item.Type);
                    BuildCode(Code_Space);
                }

                if (prev.Index != item.Index && (next.Index & HexDigital[depth]) == (item.Index & HexDigital[depth]))
                {
                    BuildCode(Code_Parenthesis_Bracket_Begin);

                    ++depth;
                }

                BuildCondition(item);
            }

            while (depth != 0)
            {
                BuildCode(Code_Parenthesis_Bracket_End);

                --depth;
            }

            if (conditions.Count == 0)
            {
                BuildCode(Code_True_Expression);
            }
        }

        /// <summary>
        /// 向后拼接一个条件连接符。
        /// </summary>
        /// <param name="conditionType">条件连接符</param>
        protected virtual void BuildConditionType(ConditionTypes conditionType)
        {
            switch (conditionType)
            {
                case ConditionTypes.And:
                    BuildCode(Code_And);
                    break;
                case ConditionTypes.Or:
                    BuildCode(Code_Or);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region - Join -

        /// <summary>
        /// 拼接一个表连接。
        /// </summary>
        /// <param name="join">表连接信息</param>
        protected virtual void BuildJoin(Join join)
        {
            BuildCode(Code_Space);

            BuildJoinDirection(join.Direction);

            BuildCode(Code_Space);

            BuildCode(Code_Join);

            BuildCode(Code_Space);

            Invoke(nameof(BuildTable), join.Table);

            BuildCode(Code_Space);

            var alias = GetAlias(join.Table);

            if (alias != null)
            {
                BuildName(alias);

                BuildCode(Code_Space);
            }

            BuildCode(Code_On);

            BuildCode(Code_Space);

            if (join.On != null && join.On.Count != 0)
            {
                BuildConditions(join.On);
            }
            else
            {
                BuildCode(Code_True_Expression);
            }
        }

        /// <summary>
        /// 拼接一个表连接方向。
        /// </summary>
        /// <param name="joinDirection">表连接方向</param>
        protected virtual void BuildJoinDirection(JoinDirections joinDirection)
        {
            switch (joinDirection)
            {
                case JoinDirections.Left:
                    BuildCode(Code_Left);
                    break;
                case JoinDirections.Right:
                    BuildCode(Code_Right);
                    break;
                case JoinDirections.Inner:
                    BuildCode(Code_Inner);
                    break;
                case JoinDirections.Full:
                    BuildCode(Code_Full);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region - ToSQL -

        /// <summary>
        /// 获取 T-SQL。
        /// </summary>
        /// <returns>返回一个 String 值。</returns>
        public string ToSQL()
        {
            return Builder.ToString();
        }

        /// <summary>
        /// 清空当前实例的信息。
        /// </summary>
        public void Clear()
        {
            AliasCache.Clear();

            Builder.Length = 0;

            aliasIndex = 0;
        }

        /// <summary>
        /// 获取 T-SQL。
        /// </summary>
        /// <returns>返回一个 String 值。</returns>
        public override string ToString()
        {
            return ToSQL();
        }

        #endregion
    }
}
