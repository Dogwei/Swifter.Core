using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    internal sealed unsafe class JsonDefaultSerializer : IValueWriter, IValueWriter<Guid>, IDataWriter<string>, IDataWriter<int>
    {
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonDefaultSerializer(int maxDepth)
        {
            this.maxDepth = maxDepth;

            offset = 0;

            hGlobal = HGlobalChars.ThreadInstance;
            
            Expand(255);
        }

        public IValueWriter this[int key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return this;
            }
        }

        public IValueWriter this[string key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                InternalWriteString(key);

                WriteKeyAfter();

                return this;
            }
        }

        
        public readonly int maxDepth;

        public HGlobalChars hGlobal;
        public int offset;
        public int depth;
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand(int expandMinSize)
        {
            if (hGlobal.count - offset < expandMinSize)
            {
                hGlobal.Expand(expandMinSize);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(char c)
        {
            hGlobal.chars[offset] = c;

            ++offset;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteLongString(string value)
        {
            Expand(2);

            Append('"');

            fixed (char* lpValue = value)
            {
                int length = value.Length;

                for (int i = 0; i < value.Length;)
                {
                    int count = length - i;

                    Expand(count + 2);

                    for (int end = count + offset; offset < end; ++i)
                    {
                        var c = lpValue[i];

                        switch (c)
                        {
                            case '\\':
                                Append('\\');
                                Append('\\');
                                break;
                            case '"':
                                Append('\\');
                                Append('"');
                                break;
                            case '\n':
                                Append('\\');
                                Append('n');
                                break;
                            case '\r':
                                Append('\\');
                                Append('r');
                                break;
                            case '\t':
                                Append('\\');
                                Append('t');
                                break;
                            default:
                                Append(c);
                                break;
                        }
                    }
                }
            }

            Expand(2);

            Append('"');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteString(string value)
        {
            int length = value.Length;

            if (length > 300)
            {
                InternalWriteLongString(value);

                return;
            }

            Expand(length * 2 + 2);

            var chars = hGlobal.chars;
            
            int offset = this.offset;

            chars[offset] = '"';
            ++offset;

            for (int i = 0; i < length; ++i)
            {
                var c = value[i];

                switch (c)
                {
                    case '\\':
                        chars[offset] = '\\';
                        ++offset;
                        chars[offset] = '\\';
                        ++offset;
                        break;
                    case '"':
                        chars[offset] = '\\';
                        ++offset;
                        chars[offset] = '"';
                        ++offset;
                        break;
                    case '\n':
                        chars[offset] = '\\';
                        ++offset;
                        chars[offset] = 'n';
                        ++offset;
                        break;
                    case '\r':
                        chars[offset] = '\\';
                        ++offset;
                        chars[offset] = 'r';
                        ++offset;
                        break;
                    case '\t':
                        chars[offset] = '\\';
                        ++offset;
                        chars[offset] = 't';
                        ++offset;
                        break;
                    default:
                        chars[offset] = c;
                        ++offset;
                        break;
                }
            }

            chars[offset] = '"';
            ++offset;

            this.offset = offset;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool AddDepth()
        {
            ++depth;

            if (depth <= maxDepth)
            {
                return true;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SubtractDepth()
        {
            --depth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override string ToString()
        {
            return new string(hGlobal.chars, 0, offset - 1);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteTo(TextWriter textWriter)
        {
            textWriter.Write(ToString());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Initialize()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Initialize(int capacity)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataWriter<T> As<T>()
        {
            if ((object)this is IDataWriter<T> writer)
            {
                return writer;
            }

            throw new NotSupportedException(StringHelper.Format("JsonSerializer not support the type '{0}' as Key.", typeof(T).Name));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int Count => 0;

        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnWriteValue(string key, IValueReader valueReader)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnWriteValue(int key, IValueReader valueReader)
        {
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValueAfter()
        {
            Append(',');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyAfter()
        {
            Append(':');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void DirectWrite(object value)
        {
            if (value == null)
            {
                WriteNull();

                return;
            }

            if (value is IConvertible)
            {
                switch (((IConvertible)value).GetTypeCode())
                {
                    case TypeCode.DBNull:
                        WriteNull();
                        return;
                    case TypeCode.Boolean:
                        WriteBoolean((bool)value);
                        return;
                    case TypeCode.Char:
                        WriteChar((char)value);
                        return;
                    case TypeCode.SByte:
                        WriteSByte((sbyte)value);
                        return;
                    case TypeCode.Byte:
                        WriteByte((byte)value);
                        return;
                    case TypeCode.Int16:
                        WriteInt16((short)value);
                        return;
                    case TypeCode.UInt16:
                        WriteUInt16((ushort)value);
                        return;
                    case TypeCode.Int32:
                        WriteInt32((int)value);
                        return;
                    case TypeCode.UInt32:
                        WriteUInt32((uint)value);
                        return;
                    case TypeCode.Int64:
                        WriteInt64((long)value);
                        return;
                    case TypeCode.UInt64:
                        WriteUInt64((ulong)value);
                        return;
                    case TypeCode.Single:
                        WriteSingle((float)value);
                        return;
                    case TypeCode.Double:
                        WriteDouble((double)value);
                        return;
                    case TypeCode.Decimal:
                        WriteDecimal((decimal)value);
                        return;
                    case TypeCode.DateTime:
                        WriteDateTime((DateTime)value);
                        return;
                    case TypeCode.String:
                        WriteString((string)value);
                        return;
                }
            }

            if (value is Guid)
            {
                WriteValue((Guid)value);

                return;
            }

            WriteString(value.ToString());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteNull()
        {
            Expand(6);

            Append('n');
            Append('u');
            Append('l');
            Append('l');

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteArray(IDataReader<int> dataReader)
        {
            Expand(2);

            Append('[');

            int tOffset = offset;

            if (AddDepth())
            {
                dataReader.OnReadAll(this);
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }

            Append(']');

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            Expand(6);

            if (value)
            {
                // true
                Append('t');
                Append('r');
                Append('u');
                Append('e');
            }
            else
            {
                // false
                Append('f');
                Append('a');
                Append('l');
                Append('s');
                Append('e');
            }

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            Expand(4);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteChar(char value)
        {
            Expand(4);

            Append('"');
            Append(value);
            Append('"');

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDateTime(DateTime value)
        {
            Expand(32);

            Append('"');

            offset += DateTimeHelper.ToISOString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDecimal(decimal value)
        {
            Expand(33);

            offset += NumberHelper.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            Expand(19);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);


            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            Expand(8);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);


            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            Expand(12);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteObject(IDataReader<string> dataReader)
        {
            Expand(2);

            Append('{');

            int tOffset = offset;

            if (AddDepth())
            {
                dataReader.OnReadAll(this);
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }
            
            Append('}');
            
            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSByte(sbyte value)
        {
            Expand(5);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSingle(float value)
        {
            Expand(19);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteString(value);

                WriteValueAfter();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            Expand(7);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt32(uint value)
        {
            Expand(11);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValue(Guid value)
        {
            Expand(40);

            Append('"');

            offset += NumberHelper.ToString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }
    }
}