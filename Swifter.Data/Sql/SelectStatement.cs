namespace Swifter.Data.Sql
{
    /// <summary>
    /// Select 语句信息。
    /// </summary>
    public sealed class SelectStatement : ITable
    {
        /// <summary>
        /// 构建 Select 语句信息
        /// </summary>
        /// <param name="mainTable">表</param>
        public SelectStatement(ITable mainTable)
        {
            MainTable = mainTable;

            Columns = new SelectColumns();
            Joins = new Joins();
            Where = new Conditions();
            OrderBys = new OrderBys();
            GroupBys = new GroupBys();
            Having = new Conditions();
        }

        /// <summary>
        /// 构建 Select 语句信息
        /// </summary>
        /// <param name="mainTable">表</param>
        public SelectStatement(Table mainTable) : this((ITable)mainTable)
        {
        }

        /// <summary>
        /// 需要查询的列的集合。
        /// </summary>
        public SelectColumns Columns { get; }

        /// <summary>
        /// 主表。
        /// </summary>
        public ITable MainTable { get; }

        /// <summary>
        /// 要关联的表集合。
        /// </summary>
        public Joins Joins { get; }

        /// <summary>
        /// 查询条件。
        /// </summary>
        public Conditions Where { get; }

        /// <summary>
        /// 排序列的集合。
        /// </summary>
        public OrderBys OrderBys { get; }

        /// <summary>
        /// 分组列的集合。
        /// </summary>
        public GroupBys GroupBys { get; }

        /// <summary>
        /// 分组查询的条件。
        /// </summary>
        public Conditions Having { get; }

        /// <summary>
        /// 结果集偏移行数。
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// 结果集行数数量。
        /// </summary>
        public int? Limit { get; set; }
    }
}