using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    internal sealed unsafe class JsonSerializer : IValueWriter, IValueWriter<Guid>, IDataWriter<string>, IDataWriter<int>, IValueFilter<string>, IValueFilter<int>
    {
        /// <summary>
        /// True: In Array, False: In Object.
        /// </summary>
        public bool isInArray;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonSerializer(JsonFormatterOptions options, int maxDepth)
        {
            this.options = options;

            this.maxDepth = maxDepth;

            offset = 0;

            hGlobal = HGlobalChars.ThreadInstance;

            Expand(255);

            if ((options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.MultiReferencingReference)) != 0)
            {
                objectIds = new IdCache<int>();
            }
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
                WriteKeyBefore();

                InternalWriteString(key);

                WriteKeyAfter();

                return this;
            }
        }


        public readonly IdCache<int> objectIds;
        public readonly JsonFormatterOptions options;
        public readonly int maxDepth;

        public string indentedChars;
        public string lineBreak;
        public string middleChars;

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

            Append('"');

            int offset = this.offset;

            for (int i = 0; i < length; ++i)
            {
                var c = value[i];

                switch (c)
                {
                    case '\\':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        break;
                    case '"':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = '"';
                        ++offset;
                        break;
                    case '\n':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = 'n';
                        ++offset;
                        break;
                    case '\r':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = 'r';
                        ++offset;
                        break;
                    case '\t':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = 't';
                        ++offset;
                        break;
                    default:
                        hGlobal.chars[offset] = c;
                        ++offset;
                        break;
                }
            }

            this.offset = offset;

            Append('"');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SaveObjectId(IDataReader dataReader)
        {
            long objectId = dataReader.ObjectId;

            objectIds[objectId] = objectIds.Count;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int GetObjectId(IDataReader dataReader)
        {
            long objectId = dataReader.ObjectId;

            if (objectId == 0)
            {
                return -1;
            }

            if (objectIds.TryGetValue(objectId, out int result))
            {
                return result;
            }

            objectIds[objectId] = objectIds.Count;

            return -1;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool AddDepth()
        {
            ++depth;

            if (depth <= maxDepth)
            {
                return true;
            }

            if ((options & JsonFormatterOptions.OutOfDepthException) != 0)
            {
                throw new JsonOutOfDepthException();
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SubtractDepth()
        {
            --depth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void DirectWrite(string value)
        {
            int length = value.Length;

            Expand(length + 2);

            for (int i = 0; i < length; ++i)
            {
                Append(value[i]);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool Filter(ValueCopyer valueCopyer)
        {
            var basicType = valueCopyer.GetBasicType();

            if ((options & JsonFormatterOptions.MultiReferencingNull) != 0 && (basicType == BasicTypes.Array || basicType == BasicTypes.Object))
            {
                var dataReader = (IDataReader)valueCopyer.Value;

                var objectId = GetObjectId(dataReader);

                if (objectId != -1)
                {
                    valueCopyer.DirectWrite(null);

                    basicType = BasicTypes.Null;
                }
            }

            if ((options & JsonFormatterOptions.IgnoreNull) != 0)
            {
                if (basicType == BasicTypes.Null)
                {
                    return false;
                }
            }

            if ((options & JsonFormatterOptions.IgnoreZero) != 0)
            {
                switch (valueCopyer.GetBasicType())
                {
                    case BasicTypes.SByte:
                        return valueCopyer.ReadSByte() != 0;
                    case BasicTypes.Int16:
                        return valueCopyer.ReadInt16() != 0;
                    case BasicTypes.Int32:
                        return valueCopyer.ReadInt32() != 0;
                    case BasicTypes.Int64:
                        return valueCopyer.ReadInt64() != 0;
                    case BasicTypes.Byte:
                        return valueCopyer.ReadByte() != 0;
                    case BasicTypes.UInt16:
                        return valueCopyer.ReadUInt16() != 0;
                    case BasicTypes.UInt32:
                        return valueCopyer.ReadUInt32() != 0;
                    case BasicTypes.UInt64:
                        return valueCopyer.ReadUInt64() != 0;
                    case BasicTypes.Single:
                        return valueCopyer.ReadSingle() != 0;
                    case BasicTypes.Double:
                        return valueCopyer.ReadDouble() != 0;
                    case BasicTypes.Decimal:
                        return valueCopyer.ReadDecimal() != 0;
                }
            }

            if ((options & JsonFormatterOptions.IgnoreEmptyString) != 0)
            {
                switch (valueCopyer.GetBasicType())
                {
                    case BasicTypes.String:
                        return valueCopyer.ReadString() != "";
                }
            }

            return true;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool Filter(ValueFilterInfo<string> valueInfo)
        {
            if (!Filter(valueInfo.ValueCopyer))
            {
                return false;
            }

            return true;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool Filter(ValueFilterInfo<int> valueInfo)
        {
            if (!Filter(valueInfo.ValueCopyer))
            {
                return false;
            }

            return true;
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
            if (this is IDataWriter<T>)
            {
                return (IDataWriter<T>)(object)this;
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
        public void WriteValueBefore()
        {
            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                if (isInArray)
                {
                    DirectWrite(lineBreak);

                    for (int i = depth; i > 0; --i)
                    {
                        DirectWrite(indentedChars);
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValueAfter()
        {
            Append(',');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyBefore()
        {
            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                if (!isInArray)
                {
                    DirectWrite(lineBreak);

                    for (int i = depth; i > 0; --i)
                    {
                        DirectWrite(indentedChars);
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyAfter()
        {
            Append(':');

            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                DirectWrite(middleChars);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStructBefore()
        {
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStructAfter()
        {
            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                DirectWrite(lineBreak);

                for (int i = depth; i > 0; --i)
                {
                    DirectWrite(indentedChars);
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool CheckObject(IDataReader dataReader)
        {
            if ((options & JsonFormatterOptions.MultiReferencingNull) != 0)
            {
                if (options >= JsonFormatterOptions.IgnoreNull)
                {
                    SaveObjectId(dataReader);

                    return false;
                }

                int objectId = GetObjectId(dataReader);

                if (objectId != -1)
                {
                    WriteNull();

                    return true;
                }
            }
            else if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                int objectId = GetObjectId(dataReader);

                if (objectId != -1)
                {
                    WriteValueBefore();

                    DirectWrite("ref_" + objectId);

                    WriteValueAfter();

                    return true;
                }
            }

            return false;
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
            WriteValueBefore();

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
            if (CheckObject(dataReader))
            {
                return;
            }

            WriteValueBefore();

            Expand(2);

            Append('[');

            WriteStructBefore();

            var isInArray = this.isInArray;

            this.isInArray = true;

            int tOffset = offset;

            if (AddDepth())
            {
                if (options >= JsonFormatterOptions.IgnoreNull && (options & JsonFormatterOptions.ArrayOnFilter) != 0)
                {
                    dataReader.OnReadAll(this, this);
                }
                else
                {
                    dataReader.OnReadAll(this);
                }
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }

            WriteStructAfter();

            Append(']');

            this.isInArray = isInArray;

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            WriteValueBefore();

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
            WriteValueBefore();

            Expand(4);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteChar(char value)
        {
            WriteValueBefore();

            Expand(4);

            Append('"');
            Append(value);
            Append('"');

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDateTime(DateTime value)
        {
            WriteValueBefore();

            Expand(32);

            Append('"');

            offset += DateTimeHelper.ToISOString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDecimal(decimal value)
        {
            WriteValueBefore();

            Expand(33);

            offset += NumberHelper.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            WriteValueBefore();


            Expand(19);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);


            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            WriteValueBefore();


            Expand(8);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);


            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            WriteValueBefore();


            Expand(12);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            WriteValueBefore();


            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteObject(IDataReader<string> dataReader)
        {
            if (CheckObject(dataReader))
            {
                return;
            }

            WriteValueBefore();

            Expand(2);

            Append('{');

            WriteStructBefore();

            var isInArray = this.isInArray;

            this.isInArray = false;

            int tOffset = offset;

            if (AddDepth())
            {
                if (options >= JsonFormatterOptions.IgnoreNull)
                {
                    dataReader.OnReadAll(this, this);
                }
                else
                {
                    dataReader.OnReadAll(this);
                }
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }

            WriteStructAfter();

            Append('}');

            this.isInArray = isInArray;

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSByte(sbyte value)
        {
            WriteValueBefore();


            Expand(5);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSingle(float value)
        {
            WriteValueBefore();


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
                WriteValueBefore();
                
                InternalWriteString(value);

                WriteValueAfter();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            WriteValueBefore();


            Expand(7);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt32(uint value)
        {
            WriteValueBefore();


            Expand(11);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            WriteValueBefore();


            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValue(Guid value)
        {
            WriteValueBefore();


            Expand(40);

            Append('"');

            offset += NumberHelper.ToString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }
    }
}