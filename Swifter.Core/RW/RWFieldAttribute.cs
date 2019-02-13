using System;

namespace Swifter.RW
{
    /// <summary>
    /// 表示对象读取器的一个字段的特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class RWFieldAttribute : Attribute
    {
        /// <summary>
        /// 默认排序值。
        /// </summary>
        public const int DefaultOrder = 999;

        /// <summary>
        /// 初始化对象读取器的一个字段的特性。
        /// </summary>
        public RWFieldAttribute()
        {

        }

        /// <summary>
        /// 初始化具有指定名称的对象读取器的一个字段的特性。
        /// </summary>
        /// <param name="name">指定名称</param>
        public RWFieldAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 此字段的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 此字段的排序值。约小越靠前，默认值为最靠后。
        /// </summary>
        public int Order { get; set; } = DefaultOrder;

        /// <summary>
        /// 字段的可访问性。
        /// </summary>
        public RWFieldAccess Access { get; set; }
    }
}