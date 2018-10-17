using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class HelperValueRW : IValueRW
    {
        public BasicTypes BasicTypes { get; set; }

        public object Content { get; set; }

        public object DirectRead()
        {
            BasicTypes = BasicTypes.Direct;

            return null;
        }

        public void DirectWrite(object value)
        {
            BasicTypes = BasicTypes.Direct;
        }

        public BasicTypes GetBasicType()
        {
            return BasicTypes;
        }

        public void ReadArray(IDataWriter<int> valueWriter)
        {
            BasicTypes = BasicTypes.Array;

            Content = valueWriter;
        }

        public bool ReadBoolean()
        {
            BasicTypes = BasicTypes.Boolean;

            return default(bool);
        }

        public byte ReadByte()
        {
            BasicTypes = BasicTypes.Byte;

            return default(byte);
        }

        public char ReadChar()
        {
            BasicTypes = BasicTypes.Char;

            return default(char);
        }

        public DateTime ReadDateTime()
        {
            BasicTypes = BasicTypes.DateTime;

            return default(DateTime);
        }

        public decimal ReadDecimal()
        {
            BasicTypes = BasicTypes.Decimal;

            return default(decimal);
        }

        public double ReadDouble()
        {
            BasicTypes = BasicTypes.Double;

            return default(double);
        }

        public short ReadInt16()
        {
            BasicTypes = BasicTypes.Int16;

            return default(short);
        }

        public int ReadInt32()
        {
            BasicTypes = BasicTypes.Int32;

            return default(int);
        }

        public long ReadInt64()
        {
            BasicTypes = BasicTypes.Int64;

            return default(long);
        }

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            BasicTypes = BasicTypes.Object;

            Content = valueWriter;
        }

        public sbyte ReadSByte()
        {
            BasicTypes = BasicTypes.SByte;

            return default(sbyte);
        }

        public float ReadSingle()
        {
            BasicTypes = BasicTypes.Single;

            return default(float);
        }

        public string ReadString()
        {
            BasicTypes = BasicTypes.String;

            return default(string);
        }

        public ushort ReadUInt16()
        {
            BasicTypes = BasicTypes.UInt16;

            return default(ushort);
        }

        public uint ReadUInt32()
        {
            BasicTypes = BasicTypes.UInt32;

            return default(uint);
        }

        public ulong ReadUInt64()
        {
            BasicTypes = BasicTypes.UInt64;

            return default(ulong);
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            BasicTypes = BasicTypes.Array;

            Content = dataReader;
        }

        public void WriteBoolean(bool value)
        {
            BasicTypes = BasicTypes.Boolean;
        }

        public void WriteByte(byte value)
        {
            BasicTypes = BasicTypes.Byte;
        }

        public void WriteChar(char value)
        {
            BasicTypes = BasicTypes.Char;
        }

        public void WriteDateTime(DateTime value)
        {
            BasicTypes = BasicTypes.DateTime;
        }

        public void WriteDecimal(decimal value)
        {
            BasicTypes = BasicTypes.Decimal;
        }

        public void WriteDouble(double value)
        {
            BasicTypes = BasicTypes.Double;
        }

        public void WriteInt16(short value)
        {
            BasicTypes = BasicTypes.Int16;
        }

        public void WriteInt32(int value)
        {
            BasicTypes = BasicTypes.Int32;
        }

        public void WriteInt64(long value)
        {
            BasicTypes = BasicTypes.Int64;
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            BasicTypes = BasicTypes.Object;

            Content = dataReader;
        }

        public void WriteSByte(sbyte value)
        {
            BasicTypes = BasicTypes.SByte;
        }

        public void WriteSingle(float value)
        {
            BasicTypes = BasicTypes.Single;
        }

        public void WriteString(string value)
        {
            BasicTypes = BasicTypes.String;
        }

        public void WriteUInt16(ushort value)
        {
            BasicTypes = BasicTypes.UInt16;
        }

        public void WriteUInt32(uint value)
        {
            BasicTypes = BasicTypes.UInt32;
        }

        public void WriteUInt64(ulong value)
        {
            BasicTypes = BasicTypes.UInt64;
        }
    }
}
