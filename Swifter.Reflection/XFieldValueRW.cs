using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 提供 XObjectRW 的字段读写器。
    /// </summary>
    public class XFieldValueRW : IValueRW
    {
        XObjectRW objectRW;
        internal IXFieldRW fieldRW;

        internal XFieldValueRW(XObjectRW objectRW, IXFieldRW fieldRW)
        {
            this.objectRW = objectRW;
            this.fieldRW = fieldRW;
        }

        internal XFieldValueRW SetFieldRW(IXFieldRW fieldRW)
        {
            this.fieldRW = fieldRW;

            return this;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal T _read<T>()
        {
            return fieldRW.ReadValue<T>(objectRW);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void _write<T>(T value)
        {
            fieldRW.WriteValue(objectRW, value);
        }

        /// <summary>
        /// 直接读取值。
        /// </summary>
        /// <returns>值</returns>
        public object DirectRead()
        {
            return _read<object>();
        }

        /// <summary>
        /// 直接写入值。
        /// </summary>
        /// <param name="value">值</param>
        public void DirectWrite(object value)
        {
            _write(value);
        }

        /// <summary>
        /// 获取该字段类型的 BasicTypes 值。
        /// </summary>
        /// <returns></returns>
        public BasicTypes GetBasicType()
        {
            return fieldRW.BasicType;
        }

        /// <summary>
        /// 读取一个数组结构。
        /// </summary>
        /// <param name="valueWriter">数组结构写入器</param>
        public void ReadArray(IDataWriter<int> valueWriter)
        {
            ValueCopyer valueCopyer = new ValueCopyer();

            fieldRW.OnReadValue(objectRW, valueCopyer);

            valueCopyer.ReadArray(valueWriter);
        }

        /// <summary>
        /// 读取一个 Boolean 值。
        /// </summary>
        /// <returns>返回 bool 值。</returns>
        public bool ReadBoolean()
        {
            return _read<bool>();
        }

        /// <summary>
        /// 读取一个 Byte 值。
        /// </summary>
        /// <returns>返回 byte 值。</returns>
        public byte ReadByte()
        {
            return _read<byte>();
        }

        /// <summary>
        /// 读取一个 Char 值。
        /// </summary>
        /// <returns>返回 char 值。</returns>
        public char ReadChar()
        {
            return _read<char>();
        }

        /// <summary>
        /// 读取一个 DateTime 值。
        /// </summary>
        /// <returns>返回 DateTime 值。</returns>
        public DateTime ReadDateTime()
        {
            return _read<DateTime>();
        }

        /// <summary>
        /// 读取一个 Decimal 值。
        /// </summary>
        /// <returns>返回 decimal 值。</returns>
        public decimal ReadDecimal()
        {
            return _read<decimal>();
        }

        /// <summary>
        /// 读取一个 Double 值。
        /// </summary>
        /// <returns>返回 double 值。</returns>
        public double ReadDouble()
        {
            return _read<double>();
        }

        /// <summary>
        /// 读取一个 Int16 值。
        /// </summary>
        /// <returns>返回 short 值。</returns>
        public short ReadInt16()
        {
            return _read<short>();
        }

        /// <summary>
        /// 读取一个 Int32 值。
        /// </summary>
        /// <returns>返回 int 值。</returns>
        public int ReadInt32()
        {
            return _read<int>();
        }

        /// <summary>
        /// 读取一个 Int64 值。
        /// </summary>
        /// <returns>返回 long 值。</returns>
        public long ReadInt64()
        {
            return _read<long>();
        }

        /// <summary>
        /// 读取一个对象结构数据。
        /// </summary>
        /// <param name="valueWriter">对象结构数据写入器</param>
        public void ReadObject(IDataWriter<string> valueWriter)
        {
            ValueCopyer valueCopyer = new ValueCopyer();

            fieldRW.OnReadValue(objectRW, valueCopyer);

            valueCopyer.ReadObject(valueWriter);
        }

        /// <summary>
        /// 读取一个 SByte 值。
        /// </summary>
        /// <returns>返回 sbyte 值。</returns>
        public sbyte ReadSByte()
        {
            return _read<sbyte>();
        }

        /// <summary>
        /// 读取一个 Single 值。
        /// </summary>
        /// <returns>返回 float 值。</returns>
        public float ReadSingle()
        {
            return _read<float>();
        }

        /// <summary>
        /// 读取一个 String 值。
        /// </summary>
        /// <returns>返回 string 值。</returns>
        public string ReadString()
        {
            return _read<string>();
        }

        /// <summary>
        /// 读取一个 UInt16 值。
        /// </summary>
        /// <returns>返回 ushort 值。</returns>
        public ushort ReadUInt16()
        {
            return _read<ushort>();
        }

        /// <summary>
        /// 读取一个 UInt32 值。
        /// </summary>
        /// <returns>返回 uint 值。</returns>
        public uint ReadUInt32()
        {
            return _read<uint>();
        }

        /// <summary>
        /// 读取一个 UInt64 值。
        /// </summary>
        /// <returns>返回 ulong 值。</returns>
        public ulong ReadUInt64()
        {
            return _read<ulong>();
        }

        /// <summary>
        /// 写入一个数组结构数据。
        /// </summary>
        /// <param name="dataReader">数组结构数据读取器</param>
        public void WriteArray(IDataReader<int> dataReader)
        {
            ValueCopyer valueCopyer = new ValueCopyer();

            valueCopyer.WriteArray(dataReader);

            fieldRW.OnWriteValue(objectRW, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Boolean 值。
        /// </summary>
        /// <param name="value">bool 值</param>
        public void WriteBoolean(bool value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 Byte 值。
        /// </summary>
        /// <param name="value">byte 值</param>
        public void WriteByte(byte value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 Char 值。
        /// </summary>
        /// <param name="value">char 值</param>
        public void WriteChar(char value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime 值</param>
        public void WriteDateTime(DateTime value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 Decimal 值。
        /// </summary>
        /// <param name="value">decimal 值</param>
        public void WriteDecimal(decimal value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 Double 值。
        /// </summary>
        /// <param name="value">double 值</param>
        public void WriteDouble(double value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 Int16 值。
        /// </summary>
        /// <param name="value">short 值</param>
        public void WriteInt16(short value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 Int32 值。
        /// </summary>
        /// <param name="value">int 值</param>
        public void WriteInt32(int value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 Int64 值。
        /// </summary>
        /// <param name="value">long 值</param>
        public void WriteInt64(long value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个对象结构数据。
        /// </summary>
        /// <param name="dataReader">对象结构数据读取器</param>
        public void WriteObject(IDataReader<string> dataReader)
        {
            ValueCopyer valueCopyer = new ValueCopyer();

            valueCopyer.WriteObject(dataReader);

            fieldRW.OnWriteValue(objectRW, valueCopyer);
        }

        /// <summary>
        /// 写入一个 SByte 值。
        /// </summary>
        /// <param name="value">sbyte 值</param>
        public void WriteSByte(sbyte value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 Single 值。
        /// </summary>
        /// <param name="value">float 值</param>
        public void WriteSingle(float value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 String 值。
        /// </summary>
        /// <param name="value">bool 值</param>
        public void WriteString(string value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 UInt16 值。
        /// </summary>
        /// <param name="value">ushort 值</param>
        public void WriteUInt16(ushort value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 UInt32 值。
        /// </summary>
        /// <param name="value">uint 值</param>
        public void WriteUInt32(uint value)
        {
            _write(value);
        }

        /// <summary>
        /// 写入一个 UInt64 值。
        /// </summary>
        /// <param name="value">ulong 值</param>
        public void WriteUInt64(ulong value)
        {
            _write(value);
        }

        /// <summary>
        /// 获取字段或属性读写器的名称。
        /// </summary>
        /// <returns>返回一个名称</returns>
        public override string ToString()
        {
            return objectRW.ToString() + "[\"" + fieldRW.Name + "\"]";
        }
    }

    /// <summary>
    /// 提供 XObjectRW 的字段读写器。
    /// </summary>
    /// <typeparam name="T">字段类型</typeparam>
    public sealed class XFieldValueRW<T> : XFieldValueRW, IValueRW<T>
    {
        internal XFieldValueRW(XObjectRW objectRW, IXFieldRW fieldRW) : base(objectRW, fieldRW)
        {
        }

        /// <summary>
        /// 直接读取值。
        /// </summary>
        /// <returns>值</returns>
        public T ReadValue()
        {
            return _read<T>();
        }

        /// <summary>
        /// 直接设置值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(T value)
        {
            _write(value);
        }
    }
}