using Swifter.Readers;
using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.Writers
{
    internal sealed class TableToArrayWriter : IDataWriter<int>
    {
        public readonly ITableWriter tableWriter;

        public TableToArrayWriter(ITableWriter tableWriter)
        {
            this.tableWriter = tableWriter;

            Count = 0;
        }

        public IValueWriter this[int key]
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

        public int Count { get; private set; }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key < 0)
            {
                throw new IndexOutOfRangeException(nameof(key) + " : " + key);
            }

            if (key == Count - 1)
            {
                goto ReadObject;
            }

            if (key == Count)
            {
                tableWriter.Next();

                ++Count;

                goto ReadObject;
            }

            throw new ArgumentException(StringHelper.Format("Can only write current or next item By '{0}'.", nameof(TableToArrayWriter)));

            ReadObject:
            valueReader.ReadObject(tableWriter);
        }

        IDataWriter<T> IDataWriter.As<T>()
        {
            if (this is IDataWriter<T>)
            {
                return (IDataWriter<T>)(object)this;
            }

            return new AsDataWriter<int, T>(this);
        }
    }
}
