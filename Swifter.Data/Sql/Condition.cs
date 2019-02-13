using Swifter.Tools;
using System;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// 条件信息
    /// </summary>
    public sealed class Condition : IComparable<Condition>
    {
        internal static readonly Condition Empty = new Condition();

        readonly string stIndex;

        Condition()
        {
            stIndex = "0";
        }

        /// <summary>
        /// 构建条件信息
        /// </summary>
        /// <param name="index">十六索引</param>
        public Condition(string index)
        {
            stIndex = index;

            Index = (int)NumberHelper.Hex.ParseInt64(StringHelper.Reverse(index));
        }

        /// <summary>
        /// 构建条件信息
        /// </summary>
        /// <param name="index">十六索引</param>
        /// <param name="type">连接符</param>
        /// <param name="comparison">比较符</param>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public Condition(string index, ConditionTypes type, Comparisons comparison, IValue before, IValue after) : this(index)
        {
            Type = type;
            Comparison = comparison;
            Before = before;
            After = after;
        }

        /// <summary>
        /// 构建条件信息
        /// </summary>
        /// <param name="type">连接符</param>
        /// <param name="comparison">比较符</param>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public Condition(ConditionTypes type, Comparisons comparison, IValue before, IValue after) : this()
        {
            Type = type;
            Comparison = comparison;
            Before = before;
            After = after;
        }

        /// <summary>
        /// 一个倒置的索引值。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 连接符。
        /// </summary>
        public ConditionTypes Type { get; set; }

        /// <summary>
        /// 比较符。
        /// </summary>
        public Comparisons Comparison { get; set; }

        /// <summary>
        /// 前置值。
        /// </summary>
        public IValue Before { get; set; }

        /// <summary>
        /// 后置值。
        /// </summary>
        public IValue After { get; set; }

        /// <summary>
        /// 和另一个条件比较顺序。
        /// </summary>
        /// <param name="other">另一个条件</param>
        /// <returns>返回比较结果</returns>
        public int CompareTo(Condition other)
        {
            return stIndex.CompareTo(other.stIndex);
        }
    }
}