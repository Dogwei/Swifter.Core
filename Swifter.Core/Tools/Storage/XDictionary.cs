using Swifter.VirtualViews;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Swifter.Tools
{
    /// <summary>
    /// 参数字典的初始化配置
    /// </summary>
    public enum XDictionaryOption
    {
        /// <summary>
        /// 默认值，无配置
        /// </summary>
        None = 0x0,
        /// <summary>
        /// 允许 Null Key
        /// </summary>
        AllowNullKey = 0x1,
        /// <summary>
        /// 允许 Key 重复
        /// </summary>
        AllowRepeat = 0x2
    }

    /// <summary>
    /// 参数字典
    /// </summary>
    /// <typeparam name="TKey">字典 Key 的类型</typeparam>
    /// <typeparam name="TValue">字典 Value 的类型</typeparam>
    [Serializable]

    public sealed partial class XDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IDictionary {
        private const int InitializeSize = 3; // 初始数组大小
        private const int UnUsedHashCode = -1; // 未使用的实体 HashCode
        private const int FreedHashCode = -2; // 移除的实体 HashCode

        private int[] buckets; // Hash 值与元素索引的映射 -1 表示未使用
        private Entity[] entries; // 元素集合 无序
        private int count; // 已保存元素数量
        private int freed; // 已释放的空间 Index。 -1 表示无

        private readonly XDictionaryOption option;

        private readonly IEqualityComparer<TKey> keyComparer;

        /// <summary>
        /// 字典中保存的元素
        /// </summary>
        [Serializable]
        private struct Entity
        {
            public int hashCode; // Key 的 Hash 值 -1 表示未使用 -2表示已释放
            public int next; // 与该 Hash 值相同的其他元素的索引
            public TKey key; // 键
            public TValue value; // 值
        }


        /// <summary>
        /// 使用默认参数初始化字典
        /// </summary>
        public XDictionary()
        {
            keyComparer = EqualityComparer<TKey>.Default;

            freed = -1;
            Resize(InitializeSize);
        }

        /// <summary>
        /// 使用默认参数初始化字典，并指定初始字典容量
        /// </summary>
        /// <param name="Size">初始容量</param>
        public XDictionary(int Size)
        {
            keyComparer = EqualityComparer<TKey>.Default;

            freed = -1;
            Resize(Size);
        }

        /// <summary>
        /// 获取元素数量。
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// 指定字典配置参数和初始容量初始化字典
        /// </summary>
        /// <param name="Size">初始容量</param>
        /// <param name="option">配置参数</param>
        public XDictionary(int Size, XDictionaryOption option)
            : this(Size)
        {
            this.option = option;
        }

        /// <summary>
        /// 使用默认初始容量和指定配置参数初始字典
        /// </summary>
        /// <param name="option">配置参数</param>
        public XDictionary(XDictionaryOption option)
            : this()
        {
            this.option = option;
        }

        /// <summary>
        /// 使用默认初始容量和指定配置参数初始字典。
        /// </summary>
        /// <param name="option">配置参数</param>
        /// <param name="keyComparer">Key 比较器</param>
        public XDictionary(XDictionaryOption option, IEqualityComparer<TKey> keyComparer)
            : this()
        {
            this.keyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));
            this.option = option;
        }

        /// <summary>
        /// 获得指定键的哈希值，如果该键为空，并且允许配置允许空键，则返回 0;不允许空值则发生异常。
        /// </summary>
        /// <param name="key">需要获取哈希值得键对象</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int GetHashCode(TKey key)
        {
            return keyComparer.GetHashCode(key) & int.MaxValue;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool EqualKey(TKey before, TKey after)
        {
            return keyComparer.Equals(before, after);
        }

        /// <summary>
        /// 获取或设置指定键对应的值。
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="KeyNotFoundException">键不存在将引发异常</exception>
        /// <returns>返回值</returns>
        public TValue this[TKey key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                int hashCode = GetHashCode(key);
                int last, i = LastEntry(key, out last, hashCode);

                if (i >= 0)
                {
                    return entries[i].value;
                }

                throw new KeyNotFoundException();
            }
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            set
            {
                Insert(key, value, false);
            }
        }

        /// <summary>
        /// 获取指定索引处的元素。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public KeyValuePair<TKey, TValue> GetEntry(int index)
        {
            if (index >= 0 && index < count)
            {
                if (freed != -1)
                {
                    Defragment();

                    ReBuckets();
                }

                return new KeyValuePair<TKey, TValue>(entries[index].key, entries[index].value);
            }

            throw new IndexOutOfRangeException("Index");
        }

        /// <summary>
        /// 尝试获取指定键对应的值。
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">返回值</param>
        /// <returns>返回是否存在该键</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            int hashCode = GetHashCode(key);
            int last, i = LastEntry(key, out last, hashCode);
            if (i >= 0)
            {
                value = entries[i].value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        private int FirstEntry(TKey key, out int last, int hashCode)
        {
            last = -1;
            if (count != 0)
            {
                bool isFind = false;
                int index = -1;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; last = index, index = i, i = entries[i].next)
                {
                    if (entries[i].hashCode == hashCode && EqualKey(entries[i].key, key))
                    {
                        isFind = true;
                    }
                    else if (isFind)
                    {
                        return index;
                    }
                }
                if (isFind)
                {
                    return index;
                }
            }
            return -1;
        }

        private int LastEntry(TKey key, out int last, int hashCode)
        {
            last = -1;
            if (count != 0)
            {
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; last = i, i = entries[i].next)
                {
                    if (entries[i].hashCode == hashCode && EqualKey(key, entries[i].key))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (add && freed == -1 && count == entries.Length)
            {
                Resize();
            }

            int hashCode = GetHashCode(key);

            int targetBucket = hashCode % buckets.Length;

            int last = -1;

            int similar = -1;

            for (int i = buckets[targetBucket]; i >= 0; last = i, i = entries[i].next)
            {
                if (entries[i].hashCode == hashCode && EqualKey(entries[i].key, key))
                {
                    if (add)
                    {
                        if ((option & XDictionaryOption.AllowRepeat) == 0)
                        {
                            throw new ArgumentException("Duplicate keys not allowed. -- " + key);
                        }

                        similar = i;
                        break;
                    }
                    else
                    {
                        entries[i].value = value;
                        return;
                    }
                }
            }

            int index;
            if (freed != -1)
            {
                index = freed;
                freed = entries[freed].next;
            }
            else
            {
                if (!add && count == entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % entries.Length;
                }
                index = count;
            }
            entries[index].hashCode = hashCode;
            entries[index].key = key;
            entries[index].value = value;
            if (similar < 0)
            {
                entries[index].next = buckets[targetBucket];
                buckets[targetBucket] = index;
            }
            else if (last < 0)
            {
                entries[index].next = similar;
                buckets[targetBucket] = index;
            }
            else
            {
                entries[last].next = index;
                entries[index].next = similar;
            }

            ++count;
        }

        /// <summary>
        /// 移除集合中最后一个与该键匹配的元素。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回是否有移除元素</returns>
        public bool RemoveLast(TKey key)
        {
            int hashCode = GetHashCode(key);
            int bucket = hashCode % buckets.Length;
            int last, i = LastEntry(key, out last, hashCode);
            if (i != -1)
            {
                if (last < 0)
                {
                    buckets[bucket] = entries[i].next;
                }
                else
                {
                    entries[last].next = entries[i].next;
                }

                entries[i].hashCode = FreedHashCode;
                entries[i].next = freed;
                entries[i].key = default(TKey);
                entries[i].value = default(TValue);
                freed = i;

                --count;

                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除集合中第一个与该键匹配的元素。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回是否有移除元素</returns>
        public bool RemoveFirst(TKey key)
        {
            int hashCode = GetHashCode(key);
            int bucket = hashCode % buckets.Length;
            int last, i = FirstEntry(key, out last, hashCode);
            if (i != -1)
            {
                if (last < 0)
                {
                    buckets[bucket] = entries[i].next;
                }
                else
                {
                    entries[last].next = entries[i].next;
                }

                entries[i].hashCode = FreedHashCode;
                entries[i].next = freed;
                entries[i].key = default(TKey);
                entries[i].value = default(TValue);
                freed = i;

                --count;

                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除集合中所有与该键匹配的元素。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回是否有移除元素</returns>
        public bool RemoveAll(TKey key)
        {
            if (count != 0)
            {
                int hashCode = GetHashCode(key);
                int bucket = hashCode % buckets.Length;
                int last = -1;
                int next = -1;
                bool isFind = false;
                for (int i = buckets[bucket]; i >= 0; i = next)
                {
                    next = entries[i].next;
                    if (entries[i].hashCode == hashCode && EqualKey(entries[i].key, key))
                    {
                        isFind = true;
                        entries[i].hashCode = FreedHashCode;
                        entries[i].next = freed;
                        entries[i].key = default(TKey);
                        entries[i].value = default(TValue);
                        freed = i;

                        --count;
                    }
                    else if (isFind)
                    {
                        last = i;
                    }
                    else
                    {
                        break;
                    }
                }
                if (isFind)
                {
                    if (last < 0)
                    {
                        buckets[bucket] = next;
                    }
                    else
                    {
                        entries[last].next = next;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 找到第一个与该键匹配的元素的索引。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回索引，不存在则返回 -1。</returns>
        public int FindFirstIndex(TKey key)
        {
            int hashCode = GetHashCode(key);
            int last, i = FirstEntry(key, out last, hashCode);
            return i;
        }

        /// <summary>
        /// 找到最后一个与该键匹配的元素的索引。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回索引，不存在则返回 -1。</returns>
        public int FindLastIndex(TKey key)
        {
            int hashCode = GetHashCode(key);
            int last, i = LastEntry(key, out last, hashCode);
            return i;
        }

        private void Resize()
        {
            int newSize = count * 2;

            unchecked
            {
                if ((uint)newSize > ArrayHelper.MaxPrimeArrayLength && ArrayHelper.MaxPrimeArrayLength > count)
                {
                    Resize(ArrayHelper.MaxPrimeArrayLength);
                }
                else
                {
                    Resize(ArrayHelper.GetPrime(newSize));
                }
            }
        }

        private void Resize(int newSize)
        {
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++)
            {
                newBuckets[i] = -1;
            }


            Entity[] newEntries = entries;
            Array.Resize(ref newEntries, newSize);

            int count = Math.Min(entries == null ? 0 : entries.Length, newSize);
            int index = 0;

            for (int i = 0; i < count; i++)
            {
                switch (newEntries[i].hashCode)
                {
                    case UnUsedHashCode:
                        goto Return;
                    case FreedHashCode:
                        break;
                    default:
                        if (index != i)
                        {
                            newEntries[index] = newEntries[i];
                        }

                        int bucket = newEntries[index].hashCode % newSize;
                        newEntries[index].next = newBuckets[bucket];
                        newBuckets[bucket] = index;

                        ++index;
                        break;
                }
            }

            Return:
            for (; index < newSize; index++)
            {
                newEntries[index].hashCode = UnUsedHashCode;
            }

            buckets = newBuckets;
            entries = newEntries;
            freed = -1;
        }

        private void Defragment()
        {
            freed = -1;

            for (int i = 0, j = 0; i < entries.Length; i++)
            {
                switch (entries[i].hashCode)
                {
                    case UnUsedHashCode:
                        return;
                    case FreedHashCode:
                        break;
                    default:
                        if (j != i)
                        {
                            entries[j] = entries[i];
                        }
                        ++j;
                        break;
                }
            }
        }

        private void ReBuckets()
        {
            for (int i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    int bucket = entries[i].hashCode % buckets.Length;
                    entries[i].next = buckets[bucket];
                    buckets[bucket] = i;
                }
            }
        }

        /// <summary>
        /// 整理元素并固定集合大小。
        /// </summary>
        public void Fixed()
        {
            Defragment();
            Resize(count);
        }

        /// <summary>
        /// 排序集合。
        /// </summary>
        /// <param name="comparer">元素比较器</param>
        public void Sort(IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            Defragment();

            Array.Sort(entries, 0, count, new ComparerSort(comparer));

            ReBuckets();
        }

        /// <summary>
        /// 排序集合。
        /// </summary>
        /// <param name="comparer">元素比较器</param>
        public void Sort(Comparison<KeyValuePair<TKey, TValue>> comparer)
        {
            Defragment();

            Array.Sort(entries, 0, count, new ComparisonSort(comparer));

            ReBuckets();
        }

        /// <summary>
        /// 添加一个键值对。
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        /// <summary>
        /// 获取迭代器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// 获取该键所有的值集合。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回值集合</returns>
        public ValuesCollection GetValues(TKey key)
        {
            return new ValuesCollection(this, key);
        }

        /// <summary>
        /// 遍历字典元素
        /// </summary>
        /// <param name="Key">指定 Key。</param>
        /// <param name="function">回掉函数，当此回掉函数返回 true 时则继续遍历，返回 false 则停止。</param>
        /// <returns>如果是因为回掉函数返回 false 而终止遍历的返回该元素的索引，否则返回 -1</returns>
        public int Each(TKey Key, Func<TValue, bool> function)
        {
            int HashCode = GetHashCode(Key);

            for (int i = buckets[HashCode % buckets.Length]; i != -1; i = entries[i].next)
            {
                if (entries[i].hashCode == HashCode && EqualKey(Key, entries[i].key))
                {
                    if (!function(entries[i].value))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// 遍历字典元素
        /// </summary>
        /// <param name="function">回掉函数，当此回掉函数返回 true 时则继续遍历，返回 false 则停止。</param>
        /// <returns>如果是因为回掉函数返回 false 而终止遍历的返回该元素的索引，否则返回 -1</returns>
        public int Each(Func<TKey, TValue, bool> function)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].hashCode == UnUsedHashCode)
                {
                    break;
                }
                else if (entries[i].hashCode != FreedHashCode && !function(entries[i].key, entries[i].value))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 清空集合。
        /// </summary>
        public void Clear()
        {
            if (count > 0)
            {
                for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
                Array.Clear(entries, 0, count);
                freed = -1;
                count = 0;
            }
        }

        /// <summary>
        /// 判断是否存在该键。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回否存在该键</returns>
        public bool ContainsKey(TKey key)
        {
            int hashCode = GetHashCode(key);
            int last, i = LastEntry(key, out last, hashCode);
            return i >= 0;
        }

        /// <summary>
        /// 获取键的集合。
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                return new AsEnumerate<KeyValuePair<TKey, TValue>, TKey>(this, item => item.Key);
            }
        }

        /// <summary>
        /// 获取值的集合。
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                return new AsEnumerate<KeyValuePair<TKey, TValue>, TValue>(this, item => item.Value);
            }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return RemoveAll(key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            var ValueComparer = EqualityComparer<TValue>.Default;

            return Each(item.Key, (Value) =>
            {
                return !ValueComparer.Equals(Value, item.Value);

            }) != -1;
        }

        /// <summary>
        /// 将元素集合复制到空间足够的目标数组中。
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="arrayIndex">开始写入的位置</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new NullReferenceException("Array");
            }

            if (array.Rank != 0)
            {
                throw new ArgumentException("Rank");
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new IndexOutOfRangeException("ArrayIndex");
            }

            if (array.Length - arrayIndex < count)
            {
                throw new IndexOutOfRangeException("ArrayIndex");
            }

            Each((key, value) =>
            {
                array[arrayIndex] = new KeyValuePair<TKey, TValue>(key, value);

                ++arrayIndex;

                return true;
            });
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            var ValueComparer = EqualityComparer<TValue>.Default;

            int HashCode = GetHashCode(item.Key);

            for (int i = buckets[HashCode % buckets.Length], LastIndex = -1; i != -1; LastIndex = i, i = entries[i].next)
            {
                if (entries[i].hashCode == HashCode && EqualKey(item.Key, entries[i].key) && ValueComparer.Equals(item.Value, entries[i].value))
                {
                    int TargetBucket = HashCode % buckets.Length;

                    if (LastIndex == -1)
                    {
                        buckets[TargetBucket] = entries[i].next;
                    }
                    else
                    {
                        entries[LastIndex].next = entries[i].next;
                    }

                    entries[i].hashCode = FreedHashCode;
                    entries[i].next = freed;
                    entries[i].key = default(TKey);
                    entries[i].value = default(TValue);
                    freed = i;

                    --count;

                    return true;
                }
            }

            return false;
        }

        bool IDictionary.Contains(object key)
        {
            return ContainsKey((TKey)key);
        }

        void IDictionary.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        void IDictionary.Clear()
        {
            Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this);
        }

        void IDictionary.Remove(object key)
        {
            RemoveAll((TKey)key);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new NullReferenceException("Array");
            }

            if (array.Rank != 0)
            {
                throw new ArgumentException("Rank");
            }

            if (index < 0 || index >= array.Length)
            {
                throw new IndexOutOfRangeException("Index");
            }

            if (array.Length - index < count)
            {
                throw new IndexOutOfRangeException("Index");
            }

            if (array is KeyValuePair<TKey, TValue>[])
            {
                CopyTo((KeyValuePair<TKey, TValue>[])array, index);
            }
            else if (array is DictionaryEntry[])
            {
                var EntryArray = (DictionaryEntry[])array;

                var Enumerator = (IDictionaryEnumerator)new Enumerator(this);

                while (Enumerator.MoveNext())
                {
                    EntryArray[index] = Enumerator.Entry;

                    ++index;
                }
            }
            else
            {
                object[] Objects = array as object[];

                if (Objects == null)
                {
                    throw new ArgumentException("InvalidArrayType");
                }

                Each((Key, Value) =>
                {
                    Objects[index] = new KeyValuePair<TKey, TValue>(Key, Value);

                    ++index;

                    return true;
                });
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return new AsEnumerate<KeyValuePair<TKey, TValue>, TKey>(this, item => item.Key);
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return new AsEnumerate<KeyValuePair<TKey, TValue>, TValue>(this, item => item.Value);
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        int ICollection.Count
        {
            get
            {
                return count;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 值的集合。
        /// </summary>
        public sealed class ValuesCollection : IEnumerable<TValue>
        {
            private XDictionary<TKey, TValue> content;
            private int hashCode;
            private TKey key;
            private int length;

            internal ValuesCollection(XDictionary<TKey, TValue> content, TKey key)
            {
                this.content = content;
                this.key = key;
                hashCode = content.GetHashCode(key);
                length = -1;
            }

            /// <summary>
            /// 值的数量
            /// </summary>
            public int Length
            {
                get
                {
                    if (length == -1)
                    {
                        var Enumerator = GetEnumerator();

                        length = 0;

                        while (Enumerator.MoveNext())
                        {
                            ++length;
                        }
                    }

                    return length;
                }
            }

            /// <summary>
            /// 获取迭代器。
            /// </summary>
            /// <returns></returns>
            public IEnumerator<TValue> GetEnumerator()
            {
                return new Enumerator(this);
            }

            /// <summary>
            /// 转换为数组。
            /// </summary>
            /// <returns></returns>
            public TValue[] ToArray()
            {
                TValue[] Result = new TValue[Length];

                var Enumerator = GetEnumerator();

                for (int i = 0; i < Result.Length; i++)
                {
                    Enumerator.MoveNext();
                    Result[i] = Enumerator.Current;
                }

                return Result;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private sealed class Enumerator : IEnumerator<TValue>
            {
                public Enumerator(ValuesCollection values)
                {
                    content = values.content;
                    hashCode = values.hashCode;
                    key = values.key;

                    index = -1;
                }

                private readonly XDictionary<TKey, TValue> content;
                private readonly int hashCode;
                private readonly TKey key;

                private int index;

                public TValue Current
                {
                    get
                    {
                        return content.entries[index].value;
                    }
                }

                object IEnumerator.Current { get { return Current; } }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    var Entries = content.entries;

                    if (index == -1)
                    {
                        var Buckets = content.buckets;

                        for (index = Buckets[hashCode % Buckets.Length]; index >= 0; index = Entries[index].next)
                        {
                            if (Entries[index].hashCode == hashCode && content.EqualKey(key, Entries[index].key))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        index = content.entries[index].next;
                    }

                    if (index == -1)
                    {
                        return false;
                    }

                    if (Entries[index].hashCode == hashCode && content.EqualKey(key, Entries[index].key))
                    {
                        return true;
                    }

                    return false;
                }

                public void Reset()
                {
                    index = -1;
                }
            }
        }

        private sealed class ComparerSort : IComparer<Entity>
        {
            private readonly IComparer<KeyValuePair<TKey, TValue>> comparer;

            public ComparerSort(IComparer<KeyValuePair<TKey, TValue>> comparer)
            {
                this.comparer = comparer;
            }

            public int Compare(Entity x, Entity y)
            {
                return comparer.Compare(
                    new KeyValuePair<TKey, TValue>(x.key, x.value),
                    new KeyValuePair<TKey, TValue>(y.key, y.value));
            }
        }

        private sealed class ComparisonSort : IComparer<Entity>
        {
            private readonly Comparison<KeyValuePair<TKey, TValue>> comparer;

            public ComparisonSort(Comparison<KeyValuePair<TKey, TValue>> comparer)
            {
                this.comparer = comparer;
            }

            public int Compare(Entity x, Entity y)
            {
                return comparer(
                    new KeyValuePair<TKey, TValue>(x.key, x.value),
                    new KeyValuePair<TKey, TValue>(y.key, y.value));
            }
        }

        /// <summary>
        /// 参数字典迭代器
        /// </summary>
        private sealed class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly XDictionary<TKey, TValue> content;
            private int index;
            private int number;

            public Enumerator(XDictionary<TKey, TValue> content)
            {
                this.content = content;

                number = 0;
                index = -1;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(content.entries[index].key, content.entries[index].value);
                }
            }

            public void Dispose() { }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    return content.entries[index].key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    return content.entries[index].value;
                }
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    return new DictionaryEntry(content.entries[index].key, content.entries[index].value);
                }
            }

            public bool MoveNext()
            {
                ++index;

                while (number < content.count)
                {
                    switch (content.entries[index].hashCode)
                    {
                        case UnUsedHashCode:
                            return false;
                        case FreedHashCode:
                            ++index;
                            break;
                        default:
                            ++number;
                            return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                number = 0;
                index = -1;
            }
        }
    }
}