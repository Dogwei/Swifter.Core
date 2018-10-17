using Swifter.Readers;
using Swifter.Tools;
using Swifter.VirtualViews;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataSet Reader impl.
    /// </summary>
    internal sealed class DataSetRW : IDataRW<int>, IInitialize<DataSet>
    {
        IValueReader IDataReader<int>.this[int key] => new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key]=> throw new NotSupportedException();

        public IEnumerable<int> Keys => ArrayView<int>.CreateIndexView(Count);

        public int Count => Content.Tables.Count;

        public long ObjectId => (long)Pointer.UnBox(Content);

        public DataSet Content { get; private set; }
        
        public void Initialize(DataSet obj)
        {
            Content = obj;
        }

        public void Initialize()
        {
            Initialize(1);
        }

        public void Initialize(int capacity)
        {
            if (Content == null)
            {
                Content = new DataSet();
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            for (int i = 0; i < Content.Tables.Count; i++)
            {
                OnReadValue(i, dataWriter[i]);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < Content.Tables.Count; i++)
            {
                OnReadValue(i, valueInfo.ValueCopyer);

                valueInfo.Key = i;
                valueInfo.Type = valueInfo.ValueCopyer.Type;

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<DataTable>.Content.WriteValue(valueWriter, Content.Tables[key]);
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key == Content.Tables.Count)
            {
                Content.Tables.Add(ValueInterface<DataTable>.Content.ReadValue(valueReader));
            }

            throw new NotSupportedException();
        }

        IDataReader<T> Readers.IDataReader.As<T>()
        {
            if (((object)this) is IDataReader<T> dataReader)
            {
                return dataReader;
            }

            return new AsDataReader<int, T>(this);
        }

        IDataWriter<T> IDataWriter.As<T>()
        {
            if (((object)this) is IDataWriter<T> dataWriter)
            {
                return dataWriter;
            }

            return new AsDataWriter<int, T>(this);
        }
    }

    internal sealed class DataSetInterface<T> : IValueInterface<T> where T : DataSet
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T>)
            {
                return ((IValueReader<T>)valueReader).ReadValue();
            }

            var result = Activator.CreateInstance<T>();

            var dataReader = new DataSetRW();

            dataReader.Initialize(result);

            valueReader.ReadArray(dataReader);

            return result;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var dataReader = new DataSetRW();

            dataReader.Initialize(value);

            valueWriter.WriteArray(dataReader);
        }
    }
}