using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供数字类的方法。
    /// 这些方法都是高效的。
    /// </summary>
    public sealed unsafe partial class NumberHelper
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
    }
}