using Swifter.Tools;
using System;
using System.Reflection;

namespace Swifter.Data
{
    /// <summary>
    /// T-SQL 条件比较符
    /// </summary>
    public enum Comparisons
    {
        /// <summary>
        /// 等于。
        /// </summary>
        [Comparison("==", "=", "E")]
        Equal,
        /// <summary>
        /// 不等于。
        /// </summary>
        [Comparison("!=", "<>", "NE")]
        NotEqual,
        /// <summary>
        /// 大于。
        /// </summary>
        [Comparison(">", "GT")]
        GreaterThan,
        /// <summary>
        /// 小于。
        /// </summary>
        [Comparison("<", "LT")]
        LessThan,
        /// <summary>
        /// 大于等于。
        /// </summary>
        [Comparison(">=", "GE")]
        GreaterEqual,
        /// <summary>
        /// 小于等于。
        /// </summary>
        [Comparison("<=", "LE")]
        LessEqual,
        /// <summary>
        /// 包含该值。
        /// </summary>
        [Comparison("Fuzzy")]
        Fuzzy,
        /// <summary>
        /// 在一些值中。
        /// </summary>
        [Comparison("In")]
        In,
        /// <summary>
        /// 在两个值之间。
        /// </summary>
        [Comparison("BWA")]
        BetweenAnd,
        /// <summary>
        /// 开头与其相符。
        /// </summary>
        [Comparison("SW")]
        StartWith,
        /// <summary>
        /// 结尾与其相符。
        /// </summary>
        [Comparison("EW")]
        EndWith,
        /// <summary>
        /// 开头与其不相符。
        /// </summary>
        [Comparison("NSW")]
        NotStartWith,
        /// <summary>
        /// 结尾与其不相符。
        /// </summary>
        [Comparison("NEW")]
        NotEndWith,
        /// <summary>
        /// 按位与不为空。
        /// </summary>
        [Comparison("BA")]
        BitAnd,
    }

    sealed class ComparisonAttribute : Attribute
    {
        static readonly NameCache<Comparisons> NamesCache = new NameCache<Comparisons>();

        static ComparisonAttribute()
        {
            foreach (var item in typeof(Comparisons).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var value = (Comparisons)item.GetValue(null);

                NamesCache.DirectAdd(item.Name, value);

                foreach (ComparisonAttribute attribute in item.GetCustomAttributes(typeof(ComparisonAttribute), false))
                {
                    foreach (var name in attribute.Names)
                    {
                        NamesCache.DirectAdd(name, value);
                    }
                }
            }
        }

        public static Comparisons GetComparison(string name)
        {
            return NamesCache[name];
        }

        public readonly string[] Names;

        public ComparisonAttribute(params string[] names)
        {
            Names = names;
        }
    }
}