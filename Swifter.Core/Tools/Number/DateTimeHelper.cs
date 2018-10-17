using System;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供对象日期和时间操作的方法。
    /// </summary>
    public unsafe static class DateTimeHelper
    {
        /// <summary>
        /// 本地时间与 UTC 时间的时差 Tick 值。
        /// </summary>
        public static readonly long UTCDifference;

        /// <summary>
        /// 一毫秒的 Tick 值。
        /// </summary>
        public const long OneMillisecond = 10000;

        /// <summary>
        /// 一秒的 Tick 值。
        /// </summary>
        public const long OneSecond = OneMillisecond * 1000;

        /// <summary>
        /// 一分钟的 Tick 值。
        /// </summary>
        public const long OneMinute = OneSecond * 60;

        /// <summary>
        /// 一小时的 Tick 值。
        /// </summary>
        public const long OneHour = OneMinute * 60;

        /// <summary>
        /// 一天的 Tick 值。
        /// </summary>
        public const long OneDay = OneHour * 24;

        /// <summary>
        /// ISO 格式日期字符串的最大长度。
        /// </summary>
        public const int ISOStringMaxLength = 50;

        static DateTimeHelper()
        {
            var tempDateTime = DateTime.Now;

            UTCDifference = (tempDateTime - tempDateTime.ToUniversalTime()).Ticks;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int ToISOString(DateTime value, char* chars, long uTCDifference)
        {
            var c = chars;
            var dec = NumberHelper.Decimal;

            uint year = (uint)value.Year;

            dec.AppendD1(ref c, year / 1000);
            dec.AppendD3(ref c, year % 1000);

            *c++ = '-';

            dec.AppendD2(ref c, (uint)value.Month);

            *c++ = '-';

            dec.AppendD2(ref c, (uint)value.Day);

            *c++ = 'T';

            dec.AppendD2(ref c, (uint)value.Hour);

            *c++ = ':';

            dec.AppendD2(ref c, (uint)value.Minute);

            *c++ = ':';

            dec.AppendD2(ref c, (uint)value.Second);

            *c++ = '.';

            dec.AppendD3(ref c, (uint)value.Millisecond);

            if (uTCDifference > 0)
            {
                *c++ = '+';
            }
            else if (uTCDifference < 0)
            {
                *c++ = '-';

                uTCDifference = -uTCDifference;
            }
            else
            {
                *c++ = 'Z';

                goto Return;
            }


            long tDHour = uTCDifference / OneHour;

            if (tDHour < 100)
            {
                dec.AppendD2(ref c, (uint)tDHour);
            }
            else
            {
                throw new FormatException("UTC Time Difference too big.");
            }

            *c++ = ':';

            dec.AppendD2(ref c, (uint)((uTCDifference % OneHour) / OneMinute));

            Return:
            return (int)(c - chars);
        }

        /// <summary>
        /// 将日期和时间以 ISO8061 格式字符串写入到字符串中。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <param name="chars">字符串</param>
        /// <returns>返回写入结束位置，最后一个字符写入位置 + 1。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int ToISOString(DateTime value, char* chars)
        {
            return ToISOString(value, chars, UTCDifference);
        }

        /// <summary>
        /// 将日期和时间的 UTC 时间以 ISO8061 格式字符串写入到字符串中。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <param name="chars">字符串</param>
        /// <returns>返回写入结束位置，最后一个字符写入位置 + 1。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int ToUTCISOString(DateTime value, char* chars)
        {
            return ToISOString(value.AddTicks(-UTCDifference), chars, 0);
        }

        /// <summary>
        /// 将日期和时间以 ISO8061 格式化字符串。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <returns>返回一个字符串。</returns>
        public unsafe static string ToISOString(DateTime value)
        {
            char* chars = stackalloc char[ISOStringMaxLength];

            int length = ToISOString(value, chars, 0);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将日期和时间的 UTC 时间以 ISO8061 格式化字符串。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <returns>返回一个字符串。</returns>
        public unsafe static string ToUTCISOString(DateTime value)
        {
            char* chars = stackalloc char[ISOStringMaxLength];

            int length = ToUTCISOString(value, chars);

            return new string(chars, 0, length);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private unsafe static void SkipNoMeta(char* chars, ref int begin, bool ignoreMinu)
        {
            char c = chars[begin];

            if (c >= '0' && c <= '9')
            {
                return;
            }

            if (c >= 'a' && c <= 'z')
            {
                return;
            }

            if (c >= 'A' && c <= 'Z')
            {
                return;
            }

            if (c == '+')
            {
                return;
            }

            if (ignoreMinu && c == '-')
            {
                return;
            }

            ++begin;
        }

        /// <summary>
        /// 尝试解析 ISO8061 格式日期和时间字符串。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">解析结束位置。</param>
        /// <param name="value">成功返回日期和时间对象，失败返回日期和时间最小值。</param>
        /// <returns>返回成功或失败。</returns>
        public static bool TryParseISODateTime(char* chars, int length, out DateTime value)
        {
            var index = 0;

            int year = 1,
                month = 1,
                day = 1,
                hour = 0,
                minute = 0,
                second = 0,
                millisecond = 0,
                week = 0; // if 0 then no using.

            long ticks = 0; // if 0 then no using.

            // Date
            // yyyy-MM-dd
            // yyyyMMdd
            // yyyy-ddd
            // yyyyddd
            // yyyy-Www-d
            // yyyyWwwd

            // Time
            // hh:mm:ss.iii
            // hh:mm:ss
            // hh:mm
            // hhmmssiii
            // hhmmss
            // hhmm

            // yyyyddd
            if (length < 7)
            {
                goto False;
            }

            /* 读取前四位数字为年份，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 4, out year) != 4)
            {
                goto False;
            }

            index += 4;

            SkipNoMeta(chars, ref index, false);

            switch (chars[index])
            {
                case 'W':
                case 'w':
                    goto Wwwd;
            }

            // ddd
            switch (length - index)
            {
                case 0:
                case 1:
                case 2:
                    goto False;
                case 3:
                    goto ddd;

            }

            switch (chars[index + 3])
            {
                case 'T':
                case 't':
                    goto ddd;
            }

            /* 读取两位数字为月份，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 2, out month) != 2)
            {
                goto False;
            }

            index += 2;

            SkipNoMeta(chars, ref index, false);

            // dd
            if (length - index < 2)
            {
                goto False;
            }

            /* 读取两位数字为天份，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 2, out day) != 2)
            {
                goto False;
            }

            index += 2;

            goto Time;
            ddd:

            /* 读取三位数字为一年第几天，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 3, out day) != 3)
            {
                goto False;
            }

            index += 3;

            goto Time;

            Wwwd:

            // Wwwd
            if (length - index < 4)
            {
                goto False;
            }

            ++index;

            /* 读取两位数字为第几个星期，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 2, out week) != 2)
            {
                goto False;
            }

            index += 2;

            SkipNoMeta(chars, ref index, false);

            // d
            if (length - index < 1)
            {
                goto False;
            }

            /* 读取一位数字为星期的第几天，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 1, out day) != 1)
            {
                goto False;
            }

            ++index;

            Time:

            if (length - index < 1)
            {
                goto True;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
                case 'T':
                case 't':
                    ++index;
                    break;
            }

            // Z
            if (length - index < 1)
            {
                goto False;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
            }

            // hhmm
            if (length - index < 4)
            {
                goto False;
            }

            /* 读取两位数字为小时，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 2, out hour) != 2)
            {
                goto False;
            }

            index += 2;

            SkipNoMeta(chars, ref index, true);

            if (length - index < 2)
            {
                goto False;
            }

            /* 读取两位数字为分钟，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 2, out minute) != 2)
            {
                goto False;
            }

            index += 2;

            if (length - index < 1)
            {
                goto True;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
            }

            if (length - index < 2)
            {
                goto False;
            }

            /* 读取两位数字为秒钟，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 2, out second) != 2)
            {
                goto False;
            }

            index += 2;

            if (length - index < 1)
            {
                goto True;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
            }

            if (length - index < 3)
            {
                goto False;
            }

            /* 读取三位数字为毫秒，如果读取失败则返回 false */
            if (NumberHelper.Decimal.TryParse(chars + index, 3, out millisecond) != 3)
            {
                goto False;
            }

            index += 3;

            if (length - index < 1)
            {
                goto True;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
            }

            goto False;
            Difference:

            ticks = UTCDifference;

            bool isPlus;

            switch (chars[index])
            {
                case '+':
                    isPlus = true;
                    break;
                case '-':
                    isPlus = false;
                    break;
                case 'Z':
                    goto True;
                default:
                    goto False;
            }

            ++index;

            if (length - index < 2)
            {
                goto False;
            }

            int dHour;
            int dMinute = 0;

            /* 读取两位数为时差小时部分 */
            if (NumberHelper.Decimal.TryParse(chars + index, 2, out dHour) != 2)
            {
                goto False;
            }

            index += 2;

            if (length - index != 0)
            {
                SkipNoMeta(chars, ref index, true);

                if (length - index < 2)
                {
                    goto False;
                }

                /* 读取两位数为时差分钟部分 */
                if (NumberHelper.Decimal.TryParse(chars + index, 2, out dMinute) != 2)
                {
                    goto False;
                }

                index += 2;
            }

            if (isPlus)
            {
                ticks -= (dHour * OneHour) + (dMinute * OneMinute);
            }
            else
            {
                ticks += (dHour * OneHour) + (dMinute * OneMinute);
            }

            True:

            value = new DateTime(year, month, day, hour, minute, second, millisecond);

            if (week != 0)
            {
                value = value.AddDays(week * 7);
            }

            if (ticks != 0)
            {
                value = value.AddTicks(ticks);
            }

            return true;

            False:

            value = DateTime.MinValue;

            return false;
        }

        /// <summary>
        /// 尝试解析 ISO8061 格式日期和时间字符串。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">成功返回日期和时间对象，失败返回日期和时间最小值。</param>
        /// <returns>返回成功或失败。</returns>
        public unsafe static bool TryParseISODateTime(string text, out DateTime value)
        {
            if (text == null)
            {
                throw new NullReferenceException("text");
            }

            fixed (char* chars = text)
            {
                return TryParseISODateTime(chars, text.Length, out value);
            }
        }
    }
}