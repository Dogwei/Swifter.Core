using System;
using System.Runtime.CompilerServices;


namespace Swifter.Tools
{
    public sealed unsafe partial class NumberHelper
    {
        /// <summary>
        /// 获取 UInt64 值的字符串表现形式长度。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <returns>返回字符串表现形式长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte GetLength(ulong value)
        {
            var t = uInt64NumbersLength;
            var n = uInt64Numbers;
            var v = value;

            if (v >= n[5])
            {
                if (v >= n[10])
                    if (t > 15 && v >= n[15])
                        if (t > 17 && v >= n[17])
                            if (t > 19 && v >= n[19])
                                goto GE20;
                            else if (t > 18 && v >= n[18])
                                return 19;
                            else
                                return 18;
                        else if (t > 16 && v >= n[16])
                            return 17;
                        else
                            return 16;
                    else if (t > 12 && v >= n[12])
                        if (t > 14 && v >= n[14])
                            return 15;
                        else if (t > 13 && v >= n[13])
                            return 14;
                        else
                            return 13;
                    else if (t > 11 && v >= n[11])
                        return 12;
                    else
                        return 11;
                else if (v >= n[7])
                    if (v >= n[9])
                        return 10;
                    else if (v > n[8])
                        return 9;
                    else
                        return 8;
                else if (v >= n[6])
                    return 7;
                else
                    return 6;
            }
            else if (v >= n[2])
                if (v >= n[4])
                    return 5;
                else if (v >= n[3])
                    return 4;
                else
                    return 3;
            else if (v >= n[1])
                return 2;
            else
                return 1;

            GE20:

            if (t > 15 && v >= n[20])
            {
                return (byte)(20 + GetLength(t / n[20]));
            }

            return 20;
        }

        /// <summary>
        /// 将一个 UInt64 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(ulong value, char* chars)
        {
            var l = threeDigitalsLength;

            if (l == 1000)
            {
                return ToDecimalString(value, chars);
            }

            var t = uInt64NumbersLength;
            var n = uInt64Numbers;
            var c = chars;
            var v = value;
            ulong s;

            if (v >= n[5])
            {
                if (v >= n[10])
                    if (t > 15 && v >= n[15])
                        if (t > 17 && v >= n[17])
                            if (t > 19 && v >= n[19])
                                goto L20;
                            else if (t > 18 && v >= n[18])
                                goto L19;
                            else
                                goto L18;
                        else if (t > 16 && v >= n[16])
                            goto L17;
                        else
                            goto L16;
                    else if (t > 12 && v >= n[12])
                        if (t > 14 && v >= n[14])
                            goto L15;
                        else if (t > 13 && v >= n[13])
                            goto L14;
                        else
                            goto L13;
                    else if (t > 11 && v >= n[11])
                        goto L12;
                    else
                        goto L11;
                else if (v >= n[7])
                    if (v >= n[9])
                        goto L10;
                    else if (v > n[8])
                        goto L9;
                    else
                        goto L8;
                else if (v >= n[6])
                    goto L7;
                else
                    goto L6;
            }
            else if (v >= n[2])
                if (v >= n[4])
                    goto L5;
                else if (v >= n[3])
                    goto L4;
                else
                    goto L3;
            else if (v >= n[1])
                goto L2;
            else
                goto L1;



            L20:

            s = v / n[18];

            if (t > 20 && v >= n[20])
            {
                var r = ToString(s, c);

                c += r;

                AppendD18(ref c, v - s * n[18]);

                return r + 18;
            }

            AppendD2(ref c, s);
            AppendD18(ref c, v - s * n[18]);
            return 20;

        L19:
            s = v / n[18];
            AppendD1(ref c, s);
            AppendD18(ref c, v - s * n[18]);
            return 19;

        L18:
            AppendD18(ref c, v);
            return 18;

        L17:
            s = v / n[15];
            AppendD2(ref c, s);
            AppendD15(ref c, v - s * n[15]);
            return 17;

        L16:
            s = v / n[15];
            AppendD1(ref c, s);
            AppendD15(ref c, v - s * n[15]);
            return 16;

        L15:
            AppendD15(ref c, v);
            return 15;

        L14:
            s = v / n[12];
            AppendD2(ref c, s);
            AppendD12(ref c, v - s * n[12]);
            return 14;

        L13:
            s = v / n[12];
            AppendD1(ref c, s);
            AppendD12(ref c, v - s * n[12]);
            return 13;

        L12:
            AppendD12(ref c, v);
            return 12;

        L11:
            s = v / n[9];
            AppendD2(ref c, s);
            AppendD9(ref c, v - s * n[9]);
            return 11;

        L10:
            s = v / n[9];
            AppendD1(ref c, s);
            AppendD9(ref c, v - s * n[9]);
            return 10;

        L9:
            AppendD9(ref c, v);
            return 9;

        L8:
            s = v / n[6];
            AppendD2(ref c, s);
            AppendD6(ref c, v - s * n[6]);
            return 8;

        L7:
            s = v / n[6];
            AppendD1(ref c, s);
            AppendD6(ref c, v - s * n[6]);
            return 7;

        L6:
            AppendD6(ref c, v);
            return 6;

        L5:
            s = v / l;
            AppendD2(ref c, s);
            AppendD3(ref c, v - s * l);
            return 5;

        L4:
            s = v / l;
            AppendD1(ref c, s);
            AppendD3(ref c, v - s * l);
            return 4;

        L3:
            AppendD3(ref c, v);
            return 3;

        L2:
            AppendD2(ref c, v);
            return 2;

        L1:
            AppendD1(ref c, v);
            return 1;
        }

        /// <summary>
        /// 将指定长度的 UInt64 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <param name="length">指定长度</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(ulong value, uint length, char* chars)
        {
            var t = length;
            var n = uInt64Numbers;
            var c = chars;
            var v = value;
            var l = threeDigitalsLength;
            ulong s;

            switch (t)
            {
                case 0:
                    return 0;
                case 1:
                    goto L1;
                case 2:
                    goto L2;
                case 3:
                    goto L3;
                case 4:
                    goto L4;
                case 5:
                    goto L5;
                case 6:
                    goto L6;
                case 7:
                    goto L7;
                case 8:
                    goto L8;
                case 9:
                    goto L9;
                case 10:
                    goto L10;
                case 11:
                    goto L11;
                case 12:
                    goto L12;
                case 13:
                    goto L13;
                case 14:
                    goto L14;
                case 15:
                    goto L15;
                case 16:
                    goto L16;
                case 17:
                    goto L17;
                case 18:
                    goto L18;
                case 19:
                    goto L19;
            }

            s = v / n[18];

            if (t > 20)
            {
                c += ToString(s, c);

                AppendD18(ref c, v - s * n[18]);

                return (int)t;
            }

            AppendD2(ref c, s);
            AppendD18(ref c, v - s * n[18]);
            return 20;

        L19:
            s = v / n[18];
            AppendD1(ref c, s);
            AppendD18(ref c, v - s * n[18]);
            return 19;

        L18:
            AppendD18(ref c, v);
            return 18;

        L17:
            s = v / n[15];
            AppendD2(ref c, s);
            AppendD15(ref c, v - s * n[15]);
            return 17;

        L16:
            s = v / n[15];
            AppendD1(ref c, s);
            AppendD15(ref c, v - s * n[15]);
            return 16;

        L15:
            AppendD15(ref c, v);
            return 15;

        L14:
            s = v / n[12];
            AppendD2(ref c, s);
            AppendD12(ref c, v - s * n[12]);
            return 14;

        L13:
            s = v / n[12];
            AppendD1(ref c, s);
            AppendD12(ref c, v - s * n[12]);
            return 13;

        L12:
            AppendD12(ref c, v);
            return 12;

        L11:
            s = v / n[9];
            AppendD2(ref c, s);
            AppendD9(ref c, v - s * n[9]);
            return 11;

        L10:
            s = v / n[9];
            AppendD1(ref c, s);
            AppendD9(ref c, v - s * n[9]);
            return 10;

        L9:
            AppendD9(ref c, v);
            return 9;

        L8:
            s = v / n[6];
            AppendD2(ref c, s);
            AppendD6(ref c, v - s * n[6]);
            return 8;

        L7:
            s = v / n[6];
            AppendD1(ref c, s);
            AppendD6(ref c, v - s * n[6]);
            return 7;

        L6:
            AppendD6(ref c, v);
            return 6;

        L5:
            s = v / l;
            AppendD2(ref c, s);
            AppendD3(ref c, v - s * l);
            return 5;

        L4:
            s = v / l;
            AppendD1(ref c, s);
            AppendD3(ref c, v - s * l);
            return 4;

        L3:
            AppendD3(ref c, v);
            return 3;

        L2:
            AppendD2(ref c, v);
            return 2;

        L1:
            AppendD1(ref c, v);
            return 1;
        }

        /// <summary>
        /// 将一个 Int64 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">Int64 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(long value, char* chars)
        {
            if (value >= 0)
            {
                return ToString((ulong)(value), chars);
            }
            else
            {
                *chars = NegativeSign;

                return ToString((ulong)(-value), chars + 1) + 1;
            }
        }

        /// <summary>
        /// 将一个字节正整数写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(uint* value, int length, char* chars)
        {
            var temp = stackalloc uint[length];

            for (int i = length - 1; i >= 0; --i)
            {
                temp[i] = value[i];
            }

            return DirectOperateToString(temp, length, chars);
        }


        /// <summary>
        /// 将 UInt64 值转换为字符串表现形式。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(ulong value)
        {
            var chars = stackalloc char[64];

            var length = ToString(value, chars);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将 Int64 值转换为字符串表现形式。
        /// </summary>
        /// <param name="value">Int64 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(long value)
        {
            var chars = stackalloc char[64];

            var length = ToString(value, chars);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将字节正整数转换为字符串表现形式。
        /// </summary>
        /// <param name="value">字节正整数</param>
        /// <param name="length">字节正整数长度</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(uint* value, int length)
        {
            var chars = stackalloc char[(uInt32NumbersLength * length) + 1];

            var writeCount = ToString(value, length, chars);

            return new string(chars, 0, writeCount);
        }

        /// <summary>
        /// 将一个字节正整数写入到空间足够的字符串中。此方法对字节正整数直接运算，所以会改变它的值。
        /// </summary>
        /// <param name="value">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int DirectOperateToString(uint* value, int length, char* chars)
        {
            uint r;
            int l;

            // if Decimal use constant optimization.
            if (baseDivisor == DecimalBaseDivisor)
            {
                l = Div(value, length, DecimalBaseDivisor, out r);
            }
            else
            {
                l = Div(value, length, baseDivisor, out r);
            }

            if (l == 0)
            {
                return ToString((ulong)r, chars);
            }

            int s;

            if (l == 1)
            {
                s = ToString(*value, chars);
            }
            else
            {
                s = DirectOperateToString(value, l, chars);
            }

            return s + ToString((ulong)r, baseLength, chars + s);
        }





        /// <summary>
        /// 尝试从字符串开始位置解析一个 Int64 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Int64 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, out long value, bool exception = false)
        {
            var v = 0L; // Value
            var c = chars;
            var l = length;
            var u = int64NumbersLength;
            var r = radix;
            var n = false; // IsNegative

            if (l <= 0)
            {
                goto EmptyLength;
            }

            if (*c == PositiveSign)
            {
                ++c;
                --l;
            }
            else if (*c == NegativeSign)
            {
                ++c;
                --l;

                n = true;
            }

            while (*c == DigitalsZeroValue)
            {
                ++c;
                --l;
            }

            var e = c + Math.Min(u, l);

        Loop:
            var d = ToRadix(*c);

            if (d >= r)
            {
                goto OutOfRadix;
            }

            ++c;

            if (c == e)
            {
                if (l >= u && v < (n ? (Int64MinValue + d) / r : (NegativeInt64MaxValue + d) / r))
                {
                    --c;

                    goto OutOfRange;
                }

                v = v * r - d;

                if (l > u)
                {
                    goto OutOfRange;
                }
            }
            else
            {
                v = v * r - d;

                goto Loop;
            }

        Return:

            value = n ? v : -v;

            return (int)(c - chars);

        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto Return;

        OutOfRange:
            if (exception) ThreadException = new OverflowException("value out of range.");
            goto Return;

        EmptyLength:
            if (exception) ThreadException = new ArgumentException("Length cna't be less than 1");
            goto Return;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 UInt64 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 UInt64 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, out ulong value, bool exception = false)
        {
            var v = 0UL; // Value
            var c = chars;
            var l = length;
            var u = uInt64NumbersLength;
            var r = radix;

            if (l <= 0)
            {
                goto EmptyLength;
            }

            if (*c == PositiveSign)
            {
                ++c;
                --l;
            }

            while (*c == DigitalsZeroValue)
            {
                ++c;
                --l;
            }

            var e = c + Math.Min(u, l);

        Loop:
            var d = ToRadix(*c);

            if (d >= r)
            {
                goto OutOfRadix;
            }

            ++c;

            if (c == e)
            {
                if (l >= u && v > (UInt64MaxValue - d) / r)
                {
                    --c;

                    goto OutOfRange;
                }

                v = v * r + d;

                if (l > u)
                {
                    goto OutOfRange;
                }
            }
            else
            {
                v = v * r + d;

                goto Loop;
            }


        Return:

            value = v;

            return (int)(c - chars);

        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto Return;

        OutOfRange:
            if (exception) ThreadException = new OverflowException("value out of range.");
            goto Return;

        EmptyLength:
            if (exception) ThreadException = new ArgumentException("Length cna't be less than 1");
            goto Return;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个字节正整数值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">字节正整数空间</param>
        /// <param name="writeCount">返回写入长度</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, uint* value, out int writeCount, bool exception = false)
        {
            int writeLength = 0;
            var current = chars;
            var num = 0U;
            var numbers = uInt32Numbers;
            var baseCount = baseLength;
            int count = baseCount;
            var r = radix;

            if (length <= 0)
            {
                goto EmptyLength;
            }

            int i = length / baseCount;

        Loop:

            if (i == 0)
            {
                count = length % baseCount;
            }

            for (var end = current + count; current < end; ++current)
            {
                var digit = ToRadix(*current);

                if (digit >= r)
                {
                    goto OutOfRadix;
                }

                num = num * r + digit;
            }

        MultAndAdd:

            uint carry;

            if (r == 10 && count == 9)
            {
                writeLength = Mult(value, writeLength, DecimalBaseDivisor, out carry);
            }
            else
            {
                writeLength = Mult(value, writeLength, numbers[count], out carry);
            }


            if (carry != 0)
            {
                value[writeLength] = carry;

                ++writeLength;
            }

            writeLength = Add(value, writeLength, num, out carry);

            if (carry != 0)
            {
                value[writeLength] = carry;

                ++writeLength;
            }

            if (i != 0)
            {
                --i;

                num = 0;

                goto Loop;
            }

            writeCount = writeLength;

            return (int)(current - chars);


        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto ErrorReturn;

        EmptyLength:
            if (exception) ThreadException = new ArgumentException("Length cna't be less than 1");
            goto ErrorReturn;


        ErrorReturn:

            count = ((int)(current - chars)) % baseCount;

            i = 0;

            goto MultAndAdd;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 Int32 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Int32 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, out int value, bool exception = false)
        {
            int r = TryParse(chars, length, out long int64Value, exception);

            if (int64Value > int.MaxValue || int64Value < int.MinValue)
            {
                ThreadException = new OverflowException("Value out of Int32 range.");
            }

            value = (int)int64Value;

            return r;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 UInt64 值。此方法允许指数。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 UInt64 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParseExp(char* chars, int length, out ulong value, bool exception = false)
        {
            var temp = 0UL;
            var numbers = uInt64Numbers;
            var maxLength = uInt64NumbersLength;
            var r = radix;
            var fractionalIndex = -1;
            var nonZeroFractionalIndex = -1;
            int integerCount = 0;

            const int large = 5;

            int index = 0;

            if (length <= 0)
            {
                goto EmptyLength;
            }

            if (chars[index] == PositiveSign)
            {
                if (length == 1)
                {
                    goto FormatError;
                }

                ++index;
            }

            while (index < length && chars[index] == DigitalsZeroValue)
            {
                ++index;
            }

            for (; index < length; ++index)
            {
                var digit = ToRadix(chars[index]);

                if (digit >= r)
                {
                    break;
                }

                ++integerCount;

                if (integerCount == maxLength)
                {
                    if (temp > (UInt64MaxValue - digit) / r)
                    {
                        goto OutOfRange;
                    }

                    temp = temp * r + digit;

                    break;
                }
                else
                {
                    temp = temp * r + digit;
                }
            }

        End:

            if (index < length)
            {
                switch (chars[index])
                {
                    case DotSign:
                        ++index;
                        goto Fractional;
                    case ExponentSign:
                    case exponentSign:
                        ++index;
                        goto Exponent;
                }

                goto OutOfRadix;
            }

        Return:

            value = temp;

            return index;

        Fractional:

            if (fractionalIndex != -1 || index == 1 || (index == 2 && chars[0] == PositiveSign))
            {
                goto FormatError;
            }

            fractionalIndex = index;

            for (; index < length; ++index)
            {
                var digit = ToRadix(chars[index]);

                if (digit >= r)
                {
                    break;
                }

                if (digit != 0 && nonZeroFractionalIndex == -1)
                {
                    nonZeroFractionalIndex = index;
                }
            }

            goto End;

        Exponent:

            long exponent;

            index += TryParse(chars + index, length - index, out exponent, exception);

            if (exponent > 0)
            {
                if (temp != 0)
                {
                    var afterLength = integerCount + exponent;

                    if (afterLength > maxLength || (afterLength == maxLength && temp > UInt64MaxValue / numbers[exponent]))
                    {
                        goto OutOfRange;
                    }
                }
                else if (nonZeroFractionalIndex != -1 && exponent > maxLength + (nonZeroFractionalIndex - fractionalIndex))
                {
                    goto OutOfRange;
                }

                if (nonZeroFractionalIndex != -1)
                {
                    var zeroFractionalCount = nonZeroFractionalIndex - fractionalIndex;

                    if (zeroFractionalCount >= exponent)
                    {
                        goto Break;
                    }

                    exponent -= zeroFractionalCount;

                    if (temp != 0)
                    {
                        while (zeroFractionalCount >= large)
                        {
                            zeroFractionalCount -= large;

                            temp *= numbers[large];
                        }

                        while (zeroFractionalCount > 0)
                        {
                            --zeroFractionalCount;

                            temp *= r;
                        }
                    }

                    while (true)
                    {
                        var digit = ToRadix(chars[nonZeroFractionalIndex]);

                        if (digit >= r)
                        {
                            switch (chars[nonZeroFractionalIndex])
                            {
                                case ExponentSign:
                                case exponentSign:
                                    goto Break;
                            }

                            goto OutOfRadix;
                        }

                        --exponent;
                        ++nonZeroFractionalIndex;

                        if (exponent >= 1)
                        {
                            temp = temp * r + digit;
                        }
                        else
                        {
                            if (temp > (UInt64MaxValue - digit) / r)
                            {
                                goto OutOfRange;
                            }

                            temp = temp * r + digit;

                            break;
                        }

                    }
                }

            Break:

                if (temp == 0)
                {
                    goto Return;
                }

                while (exponent > large)
                {
                    temp *= numbers[large];

                    exponent -= large;
                }

                while (exponent > 1)
                {
                    temp *= r;

                    --exponent;
                }

                if (exponent == 1)
                {
                    --exponent;

                    if (temp > UInt64MaxValue / r)
                    {
                        goto OutOfRange;
                    }

                    temp *= r;
                }
            }
            else
            {
                while (exponent <= -large && temp != 0)
                {
                    temp /= numbers[large];

                    exponent += large;
                }

                while (exponent < 0 && temp != 0)
                {
                    temp /= r;

                    ++exponent;
                }
            }

            goto Return;

        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto Return;

        OutOfRange:
            if (exception) ThreadException = new OverflowException("value out of range.");
            index = 0;
            goto Return;

        FormatError:
            if (exception) ThreadException = new FormatException("number text format error.");
            index = 0;
            goto Return;

        EmptyLength:
            if (exception) ThreadException = new ArgumentException("Length cna't be less than 1");
            goto Return;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 Int64 值。此方法允许指数。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Int64 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParseExp(char* chars, int length, out long value, bool exception = false)
        {
            if (length <= 0)
            {
                goto EmptyLength;
            }

            if (chars[0] == NegativeSign)
            {
                if (length == 1)
                {
                    goto FormatError;
                }

                var result = TryParseExp(chars + 1, length - 1, out ulong temp, exception);

                if (temp > PositiveInt64MinValue)
                {
                    goto OutOfRange;
                }

                value = -(long)temp;

                return result + 1;
            }
            else
            {
                var result = TryParseExp(chars, length, out ulong temp, exception);

                if (temp > Int64MaxValue)
                {
                    goto OutOfRange;
                }

                value = (long)temp;

                return result;
            }

        OutOfRange:
            if (exception) ThreadException = new OverflowException("value out of range.");
            value = 0;
            return 0;

        FormatError:
            if (exception) ThreadException = new FormatException("number text format error.");
            value = 0;
            return 0;

        EmptyLength:
            if (exception) ThreadException = new ArgumentException("Length cna't be less than 1");
            value = 0;
            return 0;
        }
        
        /// <summary>
        /// 尝试将字符串转换为 Int64 值。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">返回一个 Int64 值</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParse(string text, out long value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParse(chars, length, out value) == length;
            }
        }




        /// <summary>
        /// 尝试将字符串转换为 UInt64 值。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">返回一个 UInt64 值</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParse(string text, out ulong value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParse(chars, length, out value) == length;
            }
        }

        /// <summary>
        /// 尝试将字符串转换为字节正整数值。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value">字节正整数</param>
        /// <param name="length">返回写入长度</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParse(string text, uint* value, out int length)
        {
            fixed (char* chars = text)
            {
                return TryParse(chars, text.Length, value, out length, true) == text.Length;
            }
        }

        /// <summary>
        /// 尝试将字符串转换为 UInt64 值。此方法允许指数。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">返回一个 UInt64 值</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParseExp(string text, out ulong value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParseExp(chars, length, out value) == length;
            }
        }

        /// <summary>
        /// 尝试将字符串转换为 Int64 值。此方法允许指数。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">返回一个 Int64 值</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParseExp(string text, out long value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParseExp(chars, length, out value) == length;
            }
        }


        /// <summary>
        /// 将字符串转换为 Int64 值。失败将引发异常。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个 Int64 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ParseInt64(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParse(chars, length, out long value, true) == length)
                {
                    return value;
                }

                throw ThreadException;
            }
        }

        /// <summary>
        /// 将字符串转换为 UInt64 值。失败将引发异常。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个 UInt64 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ParseUInt64(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParse(chars, length, out ulong value, true) == length)
                {
                    return value;
                }

                throw ThreadException;
            }
        }

        /// <summary>
        /// 将字符串转换为 UInt64 值。失败将引发异常。此方法允许指数。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个 UInt64 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ParseUInt64Exp(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParseExp(chars, length, out ulong value, true) == length)
                {
                    return value;
                }

                throw ThreadException;
            }
        }

        /// <summary>
        /// 将字符串转换为 Int64 值。失败将引发异常。此方法允许指数。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个 Int64 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ParseInt64Exp(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParseExp(chars, length, out long value, true) == length)
                {
                    return value;
                }

                throw ThreadException;
            }
        }

        /// <summary>
        /// 将字符串转换为字节正整数值。失败将引发异常。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">字节正整数</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ParseBigInteger(string text, uint* value)
        {
            if (TryParse(text, value, out int r))
            {
                return r;
            }

            throw ThreadException;
        }
    }
}
