using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Data.Common;

namespace Swifter.Data
{
    sealed class ReadScalarReader : IValueReader, IValueReader<Guid>
    {
        readonly DbDataReader dbDataReader;

        string[] names;

        bool readed;

        int ordinal;

        public ReadScalarReader(DbDataReader dbDataReader)
        {
            this.dbDataReader = dbDataReader;
        }
        
        public object DirectRead()
        {
            if (readed || dbDataReader.Read())
            {
                readed = true;

                var value = dbDataReader[ordinal];

                if (value == DBNull.Value)
                    return null;

                return value;
            }

            return null;
        }
        
        public void ReadArray(IDataWriter<int> valueWriter)
        {
            valueWriter.Initialize();

            if (readed || dbDataReader.Read())
            {
                readed = true;

                int i = 0;

                do
                {
                    valueWriter.OnWriteValue(i, this);

                    ++i;

                } while (dbDataReader.Read());
            }
        }

        public bool ReadBoolean()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToBoolean(dbDataReader[ordinal]);
            return default;
        }

        public byte ReadByte()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToByte(dbDataReader[ordinal]);
            return default;
        }

        public char ReadChar()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToChar(dbDataReader[ordinal]);
            return default;
        }

        public DateTime ReadDateTime()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToDateTime(dbDataReader[ordinal]);
            return default;
        }

        public decimal ReadDecimal()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToDecimal(dbDataReader[ordinal]);
            return default;
        }

        public double ReadDouble()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToDouble(dbDataReader[ordinal]);
            return default;
        }

        public short ReadInt16()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToInt16(dbDataReader[ordinal]);
            return default;
        }

        public int ReadInt32()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToInt32(dbDataReader[ordinal]);
            return default;
        }

        public long ReadInt64()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToInt64(dbDataReader[ordinal]);
            return default;
        }

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            if (readed || dbDataReader.Read())
            {
                readed = true;

                valueWriter.Initialize();

                if (names == null)
                {
                    names = new string[dbDataReader.FieldCount];

                    for (int i = names.Length - 1; i >= 0; --i)
                    {
                        names[i] = dbDataReader.GetName(i);
                    }
                }
                
                for (ordinal = names.Length - 1; ordinal >= 0; --ordinal)
                {
                    valueWriter.OnWriteValue(names[ordinal], this);
                }
            }
        }

        public sbyte ReadSByte()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToSByte(dbDataReader[ordinal]);
            return default;
        }

        public float ReadSingle()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToSingle(dbDataReader[ordinal]);
            return default;
        }

        public string ReadString()
        {
            if (readed || dbDataReader.Read())
            {
                var value = dbDataReader[ordinal];

                if (value == DBNull.Value)
                    return null;

                if (value is string st)
                    return st;

                return Convert.ToString(value);
            }

            return default;
        }

        public ushort ReadUInt16()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToUInt16(dbDataReader[ordinal]);
            return default;
        }

        public uint ReadUInt32()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToUInt32(dbDataReader[ordinal]);
            return default;
        }

        public ulong ReadUInt64()
        {
            if (readed || dbDataReader.Read())
                return Convert.ToUInt64(dbDataReader[ordinal]);
            return default;
        }

        public Guid ReadValue()
        {
            if (readed || dbDataReader.Read())
            {
                var value = dbDataReader[ordinal];

                if (value is Guid guid)
                    return guid;

                if (value is string st)
                    return new Guid(st);

                throw new InvalidCastException();
            }
            return default;
        }

        public T? ReadNullable<T>() where T : struct
        {
            if (readed || dbDataReader.Read())
            {
                var value = dbDataReader[ordinal];

                if (value == DBNull.Value)
                    return null;

                if (value is T t)
                    return t;

                return XConvert.Convert<T>(value);
            }

            return default;
        }
    }
}