using Swifter.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.VirtualViews
{
    /// <summary>
    /// 元素类型转换迭代器。
    /// </summary>
    /// <typeparam name="TSource">原元素类型</typeparam>
    /// <typeparam name="TDestination">目标元素类型</typeparam>
    public sealed class AsEnumerate<TSource, TDestination> : IEnumerable<TDestination>, IEnumerator<TDestination>, ICollection<TDestination>, ICollection
    {
        private readonly IEnumerable<TSource> enumerable;
        private readonly IEnumerator<TSource> enumerator;
        private readonly IAsValue asValue;

        /// <summary>
        /// 创建默认转换的迭代器。
        /// </summary>
        /// <param name="enumerable">原迭代器</param>
        public AsEnumerate(IEnumerable<TSource> enumerable) : this(enumerable, new DefaultAsValue())
        {
        }

        /// <summary>
        /// 创建指定转换的迭代器。
        /// </summary>
        /// <param name="enumerable">原迭代器</param>
        /// <param name="asValue">转换接口实现</param>
        public AsEnumerate(IEnumerable<TSource> enumerable, IAsValue asValue)
        {
            this.enumerable = enumerable;

            this.asValue = asValue;
        }

        /// <summary>
        /// 创建指定转换的迭代器。
        /// </summary>
        /// <param name="enumerable">原迭代器</param>
        /// <param name="asValue">转换委托</param>
        public AsEnumerate(IEnumerable<TSource> enumerable, Func<TSource, TDestination> asValue)
        {
            this.enumerable = enumerable;

            this.asValue = new DelegateAsValue(asValue);
        }

        private AsEnumerate(IEnumerator<TSource> enumerator, IAsValue asValue)
        {
            this.enumerator = enumerator;

            this.asValue = asValue;
        }

        TDestination IEnumerator<TDestination>.Current => asValue.AsValue(enumerator.Current);

        object IEnumerator.Current => asValue.AsValue(enumerator.Current);

        int ICollection<TDestination>.Count => enumerable.Count();

        int ICollection.Count => enumerable.Count();

        bool ICollection<TDestination>.IsReadOnly => true;

        object ICollection.SyncRoot => null;

        bool ICollection.IsSynchronized => false;

        void ICollection<TDestination>.Add(TDestination item)
        {
            throw new NotSupportedException();
        }

        void ICollection<TDestination>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<TDestination>.Contains(TDestination item)
        {
            return this.Contains(item);
        }

        void ICollection<TDestination>.CopyTo(TDestination[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo(array, index);
        }

        void IDisposable.Dispose()
        {
            enumerator.Dispose();
        }

        IEnumerator<TDestination> IEnumerable<TDestination>.GetEnumerator()
        {
            return new AsEnumerate<TSource, TDestination>(enumerable.GetEnumerator(), asValue);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AsEnumerate<TSource, TDestination>(enumerable.GetEnumerator(), asValue);
        }

        bool IEnumerator.MoveNext()
        {
            return enumerator.MoveNext();
        }

        bool ICollection<TDestination>.Remove(TDestination item)
        {
            throw new NotSupportedException();
        }

        void IEnumerator.Reset()
        {
            enumerator.Reset();
        }

        /// <summary>
        /// 转换接口。
        /// </summary>
        public interface IAsValue
        {
            /// <summary>
            /// 将原元素转换为目标元素的方法。
            /// </summary>
            /// <param name="source">原元素</param>
            /// <returns>返回目标元素</returns>
            TDestination AsValue(TSource source);
        }

        private sealed class DefaultAsValue : IAsValue
        {
            public TDestination AsValue(TSource source)
            {
                return XConvert<TSource, TDestination>.Convert(source);
            }
        }

        private sealed class DelegateAsValue : IAsValue
        {
            public readonly Func<TSource, TDestination> asValue;

            public DelegateAsValue(Func<TSource, TDestination> asValue)
            {
                this.asValue = asValue;
            }

            public TDestination AsValue(TSource source)
            {
                return asValue(source);
            }
        }
    }
}