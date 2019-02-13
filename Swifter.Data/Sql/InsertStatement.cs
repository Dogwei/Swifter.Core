namespace Swifter.Data.Sql
{
    /// <summary>
    /// Insert 语句信息。
    /// </summary>
    public sealed class InsertStatement
    {
        /// <summary>
        /// 构建 Insert 语句信息。
        /// </summary>
        /// <param name="table">需要 Insert 的表</param>
        /// <param name="getIdentity">是否获取 Identity</param>
        public InsertStatement(Table table, bool getIdentity = false)
        {
            Table = table;

            GetIdentity = getIdentity;

            Values = new AssignValues();
        }

        /// <summary>
        /// 需要 Insert 的表。
        /// </summary>
        public Table Table { get; }

        /// <summary>
        /// 需要赋值的列。
        /// </summary>
        public AssignValues Values { get; }

        /// <summary>
        /// 是否获取 Identity。
        /// </summary>
        public bool GetIdentity { get; }
    }
}