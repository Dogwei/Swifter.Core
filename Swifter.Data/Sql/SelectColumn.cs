namespace Swifter.Data.Sql
{
    /// <summary>
    /// 查询列。
    /// </summary>
    public sealed class SelectColumn : ISelectColumn, IValue
    {
        /// <summary>
        /// 构建查询列。
        /// </summary>
        /// <param name="column">列信息</param>
        /// <param name="alias">别名</param>
        public SelectColumn(Column column, string alias = null)
        {
            Column = column;
            Alias = alias;
        }

        /// <summary>
        /// 列信息。
        /// </summary>
        public Column Column { get; }

        /// <summary>
        /// 别名。
        /// </summary>
        public string Alias { get; set; }
    }
}