namespace Swifter.Data.Sql
{
    /// <summary>
    /// T-SQL 查询列
    /// </summary>
    public interface ISelectColumn
    {
        /// <summary>
        /// 别名
        /// </summary>
        string Alias { get; }
    }
}