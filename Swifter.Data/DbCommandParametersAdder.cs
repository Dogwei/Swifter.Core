using Swifter.Readers;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Swifter.Data
{
    internal sealed class DbCommandParametersAdder : IDataWriter<string>, IValueWriter
    {
        private readonly DbCommand dbCommand;

        private string name;

        public DbCommandParametersAdder(DbCommand dbCommand)
        {
            this.dbCommand = dbCommand;
        }

        public IValueWriter this[string key]
        {
            get
            {
                name = key;

                return this;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return null;
            }
        }

        public int Count
        {
            get
            {
                return dbCommand.Parameters.Count;
            }
        }

        public IDataWriter<T> As<T>()
        {
            throw new NotSupportedException();
        }

        public void DirectWrite(object value)
        {
            var item = dbCommand.CreateParameter();

            item.ParameterName = name;
            item.Value = value;

            dbCommand.Parameters.Add(item);
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            name = key;

            DirectWrite(valueReader.DirectRead());
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            throw new NotSupportedException("T-SQL Not Supported Array Parameter.");
        }

        public void WriteBoolean(bool value)
        {
            DirectWrite(value);
        }

        public void WriteByte(byte value)
        {
            DirectWrite(value);
        }

        public void WriteChar(char value)
        {
            DirectWrite(value);
        }

        public void WriteDateTime(DateTime value)
        {
            DirectWrite(value);
        }

        public void WriteDecimal(decimal value)
        {
            DirectWrite(value);
        }

        public void WriteDouble(double value)
        {
            DirectWrite(value);
        }

        public void WriteInt16(short value)
        {
            DirectWrite(value);
        }

        public void WriteInt32(int value)
        {
            DirectWrite(value);
        }

        public void WriteInt64(long value)
        {
            DirectWrite(value);
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            throw new NotSupportedException("T-SQL Not Supported Object Parameter.");
        }

        public void WriteSByte(sbyte value)
        {
            DirectWrite(value);
        }

        public void WriteSingle(float value)
        {
            DirectWrite(value);
        }

        public void WriteString(string value)
        {
            DirectWrite(value);
        }

        public void WriteUInt16(ushort value)
        {
            DirectWrite(value);
        }

        public void WriteUInt32(uint value)
        {
            DirectWrite(value);
        }

        public void WriteUInt64(ulong value)
        {
            DirectWrite(value);
        }
    }
}
