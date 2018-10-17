using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.VirtualViews
{
    /// <summary>
    /// 提供虚拟视图的扩展方法。
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// 将迭代器中的元素复制到空间足够的数组中。
        /// </summary>
        /// <typeparam name="TElement">迭代器元素</typeparam>
        /// <param name="elements">迭代器</param>
        /// <param name="array">空间足够的数组</param>
        /// <param name="arrayIndex">开始写入的位置</param>
        public static void CopyTo<TElement>(this IEnumerable<TElement> elements, TElement[] array, int arrayIndex)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));
            if (elements == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            foreach (var item in elements)
            {
                array[arrayIndex] = item;

                ++arrayIndex;
            }
        }

        /// <summary>
        /// 将迭代器中的元素复制到空间足够的数组中。
        /// </summary>
        /// <param name="elements">迭代器</param>
        /// <param name="array">空间足够的数组</param>
        /// <param name="arrayIndex">开始写入的位置</param>
        public static void CopyTo(this IEnumerable elements, Array array, int arrayIndex)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));
            if (elements == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            foreach (var item in elements)
            {
                array.SetValue(item, arrayIndex);

                ++arrayIndex;
            }
        }
    }
}
