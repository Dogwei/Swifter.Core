using Swifter.RW;
using Swifter.Tools;
using Swifter.VirtualViews;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Swifter.Readers
{
    /// <summary>
    /// 重写数据库读取器，使它成为表格读取器。
    /// </summary>
    public sealed class OverrideDbDataReader : ITableReader
    {
        /// <summary>
        /// 数据源。
        /// </summary>
        public readonly DbDataReader dbDataReader;

        /// <summary>
        /// 初始化数据读取器。
        /// </summary>
        /// <param name="dbDataReader">数据源</param>
        public OverrideDbDataReader(DbDataReader dbDataReader)
        {
            this.dbDataReader = dbDataReader;
        }

        /// <summary>
        /// 获取位于指定索引处的值读取器。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <returns>返回值读取器</returns>
        public IValueReader this[int key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return new ValueReader(dbDataReader, key);
            }
        }


        /// <summary>
        /// 获取位于指定名称的值读取器。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <returns>返回值读取器</returns>
        public IValueReader this[string key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return this[dbDataReader.GetOrdinal(key)];
            }
        }

        IEnumerable<int> IDataReader<int>.Keys
        {
            get
            {
                return ArrayView<int>.CreateIndexView(dbDataReader.FieldCount);
            }
        }

        /// <summary>
        /// 获取表格列的数量。
        /// </summary>
        public int Count
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return dbDataReader.FieldCount;
            }
        }

        /// <summary>
        /// 获取表格列的名称集合。
        /// </summary>
        public IEnumerable<string> Keys
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return ArrayView<string>.Create(index => dbDataReader.GetName(index), Count);
            }
        }

        /// <summary>
        /// 获取数据源的 Id。
        /// </summary>
        public long ObjectId
        {
            get
            {
                return (long)Pointer.UnBox(dbDataReader);
            }
        }

        /// <summary>
        /// 读取所有值当前行的所有值，然后写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = dbDataReader.FieldCount;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[i]);
            }
        }

        /// <summary>
        /// 读取所有值当前行的所有值，然后写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            int length = dbDataReader.FieldCount;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[dbDataReader.GetName(i)]);
            }
        }

        /// <summary>
        /// 读取指定位置的值，然后写入到值写入器中。
        /// </summary>
        /// <param name="key">指定位置</param>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (dbDataReader.IsDBNull(key))
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var typeHandle = (long)TypeHelper.GetTypeHandle(dbDataReader.GetFieldType(key));

            if (typeHandle == TypeInfo<string>.Int64TypeHandle) ValueInterface<string>.Content.WriteValue(valueWriter, dbDataReader.GetString(key));
            else if (typeHandle == TypeInfo<int>.Int64TypeHandle) ValueInterface<int>.Content.WriteValue(valueWriter, dbDataReader.GetInt32(key));
            else if (typeHandle == TypeInfo<double>.Int64TypeHandle) ValueInterface<double>.Content.WriteValue(valueWriter, dbDataReader.GetDouble(key));
            else if (typeHandle == TypeInfo<decimal>.Int64TypeHandle) ValueInterface<decimal>.Content.WriteValue(valueWriter, dbDataReader.GetDecimal(key));
            else if (typeHandle == TypeInfo<DateTime>.Int64TypeHandle) ValueInterface<DateTime>.Content.WriteValue(valueWriter, dbDataReader.GetDateTime(key));
            else if (typeHandle == TypeInfo<bool>.Int64TypeHandle) ValueInterface<bool>.Content.WriteValue(valueWriter, dbDataReader.GetBoolean(key));
            else if (typeHandle == TypeInfo<Guid>.Int64TypeHandle) ValueInterface<Guid>.Content.WriteValue(valueWriter, dbDataReader.GetGuid(key));
            else if (typeHandle == TypeInfo<long>.Int64TypeHandle) ValueInterface<long>.Content.WriteValue(valueWriter, dbDataReader.GetInt64(key));
            else if (typeHandle == TypeInfo<short>.Int64TypeHandle) ValueInterface<short>.Content.WriteValue(valueWriter, dbDataReader.GetInt16(key));
            else if (typeHandle == TypeInfo<byte>.Int64TypeHandle) ValueInterface<byte>.Content.WriteValue(valueWriter, dbDataReader.GetByte(key));
            else if (typeHandle == TypeInfo<float>.Int64TypeHandle) ValueInterface<float>.Content.WriteValue(valueWriter, dbDataReader.GetFloat(key));
            else if (typeHandle == TypeInfo<char>.Int64TypeHandle) ValueInterface<char>.Content.WriteValue(valueWriter, dbDataReader.GetChar(key));
            else ValueInterface<object>.Content.WriteValue(valueWriter, dbDataReader.GetValue(key));
        }

        /// <summary>
        /// 读取指定名称的值，然后写入到值写入器中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            OnReadValue(dbDataReader.GetOrdinal(key), valueWriter);
        }

        /// <summary>
        /// 读取下一行数据。
        /// </summary>
        /// <returns>返回是否有下一行数据</returns>
        public bool Read()
        {
            return dbDataReader.Read();
        }

        /// <summary>
        /// 读取当前行的所有数据并进行筛选，然后将筛选结果写入器数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
        {
            int length = dbDataReader.FieldCount;

            var valueInfo = new ValueFilterInfo<string>();

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, valueInfo.ValueCopyer);

                valueInfo.Key = dbDataReader.GetName(i);
                valueInfo.Type = valueInfo.ValueCopyer.Type;

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        /// <summary>
        /// 读取当前行的所有数据并进行筛选，然后将筛选结果写入器数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = dbDataReader.FieldCount;

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

        IDataReader<T> IDataReader.As<T>()
        {
            if (this is IDataReader<T>)
            {
                return (IDataReader<T>)(object)this;
            }

            return new AsDataReader<string, T>(this);
        }

        private sealed class ValueReader : IValueReader
        {
            public readonly DbDataReader dbDataReader;
            public readonly int ordinal;

            public ValueReader(DbDataReader dbDataReader, int ordinal)
            {
                this.dbDataReader = dbDataReader;
                this.ordinal = ordinal;
            }

            public BasicTypes GetBasicType()
            {
                if (dbDataReader.IsDBNull(ordinal))
                {
                    return BasicTypes.Null;
                }

                var type = dbDataReader.GetFieldType(ordinal);

                var result = TypeHelper.GetBasicType(type);

                if (result != BasicTypes.Object && result != BasicTypes.Array)
                {
                    return result;
                }

                return BasicTypes.Direct;
            }

            public void ReadArray(IDataWriter<int> valueWriter)
            {
                throw new NotSupportedException(StringHelper.Format("Type '{0}' not support '{1}'.", nameof(OverrideDbDataReader), nameof(ReadArray)));
            }

            public bool ReadBoolean()
            {
                return dbDataReader.GetBoolean(ordinal);
            }

            public byte ReadByte()
            {
                return dbDataReader.GetByte(ordinal);
            }

            public char ReadChar()
            {
                return dbDataReader.GetChar(ordinal);
            }

            public DateTime ReadDateTime()
            {
                return dbDataReader.GetDateTime(ordinal);
            }

            public decimal ReadDecimal()
            {
                return dbDataReader.GetDecimal(ordinal);
            }

            public object DirectRead()
            {
                return dbDataReader.GetValue(ordinal);
            }

            public double ReadDouble()
            {
                return dbDataReader.GetDouble(ordinal);
            }

            public short ReadInt16()
            {
                return dbDataReader.GetInt16(ordinal);
            }

            public int ReadInt32()
            {
                return dbDataReader.GetInt32(ordinal);
            }

            public long ReadInt64()
            {
                return dbDataReader.GetInt64(ordinal);
            }

            public void ReadObject(IDataWriter<string> valueWriter)
            {
                throw new NotSupportedException(StringHelper.Format("Type '{0}' not support '{1}'.", nameof(OverrideDbDataReader), nameof(ReadObject)));
            }

            public sbyte ReadSByte()
            {
                return (sbyte)dbDataReader.GetInt16(ordinal);
            }

            public float ReadSingle()
            {
                return dbDataReader.GetFloat(ordinal);
            }

            public string ReadString()
            {
                return dbDataReader.GetString(ordinal);
            }

            public ushort ReadUInt16()
            {
                return (ushort)dbDataReader.GetInt32(ordinal);
            }

            public uint ReadUInt32()
            {
                return (uint)dbDataReader.GetInt64(ordinal);
            }

            public ulong ReadUInt64()
            {
                return (ulong)dbDataReader.GetInt64(ordinal);
            }
        }
    }

    internal sealed class DbDataReaderInterface<T> : IValueInterface<T> where T : DbDataReader
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T>)
            {
                return ((IValueReader<T>)valueReader).ReadValue();
            }

            return DConvert<T>.Convert(valueReader.DirectRead());
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var dataReader = new OverrideDbDataReader(value);
            var toArrayReader = new TableToArrayReader(dataReader);

            valueWriter.WriteArray(toArrayReader);
        }
    }
}