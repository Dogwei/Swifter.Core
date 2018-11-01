namespace Swifter.Json
{
    /// <summary>
    /// JSON 格式化器配置项。
    /// </summary>
    public enum JsonFormatterOptions : byte
    {
        /// <summary>
        /// 默认配置项
        /// </summary>
        Default = 0,

        /// <summary>
        /// 序列化时不考虑对象多引用关系
        /// </summary>
        MultiReferencingNone = 0,

        /// <summary>
        /// 序列化时跳过已序列化的对象，使用 Null 表示。
        /// </summary>
        MultiReferencingNull = 0x1,

        /// <summary>
        /// 允许使用 ref_xxx 写法表示对象多引用。
        /// </summary>
        MultiReferencingReference = 0x2,

        /// <summary>
        /// 序列化时对 JSON 进行缩进美化。
        /// </summary>
        Indented = 0x4,

        /// <summary>
        /// 超出深度时抛出异常，否则将跳过超出部分。
        /// </summary>
        OutOfDepthException = 0x8,

        /// <summary>
        /// 启用筛选并筛选掉 Null 值
        /// </summary>
        IgnoreNull = 0x10,

        /// <summary>
        /// 启用筛选并筛选掉 0 值
        /// </summary>
        IgnoreZero = 0x20,

        /// <summary>
        /// 数组元素启用筛选
        /// </summary>
        ArrayOnFilter = 0x40,

        /// <summary>
        /// 启用筛选并筛选掉 "" 值 (空字符串)
        /// </summary>
        IgnoreEmptyString = 0x80,
    }
}