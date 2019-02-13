using System;
using System.Runtime.CompilerServices;


namespace Swifter.Tools
{
    public sealed unsafe partial class NumberHelper
    {
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

                    if (t != 0)
                    {
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
            if (exception) ThreadException = new FormatException("Double text format error.");
            goto ReturnFalse;
        FormatException:
            if (exception) ThreadException = new FormatException("Double text format error.");
            goto ReturnFalse;
        OutOfRange:
            if (exception) ThreadException = new OverflowException("Value out of Double range.");
            goto ReturnFalse;
        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto Return;

        ReturnFalse:
            value = 0;
            return 0;
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

                throw ThreadException;
            }
        }
    }
}