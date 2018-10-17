using System;
using System.Collections.Generic;

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
        /// 判断一个数字是否为素数
        /// </summary>
        /// <param name="candidate">数字</param>
        /// <returns>返回一个 bool 值。</returns>
        private static bool IsPrime(int candidate)
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

            return min;
        }

        /// <summary>
        /// 筛选数组元素
        /// </summary>
        /// <typeparam name="TIn">输入数组类型</typeparam>
        /// <typeparam name="TOut">输出数组类型</typeparam>
        /// <param name="InArray">输入数组</param>
        /// <param name="Filter">输入数组筛选器</param>
        /// <param name="AsValue">输入数组元素转输出数组元素委托</param>
        /// <returns>返回一个新的数组</returns>
        public static TOut[] Filter<TIn, TOut>(TIn[] InArray, Func<TIn, bool> Filter, Func<TIn, TOut> AsValue)
        {
            TOut[] NewArray = new TOut[InArray.Length];

            int ResultLength = 0;

            foreach (var Item in InArray)
            {
                if (Filter(Item))
                {
                    NewArray[ResultLength] = AsValue(Item);

                    ++ResultLength;
                }
            }

            if (ResultLength != InArray.Length)
            {
                Array.Resize(ref NewArray, ResultLength);
            }

            return NewArray;
        }

        /// <summary>
        /// 筛选数组元素
        /// </summary>
        /// <typeparam name="TIn">输入数组类型</typeparam>
        /// <typeparam name="TOut">输出数组类型</typeparam>
        /// <param name="InSource">输入源</param>
        /// <param name="Filter">输入数组筛选器</param>
        /// <param name="AsValue">输入数组元素转输出数组元素委托</param>
        /// <returns>返回一个新的数组</returns>
        public static TOut[] Filter<TIn, TOut>(IEnumerable<TIn> InSource, Func<TIn, bool> Filter, Func<TIn, TOut> AsValue)
        {
            int Length = 0;

            foreach (var Item in InSource)
            {
                ++Length;
            }

            TOut[] NewArray = new TOut[Length];

            int ResultLength = 0;

            foreach (var Item in InSource)
            {
                if (Filter(Item))
                {
                    NewArray[ResultLength] = AsValue(Item);

                    ++ResultLength;
                }
            }

            if (ResultLength != NewArray.Length)
            {
                Array.Resize(ref NewArray, ResultLength);
            }

            return NewArray;
        }
    }
}