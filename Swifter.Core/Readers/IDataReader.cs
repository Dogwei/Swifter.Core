using Swifter.Writers;
using System.Collections.Generic;

namespace Swifter.Readers
{
    /// <summary>
    /// 提供数据的读取方法。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public interface IDataReader<TKey>: IDataReader
    {
        /// <summary>
        /// 获取指定键的值读取器实例。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <returns>返回值读取器实例</returns>
        IValueReader this[TKey key] { get; }

        /// <summary>
        /// 将指定键对应的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueWriter">值写入器</param>
        void OnReadValue(TKey key, IValueWriter valueWriter);

        /// <summary>
        /// 将数据中的所有键与值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        void OnReadAll(IDataWriter<TKey> dataWriter);

        /// <summary>
        /// 将数据中的所有键与值进行筛选，并将满足筛选的键与值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">键值筛选器</param>
        void OnReadAll(IDataWriter<TKey> dataWriter, IValueFilter<TKey> valueFilter);

        /// <summary>
        /// 获取该数据所有的键。
        /// </summary>
        IEnumerable<TKey> Keys { get; }
    }

    /// <summary>
    /// 表示一个数据读取器。
    /// </summary>
    public interface IDataReader
    {
        /// <summary>
        /// 获取数据源键的数量。
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取数据源的 Id.
        /// 要求全局唯一。
        /// 不能是 HashCode 值。
        /// </summary>
        long ObjectId { get; }

        /// <summary>
        /// 将此数据读取器转换为具有键的类型的具体数据读取器。
        /// </summary>
        /// <typeparam name="T">键的类型</typeparam>
        /// <returns>返回具体数据读取器</returns>
        IDataReader<T> As<T>();
    }
}