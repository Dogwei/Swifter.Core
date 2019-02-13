using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    public sealed unsafe partial class NumberHelper
    {
        /* ['0', '1', '2', '3',... 'a', 'b', 'c',... 'A', 'B', 'C',... 'Z', '~', '!'] */
        private static readonly char* Digitals;
        /* ['0': 0, '1': 1, '2': 2,... '9': 9, 'a': 10, 'b':11,... 'A': 36, 'B': 37,... 'Z': 61, '~': 62, '!': 63, Other: ErrorRadix] */
        private static readonly byte* Radixes;
        /* ['0': 0, '1': 1, '2': 2,... '9': 9, 'a': 10, 'b':11,... 'A': 10, 'B': 11,... 'Z': 35, Other: ErrorRadix] */
        private static readonly byte* IgnoreCaseRadixes;

        [ThreadStatic]
        private static Exception ThreadException;

        /// <summary>
        /// 十进制实例。
        /// </summary>
        public static readonly NumberHelper Decimal;
        /// <summary>
        /// 十六进制实例。
        /// </summary>
        public static readonly NumberHelper Hex;
        /// <summary>
        /// 八进制实例。
        /// </summary>
        public static readonly NumberHelper Octal;
        /// <summary>
        /// 二进制实例。
        /// </summary>
        public static readonly NumberHelper Binary;

        static readonly NumberHelper[] Instances;
        static readonly object InstancesLock;

        /// <summary>
        /// 获取指定进制的 NumberHelper 实例。
        /// </summary>
        /// <param name="radix">进制数</param>
        /// <returns>返回 NumberHelper 实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static NumberHelper InstanceByRadix(byte radix)
        {
            if (radix > MaxRadix || radix < MinRadix)
            {
                throw new ArgumentOutOfRangeException(nameof(radix));
            }

            var instance = Instances[radix];

            if (instance == null)
            {
                lock (InstancesLock)
                {
                    instance = Instances[radix];

                    if (instance == null)
                    {
                        instance = new NumberHelper(radix);

                        Instances[radix] = instance;
                    }
                }
            }

            return instance;
        }

        static NumberHelper()
        {
            Instances = new NumberHelper[MaxRadix + 1];
            InstancesLock = new object();

            Digitals = (char*)Marshal.AllocHGlobal(MaxRadix * sizeof(char));
            Radixes = (byte*)Marshal.AllocHGlobal((DigitalsMaxValue + 1) * sizeof(byte));
            IgnoreCaseRadixes = (byte*)Marshal.AllocHGlobal((DigitalsMaxValue + 1) * sizeof(byte));

            for (int i = 0; i < DigitalsMaxValue; i++)
            {
                Radixes[i] = ErrorRadix;
                IgnoreCaseRadixes[i] = ErrorRadix;
            }

            for (uint i = 0; i < MaxRadix; i++)
            {
                char digital = SlowToDigital(i);

                Digitals[i] = digital;
                Radixes[digital] = SlowToRadix(digital);
                IgnoreCaseRadixes[digital] = SlowToRadixIgnoreCase(digital);
            }

            Decimal = InstanceByRadix(10);
            Hex = InstanceByRadix(16);
            Octal = InstanceByRadix(8);
            Binary = InstanceByRadix(2);
        }




        private static char SlowToDigital(uint value)
        {
            if (value >= 0 && value <= 9)
            {
                return (char)(value + '0');
            }

            if (value >= 10 && value <= 35)
            {
                return (char)(value - 10 + 'a');
            }

            if (value >= 36 && value <= 61)
            {
                return (char)(value - 36 + 'A');
            }

            switch (value)
            {
                case 62:
                    return '~';
                case 63:
                    return '!';
                default:
                    return ErrorDigital;
            }
        }

        private static byte SlowToRadix(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return (byte)(value - '0');
            }

            if (value >= 'a' && value <= 'z')
            {
                return (byte)(value - 'a' + 10);
            }

            if (value >= 'A' && value <= 'Z')
            {
                return (byte)(value - 'A' + 36);
            }

            switch (value)
            {
                case '~':
                    return 62;
                case '!':
                    return 63;
                default:
                    return ErrorRadix;
            }
        }

        private static byte SlowToRadixIgnoreCase(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return (byte)(value - '0');
            }

            if (value >= 'a' && value <= 'z')
            {
                return (byte)(value - 'a' + 10);
            }

            if (value >= 'A' && value <= 'Z')
            {
                return (byte)(value - 'A' + 10);
            }

            return ErrorRadix;
        }

        private static ulong SlowPow(ulong x, uint y)
        {
            ulong result = 1;

            while (y > 0)
            {
                result *= x;

                --y;
            }

            return result;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool IsNaN(double d)
        {
            return (*(UInt64*)(&d) & 0x7FFFFFFFFFFFFFFFL) > 0x7FF0000000000000L;
        }



        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int Append(char* chars, string text)
        {
            for (int i = text.Length - 1; i >= 0; --i)
            {
                chars[i] = text[i];
            }

            return text.Length;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static void Append(ref char* chars, char c)
        {
            *chars = c;
            ++chars;
        }








        /// <summary>
        /// 将一个 Guid 值写入到一个空间足够的字符串中。
        /// </summary>
        /// <param name="value">Guid 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ToString(Guid value, char* chars)
        {
            var h = Hex;
            var c = chars;
            var p = (GuidStruct*)(&value);

            h.AppendD2(ref c, p->_a1);
            h.AppendD2(ref c, p->_a2);
            h.AppendD2(ref c, p->_a3);
            h.AppendD2(ref c, p->_a4);

            Append(ref c, '-');

            h.AppendD2(ref c, p->_b1);
            h.AppendD2(ref c, p->_b2);

            Append(ref c, '-');

            h.AppendD2(ref c, p->_c1);
            h.AppendD2(ref c, p->_c2);

            Append(ref c, '-');

            h.AppendD2(ref c, p->_d);
            h.AppendD2(ref c, p->_e);

            Append(ref c, '-');

            h.AppendD2(ref c, p->_f);
            h.AppendD2(ref c, p->_g);
            h.AppendD2(ref c, p->_h);
            h.AppendD2(ref c, p->_i);
            h.AppendD2(ref c, p->_j);
            h.AppendD2(ref c, p->_k);

            return 36;
        }

        /// <summary>
        /// 将一个 Decimal 值写入到一个空间足够的字符串中。
        /// </summary>
        /// <param name="value">Decimal 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ToString(decimal value, char* chars)
        {
            var t = value;
            var c = chars;
            var d = Decimal;
            var n = 0;

            var p = (uint*)(&t);

            if ((p[0] & SignMask) != 0)
            {
                *c = NegativeSign;

                ++n;
                ++c;
            }

            // scale
            int s = ((byte*)p)[2];

            p[0] = p[2];
            p[2] = p[1];
            p[1] = p[3];

            var l = d.DirectOperateToString(p, 3, c);

            if (s == 0)
            {
                return l + n;
            }
            else if (s >= l)
            {
                // = scale + lengthOf(0.)
                var r = s + 2;

                while (l > 0)
                {
                    --l;
                    --r;

                    c[r] = c[l];
                }

                while (r > 2)
                {
                    --r;

                    c[r] = DigitalsZeroValue;
                }

                c[1] = DotSign;
                c[0] = DigitalsZeroValue;

                return s + n + 2;
            }
            else
            {
                var r = l;

                while (s > 0)
                {
                    c[r] = c[--r];

                    --s;
                }

                c[r] = DotSign;

                return l + n + 1;
            }
        }

        /// <summary>
        /// 将一个 UInt64 值以十进制格式写入到一个空间足够的字符串中。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ToDecimalString(ulong value, char* chars)
        {
            if (value >= 100000)
            {
                if (value >= 10000000000)
                    if (value >= 1000000000000000)
                        if (value >= 100000000000000000)
                            if (value >= 10000000000000000000)
                                goto L20;
                            else if (value >= 1000000000000000000)
                                goto L19;
                            else
                                goto L18;
                        else if (value >= 10000000000000000)
                            goto L17;
                        else
                            goto L16;
                    else if (value >= 1000000000000)
                        if (value >= 100000000000000)
                            goto L15;
                        else if (value >= 10000000000000)
                            goto L14;
                        else
                            goto L13;
                    else if (value >= 100000000000)
                        goto L12;
                    else
                        goto L11;
                else if (value >= 10000000)
                    if (value >= 1000000000)
                        goto L10;
                    else if (value > 100000000)
                        goto L9;
                    else
                        goto L8;
                else if (value >= 1000000)
                    goto L7;
                else
                    goto L6;
            }
            else if (value >= 100)
                if (value >= 10000)
                    goto L5;
                else if (value >= 1000)
                    goto L4;
                else
                    goto L3;
            else if (value >= 10)
                goto L2;
            else
                goto L1;

            L20:
            ulong s = value / 1000000000000000000;
            Decimal.AppendD2(ref chars, s);
            Decimal.AppendD18(ref chars, value - s * 1000000000000000000);
            return 20;

        L19:
            s = value / 1000000000000000000;
            Decimal.AppendD1(ref chars, s);
            Decimal.AppendD18(ref chars, value - s * 1000000000000000000);
            return 19;

        L18:
            Decimal.AppendD18(ref chars, value);
            return 18;

        L17:
            s = value / 1000000000000000;
            Decimal.AppendD2(ref chars, s);
            Decimal.AppendD15(ref chars, value - s * 1000000000000000);
            return 17;

        L16:
            s = value / 1000000000000000;
            Decimal.AppendD1(ref chars, s);
            Decimal.AppendD15(ref chars, value - s * 1000000000000000);
            return 16;

        L15:
            Decimal.AppendD15(ref chars, value);
            return 15;

        L14:
            s = value / 1000000000000;
            Decimal.AppendD2(ref chars, s);
            Decimal.AppendD12(ref chars, value - s * 1000000000000);
            return 14;

        L13:
            s = value / 1000000000000;
            Decimal.AppendD1(ref chars, s);
            Decimal.AppendD12(ref chars, value - s * 1000000000000);
            return 13;

        L12:
            Decimal.AppendD12(ref chars, value);
            return 12;

        L11:
            s = value / 1000000000;
            Decimal.AppendD2(ref chars, s);
            Decimal.AppendD9(ref chars, value - s * 1000000000);
            return 11;

        L10:
            s = value / 1000000000;
            Decimal.AppendD1(ref chars, s);
            Decimal.AppendD9(ref chars, value - s * 1000000000);
            return 10;

        L9:
            Decimal.AppendD9(ref chars, value);
            return 9;

        L8:
            s = value / 1000000;
            Decimal.AppendD2(ref chars, s);
            Decimal.AppendD6(ref chars, value - s * 1000000);
            return 8;

        L7:
            s = value / 1000000;
            Decimal.AppendD1(ref chars, s);
            Decimal.AppendD6(ref chars, value - s * 1000000);
            return 7;

        L6:
            Decimal.AppendD6(ref chars, value);
            return 6;

        L5:
            s = value / 1000;
            Decimal.AppendD2(ref chars, s);
            Decimal.AppendD3(ref chars, value - s * 1000);
            return 5;

        L4:
            s = value / 1000;
            Decimal.AppendD1(ref chars, s);
            Decimal.AppendD3(ref chars, value - s * 1000);
            return 4;

        L3:
            Decimal.AppendD3(ref chars, value);
            return 3;

        L2:
            Decimal.AppendD2(ref chars, value);
            return 2;

        L1:
            Decimal.AppendD1(ref chars, value);
            return 1;
        }










        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool TryParseHexByte(ref char* chars, out byte value)
        {
            var h = Hex;
            var a = h.ToRadix(chars[0]);
            var b = h.ToRadix(chars[1]);

            if ((a | b) >= 16)
            {
                goto OutOfRadix;
            }

            value = (byte)((a << 4) | b);

            chars += 2;

            return true;

        OutOfRadix:
            ThreadException = new FormatException("Digit out of radix.");
            value = 0;
            return false;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 Guid 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Guid 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int TryParse(char* chars, int length, out Guid value, bool exception = false)
        {
            GuidStruct r;

            var c = chars;
            var l = length;

            if (l < 32)
            {
                goto LengthError;
            }

            if (*c == '{')
            {
                ++c;
                --l;
            }

            if (!TryParseHexByte(ref c, out r._a1)) goto False;
            if (!TryParseHexByte(ref c, out r._a2)) goto False;
            if (!TryParseHexByte(ref c, out r._a3)) goto False;
            if (!TryParseHexByte(ref c, out r._a4)) goto False;


            if (*c == '-')
            {
                ++c;
                --l;
            }

            if (!TryParseHexByte(ref c, out r._b1)) goto False;
            if (!TryParseHexByte(ref c, out r._b2)) goto False;

            if (*c == '-')
            {
                ++c;
                --l;
            }

            if (!TryParseHexByte(ref c, out r._c1)) goto False;
            if (!TryParseHexByte(ref c, out r._c2)) goto False;

            if (*c == '-')
            {
                ++c;
                --l;
            }

            if (!TryParseHexByte(ref c, out r._d)) goto False;
            if (!TryParseHexByte(ref c, out r._e)) goto False;

            if (*c == '-')
            {
                ++c;
                --l;
            }

            if (l < 32)
            {
                goto LengthError;
            }

            if (!TryParseHexByte(ref c, out r._f)) goto False;
            if (!TryParseHexByte(ref c, out r._g)) goto False;
            if (!TryParseHexByte(ref c, out r._h)) goto False;
            if (!TryParseHexByte(ref c, out r._i)) goto False;
            if (!TryParseHexByte(ref c, out r._j)) goto False;
            if (!TryParseHexByte(ref c, out r._k)) goto False;

            if (*c == '}')
            {
                ++c;
                --l;
            }

            value = *(Guid*)(&r);
            return (int)(c - chars);

        LengthError:
            if (exception) ThreadException = new FormatException("Guid length error!");

            False:
            value = Guid.Empty;
            return 0;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 Decimal 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Decimal 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int TryParse(char* chars, int length, out decimal value, bool exception = false)
        {
            int index = 0;
            int sign = 0;
            int dotIndex = -1;
            int scale = 0;
            var dec = Decimal;
            var aValue = stackalloc uint[4];
            var numbers = dec.uInt32Numbers;
            var disabled = false;
            int aValueCount = 0;
            int result = -1;
            int zeroCount = 0;

            if (length <= 0)
            {
                goto EmptyLength;
            }

            switch (chars[0])
            {
                case PositiveSign:
                    ++index;
                    sign = 1;
                    break;
                case NegativeSign:
                    ++index;
                    sign = -1;
                    break;
            }

        Loop:

            uint add = 0;
            int i = 0;
            int zero = 0;

            for (; i < DecimalBaseDivisorLength && index < length; ++index)
            {
                char c = chars[index];

                if (c == DigitalsZeroValue)
                {
                    ++zero;
                }
                else
                {
                    uint n = (uint)(c - DigitalsZeroValue);

                    if (n > 0 && n < DecimalRadix)
                    {
                        if (zero == 0)
                        {
                            add = add * DecimalRadix + n;
                        }
                        else
                        {
                            add = add * numbers[zero + 1] + n;

                            zero = 0;
                        }
                    }
                    else
                    {
                        switch (c)
                        {
                            case DotSign:
                                if (dotIndex != -1)
                                {
                                    goto FormatException;
                                }

                                dotIndex = index;
                                continue;
                            case ExponentSign:
                            case exponentSign:
                                goto Exponent;
                        }

                        goto OutOfRadix;
                    }
                }


                ++i;
            }

            uint carry;

            int mult;

        GetAndMultAndAdd:

            if (zero != i && zeroCount != 0)
            {
                i += zeroCount;

                zeroCount = 0;
            }

            if (zero != 0)
            {
                mult = i - zero;

                zeroCount += zero;
            }
            else
            {
                mult = i;
            }

            i = 0;

            zero = 0;

        MultAndAdd:

            while (mult > 0 && aValueCount != 0)
            {
                if (mult >= DecimalBaseDivisorLength)
                {
                    Mult(aValue, aValueCount, DecimalBaseDivisor, out carry);

                    mult -= DecimalBaseDivisorLength;
                }
                else
                {
                    Mult(aValue, aValueCount, numbers[mult], out carry);

                    mult = 0;
                }

                if (carry != 0)
                {
                    aValue[aValueCount] = carry;

                    ++aValueCount;
                }
            }

            if (add != 0)
            {
                Add(aValue, aValueCount, add, out carry);

                if (carry != 0)
                {
                    aValue[aValueCount] = carry;

                    ++aValueCount;
                }

                add = 0;
            }

            if (aValueCount > 3)
            {
                goto OutOfRange;
            }

            if (index < length && !disabled)
            {
                goto Loop;
            }

        Return:

            if (dotIndex == -1 && zeroCount != 0)
            {
                mult = zeroCount;

                zeroCount = 0;

                disabled = true;

                goto MultAndAdd;
            }

            if ((i | zero) != 0)
            {
                disabled = true;

                goto GetAndMultAndAdd;
            }

            if (result == -1)
            {
                result = index;
            }

            if (dotIndex != -1)
            {
                int t = sign == 0 ? 0 : 1;

                if (dotIndex == t)
                {
                    goto FormatException;
                }

                scale += index - dotIndex - 1 - t - zeroCount;

                dotIndex = -1;

                zeroCount = 0;
            }

            if (scale < 0)
            {
                mult = -scale;

                scale = 0;

                disabled = true;

                goto MultAndAdd;
            }

            if (aValueCount == 0 && *aValue == 0)
            {
                value = 0;

                return result;
            }
            else
            {
                if (scale > DecimalMaxScale)
                {
                    goto ExponentOutOfRange;
                }

                fixed (decimal* pValue = &value)
                {
                    var upValue = (uint*)pValue;

                    upValue[2] = aValue[0];
                    upValue[3] = aValue[1];
                    upValue[1] = aValue[2];

                    if (sign == -1)
                    {
                        upValue[0] = SignMask;
                    }

                    if (scale != 0)
                    {
                        ((byte*)upValue)[2] = (byte)scale;
                    }
                }

                return result;
            }


        Exponent:

            int exponent;

            result = index + 1;

            result += dec.TryParse(chars + result, length - result, out exponent);

            scale -= exponent;

            goto Return;

        EmptyLength:
            if (exception) ThreadException = new FormatException("Decimal text format error.");
            goto ReturnFalse;
        FormatException:
            if (exception) ThreadException = new FormatException("Decimal text format error.");
            goto ReturnFalse;
        OutOfRange:
            if (exception) ThreadException = new OverflowException("Value out of decimal range.");
            goto ReturnFalse;
        OutOfRadix:
            if (exception) ThreadException = new OverflowException("Digit out of Decimal radix.");
            goto Return;
        ExponentOutOfRange:
            if (exception) ThreadException = new OverflowException("Exponent out of Decimal range.");
            goto ReturnFalse;

        ReturnFalse:
            value = 0;
            return 0;

        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static void ToDecimalNumber(char* chars, int count, uint* number, ref int length)
        {
            var decimalInstance = Decimal;

            for (int i = 0; i < count;)
            {
                int j = 0;
                uint t = 0;

                for (; j < DecimalBaseDivisorLength && i < count; ++j, ++i)
                {
                    t = t * DecimalRadix + decimalInstance.ToRadix(chars[i]);
                }

                uint carry;

                if (j == DecimalBaseDivisorLength)
                {
                    Mult(number, length, DecimalBaseDivisor, out carry);
                }
                else
                {
                    Mult(number, length, decimalInstance.uInt32Numbers[j], out carry);
                }

                if (carry != 0)
                {
                    number[length] = carry;

                    ++length;
                }

                Add(number, length, t, out carry);

                if (carry != 0)
                {
                    number[length] = carry;

                    ++length;
                }
            }
        }

        /// <summary>
        /// 将一个 NumberInfo 转换为 Decimal。转换失败则引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 Decimal</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static decimal ToDecimal(NumberInfo numberInfo)
        {
            if (numberInfo.radix != DecimalRadix)
            {
                throw new FormatException("decimal radix.");
            }

            if (numberInfo.exponentCount > 5)
            {
                throw new OverflowException("Exponent too big.");
            }

            if (numberInfo.integerCount + numberInfo.fractionalCount > DecimalMaxScale + 2)
            {
                throw new OverflowException("Number out of Decimal range.");
            }

            var decimalInstance = Decimal;

            var exponent = decimalInstance.UncheckedParse(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

            if (numberInfo.exponentIsNegative)
            {
                exponent = -exponent;
            }

            var number = stackalloc uint[5];
            var numberLength = 0;

            ToDecimalNumber(numberInfo.chars + numberInfo.integerBegin, numberInfo.integerCount, number, ref numberLength);

            ToDecimalNumber(numberInfo.chars + numberInfo.fractionalBegin, numberInfo.fractionalCount, number, ref numberLength);

            var scale = numberInfo.fractionalCount - exponent;

            if (scale < 0)
            {
                scale = -scale;

                if (scale > DecimalMaxScale - (numberInfo.integerCount + numberInfo.fractionalCount))
                {
                    throw new OverflowException("Exponent too big.");
                }

                while (scale > 0)
                {
                    Mult(number, numberLength, DecimalRadix, out uint carry);

                    if (carry != 0)
                    {
                        number[numberLength] = carry;

                        ++numberLength;
                    }

                    --scale;
                }
            }

            if (numberLength > 3)
            {
                throw new OverflowException("Number out of Decimal range.");
            }

            decimal r;

            var upValue = (uint*)(&r);

            upValue[2] = number[0];
            upValue[3] = number[1];
            upValue[1] = number[2];

            if (numberInfo.isNegative)
            {
                upValue[0] = SignMask;
            }

            if (scale != 0)
            {
                if (scale > DecimalMaxScale)
                {
                    throw new OverflowException("Scale too big.");
                }

                ((byte*)upValue)[2] = (byte)scale;
            }

            return r;
        }







        /// <summary>
        /// 字节正整数乘以 UInt32 值。
        /// </summary>
        /// <param name="number">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="value">UInt32 值</param>
        /// <param name="carry">进位值</param>
        /// <returns>返回字节正整数的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int Mult(uint* number, int length, uint value, out uint carry)
        {
            ulong c = 0;

            for (int i = 0; i < length; ++i)
            {
                c = ((ulong)number[i]) * value + c;

                number[i] = (uint)c;

                c >>= 32;
            }

            if (c == 0)
            {
                carry = 0;

                --number;

                while (length >= 2 && number[length] == 0)
                {
                    --length;
                }

                return length;
            }

            carry = (uint)c;

            return length;
        }

        /// <summary>
        /// 字节正整数除以 UInt32 值。
        /// </summary>
        /// <param name="number">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="value">UInt32 值</param>
        /// <param name="remainder">余数</param>
        /// <returns>返回字节正整数的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int Div(uint* number, int length, uint value, out uint remainder)
        {
            ulong carry = 0;

            if (value == 0)
            {
                throw new DivideByZeroException();
            }

            if (value == 1)
            {
                remainder = 0;

                return length;
            }

            int i = length - 1;

        Top:

            if (i >= 0)
            {
                ulong t = number[i] + (carry << 32);

                if (t < value)
                {
                    carry = t;

                    number[i] = 0;

                    --length;
                    --i;

                    goto Top;
                }

                goto Div;

            Loop:

                t = number[i] + (carry << 32);

                if (t < value)
                {
                    carry = t;

                    number[i] = 0;

                    goto Next;
                }

            Div:
                carry = t / value;

                number[i] = (uint)carry;

                carry = (t - carry * value);

            Next:
                --i;

                if (i >= 0)
                {
                    goto Loop;
                }
            }

            remainder = (uint)carry;

            return length;
        }

        /// <summary>
        /// 字节正整数加上 UInt32 值。
        /// </summary>
        /// <param name="number">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="value">UInt32 值</param>
        /// <param name="carry">进位值</param>
        /// <returns>返回字节正整数的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int Add(uint* number, int length, uint value, out uint carry)
        {
            if (length <= 0)
            {
                carry = value;

                return 0;
            }

            var t = *number;

            *number = t + value;

            if (t > uint.MaxValue - value)
            {
                for (int i = 1; i < length; ++i)
                {
                    if (number[i] != uint.MaxValue)
                    {
                        ++number[i];

                        carry = 0;

                        return length;
                    }

                    number[i] = 0;
                }

                carry = 1;

                return length;
            }

            carry = 0;

            return length;
        }

        /// <summary>
        /// 字节正整数减去 UInt32 值。
        /// </summary>
        /// <param name="number">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="value">UInt32 值</param>
        /// <param name="remainder">余数</param>
        /// <returns>返回字节正整数的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int Sub(uint* number, int length, uint value, out uint remainder)
        {
            if (length <= 0)
            {
                remainder = value;

                return 0;
            }

            if (*number >= value)
            {
                *number -= value;

                remainder = 0;

                if (*number == 0 && length == 1)
                {
                    return 0;
                }

                return length;
            }

            if (length != 1)
            {
                *number = (uint)((0x100000000 + *number) - value);

                for (int i = 1; i < length; ++i)
                {
                    if (number[i] == 0)
                    {
                        number[i] = 0xFFFFFFFF;
                    }
                    else if (number[i] == 1 && i + 1 == length)
                    {
                        number[i] = 0;

                        remainder = 0;

                        return length - 1;
                    }
                    else
                    {
                        --number[i];

                        remainder = 0;

                        return length;
                    }
                }
            }

            remainder = value - *number;

            *number = 0;

            return 0;
        }







        /// <summary>
        /// 将一个 Guid 值转换为 String 表现形式。转换失败将引发异常。
        /// </summary>
        /// <param name="value">Guid 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ToString(Guid value)
        {
            var result = new string('\0', 36);

            fixed (char* pResult = result)
            {
                ToString(value, pResult);
            }

            return result;
        }

        /// <summary>
        /// 将一个 Decimal 值转换为 String 表现形式。转换失败将引发异常。
        /// </summary>
        /// <param name="value">Decimal 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ToString(decimal value)
        {
            char* chars = stackalloc char[30];

            int length = ToString(value, chars);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 尝试将 String 值转换为 Guid 值。
        /// </summary>
        /// <param name="text">String 值</param>
        /// <param name="value">返回 Guid 值</param>
        /// <returns>返回成功否</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool TryParse(string text, out Guid value)
        {
            fixed (char* chars = text)
            {
                return TryParse(chars, text.Length, out value) == text.Length;
            }
        }

        /// <summary>
        /// 将 String 值转换为 Guid 值。失败将引发异常。
        /// </summary>
        /// <param name="text">String 值</param>
        /// <returns>返回 Guid 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Guid ParseGuid(string text)
        {
            if (TryParse(text, out Guid r))
            {
                return r;
            }

            throw ThreadException;
        }

        /// <summary>
        /// 尝试将 String 值转换为 Decimal 值。
        /// </summary>
        /// <param name="text">String 值</param>
        /// <param name="value">返回 Decimal 值</param>
        /// <returns>返回成功否</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool TryParse(string text, out decimal value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParse(chars, length, out value, false) == length;
            }
        }

        /// <summary>
        /// 将 String 值转换为 Decimal 值。失败将引发异常。
        /// </summary>
        /// <param name="text">String 值</param>
        /// <returns>返回 Decimal 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static decimal ParseDecimal(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParse(chars, length, out decimal r, false) == length)
                {
                    return r;
                }
            }

            throw ThreadException;
        }
    }
}
