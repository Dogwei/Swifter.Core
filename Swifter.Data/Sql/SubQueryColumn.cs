using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// 子查询列。
    /// </summary>
    public sealed class SubQueryColumn : ISelectColumn, IValue
    {
        /// <summary>
        /// 构建子查询列。
        /// </summary>
        /// <param name="subQuery">子查询信息</param>
        /// <param name="alias">别名</param>
        public SubQueryColumn(SelectStatement subQuery, string alias = null)
        {
            SubQuery = subQuery;
            Alias = alias;
        }

        /// <summary>
        /// 别名。
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 子查询信息。
        /// </summary>
        public SelectStatement SubQuery { get; }
    }
}