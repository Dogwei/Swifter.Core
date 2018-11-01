using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 字符串辅助类
    /// </summary>
    public static class StringHelper
    {
        private const int HashCodeMultNumber = 1234567891;

        /// <summary>
        /// 颠倒字符串内容。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个新的字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static string Reverse(string text)
        {
            string result = new string('\0', text.Length);

            fixed (char* pResult = result)
            {
                for (int i = 0, j = text.Length - 1; j >= 0; i++, --j)
                {
                    pResult[i] = text[j];
                }
            }

            return result;
        }

        /// <summary>
        /// 忽略大小写获取字符串 Hash 值。
        /// </summary>
        /// <param name="st">字符串。</param>
        /// <returns>返回一个 int hash 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IgnoreCaseGetHashCode(string st)
        {
            int r = 0;

            for (int i = st.Length - 1; i >= 0; --i)
            {
                r ^= ToUpper(st[i]) * HashCodeMultNumber;
            }

            return r;
        }

        /// <summary>
        /// 获取字符串 Hash 值。
        /// </summary>
        /// <param name="st">字符串。</param>
        /// <returns>返回一个 int hash 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetHashCode(string st)
        {
            int r = 0;

            for (int i = st.Length - 1; i >= 0; --i)
            {
                r ^= st[i] * HashCodeMultNumber;
            }

            return r;
        }

        /// <summary>
        /// 忽略大小写匹配两个字符串。请确保字符串 2 是已大写的。
        /// </summary>
        /// <param name="st1">字符串 1</param>
        /// <param name="st2">字符串 2</param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IgnoreCaseEquals(string st1, string st2)
        {
            int length = st1.Length;

            if (length != st2.Length)
            {
                return false;
            }

            while (--length >= 0)
            {
                if (ToUpper(st1[length]) != st2[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 匹配两个字符串。
        /// </summary>
        /// <param name="st1">字符串 1</param>
        /// <param name="st2">字符串 2</param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool Equals(string st1, string st2)
        {
            int length = st1.Length;

            if (length != st2.Length)
            {
                return false;
            }

            while (--length >= 0)
            {
                if (st1[length] != st2[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 将字符串中的格式项 ({Index})替换为数组中相应的字符串。可以用 '\' 字符让字符串直接复制下一个字符。
        /// </summary> 
        /// <param name="text">字符串</param>
        /// <param name="args">数组</param>
        /// <returns>返回一个新的字符串。</returns>
        public unsafe static string Format(string text, params string[] args)
        {
            if (text == null)
            {
                return null;
            }

            fixed (char* pText = text)
            {
                int resultLength;
                int begin = 0;
                int end = resultLength = text.Length;

                GetLengthLoop:
                int index = IndexOf(pText, '\\', '{', begin, end);

                if (index == -1)
                {
                    if (begin == 0)
                    {
                        return text;
                    }

                    goto Format;
                }

                if (pText[index] == '\\')
                {
                    --resultLength;

                    begin = index + 2;
                }
                else
                {
                    ++index;

                    begin = index;

                    if (index < end && pText[index] >= '0' && pText[index] <= '9')
                    {
                        int number = pText[index] - '0';

                        for (++index; index < end; ++index)
                        {
                            if (pText[index] >= '0' && pText[index] <= '9')
                            {
                                number = number * 10 + pText[index] - '0';
                            }
                            else if (pText[index] == '}')
                            {
                                if (args[number] != null)
                                {
                                    resultLength += args[number].Length;

                                    resultLength -= index - begin + 2;
                                }

                                begin = index + 1;

                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                goto GetLengthLoop;

                Format:
                begin = 0;

                var result = new string('\0', resultLength);

                int resultIndex = 0;

                fixed (char* pResult = result)
                {
                    FormatLoop:
                    index = IndexOf(pText, '\\', '{', begin, end);

                    if (index == -1)
                    {
                        index = end;
                    }

                    for (; begin < index; ++begin)
                    {
                        pResult[resultIndex] = pText[begin];

                        ++resultIndex;
                    }

                    if (resultIndex == resultLength)
                    {
                        return result;
                    }

                    if (pText[index] == '\\')
                    {
                        ++index;

                        if (index == end)
                        {
                            return result;
                        }

                        pResult[resultIndex] = pText[index];

                        ++resultIndex;

                        begin = index + 1;
                    }
                    else
                    {
                        ++index;

                        if (index < end && pText[index] >= '0' && pText[index] <= '9')
                        {
                            int number = pText[index] - '0';

                            for (++index; index < end; ++index)
                            {
                                if (pText[index] >= '0' && pText[index] <= '9')
                                {
                                    number = number * 10 + pText[index] - '0';
                                }
                                else if (pText[index] == '}')
                                {
                                    if (args[number] != null)
                                    {
                                        int argsLength = args[number].Length;

                                        fixed (char* pArg = args[number])
                                        {
                                            for (int argsIndex = 0; argsIndex < argsLength; argsIndex++)
                                            {
                                                pResult[resultIndex] = pArg[argsIndex];

                                                ++resultIndex;
                                            }
                                        }
                                    }

                                    begin = index + 1;

                                    goto FormatLoop;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        for (; begin < index; ++begin)
                        {
                            pResult[resultIndex] = pText[begin];

                            ++resultIndex;
                        }
                    }

                    goto FormatLoop;
                }
            }
        }

        /// <summary>
        /// 比较两个字符串是否相同。如果字符串 1 比字符串 2 长，但两个字符串前面的内容相同也返回 true。如果字符串 1 比字符串 2 短则直接返回 false。
        /// </summary>
        /// <param name="pText">字符串 1</param>
        /// <param name="begin">字符串 1 的开始索引</param>
        /// <param name="end">字符串 1 的结束索引，比较的位置不包含此值。</param>
        /// <param name="chars">字符串 2 </param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static bool Equals(char* pText, int begin, int end, string chars)
        {
            if (end - begin < chars.Length)
            {
                return false;
            }

            for (int i = 0; i < chars.Length && begin < end; ++i, ++begin)
            {
                if (pText[begin] != chars[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 比较两个字符串是否相同，忽略英文字符大小写。如果字符串 1 比字符串 2 长，但两个字符串前面的内容相同也返回 true。如果字符串 1 比字符串 2 短则直接返回 false。
        /// </summary>
        /// <param name="pText">字符串 1</param>
        /// <param name="begin">字符串 1 的开始索引</param>
        /// <param name="end">字符串 1 的结束索引，比较的位置不包含此值。</param>
        /// <param name="chars">字符串 2 </param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static bool IgnoreCaseEquals(char* pText, int begin, int end, string chars)
        {
            if (end - begin < chars.Length)
            {
                return false;
            }

            for (int i = 0; i < chars.Length && begin < end; ++i, ++begin)
            {
                if (ToUpper(pText[begin]) != chars[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 将小写英文字符转为大写英文字符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns>返回一个字符。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char ToUpper(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return (char)(c & (~0x20));
            }

            return c;
        }

        /// <summary>
        /// 将大写英文字符转为小写英文字符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns>返回一个字符。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char ToLower(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return (char)(c | 0x20);
            }

            return c;
        }

        /// <summary>
        /// 去除字符串两端的的空白字符
        /// </summary>
        /// <param name="pText">字符串</param>
        /// <param name="begin">开始索引</param>
        /// <param name="end">结束索引，裁剪的位置不包含此值。</param>
        /// <returns>返回一个新的字符串。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static string Trim(char* pText, int begin, int end)
        {
            while (begin < end && IsWhiteSpace(pText[begin]))
            {
                ++begin;
            }

            do
            {
                --end;
            } while (end >= begin && IsWhiteSpace(pText[end]));

            if (end >= begin)
            {
                return new string(pText, begin, end - begin + 1);
            }

            return "";
        }

        /// <summary>
        /// 判断一个字符是否为空白字符
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>返回一个 Boolean 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsWhiteSpace(char c)
        {
            return c == 0x20 || (c >= 0x9 && c <= 0xd) || c == 0x85 || c == 0xa0;
        }

        /// <summary>
        /// 在字符串中找到指定字符的索引，没找到则返回 -1
        /// </summary>
        /// <param name="pText">字符串</param>
        /// <param name="c">字符</param>
        /// <param name="begin">开始查找的位置。</param>
        /// <param name="end">结束查找的位置，不包含此值。</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* pText, char c, int begin, int end)
        {
            while (begin < end)
            {
                if (pText[begin] == c)
                {
                    return begin;
                }

                ++begin;
            }

            return -1;
        }

        /// <summary>
        /// 在字符串中找到第一个字符 1 或字符 2 的索引，两个字符都没找到则返回 -1
        /// </summary>
        /// <param name="pText">字符串</param>
        /// <param name="char1">字符 1</param>
        /// <param name="char2">字符 2</param>
        /// <param name="begin">开始查找的位置。</param>
        /// <param name="end">结束查找的位置，不包含此值。</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* pText, char char1, char char2, int begin, int end)
        {
            while (begin < end)
            {
                if (pText[begin] == char1 || pText[begin] == char2)
                {
                    return begin;
                }

                ++begin;
            }

            return -1;
        }

        /// <summary>
        /// 在字符串 1 中找到字符串 2 的索引，没找到则返回 -1
        /// </summary>
        /// <param name="pText">字符串 1</param>
        /// <param name="chars">字符串 2</param>
        /// <param name="begin">开始查找的位置</param>
        /// <param name="end">结束查找的位置，不包含此值。</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* pText, string chars, int begin, int end)
        {
            if (chars.Length == 0)
            {
                return -1;
            }

            Loop:
            while (begin < end)
            {
                if (pText[begin] == chars[0])
                {
                    for (int i = 1, j = begin + 1; i < chars.Length && j < end; ++i, ++j)
                    {
                        if (chars[i] != pText[j])
                        {
                            goto Loop;
                        }
                    }

                    return begin;
                }

                ++begin;
            }

            return -1;
        }

        /// <summary>
        /// 在字符串中找到字符集合中第一个出现的索引，没找到则返回 -1
        /// </summary>
        /// <param name="pText">字符串</param>
        /// <param name="chars">字符集合</param>
        /// <param name="begin">开始查找的位置。</param>
        /// <param name="end">结束查找的位置，不包含此值。</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* pText, char[] chars, int begin, int end)
        {
            while (begin < end)
            {
                for (int i = 0; i < chars.Length; ++i)
                {
                    if (pText[begin] == chars[i])
                    {
                        return begin;
                    }
                }

                ++begin;
            }

            return -1;
        }
    }
}