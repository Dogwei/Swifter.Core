namespace Swifter.Tools
{
    /// <summary>
    /// 快速获取泛型类型的信息。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TypeInfo<T>
    {
        static TypeInfo()
        {
            Int64TypeHandle = (long)TypeHelper.GetTypeHandle(typeof(T));
            BasicType = TypeHelper.GetBasicType(typeof(T));
            IsValueType = typeof(T).IsValueType;
            IsInterface = typeof(T).IsInterface;
        }

        /// <summary>
        /// 获取 Int64 类型的 TypeHandle。
        /// </summary>
        public static readonly long Int64TypeHandle;
        /// <summary>
        /// 获取 BasicTypes 值。
        /// </summary>
        public static readonly BasicTypes BasicType;
        /// <summary>
        /// 判断是否为值类型。
        /// </summary>
        public static readonly bool IsValueType;
        /// <summary>
        /// 判断是否为接口。
        /// </summary>
        public static readonly bool IsInterface;
    }
}
