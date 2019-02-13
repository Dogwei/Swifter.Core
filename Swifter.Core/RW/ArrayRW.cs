using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal abstract class ArrayRW<T> : IDataRW<int>, IDirectContent, IInitialize<T>
    {
        public const int DefaultInitializeSize = 3;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayRW<T> Create()
        {
            return StaticArrayRW<T>.Creater.Create();
        }

        protected T content;
        protected int count;

        public abstract T Content { get; }

        public void Initialize()
        {
            Initialize(DefaultInitializeSize);
        }

        public abstract void Initialize(T content);

        public abstract void Initialize(int capacity);

        public abstract void OnWriteValue(int key, IValueReader valueReader);

        public abstract void OnReadValue(int key, IValueWriter valueWriter);

        public abstract void OnReadAll(IDataWriter<int> dataWriter);

        public abstract void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter);

        public abstract void OnWriteAll(IDataReader<int> dataReader);

        public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public virtual int Count => count;

        object IDirectContent.DirectContent
        {
            get => Content;
            set => Initialize((T)value);
        }

        public virtual object ReferenceToken => content;

        IValueRW IDataRW<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];
    }

    internal interface IArrayRWCreater<T>
    {
        ArrayRW<T> Create();
    }

    internal static class StaticArrayRW<T>
    {
        public static readonly IArrayRWCreater<T> Creater;

        static StaticArrayRW()
        {
            var type = typeof(T);

            if (type.IsArray)
            {
                int rank = type.GetArrayRank();

                var elementType = type.GetElementType();

                Type internalType;

                switch (rank)
                {
                    case 1:
                        internalType = typeof(OneRankArrayRWCreater<>).MakeGenericType(elementType);
                        break;
                    case 2:
                        internalType = typeof(TwoRankArrayRWCreater<>).MakeGenericType(elementType);
                        break;
                    default:
                        internalType = typeof(MultiRankArrayRWCreater<,>).MakeGenericType(type, elementType);
                        break;
                }

                Creater = (IArrayRWCreater<T>)Activator.CreateInstance(internalType);
            }
            else
            {
                throw new ArgumentException(StringHelper.Format("'{0}' is not a Array type.", typeof(T).Name));
            }
        }
    }

    internal sealed class OneRankArrayRW<T> : ArrayRW<T[]>
    {
        public override void Initialize(int capacity)
        {
            content = new T[capacity];
        }

        public override void Initialize(T[] content)
        {
            this.content = content;

            count = content.Length;
        }

        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key >= content.Length)
            {
                Array.Resize(ref content, (key * 3) + 1);
            }

            count = Math.Max(key + 1, count);

            content[key] = ValueInterface<T>.Content.ReadValue(valueReader);
        }

        public override T[] Content
        {
            get
            {
                if (content == null)
                {
                    return null;
                }

                if (count != content.Length)
                {
                    Array.Resize(ref content, count);
                }

                return content;
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<T>.Content.WriteValue(valueWriter, content[key]);
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = Count;

            for (int i = 0; i < length; ++i)
            {
                ValueInterface<T>.Content.WriteValue(dataWriter[i], content[i]);
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; ++i)
            {
                var value = content[i];

                ValueInterface<T>.Content.WriteValue(valueInfo.ValueCopyer, value);

                valueInfo.Key = i;
                valueInfo.Type = typeof(T);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public override void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                OnWriteValue(i, dataReader[i]);
            }
        }
    }

    internal sealed class OneRankArrayRWCreater<T> : IArrayRWCreater<T[]>
    {
        public ArrayRW<T[]> Create()
        {
            return new OneRankArrayRW<T>();
        }
    }

    internal sealed class TwoRankArrayRW<T> : ArrayRW<T[,]>
    {
        private int rank2Count;

        public override void Initialize(T[,] content)
        {
            this.content = content;

            count = content.GetLength(0);
            rank2Count = content.GetLength(1);
        }

        public override T[,] Content
        {
            get
            {
                if (content == null)
                {
                    return null;
                }

                var old1 = content.GetLength(0);
                var old2 = content.GetLength(1);

                if (old1 != count || old2 != rank2Count)
                {
                    Resize(count, rank2Count);
                }

                return content;
            }
        }

        public override void Initialize(int capacity)
        {
            content = new T[capacity, DefaultInitializeSize];
        }

        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            valueReader.ReadArray(new ChildrenRW(this, key));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void Resize(int len1, int len2)
        {
            var temp = content;

            content = new T[len1, len2];

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < rank2Count; j++)
                {
                    content[i, j] = temp[i, j];
                }
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            valueWriter.WriteArray(new ChildrenRW(this, key));
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = Count;

            for (int i = 0; i < length; ++i)
            {
                dataWriter[i].WriteArray(new ChildrenRW(this, i));
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; ++i)
            {
                valueInfo.ValueCopyer.WriteArray(new ChildrenRW(this, i));

                valueInfo.Key = i;
                valueInfo.Type = typeof(T[]);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public override void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                OnWriteValue(i, dataReader[i]);
            }
        }

        private sealed class ChildrenRW : IDataRW<int>
        {
            private readonly TwoRankArrayRW<T> content;
            private readonly int baseIndex;

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public ChildrenRW(TwoRankArrayRW<T> content, int baseIndex)
            {
                this.content = content;
                this.baseIndex = baseIndex;
            }

            public void Initialize()
            {
                CheckExpansion(DefaultInitializeSize - 1);
            }

            public void Initialize(int capacity)
            {
                // Last Index
                CheckExpansion(capacity - 1);
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            private void CheckExpansion(int index)
            {
                /* 仅在新增时检查扩容 */
                if (baseIndex >= content.count || index >= content.rank2Count)
                {
                    int len1 = content.content.GetLength(0);
                    int len2 = content.content.GetLength(1);

                    if (baseIndex >= len1)
                    {
                        if (index >= len2)
                        {
                            content.Resize(baseIndex + len1 + 1, index + len2 + 1);
                        }
                        else
                        {
                            content.Resize(baseIndex + len1 + 1, len2);
                        }
                    }
                    else if (index >= len2)
                    {
                        content.Resize(len1, index + len2 + 1);
                    }
                }
            }

            public void OnWriteValue(int key, IValueReader valueReader)
            {
                CheckExpansion(key);

                content.content[baseIndex, key] = ValueInterface<T>.Content.ReadValue(valueReader);

                content.count = Math.Max(baseIndex + 1, content.count);

                content.rank2Count = Math.Max(key + 1, content.rank2Count);
            }

            public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

            public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

            public int Count => content.rank2Count;

            public object ReferenceToken => null;

            IValueRW IDataRW<int>.this[int key] => this[key];

            IValueWriter IDataWriter<int>.this[int key] => this[key];

            IValueReader IDataReader<int>.this[int key] => this[key];

            public void OnReadValue(int key, IValueWriter valueWriter)
            {
                ValueInterface<T>.Content.WriteValue(valueWriter, content.content[baseIndex, key]);
            }

            public void OnReadAll(IDataWriter<int> dataWriter)
            {
                int length = Count;

                for (int i = 0; i < length; ++i)
                {
                    ValueInterface<T>.Content.WriteValue(dataWriter[i], content.content[baseIndex, i]);
                }
            }

            public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
            {
                int length = Count;

                var valueInfo = new ValueFilterInfo<int>();

                for (int i = 0; i < length; ++i)
                {
                    var value = content.content[baseIndex, i];

                    ValueInterface<T>.Content.WriteValue(valueInfo.ValueCopyer, value);

                    valueInfo.Key = i;
                    valueInfo.Type = typeof(T);

                    if (valueFilter.Filter(valueInfo))
                    {
                        valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                    }
                }
            }

            public void OnWriteAll(IDataReader<int> dataReader)
            {
                int length = Count;

                for (int i = 0; i < length; ++i)
                {
                    content.content[baseIndex, i] = ValueInterface<T>.Content.ReadValue(dataReader[i]);
                }
            }
        }
    }

    internal sealed class TwoRankArrayRWCreater<T> : IArrayRWCreater<T[,]>
    {
        public ArrayRW<T[,]> Create()
        {
            return new TwoRankArrayRW<T>();
        }
    }

    internal sealed class MultiRankArrayRW<TArray, TValue> : ArrayRW<TArray>
    {
        private static readonly int maxRankIndex;
        private static readonly int rank;

        static MultiRankArrayRW()
        {
            rank = typeof(TArray).GetArrayRank();

            maxRankIndex = rank - 1;
        }

        private readonly int currentRankIndex;
        private readonly int baseIndex;

        private readonly int[] counts;
        private readonly int[] indices;

        private readonly MultiRankArrayRW<TArray, TValue> rootRank;
        private readonly MultiRankArrayRW<TArray, TValue> lastRank;

        public MultiRankArrayRW()
        {
            rootRank = this;

            counts = new int[rank];
            indices = new int[rank];
        }

        internal MultiRankArrayRW(int currentRankIndex, int maxRankIndex, int index, MultiRankArrayRW<TArray, TValue> lastRank)
        {
            this.currentRankIndex = currentRankIndex;
            this.baseIndex = index;
            this.lastRank = lastRank;

            rootRank = lastRank.rootRank;
        }

        public override TArray Content
        {
            get
            {
                var content = (Array)(object)rootRank.content;

                if (content == null)
                {
                    return default;
                }

                int[] counts = rootRank.counts;

                for (int i = maxRankIndex; i >= 0; --i)
                {
                    if (content.GetLength(i) != counts[i])
                    {
                        goto Resize;
                    }
                }

                return rootRank.content;

            Resize:

                Resize(counts);

                return rootRank.content;
            }
        }

        public override int Count => rootRank.counts[currentRankIndex];

        public override object ReferenceToken
        {
            get
            {
                return baseIndex == 0 ? (object)content : null;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void Resize(int[] lengths)
        {
            var temp = (Array)(object)rootRank.content;

            Array content = Array.CreateInstance(typeof(TValue), lengths);

            CopyValues(temp, new int[maxRankIndex + 1], content, rootRank.counts, 0, maxRankIndex);

            rootRank.content = (TArray)(object)content;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void CheckExpansion(int index)
        {
            var content = (Array)(object)rootRank.content;

            int length;

            /* 仅在新增时检查扩容 */
            if (index >= rootRank.counts[currentRankIndex] && index >= (length = content.GetLength(currentRankIndex)))
            {
                var lengths = new int[maxRankIndex + 1];

                for (int i = maxRankIndex; i >= 0; --i)
                {
                    lengths[i] = content.GetLength(i);
                }

                lengths[currentRankIndex] = length + index + 1;

                Resize(lengths);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void CopyValues(Array source, int[] indices, Array destination, int[] counts, int rankIndex, int maxRankIndex)
        {
            int length = counts[rankIndex];

            if (rankIndex == maxRankIndex)
            {
                for (int i = 0; i < length; ++i)
                {
                    indices[rankIndex] = i;

                    SetValue(destination, indices, GetValue(source, indices));
                }
            }
            else
            {
                for (int i = 0; i < length; ++i)
                {
                    indices[rankIndex] = i;

                    CopyValues(source, indices, destination, counts, rankIndex + 1, maxRankIndex);
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static TValue GetValue(object array, int[] indices)
        {
            switch (indices.Length)
            {
                case 1:
                    return ((TValue[])array)[indices[0]];
                case 2:
                    return ((TValue[,])array)[indices[0], indices[1]];
                case 3:
                    return ((TValue[,,])array)[indices[0], indices[1], indices[2]];
                case 4:
                    return ((TValue[,,,])array)[indices[0], indices[1], indices[2], indices[3]];
                case 5:
                    return ((TValue[,,,,])array)[indices[0], indices[1], indices[2], indices[3], indices[4]];
                case 6:
                    return ((TValue[,,,,,])array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5]];
                default:
                    return (TValue)((Array)array).GetValue(indices);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void SetValue(object array, int[] indices, TValue value)
        {
            switch (indices.Length)
            {
                case 1:
                    ((TValue[])array)[indices[0]] = value;
                    break;
                case 2:
                    ((TValue[,])array)[indices[0], indices[1]] = value;
                    break;
                case 3:
                    ((TValue[,,])array)[indices[0], indices[1], indices[2]] = value;
                    break;
                case 4:
                    ((TValue[,,,])array)[indices[0], indices[1], indices[2], indices[3]] = value;
                    break;
                case 5:
                    ((TValue[,,,,])array)[indices[0], indices[1], indices[2], indices[3], indices[4]] = value;
                    break;
                case 6:
                    ((TValue[,,,,,])array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5]] = value;
                    break;
                default:
                    ((Array)array).SetValue(value, indices);
                    break;
            }
        }
        
        public override void Initialize(TArray content)
        {
            rootRank.content = content;

            for (int i = maxRankIndex; i >= 0; --i)
            {
                rootRank.counts[i] = ((Array)(object)content).GetLength(i);
            }
        }

        public override void Initialize(int capacity)
        {
            if (this == rootRank)
            {
                int[] lengths = new int[maxRankIndex + 1];

                for (int i = maxRankIndex; i >= 0; --i)
                {
                    lengths[i] = DefaultInitializeSize;
                }

                lengths[currentRankIndex] = capacity;

                content = (TArray)(object)Array.CreateInstance(typeof(TValue), lengths);

                return;
            }

            CheckExpansion(capacity - 1);
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = rootRank.counts[currentRankIndex];

            if (currentRankIndex == maxRankIndex)
            {
                var indices = GetIndices();

                var content = ((Array)(object)rootRank.content);

                for (int i = 0; i < length; ++i)
                {
                    indices[currentRankIndex] = i;

                    var value = GetValue(content, indices);

                    ValueInterface<TValue>.Content.WriteValue(dataWriter[i], (TValue)value);
                }
            }
            else
            {
                for (int i = 0; i < length; ++i)
                {
                    dataWriter[i].WriteArray(new MultiRankArrayRW<TArray, TValue>(currentRankIndex + 1, maxRankIndex, i, this));
                }
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = rootRank.counts[currentRankIndex];

            var valueInfo = new ValueFilterInfo<int>();

            if (currentRankIndex == maxRankIndex)
            {
                var indices = GetIndices();

                var content = ((Array)(object)rootRank.content);

                for (int i = 0; i < length; ++i)
                {
                    valueInfo.Key = i;
                    valueInfo.Type = typeof(TValue);

                    indices[currentRankIndex] = i;

                    var value = GetValue(content, indices);

                    ValueInterface<TValue>.Content.WriteValue(valueInfo.ValueCopyer, (TValue)value);

                    if (valueFilter.Filter(valueInfo))
                    {
                        valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < length; ++i)
                {
                    valueInfo.Key = i;
                    valueInfo.Type = typeof(Array);

                    valueInfo.ValueCopyer.WriteArray(new MultiRankArrayRW<TArray, TValue>(currentRankIndex + 1, maxRankIndex, i, this));

                    if (valueFilter.Filter(valueInfo))
                    {
                        valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                    }
                }
            }
        }

        public override void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                OnWriteValue(i, dataReader[i]);
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (currentRankIndex == maxRankIndex)
            {
                var indices = GetIndices();

                indices[currentRankIndex] = key;

                var value = GetValue(rootRank.content, indices);

                ValueInterface<TValue>.Content.WriteValue(valueWriter, value);

                return;
            }

            valueWriter.WriteArray(new MultiRankArrayRW<TArray, TValue>(currentRankIndex + 1, maxRankIndex, key, this));
        }
        
        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            CheckExpansion(key);
            
            rootRank.counts[currentRankIndex] = Math.Max(key + 1, rootRank.counts[currentRankIndex]);

            if (currentRankIndex == maxRankIndex)
            {
                var value = ValueInterface<TValue>.Content.ReadValue(valueReader);

                var indices = GetIndices();

                indices[currentRankIndex] = key;

                SetValue(rootRank.content, indices, value);
            }
            else
            {
                valueReader.ReadArray(new MultiRankArrayRW<TArray, TValue>(currentRankIndex + 1, maxRankIndex, key, this));
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal int[] GetIndices()
        {
            var indices = rootRank.indices;

            var rank = this;

            while (rank.lastRank != null)
            {
                var index = rank.baseIndex;

                rank = rank.lastRank;

                indices[rank.currentRankIndex] = index;
            }

            return indices;
        }
    }

    internal sealed class MultiRankArrayRWCreater<TArray, TValue> : IArrayRWCreater<TArray>
    {
        public ArrayRW<TArray> Create()
        {
            return new MultiRankArrayRW<TArray, TValue>();
        }
    }
}