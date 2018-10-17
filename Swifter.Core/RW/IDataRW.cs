using Swifter.Readers;
using Swifter.Writers;

namespace Swifter.RW
{
    /// <summary>
    /// 数据读写器
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public interface IDataRW<TKey> : IDataRW, IDataReader<TKey>, IDataWriter<TKey>
    {
    }

    /// <summary>
    /// 表示数据读写器
    /// </summary>
    public interface IDataRW : IDataReader, IDataWriter
    {
    }
}