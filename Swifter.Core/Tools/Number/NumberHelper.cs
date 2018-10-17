using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供数字类的方法。
    /// 这些方法都是高效的。
    /// </summary>
    public unsafe sealed class NumberHelper
    {
        /// <summary>
        /// 支持的最大进制。
        /// </summary>
        public const byte MaxRadix = 64;
        /// <summary>
        /// 支持的最小进制。
        /// </summary>
        public const byte MinRadix = 2;
        /// <summary>
        /// 忽略大小写的前提下支持的最大进制。
        /// </summary>
        public const byte IgnoreCaseMaxRadix = 36;
        private const byte DecimalRadix = 10;
        private const byte DecimalMaxScale = 29;

        private const char DigitalsMaxValue = '~';
        private const char DigitalsMinValue = '!';
        private const char DigitalsZeroValue = '0';

        /// <summary>
        /// 正负号。
        /// </summary>
        public const char PositiveSign = '+';
        /// <summary>
        /// 负符号。
        /// </summary>
        public const char NegativeSign = '-';
        /// <summary>
        /// 无限大符号。
        /// </summary>
        public const char InfinitySign = '∞';
        /// <summary>
        /// 指数符号。
        /// </summary>
        public const char ExponentSign = 'E';
        private const char exponentSign = 'e';
        /// <summary>
        /// 点符号。
        /// </summary>
        public const char DotSign = '.';

        /// <summary>
        /// 非数字符号。
        /// </summary>
        public const string NaNSign = "NaN";

        private const char ErrorDigital = (char)999;
        private const byte ErrorRadix = 99;

        private const double DoubleMinPositive = 4.94065645841246544E-324;
        private const double DoubleMaxPositive = 1.79769313486231570E+308;
        private const double SingleMinPositive = 1.4e-45;
        private const double SingleMaxPositive = 3.4e+38;

        private const ulong PositiveInt64MinValue = 0x8000000000000000;
        private const long NegativeInt64MaxValue = -0x7FFFFFFFFFFFFFFF;
        private const long Int64MinValue = -0x8000000000000000;
        private const long Int64MaxValue = 0x7FFFFFFFFFFFFFFF;
        private const ulong UInt64MaxValue = 0xFFFFFFFFFFFFFFFF;

        private const uint UInt32MaxValue = 0xFFFFFFFF;
        private const ulong UInt32MaxValueAddOne = ((ulong)UInt32MaxValue) + 1;

        private const uint SignMask = 0x80000000;

        private const uint DecimalBaseDivisor = 1000000000;

        private const byte DecimalBaseDivisorLength = 9;

        private const byte DecimalMaxParseLength = 36;

        /* ['0', '1', '2', '3',... 'a', 'b', 'c',... 'A', 'B', 'C',... 'Z', '~', '!'] */
        private static readonly char* Digitals;
        /* ['0': 0, '1': 1, '2': 2,... '9': 9, 'a': 10, 'b':11,... 'A': 36, 'B': 37,... 'Z': 61, '~': 62, '!': 63, Other: ErrorRadix] */
        private static readonly byte* Radixes;
        /* ['0': 0, '1': 1, '2': 2,... '9': 9, 'a': 10, 'b':11,... 'A': 10, 'B': 11,... 'Z': 35, Other: ErrorRadix] */
        private static readonly byte* IgnoreCaseRadixes;

        [ThreadStatic]
        private static Exception threadException;

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

        static NumberHelper()
        {
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

            Decimal = new NumberHelper(10);
            Hex = new NumberHelper(16);
            Octal = new NumberHelper(8);
            Binary = new NumberHelper(2);
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
            threadException = new FormatException("Digit out of radix.");
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
            if(exception) threadException = new FormatException("Guid length error!");

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

            if (zero != 0)
            {
                mult = i - zero;

                zeroCount += zero;
            }
            else if (zeroCount != 0)
            {
                mult = i + zeroCount;

                zeroCount = 0;
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
            if (exception) threadException = new FormatException("Decimal text format error.");
            goto ReturnFalse;
            FormatException:
            if (exception) threadException = new FormatException("Decimal text format error.");
            goto ReturnFalse;
            OutOfRange:
            if (exception) threadException = new OverflowException("Value out of decimal range.");
            goto ReturnFalse;
            OutOfRadix:
            if (exception) threadException = new OverflowException("Digit out of Decimal radix.");
            goto Return;
            ExponentOutOfRange:
            if (exception) threadException = new OverflowException("Exponent out of Decimal range.");
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
                throw new FormatException("radix");
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

            throw threadException;
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

            throw threadException;
        }




        /* 10 */
        private readonly byte radix;
        /* 5 */
        private readonly byte rounded;

        /* ['0': 0, '1': 1, '2': 2,... '9': 9, 'a': 10, 'b':11,... 'A': 10, 'B': 11,... 'Z': 35, Other: ErrorRadix] */
        private readonly byte* radixes;

        /* 1000 */
        private readonly uint threeDigitalsLength;
        /* 000, 001, 002, 003, 004,...999 */
        private readonly ThreeChar* threeDigitals;

        /* 20 */
        private readonly byte uInt64NumbersLength;
        /* 10 */
        private readonly byte uInt32NumbersLength;
        /* 19 */
        private readonly byte int64NumbersLength;

        /* 1, 10, 100, 1000, 10000, 100000,... */
        private readonly ulong[] uInt64Numbers;

        /* 1, 10, 100, 1000, 10000, 100000,... */
        private readonly uint[] uInt32Numbers;

        /* 309 */
        private readonly uint exponentsLength;
        /* 308 */
        private readonly uint exponentsRight;
        /* 0 */
        private const uint exponentsLeft = 0;
        /* 1, 10, 100, 1000, 1e4, 1e5, 1e6,... 1e308 */
        private readonly double[] positiveExponents;
        /* 1, 0.1, 0.01, 0.001, 1e-4, 1e-5, 1e-6,... 1e-308 */
        private readonly double[] negativeExponents;
        
        /* 5 */
        private const uint maxFractionalLength = 5;
        /* 16 */
        private readonly uint maxDoubleLength;
        /* 7 */
        private readonly uint maxSingleLength;

        /* 1000000000 */
        private readonly uint baseDivisor;

        /* 9 */
        private readonly byte baseLength;

        /* 0.00000000000000022 */
        private const double ceilingApproximateValueOfZero = 0.00000000000000022;
        /* 0.99999999999999978 */
        private const double floorApproximateValueOfOne = 0.99999999999999978;







        /// <summary>
        /// 初始化实例。
        /// </summary>
        /// <param name="radix">指定进制</param>
        public NumberHelper(byte radix)
        {
            if (radix > MaxRadix || radix < MinRadix)
            {
                throw new ArgumentOutOfRangeException(nameof(radix));
            }

            this.radix = radix;

            /* binary no rounded. */
            rounded = (byte)(radix == 2 ? 2 : (radix + 1) / 2);

            if (radix <= IgnoreCaseMaxRadix)
            {
                radixes = IgnoreCaseRadixes;
            }
            else
            {
                radixes = Radixes;
            }

            threeDigitalsLength = (uint)SlowPow(radix, 3);
            threeDigitals = (ThreeChar*)Marshal.AllocHGlobal((int)(threeDigitalsLength * sizeof(ThreeChar)));

            for (uint i = 0; i < threeDigitalsLength; i++)
            {
                threeDigitals[i] = SlowToThreeChar(i);
            }

            int64NumbersLength = SlowGetLength(0x8000000000000000);
            uInt64NumbersLength = SlowGetLength(0xFFFFFFFFFFFFFFFF);
            uInt32NumbersLength = SlowGetLength(0xFFFFFFFF);

            uInt64Numbers = new ulong[uInt64NumbersLength];

            for (uint i = 0; i < uInt64NumbersLength; i++)
            {
                uInt64Numbers[i] = SlowPow(radix, i);
            }

            uInt32Numbers = new uint[uInt32NumbersLength];

            for (uint i = 0; i < uInt32NumbersLength; i++)
            {
                uInt32Numbers[i] = (uint)SlowPow(radix, i);
            }

            exponentsLength = SlowGetLength(DoubleMaxPositive);
            // PositiveNumbersLeft = 0;
            exponentsRight = exponentsLength - 1;

            positiveExponents = new double[exponentsLength];
            negativeExponents = new double[exponentsLength];

            for (int i = 0; i < exponentsLength; i++)
            {
                positiveExponents[i] = Math.Pow(radix, i);
                negativeExponents[i] = Math.Pow(radix, -i);
            }
            
            maxDoubleLength = SlowGetLength(0xFFFFFFFFFFFFF);
            maxSingleLength = SlowGetLength(0x7FFFFF);

            baseLength = (byte)(uInt32NumbersLength - 1);
            baseDivisor = uInt32Numbers[baseLength];
        }

        /// <summary>
        /// 释放内存。
        /// </summary>
        ~NumberHelper()
        {
            Marshal.FreeHGlobal((IntPtr)threeDigitals);
        }







        private ThreeChar SlowToThreeChar(uint value)
        {
            ThreeChar t;

            t.char1 = Digitals[(value / radix / radix) % radix];
            t.char2 = Digitals[(value / radix) % radix];
            t.char3 = Digitals[value % radix];

            return t;
        }

        private byte SlowGetLength(ulong value)
        {
            byte result = 0;

            do
            {
                ++result;

                value /= radix;

            } while (value != 0);

            return result;
        }

        private uint SlowGetLength(double value)
        {
            double rP16 = Math.Pow(radix, 16);

            uint result = 0;

            while (value > rP16)
            {
                value /= rP16;

                result += 16;
            }

            while (value >= 1)
            {
                value /= radix;

                ++result;
            }

            return result;
        }











        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD1(ref char* chars, ulong value)
        {
            *chars = ((char*)(threeDigitals + value))[2];

            chars += 1;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD2(ref char* chars, ulong value)
        {
            *((TwoChar*)chars) = *(TwoChar*)((char*)(threeDigitals + value) + 1);

            chars += 2;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD3(ref char* chars, ulong value)
        {
            *((ThreeChar*)chars) = threeDigitals[value];

            chars += 3;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD6(ref char* chars, ulong value)
        {
            var l = threeDigitalsLength;
            var v = value;
            var a = v / l;

            *((ThreeChar*)chars) = threeDigitals[a];
            *((ThreeChar*)(chars + 3)) = threeDigitals[v - a * l];

            chars += 6;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD9(ref char* chars, ulong value)
        {
            var l = threeDigitalsLength;
            var v = value;
            var b = v / l;
            var a = b / l;

            v -= b * l;
            b -= a * l;

            *((ThreeChar*)chars) = threeDigitals[a];
            *((ThreeChar*)(chars + 3)) = threeDigitals[b];
            *((ThreeChar*)(chars + 6)) = threeDigitals[v];

            chars += 9;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD12(ref char* chars, ulong value)
        {
            var l = threeDigitalsLength;
            var v = value;
            var c = v / l;
            var b = c / l;
            var a = b / l;

            v -= c * l;
            c -= b * l;
            b -= a * l;

            *((ThreeChar*)chars) = threeDigitals[a];
            *((ThreeChar*)(chars + 3)) = threeDigitals[b];
            *((ThreeChar*)(chars + 6)) = threeDigitals[c];
            *((ThreeChar*)(chars + 9)) = threeDigitals[v];

            chars += 12;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD15(ref char* chars, ulong value)
        {
            var l = threeDigitalsLength;
            var v = value;
            var d = v / l;
            var c = d / l;
            var b = c / l;
            var a = b / l;

            v -= d * l;
            d -= c * l;
            c -= b * l;
            b -= a * l;

            *((ThreeChar*)chars) = threeDigitals[a];
            *((ThreeChar*)(chars + 3)) = threeDigitals[b];
            *((ThreeChar*)(chars + 6)) = threeDigitals[c];
            *((ThreeChar*)(chars + 9)) = threeDigitals[d];
            *((ThreeChar*)(chars + 12)) = threeDigitals[v];

            chars += 15;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD18(ref char* chars, ulong value)
        {
            var l = threeDigitalsLength;
            var v = value;
            var e = v / l;
            var d = e / l;
            var c = d / l;
            var b = c / l;
            var a = b / l;

            v -= e * l;
            e -= d * l;
            d -= c * l;
            c -= b * l;
            b -= a * l;

            *((ThreeChar*)chars) = threeDigitals[a];
            *((ThreeChar*)(chars + 3)) = threeDigitals[b];
            *((ThreeChar*)(chars + 6)) = threeDigitals[c];
            *((ThreeChar*)(chars + 9)) = threeDigitals[d];
            *((ThreeChar*)(chars + 12)) = threeDigitals[e];
            *((ThreeChar*)(chars + 15)) = threeDigitals[v];

            chars += 18;
        }









        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private byte ToRadix(char c)
        {
            if (c <= DigitalsMaxValue)
            {
                return radixes[c];
            }

            return ErrorRadix;
        }

        /// <summary>
        /// 获取整数部分位数。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public uint GetPositiveExponent(double value)
        {
            if (value < 1)
            {
                return 0;
            }

            var x = exponentsLeft;
            var y = exponentsLength;

            while (true)
            {
                var i = (x + y) >> 1;

                if (value >= positiveExponents[i])
                {
                    if (x == i)
                    {
                        return x;
                    }

                    x = i;
                }
                else
                {
                    y = i;
                }
            }
        }

        /// <summary>
        /// 获取数字需要移动多少位才能大于等于 1。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public uint GetNegativeExponent(double value)
        {
            if (value >= 1)
            {
                return 0;
            }

            if (value == 0)
            {
                return 0;
            }

            var x = exponentsLeft;
            var y = exponentsLength;

            while (true)
            {
                var i = (x + y) >> 1;

                if (value < negativeExponents[i])
                {
                    if (x == i)
                    {
                        return x + 1;
                    }

                    x = i;
                }
                else
                {
                    y = i;
                }
            }
        }

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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int ToString(double value, uint length, char* chars, uint maxDoubleLength)
        {
            var v = value;
            var l = length;
            var c = chars;
            var r = radix;
            var n = uInt64Numbers;
            var i = (ulong)v;
            var f = v - i; // Fractional

            if (f > floorApproximateValueOfOne)
            {
                ++i;

                f = 0;
            }
            else if (f < ceilingApproximateValueOfZero)
            {
                f = 0;
            }

            if (l < maxDoubleLength)
            {
                l = maxDoubleLength - l;

                var j = (ulong)(f * positiveExponents[l + 1]);

                if (j != 0)
                {
                    var t = j / r;

                    // Last digit rounded.
                    if (j - t * r >= rounded)
                    {
                        ++t;
                    }


                    // Trim End 0
                    DN8:
                    j = t / n[8];
                    if (t == j * n[8])
                    {
                        t = j;
                        l -= 8;
                        goto DN8;
                    }

                    j = t / n[4];
                    if (t == j * n[4])
                    {
                        t = j;
                        l -= 4;
                    }

                    j = t / n[2];
                    if (t == j * n[2])
                    {
                        t = j;
                        l -= 2;
                    }

                    j = t / r;
                    if (t == j * r)
                    {
                        t = j;
                        --l;
                    }

                    if (l == 0)
                    {
                        ++i;

                        return ToString(i, c);
                    }

                    c += ToString(i, length, c);

                    *c = DotSign;
                    ++c;

                    return (int)(c - chars) + ToString(t, l, c);
                }
            }

            return ToString(i, length, c);
        }

        /// <summary>
        /// 将一个 Double 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">Double 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(double value, char* chars)
        {
            var v = value;
            var c = chars;
            var m = maxDoubleLength;

            if (IsNaN(v))
            {
                return Append(chars, NaNSign);
            }

            if (v < 0)
            {
                *c = NegativeSign;

                ++c;

                v = -v;
            }

            if (v < DoubleMinPositive)
            {
                *chars = DigitalsZeroValue;

                return 1;
            }

            if (v > DoubleMaxPositive)
            {
                *c = InfinitySign;

                return ((int)(c - chars)) + 1;
            }

            var p = GetPositiveExponent(v);
            if (p >= m)
            {
                /* 1e16 - 10e308 */

                c += ToString(v / positiveExponents[p], 1, c, m);

                *c = ExponentSign;
                ++c;

                *c = PositiveSign;
                ++c;

                c += ToString((ulong)p, c);
            }
            else if (p == 0)
            {
                var n = GetNegativeExponent(v);

                if (n >= maxFractionalLength)
                {
                    /* 1e-5 - 10e-308 */

                    c += ToString(v * positiveExponents[n], 1, c, m);

                    *c = ExponentSign;
                    ++c;

                    *c = NegativeSign;
                    ++c;

                    c += ToString((ulong)n, c);
                }
                else
                {
                    /* 1e-1 - 1e-5 */

                    c += ToString(v, 1, c, m);
                }
            }
            else
            {
                /* 1e1 - 1e16 */

                c += ToString(v, p + 1, c, m);
            }

            return (int)(c - chars);
        }

        /// <summary>
        /// 将一个 Single 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">Single 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(float value, char* chars)
        {
            double v = value;
            var c = chars;
            var m = maxSingleLength;

            if (IsNaN(v))
            {
                return Append(chars, NaNSign);
            }

            if (v < 0)
            {
                *c = NegativeSign;

                ++c;

                v = -v;
            }

            if (v < SingleMinPositive)
            {
                *chars = DigitalsZeroValue;

                return 1;
            }

            if (v > SingleMaxPositive)
            {
                *c = InfinitySign;

                return ((int)(c - chars)) + 1;
            }

            var p = GetPositiveExponent(v);
            if (p >= maxSingleLength)
            {
                /* 1e16 - 10e38 */

                c += ToString(v / positiveExponents[p], 1, c, m);

                *c = ExponentSign;
                ++c;

                *c = PositiveSign;
                ++c;

                c += ToString((ulong)p, c);
            }
            else if (p == 0)
            {
                var n = GetNegativeExponent(v);

                if (n >= maxFractionalLength)
                {
                    /* 1e-5 - 10e-38 */

                    c += ToString(v * positiveExponents[n], 1, c, m);

                    *c = ExponentSign;
                    ++c;

                    *c = NegativeSign;
                    ++c;

                    c += ToString((ulong)n, c);
                }
                else
                {
                    /* 1e-1 - 1e-5 */

                    c += ToString(v, 1, c, m);
                }
            }
            else
            {
                /* 1e1 - 1e16 */

                c += ToString(v, p + 1, c, m);
            }

            return (int)(c - chars);
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
            if (exception) threadException = new FormatException("Digit out of radix.");
            goto Return;

            OutOfRange:
            if (exception) threadException = new OverflowException("value out of range.");
            goto Return;

            EmptyLength:
            if (exception) threadException = new ArgumentException("Length cna't be less than 1");
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
            if (exception) threadException = new FormatException("Digit out of radix.");
            goto Return;

            OutOfRange:
            if (exception) threadException = new OverflowException("value out of range.");
            goto Return;

            EmptyLength:
            if (exception) threadException = new ArgumentException("Length cna't be less than 1");
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
            if (exception) threadException = new FormatException("Digit out of radix.");
            goto ErrorReturn;

            EmptyLength:
            if (exception) threadException = new ArgumentException("Length cna't be less than 1");
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
                threadException = new OverflowException("Value out of Int32 range.");
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
            if (exception) threadException = new FormatException("Digit out of radix.");
            goto Return;

            OutOfRange:
            if (exception) threadException = new OverflowException("value out of range.");
            index = 0;
            goto Return;

            FormatError:
            if (exception) threadException = new FormatException("number text format error.");
            index = 0;
            goto Return;

            EmptyLength:
            if (exception) threadException = new ArgumentException("Length cna't be less than 1");
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
            if (exception) threadException = new OverflowException("value out of range.");
            value = 0;
            return 0;

            FormatError:
            if (exception) threadException = new FormatException("number text format error.");
            value = 0;
            return 0;

            EmptyLength:
            if (exception) threadException = new ArgumentException("Length cna't be less than 1");
            value = 0;
            return 0;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 Double 值。此方法允许指数。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Double 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, out double value, bool exception = false)
        {
            double v = 0;
            var n = false;
            var r = radix;
            int i = 0;
            var l = length;
            var m = uInt64NumbersLength - 1;
            var u = uInt64Numbers;
            var p = positiveExponents;
            var k = negativeExponents;
            var f = -1;
            var j = 0;
            long e = 0;

            if (l <= 0)
            {
                goto EmptyLength;
            }

            switch (chars[0])
            {
                case PositiveSign:
                    ++i;
                    break;
                case NegativeSign:
                    ++i;
                    n = true;
                    break;
            }

            ulong t = 0;
            int s = 0;

            while (i < l)
            {
                if (s == m)
                {
                    v = v * u[m] + t;

                    t = 0;
                    s = 0;
                }

                var d = ToRadix(chars[i]);

                if (d >= r)
                {
                    switch (chars[i])
                    {
                        case DotSign:

                            if (f != -1)
                            {
                                goto FormatException;
                            }

                            ++i;

                            f = j;
                            continue;
                        case ExponentSign:
                        case exponentSign:
                            ++i;
                            goto Exponent;

                        default:
                            goto OutOfRadix;
                    }
                }

                t = t * r + d;
                
                ++s;
                ++i;
                ++j;
            }

            Return:

            if (s != 0)
            {
                v = v * u[s] + t;
            }

            if (f != -1)
            {
                if (j == f)
                {
                    goto FormatException;
                }

                e -= j - f;
            }

            if (e > 0)
            {
                if (e >= 1024)
                {
                    goto OutOfRange;
                }

                while (e >= 100)
                {
                    v *= p[100];

                    e -= 100;
                }

                while (e >= 10)
                {
                    v *= p[10];

                    e -= 10;
                }

                while (e >= 1)
                {
                    v *= r;

                    --e;
                }
            }
            else if (e < 0)
            {
                if (e <= -1024)
                {
                    goto OutOfRange;
                }

                while (e <= -100)
                {
                    v *= k[100];

                    e += 100;
                }

                while (e <= -10)
                {
                    v *= k[10];

                    e += 10;
                }

                while (e <= -1)
                {
                    v /= r;

                    ++e;
                }
            }

            if (v > DoubleMaxPositive)
            {
                goto OutOfRange;
            }

            if (n)
            {
                value = -v;
            }
            else
            {
                value = v;
            }

            return i;

            Exponent:

            i += TryParse(chars + i, l - i, out e, exception);

            goto Return;


            EmptyLength:
            if (exception) threadException = new FormatException("Double text format error.");
            goto ReturnFalse;
            FormatException:
            if (exception) threadException = new FormatException("Double text format error.");
            goto ReturnFalse;
            OutOfRange:
            if (exception) threadException = new OverflowException("Value out of Double range.");
            goto ReturnFalse;
            OutOfRadix:
            if (exception) threadException = new FormatException("Digit out of radix.");
            goto Return;

            ReturnFalse:
            value = 0;
            return 0;
        }

        /// <summary>
        /// 创建一个 NumberInfo。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回一个 NumberInfo</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public NumberInfo GetNumberInfo(char* chars, int length)
        {
            var r = new NumberInfo();

            r.chars = chars;
            r.radix = radix;

            if (length > 0)
            {
                var index = 0;

                var signChar = chars[0];

                switch (signChar)
                {
                    case NegativeSign:
                    case PositiveSign:
                        ++index;
                        r.isNegative = signChar == NegativeSign;
                        break;
                }

                var beforeZeroCount = 0;

                while (index < length && chars[index] == DigitalsZeroValue)
                {
                    ++beforeZeroCount;
                    ++index;
                }

                var integerBegin = index;
                var integerCount = 0;

                while (index < length && ToRadix(chars[index]) < radix)
                {
                    ++integerCount;
                    ++index;
                }

                if (integerCount != 0)
                {
                    r.integerBegin = integerBegin;
                    r.integerCount = integerCount;
                }
                else if (beforeZeroCount != 0)
                {
                    r.integerBegin = index - 1;
                    r.integerCount = 1;
                }

                if (index < length && chars[index] == DotSign)
                {
                    r.isFloat = true;

                    ++index;

                    var fractionalBegin = index;
                    var fractionalCount = 0;

                    while (index < length && ToRadix(chars[index]) < radix)
                    {
                        ++fractionalCount;
                        ++index;
                    }

                    if (fractionalCount != 0)
                    {
                        for (int fractionalRight = fractionalBegin + fractionalCount - 1; fractionalRight >= 0; --fractionalRight)
                        {
                            if (chars[fractionalRight] != DigitalsZeroValue)
                            {
                                break;
                            }

                            --fractionalCount;
                        }

                        r.isFloat = fractionalCount != 0;
                        r.fractionalBegin = fractionalBegin;
                        r.fractionalCount = fractionalCount;
                    }
                }

                if (index < length)
                {
                    bool haveExponent = false;

                    switch (chars[index])
                    {
                        case exponentSign:
                        case ExponentSign:

                            ++index;

                            haveExponent = true;

                            break;
                    }

                    if (haveExponent && index < length)
                    {
                        var exponentSignChar = chars[index];

                        switch (exponentSignChar)
                        {
                            case NegativeSign:
                            case PositiveSign:
                                ++index;
                                r.exponentIsNegative = exponentSignChar == NegativeSign;
                                break;
                        }

                        var exponentBegin = index;

                        var exponentBeforeZeroCount = 0;

                        while (index < length && chars[index] == DigitalsZeroValue)
                        {
                            ++exponentBeforeZeroCount;
                            ++index;
                        }

                        var exponentCount = 0;

                        while (index < length && ToRadix(chars[index]) < radix)
                        {
                            ++exponentCount;
                            ++index;
                        }

                        if (exponentCount != 0)
                        {
                            r.exponentBegin = exponentBegin;
                            r.exponentCount = exponentCount;
                        }
                        else if (beforeZeroCount != 0)
                        {
                            r.exponentBegin = index - 1;
                            r.exponentCount = 1;
                        }
                    }
                }
            }

            return r;
        }

        /// <summary>
        /// 将 NumberInfo 转换为 UInt64。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 UInt64</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ToUInt64(NumberInfo numberInfo)
        {
            if (numberInfo.radix != radix)
            {
                throw new FormatException("radix");
            }

            if (numberInfo.exponentCount > 5)
            {
                throw new OverflowException("Exponent too big.");
            }

            var exponent = UncheckedParse(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

            if (numberInfo.exponentIsNegative)
            {
                exponent = -exponent;
            }

            int count = ((int)exponent) + numberInfo.integerCount;

            if (count > uInt64NumbersLength)
            {
                throw new OverflowException("Number out of UInt64 range.");
            }

            if (count <= 0)
            {
                return 0;
            }

            byte l = 0;
            var c = count;
            var r = 0UL;

            for (int i = 0, j = numberInfo.integerBegin; i < numberInfo.integerCount; ++i, ++j, --c)
            {
                l = ToRadix(numberInfo.chars[j]);

                if (c <= 1)
                {
                    goto End;
                }

                r = r * radix + l;
            }

            for (int i = 0, j = numberInfo.fractionalBegin; i < numberInfo.fractionalCount; ++i, ++j, --c)
            {
                l = ToRadix(numberInfo.chars[j]);

                if (c <= 1)
                {
                    goto End;
                }

                r = r * radix + l;
            }

            for (; c > 1; --c)
            {
                r = r * radix;
            }

            End:

            if (count == uInt64NumbersLength && r > (UInt64MaxValue - l) / radix)
            {
                throw new OverflowException("Number out of UInt64 range.");
            }

            r = r * radix + l;

            return r;
        }

        /// <summary>
        /// 将 NumberInfo 转换为 Int64。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 Int64</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ToInt64(NumberInfo numberInfo)
        {
            if (numberInfo.radix != radix)
            {
                throw new FormatException("radix");
            }

            ulong uInt64 = ToUInt64(numberInfo);

            if (numberInfo.isNegative)
            {
                if (uInt64 > PositiveInt64MinValue)
                {
                    throw new OverflowException("Number out of Int64 range.");
                }

                if (uInt64 == PositiveInt64MinValue)
                {
                    return long.MinValue;
                }

                return -(long)uInt64;
            }

            if (uInt64 > Int64MaxValue)
            {
                throw new OverflowException("Number out of Int64 range.");
            }

            return (long)uInt64;
        }

        /// <summary>
        /// 将 NumberInfo 转换为 Double。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 Double</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ToDouble(NumberInfo numberInfo)
        {
            if (numberInfo.radix != radix)
            {
                throw new FormatException("radix");
            }

            if (numberInfo.exponentCount > 5)
            {
                throw new OverflowException("Exponent too big.");
            }

            var exponent = UncheckedParse(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

            if (numberInfo.exponentIsNegative)
            {
                exponent = -exponent;
            }
            
            var r = 0D;

            for (int i = 0, j = numberInfo.integerBegin; i < numberInfo.integerCount; ++i, ++j)
            {
                r = r * radix + ToRadix(numberInfo.chars[j]);
            }

            for (int i = 0, j = numberInfo.fractionalBegin; i < numberInfo.fractionalCount; ++i, ++j)
            {
                r = r * radix + ToRadix(numberInfo.chars[j]);
            }

            var e = (int)exponent - numberInfo.fractionalCount;

            var exponents = positiveExponents;

            if (e < 0)
            {
                exponents = negativeExponents;

                e = -e;
            }

            if (e > 0)
            {
                while (e >= 100)
                {
                    r *= exponents[100];

                    e -= 100;
                }

                while (e >= 10)
                {
                    r *= exponents[10];

                    e -= 10;
                }

                while (e >= 1)
                {
                    r *= exponents[1];

                    --e;
                }
            }

            if (r > DoubleMaxPositive)
            {
                throw new OverflowException("Number out of Double range.");
            }

            return r;
        }

        /// <summary>
        /// 从字符串中强制解析出一个 Int64 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="count">字符串长度</param>
        /// <returns>返回一个 Int64 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long UncheckedParse(char* chars, int count)
        {
            int r = 0;

            for (int i = 0; i < count; i++)
            {
                r = r * radix + ToRadix(chars[i]);
            }

            return r;
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
        /// 将 Double 值转换为字符串表现形式。
        /// </summary>
        /// <param name="value">Double 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(double value)
        {
            var chars = stackalloc char[68];

            var length = ToString(value, chars);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将 Single 值转换为字符串表现形式。
        /// </summary>
        /// <param name="value">Single 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(float value)
        {
            var chars = stackalloc char[36];

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

                throw threadException;
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

                throw threadException;
            }
        }

        /// <summary>
        /// 尝试将字符串转换为 Double 值。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">返回一个 Double 值</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParse(string text, out double value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParse(chars, length, out value) == length;
            }
        }

        /// <summary>
        /// 将字符串转换为 Double 值。失败将引发异常。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个 Double 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ParseDouble(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParse(chars, length, out double value, true) == length)
                {
                    return value;
                }

                throw threadException;
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

                throw threadException;
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

                throw threadException;
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

            throw threadException;
        }
    }
}