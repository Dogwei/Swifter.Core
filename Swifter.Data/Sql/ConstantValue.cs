namespace Swifter.Data.Sql
{
    /// <summary>
    /// 表示一个常量值
    /// </summary>
    /// <typeparam name="T">值</typeparam>
    public struct ConstantValue<T> : IValue
    {
        /// <summary>
        /// 值
        /// </summary>
        public T Value { get; }

        internal ConstantValue(T value)
        {
            Value = value;
        }
    }
}