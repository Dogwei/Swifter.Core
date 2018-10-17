using System;
using System.Collections.Generic;

namespace Swifter.Tools
{
    /// <summary>
    /// 忽略大小写的字符串匹配器。
    /// </summary>
    public sealed class IgnoreCaseEqualityComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// 比较两个字符串是否相等。
        /// </summary>
        /// <param name="x">字符串 1</param>
        /// <param name="y">字符串 2</param>
        /// <returns>返回一个 bool 值</returns>
        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取一个字符串的 HashCode 值。
        /// </summary>
        /// <param name="obj">字符串</param>
        /// <returns>返回一个 HashCode 值</returns>
        public int GetHashCode(string obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return StringHelper.IgnoreCaseGetHashCode(obj);
        }
    }
}