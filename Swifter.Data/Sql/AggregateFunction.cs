namespace Swifter.Data.Sql
{
    /// <summary>
    /// 聚合函数列
    /// </summary>
    public abstract class AggregateFunction: ISelectColumn
    {
        /// <summary>
        /// 聚合列
        /// </summary>
        public Column Column { get; }

        /// <summary>
        /// 别名
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// 聚合函数必要的构造参数
        /// </summary>
        /// <param name="column">聚合列</param>
        /// <param name="alias">别名</param>
        protected AggregateFunction(Column column, string alias)
        {
            Column = column;
            Alias = alias;
        }
    }

    /// <summary>
    /// Count 聚合函数信息。
    /// </summary>
    public sealed class CountFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Count 聚合函数信息
        /// </summary>
        /// <param name="column">聚合列</param>
        /// <param name="alias">别名</param>
        public CountFunction(Column column, string alias) : base(column, alias)
        {
        }
    }

    /// <summary>
    /// Sum 聚合函数信息。
    /// </summary>
    public sealed class SumFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Sum 聚合函数信息
        /// </summary>
        /// <param name="column">聚合列</param>
        /// <param name="alias">别名</param>
        public SumFunction(Column column, string alias) : base(column, alias)
        {
        }
    }

    /// <summary>
    /// Max 聚合函数信息。
    /// </summary>
    public sealed class MaxFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Max 聚合函数信息
        /// </summary>
        /// <param name="column">聚合列</param>
        /// <param name="alias">别名</param>
        public MaxFunction(Column column, string alias) : base(column, alias)
        {
        }
    }

    /// <summary>
    /// Min 聚合函数信息。
    /// </summary>
    public sealed class MinFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Min 聚合函数信息
        /// </summary>
        /// <param name="column">聚合列</param>
        /// <param name="alias">别名</param>
        public MinFunction(Column column, string alias) : base(column, alias)
        {
        }
    }
}
