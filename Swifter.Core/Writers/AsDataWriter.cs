using Swifter.Readers;
using Swifter.Tools;
using Swifter.VirtualViews;
using System.Collections.Generic;

namespace Swifter.Writers
{
    /// <summary>
    /// 数据写入器键类型转换的接口。
    /// </summary>
    internal interface IAsDataWriter
    {
        IDataWriter Content { get; }
    }

    /// <summary>
    /// 数据写入器键类型转换的类型。
    /// </summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <typeparam name="TOut">输出类型</typeparam>
    public sealed class AsDataWriter<TIn, TOut> : IDataWriter<TOut>, IAsDataWriter
    {
        /// <summary>
        /// 原始数据写入器。
        /// </summary>
        public readonly IDataWriter<TIn> dataWriter;

        /// <summary>
        /// 创建数据写入器键类型转换类的实例。
        /// </summary>
        /// <param name="dataWriter">原始数据写入器</param>
        public AsDataWriter(IDataWriter<TIn> dataWriter)
        {
            this.dataWriter = dataWriter;
        }

        /// <summary>
        /// 转换键，并返回该键对应的值写入器。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回值写入器</returns>
        public IValueWriter this[TOut key]
        {
            get
            {
                return dataWriter[XConvert<TOut, TIn>.Convert(key)];
            }
        }

        /// <summary>
        /// 获取转换后的键集合。
        /// </summary>
        public IEnumerable<TOut> Keys
        {
            get
            {
                return new AsEnumerate<TIn, TOut>(dataWriter.Keys);
            }
        }

        /// <summary>
        /// 获取数据源键的数量。
        /// </summary>
        public int Count
        {
            get
            {
                return dataWriter.Count;
            }
        }

        IDataWriter IAsDataWriter.Content
        {
            get
            {
                return dataWriter;
            }
        }

        IDataWriter<Out> IDataWriter.As<Out>()
        {
            if (dataWriter is IDataWriter<Out>)
            {
                return (IDataWriter<Out>)dataWriter;
            }

            if (this is IDataWriter<Out>)
            {
                return (IDataWriter<Out>)(object)this;
            }

            return new AsDataWriter<TIn, Out>(dataWriter);
        }

        /// <summary>
        /// 初始化数据源。
        /// </summary>
        public void Initialize()
        {
            dataWriter.Initialize();
        }

        /// <summary>
        /// 初始化具有指定容量的数据源。
        /// </summary>
        /// <param name="capacity">指定容量</param>
        public void Initialize(int capacity)
        {
            dataWriter.Initialize(capacity);
        }

        /// <summary>
        /// 从值读取器中读取一个值设置到指定键的值中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(TOut key, IValueReader valueReader)
        {
            dataWriter.OnWriteValue(XConvert<TOut, TIn>.Convert(key), valueReader);
        }
    }
}