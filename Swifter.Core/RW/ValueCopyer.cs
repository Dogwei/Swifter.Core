using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 值暂存器。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public sealed class ValueCopyer<TKey> : IValueRW
    {
        private readonly IDataRW<TKey> dataRW;

        private readonly TKey key;

        private readonly ValueCopyer valueCopyer;

        /// <summary>
        /// 初始化值暂存器。
        /// </summary>
        /// <param name="dataRW">数据读写器</param>
        /// <param name="key">键</param>
        public ValueCopyer(IDataRW<TKey> dataRW, TKey key)
        {
            this.dataRW = dataRW;
            this.key = key;

            valueCopyer = new ValueCopyer();
        }

        /// <summary>
        /// 获取值的基础类型枚举。
        /// </summary>
        /// <returns>返回一个 BasicTypes 枚举值。</returns>
        public BasicTypes GetBasicType()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.GetBasicType();
        }

        /// <summary>
        /// 读取一个数组结构数据。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        public void ReadArray(IDataWriter<int> valueWriter)
        {
            dataRW.OnReadValue(key, valueCopyer);
            valueCopyer.ReadArray(valueWriter);
        }

        /// <summary>
        /// 读取一个 Boolean 值。
        /// </summary>
        /// <returns>返回一个 bool 值</returns>
        public bool ReadBoolean()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadBoolean();
        }

        /// <summary>
        /// 读取一个 Byte 值。
        /// </summary>
        /// <returns>返回一个 byte 值</returns>
        public byte ReadByte()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadByte();
        }

        /// <summary>
        /// 读取一个 Char 值。
        /// </summary>
        /// <returns>返回一个 char 值</returns>
        public char ReadChar()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadChar();
        }

        /// <summary>
        /// 读取一个 DateTime 值。
        /// </summary>
        /// <returns>返回一个 DateTime 值</returns>
        public DateTime ReadDateTime()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadDateTime();
        }

        /// <summary>
        /// 读取一个 Decimal 值。
        /// </summary>
        /// <returns>返回一个 decimal 值</returns>
        public decimal ReadDecimal()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadDecimal();
        }

        /// <summary>
        /// 读取一个未知类型的值。
        /// </summary>
        /// <returns>返回一个未知类型的值</returns>
        public object DirectRead()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.DirectRead();
        }

        /// <summary>
        /// 读取一个 Double 值。
        /// </summary>
        /// <returns>返回一个 double 值</returns>
        public double ReadDouble()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadDouble();
        }

        /// <summary>
        /// 读取一个 Int16 值。
        /// </summary>
        /// <returns>返回一个 short 值</returns>
        public short ReadInt16()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadInt16();
        }

        /// <summary>
        /// 读取一个 Int32 值。
        /// </summary>
        /// <returns>返回一个 int 值</returns>
        public int ReadInt32()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadInt32();
        }

        /// <summary>
        /// 读取一个 Int64 值。
        /// </summary>
        /// <returns>返回一个 long 值</returns>
        public long ReadInt64()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadInt64();
        }

        /// <summary>
        /// 读取一个对象结构数据。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        public void ReadObject(IDataWriter<string> valueWriter)
        {
            dataRW.OnReadValue(key, valueCopyer);
            valueCopyer.ReadObject(valueWriter);
        }

        /// <summary>
        /// 读取一个 SByte 值。
        /// </summary>
        /// <returns>返回一个 sbyte 值</returns>
        public sbyte ReadSByte()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadSByte();
        }

        /// <summary>
        /// 读取一个 Single 值。
        /// </summary>
        /// <returns>返回一个 flaot 值</returns>
        public float ReadSingle()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadSingle();
        }

        /// <summary>
        /// 读取一个 String 值。
        /// </summary>
        /// <returns>返回一个 string 值</returns>
        public string ReadString()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadString();
        }

        /// <summary>
        /// 读取一个 UInt16 值。
        /// </summary>
        /// <returns>返回一个 ushort 值</returns>
        public ushort ReadUInt16()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadUInt16();
        }

        /// <summary>
        /// 读取一个 UInt32 值。
        /// </summary>
        /// <returns>返回一个 uint 值</returns>
        public uint ReadUInt32()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadUInt32();
        }

        /// <summary>
        /// 读取一个 UInt64 值。
        /// </summary>
        /// <returns>返回一个 ulong 值</returns>
        public ulong ReadUInt64()
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadUInt64();
        }

        /// <summary>
        /// 写入一个数组结构数据。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void WriteArray(IDataReader<int> dataReader)
        {
            valueCopyer.WriteArray(dataReader);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Boolean 值。
        /// </summary>
        /// <param name="value">bool 值</param>
        public void WriteBoolean(bool value)
        {
            valueCopyer.WriteBoolean(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Byte 值。
        /// </summary>
        /// <param name="value">byte 值</param>
        public void WriteByte(byte value)
        {
            valueCopyer.WriteByte(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Char 值。
        /// </summary>
        /// <param name="value">char 值</param>
        public void WriteChar(char value)
        {
            valueCopyer.WriteChar(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime 值</param>
        public void WriteDateTime(DateTime value)
        {
            valueCopyer.WriteDateTime(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Decimal 值。
        /// </summary>
        /// <param name="value">decimal 值</param>
        public void WriteDecimal(decimal value)
        {
            valueCopyer.WriteDecimal(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个未知类型的值。
        /// </summary>
        /// <param name="value">未知类型的值</param>
        public void DirectWrite(object value)
        {
            valueCopyer.DirectWrite(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Double 值。
        /// </summary>
        /// <param name="value">double 值</param>
        public void WriteDouble(double value)
        {
            valueCopyer.WriteDouble(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Int16 值。
        /// </summary>
        /// <param name="value">short 值</param>
        public void WriteInt16(short value)
        {
            valueCopyer.WriteInt16(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Int32 值。
        /// </summary>
        /// <param name="value">int 值</param>
        public void WriteInt32(int value)
        {
            valueCopyer.WriteInt32(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Int64 值。
        /// </summary>
        /// <param name="value">long 值</param>
        public void WriteInt64(long value)
        {
            valueCopyer.WriteInt64(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个对象结构数据。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void WriteObject(IDataReader<string> dataReader)
        {
            valueCopyer.WriteObject(dataReader);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 SByte 值。
        /// </summary>
        /// <param name="value">sbyte 值</param>
        public void WriteSByte(sbyte value)
        {
            valueCopyer.WriteSByte(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Single 值。
        /// </summary>
        /// <param name="value">float 值</param>
        public void WriteSingle(float value)
        {
            valueCopyer.WriteSingle(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 String 值。
        /// </summary>
        /// <param name="value">string 值</param>
        public void WriteString(string value)
        {
            valueCopyer.WriteString(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 UInt16 值。
        /// </summary>
        /// <param name="value">ushort 值</param>
        public void WriteUInt16(ushort value)
        {
            valueCopyer.WriteUInt16(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 UInt32 值。
        /// </summary>
        /// <param name="value">uint 值</param>
        public void WriteUInt32(uint value)
        {
            valueCopyer.WriteUInt32(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 UInt64 值。
        /// </summary>
        /// <param name="value">ulong 值</param>
        public void WriteUInt64(ulong value)
        {
            valueCopyer.WriteUInt64(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 获取值读写器的名称。
        /// </summary>
        /// <returns>返回一个名称</returns>
        public override string ToString()
        {
            return dataRW.ToString() + "[\"" + key + "\"]";
        }
    }

    /// <summary>
    /// 值暂存器。
    /// </summary>
    public sealed class ValueCopyer: IValueRW
    {
        private OverlappedValue value;

        private object objValue;

        private BasicTypes valueType;

        /// <summary>
        /// 初始化值暂存器。
        /// </summary>
        public ValueCopyer()
        {
            valueType = BasicTypes.Null;
        }

        /// <summary>
        /// 获取值的基础类型枚举。
        /// </summary>
        /// <returns>返回一个 BasicTypes 枚举值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public BasicTypes GetBasicType()
        {
            return valueType;
        }

        /// <summary>
        /// 获取值的类型
        /// </summary>
        public Type Type
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                switch (valueType)
                {
                    case BasicTypes.Null:
                        return typeof(object);
                    case BasicTypes.Boolean:
                        return typeof(bool);
                    case BasicTypes.SByte:
                        return typeof(sbyte);
                    case BasicTypes.Int16:
                        return typeof(short);
                    case BasicTypes.Int32:
                        return typeof(int);
                    case BasicTypes.Int64:
                        return typeof(long);
                    case BasicTypes.Byte:
                        return typeof(byte);
                    case BasicTypes.UInt16:
                        return typeof(ushort);
                    case BasicTypes.UInt32:
                        return typeof(uint);
                    case BasicTypes.UInt64:
                        return typeof(ulong);
                    case BasicTypes.Single:
                        return typeof(float);
                    case BasicTypes.Double:
                        return typeof(double);
                    case BasicTypes.Decimal:
                        return typeof(decimal);
                    case BasicTypes.Char:
                        return typeof(char);
                    case BasicTypes.DateTime:
                        return typeof(DateTime);
                    case BasicTypes.String:
                        return typeof(string);
                }

                return objValue.GetType();
            }
        }

        /// <summary>
        /// 获取值暂存器的值。
        /// </summary>
        public object Value
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                switch (valueType)
                {
                    case BasicTypes.Null:
                        return null;
                    case BasicTypes.Boolean:
                        return value.Boolean;
                    case BasicTypes.SByte:
                        return value.SByte;
                    case BasicTypes.Int16:
                        return value.Int16;
                    case BasicTypes.Int32:
                        return value.Int32;
                    case BasicTypes.Int64:
                        return value.Int64;
                    case BasicTypes.Byte:
                        return value.Byte;
                    case BasicTypes.UInt16:
                        return value.UInt16;
                    case BasicTypes.UInt32:
                        return value.UInt32;
                    case BasicTypes.UInt64:
                        return value.UInt64;
                    case BasicTypes.Single:
                        return value.Single;
                    case BasicTypes.Double:
                        return value.Double;
                    case BasicTypes.Decimal:
                        return value.Decimal;
                    case BasicTypes.Char:
                        return value.Char;
                    case BasicTypes.DateTime:
                        return value.DateTime;
                }

                return objValue;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool TryDirectRead(object valueWriter)
        {
            if (valueType != BasicTypes.Direct)
            {
                return false;
            }

            if (valueWriter is IDirectContent)
            {
                ((IDirectContent)valueWriter).DirectContent = objValue;

                return true;
            }

            if (objValue == null)
            {
                return true;
            }

            var objValueType = objValue.GetType();

            var valueInterface = ValueInterface.GetInterface(objValueType);

            valueInterface.WriteValue(this, objValueType);

            return false;
        }

        /// <summary>
        /// 读取一个数组结构数据。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadArray(IDataWriter<int> valueWriter)
        {
            if (TryDirectRead(valueWriter))
            {
                return;
            }

            switch (valueType)
            {
                case BasicTypes.Null:
                    return;
                case BasicTypes.Boolean:
                    throw new InvalidCastException("Can't convert Boolean to Array.");
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                case BasicTypes.Single:
                case BasicTypes.Double:
                case BasicTypes.Decimal:
                    throw new InvalidCastException("Can't convert Number to Array.");
                case BasicTypes.Char:
                    throw new InvalidCastException("Can't convert Char to Array.");
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Array.");
                case BasicTypes.String:
                    throw new InvalidCastException("Can't convert String to Array.");
                case BasicTypes.Direct:
                case BasicTypes.Object:
                    throw new InvalidCastException("Can't convert Object to Array.");
            }

            var arrayValue = (IDataReader<int>)objValue;

            valueWriter.Initialize(arrayValue.Count);

            arrayValue.OnReadAll(valueWriter);
        }

        /// <summary>
        /// 读取一个 Boolean 值。
        /// </summary>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool ReadBoolean()
        {
            switch (valueType)
            {
                case BasicTypes.Boolean:
                    return value.Boolean;
                case BasicTypes.Null:
                    return false;
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                case BasicTypes.Single:
                case BasicTypes.Double:
                    return value.Int64 != 0;
                case BasicTypes.Decimal:
                    return value.Decimal != 0;
                case BasicTypes.Char:
                    throw new InvalidCastException("Can't convert Char to Boolean.");
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Boolean.");
                case BasicTypes.String:
                    return bool.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (bool)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Boolean.");
            }

            throw new InvalidCastException("Can't convert Object to Boolean.");
        }

        /// <summary>
        /// 读取一个 Byte 值。
        /// </summary>
        /// <returns>返回一个 byte 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte ReadByte()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                case BasicTypes.Char:
                    return value.Byte;
                case BasicTypes.Single:
                    return (byte)value.Single;
                case BasicTypes.Double:
                    return (byte)value.Double;
                case BasicTypes.Decimal:
                    return (byte)value.Decimal;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Byte.");
                case BasicTypes.String:
                    return byte.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (byte)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Byte.");
            }

            throw new InvalidCastException("Can't convert Object to Byte.");
        }

        /// <summary>
        /// 读取一个 Char 值。
        /// </summary>
        /// <returns>返回一个 char 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public char ReadChar()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    throw new InvalidCastException("Can't convert Null to Char.");
                case BasicTypes.Single:
                    return (char)value.Single;
                case BasicTypes.Double:
                    return (char)value.Double;
                case BasicTypes.Decimal:
                    return (char)value.Decimal;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Char.");
                case BasicTypes.String:
                    return ((string)objValue)[0];
                case BasicTypes.Direct:
                    return (char)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Byte.");
            }

            throw new InvalidCastException("Can't convert Object to Byte.");
        }

        /// <summary>
        /// 读取一个 DateTime 值。
        /// </summary>
        /// <returns>返回一个 DateTime 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DateTime ReadDateTime()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    throw new InvalidCastException("Can't convert Null to DateTime.");
                case BasicTypes.Boolean:
                    throw new InvalidCastException("Can't convert Boolean to DateTime.");
                case BasicTypes.UInt64:
                case BasicTypes.Int64:
                    return new DateTime(value.Int64);
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.Single:
                case BasicTypes.Double:
                case BasicTypes.Decimal:
                    throw new InvalidCastException("Can't convert Number to DateTime.");
                case BasicTypes.Char:
                    throw new InvalidCastException("Can't convert Char to DateTime.");
                case BasicTypes.DateTime:
                    return value.DateTime;
                case BasicTypes.String:
                    return DateTime.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (DateTime)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to DateTime.");
            }

            throw new InvalidCastException("Can't convert Object to DateTime.");
        }

        /// <summary>
        /// 读取一个 Decimal 值。
        /// </summary>
        /// <returns>返回一个 decimal 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public decimal ReadDecimal()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                    return value.Byte;
                case BasicTypes.SByte:
                    return value.SByte;
                case BasicTypes.Int16:
                    return value.Int16;
                case BasicTypes.Int32:
                    return value.Int32;
                case BasicTypes.Int64:
                    return value.Int64;
                case BasicTypes.Byte:
                    return value.Byte;
                case BasicTypes.UInt16:
                    return value.UInt16;
                case BasicTypes.UInt32:
                    return value.UInt32;
                case BasicTypes.UInt64:
                    return value.UInt64;
                case BasicTypes.Single:
                    return (decimal)value.Single;
                case BasicTypes.Double:
                    return (decimal)value.Double;
                case BasicTypes.Decimal:
                    return value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Decimal.");
                case BasicTypes.String:
                    return decimal.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (decimal)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Decimal.");
            }

            throw new InvalidCastException("Can't convert Object to Decimal.");
        }

        /// <summary>
        /// 读取一个未知类型的值。
        /// </summary>
        /// <returns>返回一个未知类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object DirectRead()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return null;
                case BasicTypes.Boolean:
                    return value.Boolean;
                case BasicTypes.SByte:
                    return value.SByte;
                case BasicTypes.Int16:
                    return value.Int16;
                case BasicTypes.Int32:
                    return value.Int32;
                case BasicTypes.Int64:
                    return value.Int64;
                case BasicTypes.Byte:
                    return value.Byte;
                case BasicTypes.UInt16:
                    return value.UInt16;
                case BasicTypes.UInt32:
                    return value.UInt32;
                case BasicTypes.UInt64:
                    return value.UInt64;
                case BasicTypes.Single:
                    return value.Single;
                case BasicTypes.Double:
                    return value.Double;
                case BasicTypes.Decimal:
                    return value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    return value.DateTime;
                case BasicTypes.String:
                    return (string)objValue;
                case BasicTypes.Direct:
                    return objValue;
            }

            if (objValue is IDirectContent)
            {
                return ((IDirectContent)objValue).DirectContent;
            }

            throw new NotSupportedException("Can't ReadDirect by Object/Array.");
        }

        /// <summary>
        /// 读取一个 Double 值。
        /// </summary>
        /// <returns>返回一个 double 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ReadDouble()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                    return value.Byte;
                case BasicTypes.SByte:
                    return value.SByte;
                case BasicTypes.Int16:
                    return value.Int16;
                case BasicTypes.Int32:
                    return value.Int32;
                case BasicTypes.Int64:
                    return value.Int64;
                case BasicTypes.Byte:
                    return value.Byte;
                case BasicTypes.UInt16:
                    return value.UInt16;
                case BasicTypes.UInt32:
                    return value.UInt32;
                case BasicTypes.UInt64:
                    return value.UInt64;
                case BasicTypes.Single:
                    return value.Single;
                case BasicTypes.Double:
                    return value.Double;
                case BasicTypes.Decimal:
                    return (double)value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Double.");
                case BasicTypes.String:
                    return double.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (double)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Double.");
            }

            throw new InvalidCastException("Can't convert Object to Double.");
        }

        /// <summary>
        /// 读取一个 Int16 值。
        /// </summary>
        /// <returns>返回一个 short 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public short ReadInt16()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Int32:
                    return (short)value.Int32;
                case BasicTypes.Int64:
                    return (short)value.Int64;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                    return value.Int16;
                case BasicTypes.Single:
                    return (short)value.Single;
                case BasicTypes.Double:
                    return (short)value.Double;
                case BasicTypes.Decimal:
                    return (short)value.Decimal;
                case BasicTypes.Char:
                    return (short)value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Int16.");
                case BasicTypes.String:
                    return short.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (short)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Int16.");
            }

            throw new InvalidCastException("Can't convert Object to Int16.");
        }

        /// <summary>
        /// 读取一个 Int32 值。
        /// </summary>
        /// <returns>返回一个 int 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ReadInt32()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                    return value.Int32;
                case BasicTypes.Int64:
                    return (int)value.Int64;
                case BasicTypes.Single:
                    return (int)value.Single;
                case BasicTypes.Double:
                    return (int)value.Double;
                case BasicTypes.Decimal:
                    return (int)value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Int32.");
                case BasicTypes.String:
                    return int.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (int)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Int32.");
            }

            throw new InvalidCastException("Can't convert Object to Int32.");
        }

        /// <summary>
        /// 读取一个 Int64 值。
        /// </summary>
        /// <returns>返回一个 long 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                    return value.Int64;
                case BasicTypes.Single:
                    return (long)value.Single;
                case BasicTypes.Double:
                    return (long)value.Double;
                case BasicTypes.Decimal:
                    return (long)value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Int64.");
                case BasicTypes.String:
                    return long.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (long)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Int64.");
            }

            throw new InvalidCastException("Can't convert Object to Int64.");
        }

        /// <summary>
        /// 读取一个对象结构数据。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadObject(IDataWriter<string> valueWriter)
        {
            if (TryDirectRead(valueWriter))
            {
                return;
            }

            switch (valueType)
            {
                case BasicTypes.Boolean:
                    throw new InvalidCastException("Can't convert Boolean to Object.");
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                case BasicTypes.Single:
                case BasicTypes.Double:
                case BasicTypes.Decimal:
                    throw new InvalidCastException("Can't convert Number to Object.");
                case BasicTypes.Char:
                    throw new InvalidCastException("Can't convert Char to Object.");
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Object.");
                case BasicTypes.String:
                    throw new InvalidCastException("Can't convert String to Object.");
                case BasicTypes.Direct:
                    throw new InvalidCastException("Can't convert DirectObject to Object.");
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Object.");
            }

            valueWriter.Initialize();

            ((IDataReader<string>)objValue).OnReadAll(valueWriter);
        }

        /// <summary>
        /// 读取一个 SByte 值。
        /// </summary>
        /// <returns>返回一个 sbyte 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public sbyte ReadSByte()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Int16:
                    return (sbyte)value.Int16;
                case BasicTypes.Int32:
                    return (sbyte)value.Int32;
                case BasicTypes.Int64:
                    return (sbyte)value.Int64;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                case BasicTypes.Char:
                    return value.SByte;
                case BasicTypes.Single:
                    return (sbyte)value.Single;
                case BasicTypes.Double:
                    return (sbyte)value.Double;
                case BasicTypes.Decimal:
                    return (sbyte)value.Decimal;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to SByte.");
                case BasicTypes.String:
                    return sbyte.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (sbyte)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to SByte.");
            }

            throw new InvalidCastException("Can't convert Object to SByte.");
        }

        /// <summary>
        /// 读取一个 Single 值。
        /// </summary>
        /// <returns>返回一个 flaot 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public float ReadSingle()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                    return value.Byte;
                case BasicTypes.SByte:
                    return value.SByte;
                case BasicTypes.Int16:
                    return value.Int16;
                case BasicTypes.Int32:
                    return value.Int32;
                case BasicTypes.Int64:
                    return value.Int64;
                case BasicTypes.Byte:
                    return value.Byte;
                case BasicTypes.UInt16:
                    return value.UInt16;
                case BasicTypes.UInt32:
                    return value.UInt32;
                case BasicTypes.UInt64:
                    return value.UInt64;
                case BasicTypes.Single:
                    return value.Single;
                case BasicTypes.Double:
                    return (float)value.Double;
                case BasicTypes.Decimal:
                    return (float)value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Single.");
                case BasicTypes.String:
                    return float.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (float)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to Single.");
            }

            throw new InvalidCastException("Can't convert Object to Single.");
        }

        /// <summary>
        /// 读取一个 String 值。
        /// </summary>
        /// <returns>返回一个 string 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ReadString()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return null;
                case BasicTypes.Boolean:
                    return value.Boolean.ToString();
                case BasicTypes.SByte:
                    return value.SByte.ToString();
                case BasicTypes.Int16:
                    return value.Int16.ToString();
                case BasicTypes.Int32:
                    return value.Int32.ToString();
                case BasicTypes.Int64:
                    return value.Int64.ToString();
                case BasicTypes.Byte:
                    return value.Byte.ToString();
                case BasicTypes.UInt16:
                    return value.UInt16.ToString();
                case BasicTypes.UInt32:
                    return value.UInt32.ToString();
                case BasicTypes.UInt64:
                    return value.UInt64.ToString();
                case BasicTypes.Single:
                    return value.Single.ToString();
                case BasicTypes.Double:
                    return value.Double.ToString();
                case BasicTypes.Decimal:
                    return value.Decimal.ToString();
                case BasicTypes.Char:
                    return value.Char.ToString();
                case BasicTypes.DateTime:
                    return value.DateTime.ToString();
                case BasicTypes.String:
                    return (string)objValue;
                case BasicTypes.Direct:
                    return (string)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to String.");
            }

            throw new InvalidCastException("Can't convert Object to String.");
        }

        /// <summary>
        /// 读取一个 UInt16 值。
        /// </summary>
        /// <returns>返回一个 ushort 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                    return value.UInt16;
                case BasicTypes.Single:
                    return (ushort)value.Single;
                case BasicTypes.Double:
                    return (ushort)value.Double;
                case BasicTypes.Decimal:
                    return (ushort)value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to UInt16.");
                case BasicTypes.String:
                    return ushort.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (ushort)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to UInt16.");
            }

            throw new InvalidCastException("Can't convert Object to UInt16.");
        }

        /// <summary>
        /// 读取一个 UInt32 值。
        /// </summary>
        /// <returns>返回一个 uint 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public uint ReadUInt32()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                    return value.UInt32;
                case BasicTypes.Single:
                    return (uint)value.Single;
                case BasicTypes.Double:
                    return (uint)value.Double;
                case BasicTypes.Decimal:
                    return (uint)value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to UInt32.");
                case BasicTypes.String:
                    return uint.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (uint)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to UInt32.");
            }

            throw new InvalidCastException("Can't convert Object to UInt32.");
        }

        /// <summary>
        /// 读取一个 UInt64 值。
        /// </summary>
        /// <returns>返回一个 ulong 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    return 0;
                case BasicTypes.Boolean:
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                    return value.UInt64;
                case BasicTypes.Single:
                    return (ulong)value.Single;
                case BasicTypes.Double:
                    return (ulong)value.Double;
                case BasicTypes.Decimal:
                    return (ulong)value.Decimal;
                case BasicTypes.Char:
                    return value.Char;
                case BasicTypes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to UInt64.");
                case BasicTypes.String:
                    return ulong.Parse((string)objValue);
                case BasicTypes.Direct:
                    return (ulong)objValue;
                case BasicTypes.Array:
                    throw new InvalidCastException("Can't convert Array to UInt64.");
            }

            throw new InvalidCastException("Can't convert Object to UInt64.");
        }

        /// <summary>
        /// 将值写入到值写入器中。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteTo(IValueWriter valueWriter)
        {
            switch (valueType)
            {
                case BasicTypes.Null:
                    valueWriter.DirectWrite(null);
                    break;
                case BasicTypes.Boolean:
                    valueWriter.WriteBoolean(value.Boolean);
                    break;
                case BasicTypes.SByte:
                    valueWriter.WriteSByte(value.SByte);
                    break;
                case BasicTypes.Int16:
                    valueWriter.WriteInt16(value.Int16);
                    break;
                case BasicTypes.Int32:
                    valueWriter.WriteInt32(value.Int32);
                    break;
                case BasicTypes.Int64:
                    valueWriter.WriteInt64(value.Int64);
                    break;
                case BasicTypes.Byte:
                    valueWriter.WriteByte(value.Byte);
                    break;
                case BasicTypes.UInt16:
                    valueWriter.WriteUInt16(value.UInt16);
                    break;
                case BasicTypes.UInt32:
                    valueWriter.WriteUInt32(value.UInt32);
                    break;
                case BasicTypes.UInt64:
                    valueWriter.WriteUInt64(value.UInt64);
                    break;
                case BasicTypes.Single:
                    valueWriter.WriteSingle(value.Single);
                    break;
                case BasicTypes.Double:
                    valueWriter.WriteDouble(value.Double);
                    break;
                case BasicTypes.Decimal:
                    valueWriter.WriteDecimal(value.Decimal);
                    break;
                case BasicTypes.Char:
                    valueWriter.WriteChar(value.Char);
                    break;
                case BasicTypes.DateTime:
                    valueWriter.WriteDateTime(value.DateTime);
                    break;
                case BasicTypes.String:
                    valueWriter.WriteString((string)objValue);
                    break;
                case BasicTypes.Direct:
                    valueWriter.DirectWrite(objValue);
                    break;
                case BasicTypes.Array:
                    valueWriter.WriteArray((IDataReader<int>)objValue);
                    break;
                case BasicTypes.Object:
                    valueWriter.WriteObject((IDataReader<string>)objValue);
                    break;
            }
        }

        /// <summary>
        /// 写入一个数组结构数据。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteArray(IDataReader<int> dataReader)
        {
            valueType = BasicTypes.Array;

            objValue = dataReader;
        }

        /// <summary>
        /// 写入一个 Boolean 值。
        /// </summary>
        /// <param name="value">bool 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            valueType = BasicTypes.Boolean;

            this.value.Boolean = value;
        }

        /// <summary>
        /// 写入一个 Byte 值。
        /// </summary>
        /// <param name="value">byte 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            valueType = BasicTypes.Byte;

            this.value.Byte = value;
        }

        /// <summary>
        /// 写入一个 Char 值。
        /// </summary>
        /// <param name="value">char 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteChar(char value)
        {
            valueType = BasicTypes.Char;

            this.value.Char = value;
        }

        /// <summary>
        /// 写入一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDateTime(DateTime value)
        {
            valueType = BasicTypes.DateTime;

            this.value.DateTime = value;
        }

        /// <summary>
        /// 写入一个 Decimal 值。
        /// </summary>
        /// <param name="value">decimal 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDecimal(decimal value)
        {
            valueType = BasicTypes.Decimal;

            this.value.Decimal = value;
        }

        /// <summary>
        /// 写入一个未知类型的值。
        /// </summary>
        /// <param name="value">未知类型的值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void DirectWrite(object value)
        {
            if (value == null)
            {
                valueType = BasicTypes.Null;

                return;
            }

            valueType = BasicTypes.Direct;

            objValue = value;
        }

        /// <summary>
        /// 写入一个 Double 值。
        /// </summary>
        /// <param name="value">double 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            valueType = BasicTypes.Double;

            this.value.Double = value;
        }

        /// <summary>
        /// 写入一个 Int16 值。
        /// </summary>
        /// <param name="value">short 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            valueType = BasicTypes.Int16;

            this.value.Int16 = value;
        }

        /// <summary>
        /// 写入一个 Int32 值。
        /// </summary>
        /// <param name="value">int 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            valueType = BasicTypes.Int32;

            this.value.Int32 = value;
        }

        /// <summary>
        /// 写入一个 Int64 值。
        /// </summary>
        /// <param name="value">long 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            valueType = BasicTypes.Int64;

            this.value.Int64 = value;
        }

        /// <summary>
        /// 写入一个对象结构数据。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteObject(IDataReader<string> dataReader)
        {
            valueType = BasicTypes.Object;

            objValue = dataReader;
        }

        /// <summary>
        /// 写入一个 SByte 值。
        /// </summary>
        /// <param name="value">sbyte 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSByte(sbyte value)
        {
            valueType = BasicTypes.SByte;

            this.value.SByte = value;
        }

        /// <summary>
        /// 写入一个 Single 值。
        /// </summary>
        /// <param name="value">float 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSingle(float value)
        {
            valueType = BasicTypes.Single;

            this.value.Single = value;
        }

        /// <summary>
        /// 写入一个 String 值。
        /// </summary>
        /// <param name="value">string 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteString(string value)
        {
            if (value == null)
            {
                valueType = BasicTypes.Null;

                return;
            }

            valueType = BasicTypes.String;

            objValue = value;
        }

        /// <summary>
        /// 写入一个 UInt16 值。
        /// </summary>
        /// <param name="value">ushort 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            valueType = BasicTypes.UInt16;

            this.value.UInt16 = value;
        }

        /// <summary>
        /// 写入一个 UInt32 值。
        /// </summary>
        /// <param name="value">uint 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt32(uint value)
        {
            valueType = BasicTypes.UInt32;

            this.value.UInt32 = value;
        }

        /// <summary>
        /// 写入一个 UInt64 值。
        /// </summary>
        /// <param name="value">ulong 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            valueType = BasicTypes.UInt64;

            this.value.UInt64 = value;
        }
    }
}