using Swifter.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.VirtualViews
{
    /// <summary>
    /// 数组视图。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ArrayView<T> : ICollection<T>, IEnumerable<T>, IEnumerator<T>
    {
        /// <summary>
        /// 创建从 0 索引开始具有指定长度的数组的视图。
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个数组视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<T> Create(T[] array, int length)
        {
            return Create(array, 0, length);
        }

        /// <summary>
        /// 创建指定索引和长度的数组的视图。
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="index">指定索引</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个数组视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<T> Create(T[] array, int index, int length)
        {
            return new ArrayView<T>(new _1RankArrayViewImpl<T>(array), index, length + index);
        }

        /// <summary>
        /// 创建指定长度的 List 的视图。
        /// </summary>
        /// <param name="list">List</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个 List 的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<T> Create(List<T> list, int length)
        {
            return Create(list, 0, length);
        }

        /// <summary>
        /// 创建指定索引和长度的 List 的视图。
        /// </summary>
        /// <param name="list">List</param>
        /// <param name="index">指定索引</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个 List 的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<T> Create(List<T> list, int index, int length)
        {
            return new ArrayView<T>(new ListViewImpl<T>(list), index, length + index);
        }

        /// <summary>
        /// 创建指定长度的索引的视图。
        /// </summary>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个索引的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<int> CreateIndexView(int length)
        {
            return CreateIndexView(0, length);
        }

        /// <summary>
        /// 创建指定索引和长度的索引的视图。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个索引的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<int> CreateIndexView(int index, int length)
        {
            return new ArrayView<int>(new IndexViewImpl(), index, index + length);
        }

        /// <summary>
        /// 创建指定长度的索引器的视图。
        /// </summary>
        /// <param name="getValue">索引器</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个索引器的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<T> Create(Func<int, T> getValue, int length)
        {
            return Create(getValue, null, 0, length);
        }

        /// <summary>
        /// 创建指定索引和长度的索引器的视图。
        /// </summary>
        /// <param name="getValue">索引器</param>
        /// <param name="index">指定索引</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个索引器的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<T> Create(Func<int, T> getValue, int index, int length)
        {
            return Create(getValue, null, index, length);
        }

        /// <summary>
        /// 创建指定长度的索引器的视图。
        /// </summary>
        /// <param name="getValue">索引器 get 方法</param>
        /// <param name="setValue">索引器 set 方法</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个索引器的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<T> Create(Func<int, T> getValue, Action<int, T> setValue,  int length)
        {
            return Create(getValue, setValue,0,  length);
        }

        /// <summary>
        /// 创建指定索引和长度的索引器的视图。
        /// </summary>
        /// <param name="getValue">索引器 get 方法</param>
        /// <param name="setValue">索引器 set 方法</param>
        /// <param name="index">指定索引</param>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个索引器的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayView<T> Create(Func<int, T> getValue, Action<int, T> setValue, int index, int length)
        {
            return new ArrayView<T>(new DelegateViewImpl<T>(getValue, setValue), index, index + length);
        }

        private readonly IArrayViewImpl<T> viewImpl;
        private readonly int first;
        private readonly int last;

        private int currentIndex;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int AsRealIndex(int index)
        {
            if (last > first && index >= 0 && (index += first) < last)
            {
                return index;
            }

            if (first > last && index >= 0 && (index = first - index) > last)
            {
                return index;
            }

            throw new IndexOutOfRangeException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int GetCount()
        {
            return last > first ? last - first : first - last;
        }

        /// <summary>
        /// 获取该集合的数量。
        /// </summary>
        public int Count => GetCount();

        /// <summary>
        /// 获取该集合是否为只读。
        /// </summary>
        public bool IsReadOnly => viewImpl.IsReadOnly;

        /// <summary>
        /// 初始化数组视图。
        /// </summary>
        /// <param name="viewImpl"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ArrayView(IArrayViewImpl<T> viewImpl, int first, int last)
        {
            this.viewImpl = viewImpl;
            this.first = first;
            this.last = last;

            currentIndex = -1;
        }

        /// <summary>
        /// 获取或设置索引处的元素。
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>返回该元素</returns>
        public T this[int index]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return viewImpl[AsRealIndex(index)];
            }
            set
            {
                viewImpl[AsRealIndex(index)] = value;
            }
        }

        /// <summary>
        /// 填充视图范围内的元素。
        /// </summary>
        /// <param name="value">元素</param>
        public void Fill(T value)
        {
            for (int i = Count - 1; i >= 0; --i)
            {
                this[i] = value;
            }
        }

        /// <summary>
        /// 清空视图范围内的元素。
        /// </summary>
        public void Clear()
        {
            Fill(default(T));
        }

        /// <summary>
        /// 使用 XConvert 转换视图。
        /// </summary>
        /// <typeparam name="TD">目标元素类型</typeparam>
        /// <returns>返回新的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ArrayView<TD> Convert<TD>()
        {
            return new ArrayView<TD>(new ConvertViewImpl<T, TD>(viewImpl), first, last);
        }

        /// <summary>
        /// 反转视图。
        /// </summary>
        /// <returns>返回新的视图</returns>
        public ArrayView<T> Reverse()
        {
            return new ArrayView<T>(viewImpl, last - 1, first - 1);
        }

        /// <summary>
        /// 切割视图。
        /// </summary>
        /// <param name="index">切割索引</param>
        /// <returns>返回新的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ArrayView<T> Slice(int index)
        {
            return Slice(index, Count - index);
        }

        /// <summary>
        /// 切割视图。
        /// </summary>
        /// <param name="index">切割索引</param>
        /// <param name="length">切割长度</param>
        /// <returns>返回新的视图</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ArrayView<T> Slice(int index, int length)
        {
            if (index < 0 || length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            int count = Count;

            length = Math.Min(count - index, length);

            if (index >= count || length <= 0)
            {
                return new ArrayView<T>(viewImpl, 0, 0);
            }

            if (last > first)
            {
                return new ArrayView<T>(viewImpl, first + index, first + index + length);
            }

            return new ArrayView<T>(viewImpl, first - index, first - index - length);
        }

        /// <summary>
        /// 将视图元素 Copy 到空间足够的数组中。
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="arrayIndex">开始写入的位置</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            int i = Count - 1;
            arrayIndex += i;

            for (; i >= 0; --i, --arrayIndex)
            {
                array[arrayIndex] = this[i];
            }
        }

        /// <summary>
        /// 将视图转换为数组。
        /// </summary>
        /// <returns>返回数组。</returns>
        public T[] ToArray()
        {
            var result = new T[Count];

            CopyTo(result, 0);

            return result;
        }

        T IEnumerator<T>.Current => this[currentIndex];

        object IEnumerator.Current => this[currentIndex];

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new ArrayView<T>(viewImpl, first, last);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArrayView<T>(viewImpl, first, last);
        }

        bool ICollection<T>.Contains(T item)
        {
            for (int i = Count - 1; i >= 0; --i)
            {
                if (item == null ? this[i] == null : item.Equals(this[i]))
                {
                    return true;
                }
            }

            return false;
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        void IDisposable.Dispose()
        {
        }

        bool IEnumerator.MoveNext()
        {
            return (++currentIndex) < Count;
        }

        void IEnumerator.Reset()
        {
            currentIndex = -1;
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 视图数据源实现。
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public interface IArrayViewImpl<T>
    {
        /// <summary>
        /// 获取或设置索引处的元素。
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>返回该元素</returns>
        T this[int index] { get; set; }

        /// <summary>
        /// 获取视图是否为只读。
        /// </summary>
        bool IsReadOnly { get; }
    }

    internal sealed class _1RankArrayViewImpl<T> : IArrayViewImpl<T>
    {
        public readonly T[] content;

        public _1RankArrayViewImpl(T[] content)
        {
            this.content = content;
        }

        public T this[int index] { get => content[index]; set => content[index] = value; }

        public bool IsReadOnly => false;
    }

    internal sealed class ListViewImpl<T> : IArrayViewImpl<T>
    {
        public readonly IList<T> content;

        public ListViewImpl(IList<T> content)
        {
            this.content = content;
        }

        public T this[int index] { get => content[index]; set => content[index] = value; }

        public bool IsReadOnly => content.IsReadOnly;
    }

    internal sealed class IndexViewImpl : IArrayViewImpl<int>
    {
        public int this[int index] { get => index; set => throw new NotSupportedException(); }

        public bool IsReadOnly => true;
    }

    internal sealed class ConvertViewImpl<TS, TD> : IArrayViewImpl<TD>
    {
        public readonly IArrayViewImpl<TS> content;

        public ConvertViewImpl(IArrayViewImpl<TS> content)
        {
            this.content = content;
        }

        public TD this[int index] { get => XConvert<TS, TD>.Convert(content[index]); set => content[index] = XConvert<TD, TS>.Convert(value); }

        public bool IsReadOnly => content.IsReadOnly;
    }

    internal sealed class DelegateViewImpl<T> : IArrayViewImpl<T>
    {
        private readonly Func<int, T> getValue;
        private readonly Action<int, T> setValue;

        public DelegateViewImpl(Func<int, T> getValue, Action<int, T> setValue)
        {
            this.getValue = getValue;
            this.setValue = setValue;
        }

        public T this[int index] { get => getValue(index); set => setValue(index, value); }

        public bool IsReadOnly => setValue == null;
    }
}