using Swifter.Readers;
using Swifter.Tools;
using Swifter.VirtualViews;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataTable Reader impl.
    /// </summary>
    internal sealed class DataTableRW : ITableRW, IInitialize<DataTable>
    {
        private int readIndex;
        private int writeIndex;

        public DataTableRW()
        {
        }

        public IValueRW this[string key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return new ValueCopyer<string>(this, key);
            }
        }

        public IValueRW this[int key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return new ValueCopyer<int>(this, key);
            }
        }

        IValueReader IDataReader<string>.this[string key]
        {
            get
            {
                return this[key];
            }
        }

        IValueWriter IDataWriter<string>.this[string key]
        {
            get
            {
                return this[key];
            }
        }

        IValueReader IDataReader<int>.this[int key]
        {
            get
            {
                return this[key];
            }
        }

        IValueWriter IDataWriter<int>.this[int key]
        {
            get
            {
                return this[key];
            }
        }

        public IEnumerable<string> Keys
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return ArrayView<string>.Create(index => Content.Columns[index].ColumnName, Count);
            }
        }

        public int Count
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return Content.Columns.Count;
            }
        }

        IEnumerable<int> IDataReader<int>.Keys=> ArrayView<int>.CreateIndexView(Count);

        IEnumerable<int> IDataWriter<int>.Keys => ArrayView<int>.CreateIndexView(Count);

        public DataTable Content { get; private set; }

        public long ObjectId
        {
            get
            {
                return (long)Pointer.UnBox(Content);
            }
        }

        public void Initialize(DataTable dataTable)
        {
            Content = dataTable;

            readIndex = -1;
            writeIndex = -1;
        }

        public void Initialize()
        {
            if (Content == null)
            {
                Content = new DataTable();
            }

            readIndex = -1;
            writeIndex = -1;
        }

        public void Initialize(int capacity)
        {
            if (Content == null)
            {
                Content = new DataTable();
            }

            readIndex = -1;
            writeIndex = -1;
        }

        public void Next()
        {
            ++writeIndex;

            Content.Rows.Add(Content.NewRow());
        }

        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            int length = Count;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[Content.Columns[i].ColumnName]);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = Count;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[i]);
            }
        }

        public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
        {
            int length = Count;

            var valueInfo = new ValueFilterInfo<string>();

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, valueInfo.ValueCopyer);

                valueInfo.Key = Content.Columns[i].ColumnName;
                valueInfo.Type = valueInfo.ValueCopyer.Type;

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, valueInfo.ValueCopyer);

                valueInfo.Key = i;
                valueInfo.Type = valueInfo.ValueCopyer.Type;

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[i]);
                }
            }
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            ValueInterface<object>.Content.WriteValue(valueWriter, Content.Rows[readIndex][key]);
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<object>.Content.WriteValue(valueWriter, Content.Rows[readIndex][key]);
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            Content.Rows[readIndex][key] = valueReader.DirectRead();
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            Content.Rows[readIndex][key] = valueReader.DirectRead();
        }

        public bool Read()
        {
            ++readIndex;

            return readIndex < Content.Rows.Count;
        }

        IDataReader<T> Readers.IDataReader.As<T>()
        {
            if (this is IDataReader<T>)
            {
                return (IDataReader<T>)(object)this;
            }

            return new AsDataReader<string, T>(this);
        }

        IDataWriter<T> IDataWriter.As<T>()
        {
            if (this is IDataWriter<T>)
            {
                return (IDataWriter<T>)(object)this;
            }

            return new AsDataWriter<string, T>(this);
        }
    }

    internal sealed class DataTableInterface<T> : IValueInterface<T> where T : DataTable
    {
        public T ReadValue(IValueReader valueReader)
        {
            var result = Activator.CreateInstance<T>();

            var dataReader = new DataTableRW();

            dataReader.Initialize(result);

            var toArrayWriter = new TableToArrayWriter(dataReader);

            valueReader.ReadArray(toArrayWriter);

            return result;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var dataReader = new OverrideDbDataReader(value.CreateDataReader());

            var toArrayReader = new TableToArrayReader(dataReader);

            valueWriter.WriteArray(toArrayReader);
        }
    }
}