using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    internal sealed unsafe class JsonDeserializer : IValueReader, IValueReader<Guid>
    {
        private const NumberStyles NumberStyle = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        private readonly List<IDataWriter> objects;

        private readonly JsonFormatterOptions options;
        private readonly char* chars;

        private int begin;
        private readonly int end;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonDeserializer(char* chars, int begin, int end, JsonFormatterOptions options)
        {
            if (begin >= end)
            {
                throw new ArgumentException("Json text cannot be empty.");
            }

            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                objects = new List<IDataWriter>();
            }

            this.options = options;
            this.chars = chars;
            this.begin = begin;
            this.end = end;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private string InternalReadString()
        {
            char textChar = chars[begin];

            int textLength = 0;

            ++begin;

            int index = begin;

            while (index < end)
            {
                if (chars[index] == textChar)
                {
                    goto String;
                }
                else if (chars[index] == '\\')
                {
                    if (index + 1 < end && chars[index + 1] == 'u')
                    {
                        index += 6;
                    }
                    else
                    {
                        index += 2;
                    }
                }
                else
                {
                    ++index;
                }

                ++textLength;
            }

            throw GetException();

            String:

            string result;

            /* 内容没有转义符，直接截取返回。 */
            if (index - begin == textLength)
            {
                result = new string(chars, begin, textLength);

                begin = index + 1;

                return result;
            }

            result = new string('\0', textLength);

            fixed (char* pResult = result)
            {
                for (int i = 0; begin < index; ++i, ++begin)
                {
                    if (chars[begin] == '\\')
                    {
                        ++begin;

                        if (begin >= index)
                        {
                            throw GetException();
                        }

                        switch (chars[begin])
                        {
                            case 'b':
                                pResult[i] = '\b';
                                break;
                            case 'f':
                                pResult[i] = '\f';
                                break;
                            case 'n':
                                pResult[i] = '\n';
                                break;
                            case 't':
                                pResult[i] = '\t';
                                break;
                            case 'r':
                                pResult[i] = '\r';
                                break;
                            case 'u':

                                if (begin + 4 >= index)
                                {
                                    throw GetException();
                                }

                                pResult[i] = (char)((GetDigital(chars[begin + 1]) << 12) | (GetDigital(chars[begin + 2]) << 8) | (GetDigital(chars[begin + 3]) << 4) | (GetDigital(chars[begin + 4])));

                                begin += 4;

                                break;
                            default:
                                pResult[i] = chars[begin];
                                break;
                        }
                    }
                    else
                    {
                        pResult[i] = chars[begin];
                    }
                }
            }

            ++begin;

            return result;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int GetDigital(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }

            if (c >= 'a' && c <= 'f')
            {
                return c - 'a' + 10;
            }

            if (c >= 'A' && c <= 'F')
            {
                return c - 'A' + 10;
            }

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private double InternalReadDouble()
        {
            var index = NumberHelper.Decimal.TryParse(chars + begin, end - begin, out double r);

            if (index != 0)
            {
                begin += index;

                return r;
            }

            return double.Parse(ReadString(), NumberStyle);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private Exception GetException()
        {
            var begin = this.begin;

            if (begin >= end)
            {
                begin = end - 1;
            }

            int line = 1;
            int lineBegin = 0;
            int column = 1;

            for (int i = 0; i < begin; i++)
            {
                if (chars[i] == '\n')
                {
                    ++line;

                    lineBegin = i;
                }
            }

            column = begin - lineBegin + 1;

            var exception = new JsonDeserializeException();

            exception.Line = line;
            exception.Column = column;
            exception.Index = begin;
            exception.Text = chars[begin].ToString();

            return exception;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonValueTypes GetValueType()
        {
            switch (chars[begin])
            {
                case '"':
                case '\'':
                    return JsonValueTypes.String;
                case '{':
                    return JsonValueTypes.Object;
                case '[':
                    return JsonValueTypes.Array;
                case 't':
                case 'T':
                    if (StringHelper.IgnoreCaseEquals(chars, begin, end, "TRUE"))
                    {
                        return JsonValueTypes.True;
                    }
                    break;
                case 'f':
                case 'F':
                    if (StringHelper.IgnoreCaseEquals(chars, begin, end, "FALSE"))
                    {
                        return JsonValueTypes.False;
                    }
                    break;
                case 'n':
                case 'N':
                    if (StringHelper.IgnoreCaseEquals(chars, begin, end, "NULL"))
                    {
                        return JsonValueTypes.Null;
                    }
                    break;
                case 'u':
                case 'U':
                    if (StringHelper.IgnoreCaseEquals(chars, begin, end, "UNDEFINED"))
                    {
                        return JsonValueTypes.Undefiend;
                    }
                    break;
                case 'r':
                case 'R':
                    if (StringHelper.IgnoreCaseEquals(chars, begin, end, "REF_"))
                    {
                        return JsonValueTypes.Reference;
                    }
                    break;
                case '-':
                case '+':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return JsonValueTypes.Number;
            }

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            //var index = NumberHelper.Decimal.TryParse(chars + begin, end - begin, out long r); // Faster.
            var index = NumberHelper.Decimal.TryParseExp(chars + begin, end - begin, out long r); // JSON Standard.

            if (index != 0)
            {
                begin += index;

                return r;
            }

            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return long.Parse(InternalReadString());
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                case JsonValueTypes.Reference:
                    throw new InvalidCastException("Cannot convert object/array to Number.");
                case JsonValueTypes.True:
                    begin += 4;
                    return 1;
                case JsonValueTypes.False:
                    begin += 5;
                    return 0;
                case JsonValueTypes.Null:
                    throw new InvalidCastException("Cannot convert NULL to Number.");
                case JsonValueTypes.Undefiend:
                    begin += 9;
                    return 0;
            }

            return long.Parse(ReadString(), NumberStyle);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ReadDouble()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return double.Parse(InternalReadString());
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                case JsonValueTypes.Reference:
                    throw new InvalidCastException("Cannot convert object/array to Number.");
                case JsonValueTypes.True:
                    begin += 4;
                    return 1;
                case JsonValueTypes.False:
                    begin += 5;
                    return 0;
                case JsonValueTypes.Null:
                    throw new InvalidCastException("Cannot convert NULL to Number.");
                case JsonValueTypes.Undefiend:
                    begin += 9;
                    return 0;
            }

            return InternalReadDouble();
        }

        public string ReadString()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Number:
                    goto Number;
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                case JsonValueTypes.Reference:
                    throw new InvalidCastException("Cannot convert object/array to String.");
                case JsonValueTypes.True:
                    begin += 4;
                    return "true";
                case JsonValueTypes.False:
                    begin += 5;
                    return "false";
                case JsonValueTypes.Null:
                    begin += 4;
                    return null;
                case JsonValueTypes.Undefiend:
                    begin += 9;
                    return null;
            }

            return InternalReadString();
            Number:

            int index = begin;

            while (index < end)
            {
                switch (chars[index])
                {
                    case '-':
                    case '+':
                    case '.':
                    case 'e':
                    case 'E':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        ++index;

                        continue;
                    default:
                        goto Return;
                }
            }

            Return:
            var r = new string(chars, begin, index - begin);

            begin = index;

            return r;
        }

        public bool ReadBoolean()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return bool.Parse(InternalReadString());
                case JsonValueTypes.Number:
                    return InternalReadDouble() != 0;
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                case JsonValueTypes.Reference:
                    throw new InvalidCastException("Cannot convert object/array to Boolean.");
                case JsonValueTypes.True:
                    begin += 4;

                    return true;
                case JsonValueTypes.False:
                    begin += 5;

                    return false;
                case JsonValueTypes.Null:
                    begin += 4;

                    return false;
                case JsonValueTypes.Undefiend:
                    begin += 9;

                    return false;
            }

            return false;
        }

        public byte ReadByte()
        {
            return checked((byte)ReadInt64());
        }

        public char ReadChar()
        {
            return ReadString()[0];
        }

        public DateTime ReadDateTime()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to DateTime.");
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                case JsonValueTypes.Reference:
                    throw new InvalidCastException("Cannot convert object/array to DateTime.");
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    throw new InvalidCastException("Cannot convert Boolean to DateTime.");
                case JsonValueTypes.Null:
                case JsonValueTypes.Undefiend:
                    throw new InvalidCastException("Cannot convert Null to DateTime.");
            }

            char textChar = chars[begin];

            int textLength = 0;

            int index = begin + 1;

            int right = index;

            while (right < end)
            {
                if (chars[right] == textChar)
                {
                    break;
                }
                else if (chars[right] == '\\')
                {
                    goto StringDecode;
                }
                else
                {
                    ++right;
                }

                ++textLength;
            }

            DateTime result;

            if (DateTimeHelper.TryParseISODateTime(chars + index, right - index, out result))
            {
                begin = right + 1;

                return result;
            }

            result = DateTime.Parse(new string(chars, index, right - index));

            begin = right + 1;

            return result;

            StringDecode:

            result = DateTime.Parse(InternalReadString());

            return result;
        }

        public decimal ReadDecimal()
        {
            var index = NumberHelper.TryParse(chars + begin, end - begin, out decimal r);

            if (index != 0)
            {
                begin += index;

                return r;
            }

            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return decimal.Parse(InternalReadString());
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                case JsonValueTypes.Reference:
                    throw new InvalidCastException("Cannot convert object/array to Number.");
                case JsonValueTypes.True:
                    begin += 4;
                    return 1;
                case JsonValueTypes.False:
                    begin += 5;
                    return 0;
                case JsonValueTypes.Null:
                    begin += 4;
                    return 0;
                case JsonValueTypes.Undefiend:
                    begin += 9;
                    return 0;
            }

            return decimal.Parse(ReadString(), NumberStyle);
        }

        public short ReadInt16()
        {
            return checked((short)ReadInt64());
        }

        public int ReadInt32()
        {
            return checked((int)ReadInt64());
        }

        public sbyte ReadSByte()
        {
            return checked((sbyte)ReadInt64());
        }

        public float ReadSingle()
        {
            return checked((float)ReadDouble());
        }

        public ushort ReadUInt16()
        {
            return checked((ushort)ReadInt64());
        }

        public uint ReadUInt32()
        {
            return checked((uint)ReadInt64());
        }

        public ulong ReadUInt64()
        {
            // var index = NumberHelper.Decimal.TryParse(chars + begin, end - begin, out ulong r); // Faster.
            var index = NumberHelper.Decimal.TryParseExp(chars + begin, end - begin, out ulong r); // JSON Standard.

            if (index != 0)
            {
                begin += index;

                return r;
            }

            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return ulong.Parse(InternalReadString());
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                case JsonValueTypes.Reference:
                    throw new InvalidCastException("Cannot convert object/array to Number.");
                case JsonValueTypes.True:
                    begin += 4;
                    return 1;
                case JsonValueTypes.False:
                    begin += 5;
                    return 0;
                case JsonValueTypes.Null:
                    throw new InvalidCastException("Cannot convert NULL to Number.");
                case JsonValueTypes.Undefiend:
                    begin += 9;
                    return 0;
            }

            return ulong.Parse(ReadString(), NumberStyle);
        }

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    throw new InvalidCastException("Cannot convert String to object.");
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to object.");
                case JsonValueTypes.Array:
                    ReadArray(valueWriter.As<int>());
                    return;
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    throw new InvalidCastException("Cannot convert Boolean to object.");
                case JsonValueTypes.Null:
                    /* 空对象直接返回 */
                    begin += 4;
                    return;
                case JsonValueTypes.Undefiend:
                    begin += 9;
                    return;
                case JsonValueTypes.Reference:
                    ReadReference(valueWriter);
                    return;
            }

            while (begin < end)
            {
                switch (chars[begin])
                {

                    case ' ':
                    case '\n':
                    case '\r':
                    case '\t':
                        ++begin;

                        continue;
                    case '{':

                        valueWriter.Initialize();

                        SaveReference(valueWriter);

                        goto case ',';
                    case '}':
                        EndCase:
                        ++begin;

                        goto ReturnValue;
                    case ',':

                        Loop:
                        ++begin;

                        if (begin < end)
                        {
                            char c = chars[begin];

                            string name;

                            int flag;

                            switch (c)
                            {
                                case ' ':
                                case '\n':
                                case '\r':
                                case '\t':
                                    goto Loop;
                                case '}':
                                    goto EndCase;
                                case '"':
                                case '\'':
                                    name = InternalReadString();

                                    flag = StringHelper.IndexOf(chars, ':', begin, end);

                                    break;
                                default:
                                    flag = StringHelper.IndexOf(chars, ':', begin, end);

                                    name = StringHelper.Trim(chars, begin, flag);

                                    break;

                            }

                            if (flag == -1)
                            {
                                throw GetException();
                            }

                            begin = flag + 1;

                            while (begin < end)
                            {
                                switch (chars[begin])
                                {
                                    case ' ':
                                    case '\n':
                                    case '\r':
                                    case '\t':
                                        ++begin;
                                        continue;
                                    default:
                                        goto ReadValue;
                                }
                            }

                            ReadValue:

                            valueWriter.OnWriteValue(name, this);

                            continue;
                        }
                        else
                        {
                            goto Exception;
                        }
                    default:
                        goto Exception;
                }
            }


            Exception:
            throw GetException();

            ReturnValue:
            return;
        }

        public void ReadArray(IDataWriter<int> valueWriter)
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    throw new InvalidCastException("Cannot convert String to array.");
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to array.");
                case JsonValueTypes.Object:
                    ReadObject(valueWriter.As<string>());
                    return;
                case JsonValueTypes.True:
                    throw new InvalidCastException("Cannot convert Boolean to array.");
                case JsonValueTypes.Null:
                    /* 空对象直接返回 */
                    begin += 4;
                    return;
                case JsonValueTypes.Undefiend:
                    begin += 9;
                    return;
                case JsonValueTypes.Reference:
                    ReadReference(valueWriter);
                    return;
            }

            int index = 0;

            while (begin < end)
            {
                switch (chars[begin])
                {
                    case ' ':
                    case '\n':
                    case '\r':
                    case '\t':

                        ++begin;

                        continue;

                    case '[':

                        valueWriter.Initialize();

                        SaveReference(valueWriter);

                        goto case ',';

                    case ']':
                        EndCase:
                        ++begin;

                        goto ReturnValue;

                    case ',':

                        ++begin;

                        while (begin < end)
                        {
                            switch (chars[begin])
                            {
                                case ' ':
                                case '\n':
                                case '\r':
                                case '\t':
                                    ++begin;
                                    continue;
                                case ']':
                                    goto EndCase;
                                default:
                                    goto ReadValue;
                            }
                        }

                        ReadValue:

                        valueWriter.OnWriteValue(index, this);

                        ++index;

                        continue;

                    default:

                        goto FormatError;
                }
            }

            FormatError:
            throw GetException();

            ReturnValue:
            return;
        }

        public object DirectRead()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return InternalReadString();
                case JsonValueTypes.Object:
                    return ValueInterface<Dictionary<string, object>>.Content.ReadValue(this);
                case JsonValueTypes.Array:
                    return ValueInterface<List<object>>.Content.ReadValue(this);
                case JsonValueTypes.Reference:
                    return ReadReference();
                case JsonValueTypes.True:
                    begin += 4;
                    return true;
                case JsonValueTypes.False:
                    begin += 5;
                    return false;
                case JsonValueTypes.Null:
                    begin += 4;
                    return null;
                case JsonValueTypes.Undefiend:
                    begin += 9;
                    return null;
                case JsonValueTypes.Number:

                    var numberInfo = NumberHelper.Decimal.GetNumberInfo(chars + begin, end - begin);

                    if (numberInfo.IsNumber)
                    {
                        begin += numberInfo.End;

                        if (numberInfo.HaveExponent)
                        {
                            if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16 && numberInfo.ExponentCount <= 2)
                            {
                                return NumberHelper.Decimal.ToDouble(numberInfo);
                            }

                            return numberInfo.ToString();
                        }

                        if (numberInfo.IsFloat)
                        {
                            if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16)
                            {
                                return NumberHelper.Decimal.ToDouble(numberInfo);
                            }

                            if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 28)
                            {
                                return NumberHelper.ToDecimal(numberInfo);
                            }

                            return numberInfo.ToString();
                        }

                        if (numberInfo.IntegerCount <= 18)
                        {
                            var int64 = NumberHelper.Decimal.ToInt64(numberInfo);

                            if (int64 <= int.MaxValue && int64 >= int.MinValue)
                            {
                                return (int)int64;
                            }

                            return int64;
                        }

                        if (numberInfo.IntegerCount <= 28)
                        {
                            return NumberHelper.ToDecimal(numberInfo);
                        }

                        return numberInfo.ToString();
                    }

                    break;
            }

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void ReadReference(IDataWriter dataWriter)
        {
            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                if (!(dataWriter is IDirectContent))
                {
                    throw new NotSupportedException("this DataWriter not support direct set Content. -- " + dataWriter);
                }

                ((IDirectContent)dataWriter).DirectContent = ReadReference();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private object ReadReference()
        {
            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                begin += 4;

                var objectId = ReadInt32();

                var objectWriter = objects[objectId];

                if (!(objectWriter is IDirectContent))
                {
                    throw new NotSupportedException("this DataWriter not support direct get Content. -- " + objectWriter);
                }

                return ((IDirectContent)objectWriter).DirectContent;
            }

            throw new NotSupportedException("Not set options -- " + nameof(JsonFormatterOptions.MultiReferencingReference) + ".");
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void SaveReference(IDataWriter dataWriter)
        {
            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                objects.Add(dataWriter);
            }
        }

        public BasicTypes GetBasicType()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return BasicTypes.String;
                case JsonValueTypes.Number:
                    return BasicTypes.Double;
                case JsonValueTypes.Object:
                    return BasicTypes.Object;
                case JsonValueTypes.Array:
                    return BasicTypes.Array;
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    return BasicTypes.Boolean;
                case JsonValueTypes.Reference:
                    return BasicTypes.Object;
            }

            return BasicTypes.Null;
        }

        Guid IValueReader<Guid>.ReadValue()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to Guid.");
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to Guid.");
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    throw new InvalidCastException("Cannot convert Boolean to Guid.");
                case JsonValueTypes.Null:
                case JsonValueTypes.Undefiend:
                    throw new InvalidCastException("Cannot convert Null to Guid.");
            }

            var index = begin;

            var textChar = chars[index];

            ++index;

            Guid r;

            index = NumberHelper.TryParse(chars + index, end, out r);

            if (index >= 32)
            {
                begin += index + 1;

                if (chars[begin] == textChar)
                {
                    ++begin;
                }


                return r;
            }

            return new Guid(InternalReadString());
        }
    }
}
