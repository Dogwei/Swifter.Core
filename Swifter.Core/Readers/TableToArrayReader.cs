using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;

namespace Swifter.Readers
{
    internal sealed class TableToArrayReader : IDataReader<int>
    {
        public readonly ITableReader tableReader;

        public TableToArrayReader(ITableReader tableReader)
        {
            this.tableReader = tableReader;
        }

        public IValueReader this[int key]
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public IEnumerable<int> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public long ObjectId
        {
            get
            {
                return (long)Pointer.UnBox(tableReader);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int index = 0;

            if (tableReader.Count != 1)
            {
                while (tableReader.Read())
                {
                    dataWriter[index].WriteObject(tableReader);

                    ++index;
                }

                return;
            }

            while (tableReader.Read())
            {
                var valueWriter = dataWriter[index];

                try
                {
                    valueWriter.WriteObject(tableReader);
                }
                catch (Exception e)
                {
                    try
                    {
                        tableReader.OnReadValue(0, valueWriter);
                    }
                    catch (Exception)
                    {

                        throw e;
                    }
                }

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            throw new NotSupportedException();
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            while (tableReader.Read())
            {
                valueInfo.ValueCopyer.WriteObject(tableReader);

                valueInfo.Key = index;
                valueInfo.Type = tableReader.GetType();

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[index]);
                }

                ++index;
            }
        }

        IDataReader<T> IDataReader.As<T>()
        {
            if (this is IDataReader<T>)
            {
                return (IDataReader<T>)(object)this;
            }

            return new AsDataReader<int, T>(this);
        }
    }
}
