using Swifter.Tools;
using Swifter.VirtualViews;
using Swifter.Writers;
using System.Collections.Generic;

namespace Swifter.Readers
{
    /// <summary>
    /// 数据读取器键类型转换的接口。
    /// </summary>
    internal interface IAsDataReader
    {
        IDataReader Content { get; }
    }

    /// <summary>
    /// 数据读取器键类型转换的类型。
    /// </summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <typeparam name="TOut">输出类型</typeparam>
    public sealed class AsDataReader<TIn, TOut> : IDataReader<TOut>, IAsDataReader
    {
        /// <summary>
        /// 原始数据读取器。
        /// </summary>
        public readonly IDataReader<TIn> dataReader;

        /// <summary>
        /// 创建数据读取器键类型转换类的实例。
        /// </summary>
        /// <param name="dataReader">原始数据读取器</param>
        public AsDataReader(IDataReader<TIn> dataReader)
        {
            this.dataReader = dataReader;
        }

        /// <summary>
        /// 转换键，并返回该键对应的值读取器。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回值读取器</returns>
        public IValueReader this[TOut key]
        {
            get
            {
                return dataReader[XConvert<TOut, TIn>.Convert(key)];
            }
        }

        /// <summary>
        /// 获取转换后的键集合。
        /// </summary>
        public IEnumerable<TOut> Keys
        {
            get
            {
                return new AsEnumerate<TIn, TOut>(dataReader.Keys);
            }
        }

        /// <summary>
        /// 获取数据源键的数量。
        /// </summary>
        public int Count
        {
            get
            {
                return dataReader.Count;
            }
        }

        IDataReader IAsDataReader.Content
        {
            get
            {
                return dataReader;
            }
        }

        /// <summary>
        /// 获取原始数据读取器的数据源 Id。
        /// </summary>
        public long ObjectId
        {
            get
            {
                return dataReader.ObjectId;
            }
        }

        IDataReader<Out> IDataReader.As<Out>()
        {
            if (dataReader is IDataReader<Out>)
            {
                return (IDataReader<Out>)dataReader;
            }

            if (this is IDataReader<Out>)
            {
                return (IDataReader<Out>)(object)this;
            }

            return new AsDataReader<TIn, Out>(dataReader);
        }

        /// <summary>
        /// 将数据中的所有转换后的键与值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public void OnReadAll(IDataWriter<TOut> dataWriter)
        {
            dataReader.OnReadAll(new ReadAllWriter(dataWriter));
        }

        /// <summary>
        /// 转换键，并将该键对应的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(TOut key, IValueWriter valueWriter)
        {
            dataReader.OnReadValue(XConvert<TOut, TIn>.Convert(key), valueWriter);
        }

        /// <summary>
        /// 将数据中的所有转换后的键与值进行筛选，并将满足筛选的键与值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">键值筛选器</param>
        public void OnReadAll(IDataWriter<TOut> dataWriter, IValueFilter<TOut> valueFilter)
        {
            var readAllWriter = new ReadAllWriter(dataWriter, valueFilter);

            dataReader.OnReadAll(readAllWriter, readAllWriter);
        }

        private sealed class ReadAllWriter : IDataWriter<TIn>, IValueFilter<TIn>
        {
            public readonly IDataWriter<TOut> dataWriter;
            public readonly IValueFilter<TOut> valueFilter;

            public ReadAllWriter(IDataWriter<TOut> dataWriter)
            {
                this.dataWriter = dataWriter;
            }

            public ReadAllWriter(IDataWriter<TOut> dataWriter, IValueFilter<TOut> valueFilter)
            {
                this.dataWriter = dataWriter;
                this.valueFilter = valueFilter;
            }

            public IValueWriter this[TIn key]
            {
                get
                {
                    return dataWriter[XConvert<TIn, TOut>.Convert(key)];
                }
            }

            public IEnumerable<TIn> Keys
            {
                get
                {
                    return new AsEnumerate<TOut, TIn>(dataWriter.Keys);
                }
            }

            public int Count
            {
                get
                {
                    return dataWriter.Count;
                }
            }

            IDataWriter<T> IDataWriter.As<T>()
            {
                if (this is IDataWriter<T>)
                {
                    return (IDataWriter<T>)(object)this;
                }

                return new AsDataWriter<TIn, T>(this);
            }

            public bool Filter(ValueFilterInfo<TIn> valueInfo)
            {
                return valueFilter.Filter(new ValueFilterInfo<TOut>(XConvert<TIn, TOut>.Convert(valueInfo.Key),
                    valueInfo.Type,
                    valueInfo.ValueCopyer));
            }

            public void Initialize()
            {
                dataWriter.Initialize();
            }

            public void Initialize(int capacity)
            {
                dataWriter.Initialize(capacity);
            }

            public void OnWriteValue(TIn key, IValueReader valueReader)
            {
                dataWriter.OnWriteValue(XConvert<TIn, TOut>.Convert(key), valueReader);
            }
        }
    }
}
