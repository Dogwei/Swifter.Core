using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;

namespace Swifter.Formatters
{
    sealed class AuxiliaryWriter<Key> : IDataWriter<Key>, IDirectContent
    {
        [ThreadStatic]
        public static IDataWriter<Key> ThreadDataWriter;

        static AuxiliaryWriter()
        {
            ValueInterface<AuxiliaryWriter<Key>>.Content = new AuxiliaryInterface<Key>();
        }

        public readonly IDataWriter<Key> dataWriter;

        public AuxiliaryWriter()
        {
            dataWriter = ThreadDataWriter;
        }

        public IValueWriter this[Key key] => dataWriter[key];

        public int Count => dataWriter.Count;

        public IEnumerable<Key> Keys => dataWriter.Keys;

        public object DirectContent
        {
            get
            {
                if (dataWriter is IDirectContent directContent)
                {
                    return directContent.DirectContent;
                }

                throw new NotSupportedException(StringHelper.Format("This data {0} does not support direct {1} content.", "writer", "get"));
            }
            set
            {
                if (dataWriter is IDirectContent directContent)
                {
                    directContent.DirectContent = value;

                    return;
                }

                throw new NotSupportedException(StringHelper.Format("This data {0} does not support direct {1} content.", "writer", "set"));
            }
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteValue(Key key, IValueReader valueReader) => dataWriter.OnWriteValue(key, valueReader);

        public void OnWriteAll(IDataReader<Key> dataReader) => dataWriter.OnWriteAll(dataReader);
    }

    sealed class AuxiliaryInterface<TKey> : IValueInterface<AuxiliaryWriter<TKey>>
    {
        static readonly bool IsArray;

        static AuxiliaryInterface()
        {
            switch (TypeInfo<TKey>.TypeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    IsArray = true;
                    break;
            }
        }

        public AuxiliaryWriter<TKey> ReadValue(IValueReader valueReader)
        {
            var auxiliaryWriter = new AuxiliaryWriter<TKey>();

            if (valueReader is IValueFiller<TKey> tReader)
            {
                tReader.FillValue(auxiliaryWriter);
            }
            else if (IsArray)
            {
                valueReader.ReadArray(auxiliaryWriter.As<int>());
            }
            else
            {
                valueReader.ReadObject(auxiliaryWriter.As<string>());
            }

            return auxiliaryWriter;
        }

        public void WriteValue(IValueWriter valueWriter, AuxiliaryWriter<TKey> value)
        {
            throw new NotSupportedException("Unable write a writer.");
        }
    }
}