using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供对数组和集合操作的方法。
    /// </summary>
    public unsafe static class ArrayHelper
    {
        private static readonly int[] Primes;
        private const int HashPrime = 101;


        /// <summary>
        /// 最大素数
        /// </summary>
        public const int MaxPrimeArrayLength = 0x7FEFFFFD;

        static ArrayHelper()
        {
            Primes = new int[] { 3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239,
                293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049,
                4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627,
                52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449,
                389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191,
                2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 };
        }

        /// <summary>
        /// 获取在 Int32 范围内大于指定值的最小素数。
        /// </summary>
        /// <param name="min">数字</param>
        /// <returns>返回一个 int 值。</returns>
        public static int GetPrime(int min)
        {
            for (int i = 0; i < Primes.Length; i++)
            {
                int prime = Primes[i];
                if (prime >= min) return prime;
            }

            for (int i = (min | 1); i < int.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % HashPrime != 0))
                    return i;
            }

            // 判断一个数字是否为素数。
            bool IsPrime(int candidate)
            {
                if ((candidate & 1) != 0)
                {
                    int limit = (int)Math.Sqrt(candidate);
                    for (int divisor = 3; divisor <= limit; divisor += 2)
                    {
                        if ((candidate % divisor) == 0)
                            return false;
                    }
                    return true;
                }

                return (candidate == 2);
            }

            return min;
        }

        /// <summary>
        /// 筛选数组元素
        /// </summary>
        /// <typeparam name="TIn">输入数组类型</typeparam>
        /// <typeparam name="TOut">输出数组类型</typeparam>
        /// <param name="input">输入数组</param>
        /// <param name="filter">输入数组筛选器</param>
        /// <param name="asFunc">输入数组元素转输出数组元素委托</param>
        /// <returns>返回一个新的数组</returns>
        public static TOut[] Filter<TIn, TOut>(TIn[] input, Func<TIn, bool> filter, Func<TIn, TOut> asFunc)
        {
            var array = new TOut[input.Length];

            int length = 0;

            foreach (var Item in input)
            {
                if (filter(Item))
                {
                    array[length] = asFunc(Item);

                    ++length;
                }
            }

            if (length != input.Length)
            {
                Array.Resize(ref array, length);
            }

            return array;
        }

        /// <summary>
        /// 筛选数组元素
        /// </summary>
        /// <typeparam name="TIn">输入数组类型</typeparam>
        /// <typeparam name="TOut">输出数组类型</typeparam>
        /// <param name="input">输入源</param>
        /// <param name="filter">输入数组筛选器</param>
        /// <param name="asFunc">输入数组元素转输出数组元素委托</param>
        /// <returns>返回一个新的数组</returns>
        public static TOut[] Filter<TIn, TOut>(IEnumerable<TIn> input, Func<TIn, bool> filter, Func<TIn, TOut> asFunc)
        {
            var list = new List<TOut>();

            foreach (var item in input)
            {
                if (filter(item))
                {
                    list.Add(asFunc(item));
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 复制集合元素到数组中。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="array">数组</param>
        /// <param name="arrayIndex">数组起始索引</param>
        public static void CopyTo<T>(IEnumerable<T> collection, T[] array, int arrayIndex)
        {
            foreach (var item in collection)
            {
                array[arrayIndex] = item;

                ++arrayIndex;
            }
        }

        /// <summary>
        /// 复制集合元素到数组中。
        /// </summary>
        /// <param name="collection">集合</param>
        /// <param name="array">数组</param>
        /// <param name="arrayIndex">数组起始索引</param>
        public static void CopyTo<T>(IEnumerable<T> collection, Array array, int arrayIndex)
        {
            if (array is T[] tArray)
            {
                CopyTo(collection, tArray, arrayIndex);

                return;
            }

            foreach (var item in collection)
            {
                array.SetValue(item, arrayIndex);

                ++arrayIndex;
            }
        }

        /// <summary>
        /// 创建 Int32 范围迭代器。
        /// </summary>
        /// <param name="start">起始值（包含）</param>
        /// <param name="end">结束值（不包含）</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<int> CreateRangeIterator(int start, int end)
        {
            while (start < end)
            {
                yield return start;
            }
        }

        /// <summary>
        /// 创建 Int32 长度迭代器。
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<int> CreateLengthIterator(int length) => CreateRangeIterator(0, length);

        /// <summary>
        /// 创建 String 系统数据读取器的字段名称迭代器。
        /// </summary>
        /// <param name="dbDataReader">系统数据读取器</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<string> CreateNamesIterator(DbDataReader dbDataReader)
        {
            var length = dbDataReader.FieldCount;

            for (int i = 0; i < length; i++)
            {
                yield return dbDataReader.GetName(i);
            }
        }

        /// <summary>
        /// 创建 String 表格的字段名称迭代器。
        /// </summary>
        /// <param name="dataTable">表格</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<string> CreateNamesIterator(DataTable dataTable)
        {
            foreach (DataColumn item in dataTable.Columns)
            {
                yield return item.ColumnName;
            }
        }

        /// <summary>
        /// 创建数组的迭代器。
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<T> CreateArrayIterator<T>(T[] array)
        {
            var length = array.Length;

            for (int i = 0; i < length; i++)
            {
                yield return array[i];
            }
        }

        /// <summary>
        /// 创建 XConvert 类型转换迭代器。
        /// </summary>
        /// <typeparam name="TIn">输入类型</typeparam>
        /// <typeparam name="TOut">输出类型</typeparam>
        /// <param name="input">输入迭代器</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<TOut> CreateAsIterator<TIn, TOut>(IEnumerable<TIn> input)
        {
            foreach (var item in input)
            {
                yield return XConvert<TIn, TOut>.Convert(item);
            }
        }
    }
}