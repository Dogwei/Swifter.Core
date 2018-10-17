using Swifter.Readers;
using Swifter.Tools;
using Swifter.VirtualViews;
using Swifter.Writers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class CollectionRW<T, TValue> : IDataRW<int>, IDirectContent, IInitialize<T> where T : ICollection<TValue>
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content
        {
            get
            {
                return content;
            }
        }

        public ValueCopyer<int> this[int key]
        {
            get
            {
                return new ValueCopyer<int>(this, key);
            }
        }

        IValueWriter IDataWriter<int>.this[int key]
        {
            get
            {
                return this[key];
            }
        }

        IValueReader IDataReader<int>.this[int key]
        {
            get
            {
                return this[key];
            }
        }

        public IEnumerable<int> Keys => ArrayView<int>.CreateIndexView(content.Count);

        public int Count
        {
            get
            {
                return content.Count;
            }
        }

        object IDirectContent.DirectContent
        {
            get
            {
                return content;
            }
            set
            {
                content = (T)value;
            }
        }

        public long ObjectId
        {
            get
            {
                return TypeInfo<T>.IsValueType ? 0 : (long)Pointer.UnBox(content);
            }
        }

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(T content)
        {
            this.content = content;
        }

        public void Initialize(int capacity)
        {
            if (TypeInfo<T>.IsInterface)
            {
                if (typeof(T).IsAssignableFrom(typeof(List<TValue>)))
                {
                    goto List;
                }

                // TODO: Others Interface initialize.
            }

            // TODO: Capacity
            content = Activator.CreateInstance<T>();

            return;

            List:

            content = (T)(object)new List<TValue>(capacity);
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var index = 0;

            foreach (var item in content)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[index], item);

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            throw new NotSupportedException();
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            content.Add(ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            foreach (var item in content)
            {
                ValueInterface<TValue>.Content.WriteValue(valueInfo.ValueCopyer, item);

                valueInfo.Key = index;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        IDataReader<TKey> IDataReader.As<TKey>()
        {
            if (this is IDataReader<TKey>)
            {
                return (IDataReader<TKey>)(object)this;
            }

            return new AsDataReader<int, TKey>(this);
        }

        IDataWriter<TKey> IDataWriter.As<TKey>()
        {
            if (this is IDataWriter<TKey>)
            {
                return (IDataWriter<TKey>)(object)this;
            }

            return new AsDataWriter<int, TKey>(this);
        }
    }

    internal sealed class CollectionRW<T> : IDataRW<int>, IDirectContent where T : ICollection
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content
        {
            get
            {
                return content;
            }
        }

        public ValueCopyer<int> this[int key]
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IValueWriter IDataWriter<int>.this[int key]
        {
            get
            {
                return this[key];
            }
        }

        IValueReader IDataReader<int>.this[int key]
        {
            get
            {
                return this[key];
            }
        }

        public IEnumerable<int> Keys
        {
            get
            {
                return ArrayView<int>.CreateIndexView(content.Count);
            }
        }

        public int Count
        {
            get
            {
                return content.Count;
            }
        }

        object IDirectContent.DirectContent
        {
            get
            {
                return content;
            }
            set
            {
                content = (T)value;
            }
        }

        public long ObjectId
        {
            get
            {
                return TypeInfo<T>.IsValueType ? 0 : (long)Pointer.UnBox(content);
            }
        }

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(T content)
        {
            this.content = content;
        }

        public void Initialize(int capacity)
        {
            if (TypeInfo<T>.IsInterface)
            {
                if (typeof(T).IsAssignableFrom(typeof(ArrayList)))
                {
                    goto Collection;
                }

                // TODO: Others Interface initialize.
            }

            // TODO: Capacity.
            content = Activator.CreateInstance<T>();

            return;

            Collection:

            content = (T)(object)new ArrayList(capacity);
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var index = 0;

            foreach (var item in content)
            {
                ValueInterface<object>.Content.WriteValue(dataWriter[index], item);

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            throw new NotSupportedException();
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            throw new NotSupportedException();
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            foreach (var item in content)
            {
                ValueInterface<object>.Content.WriteValue(valueInfo.ValueCopyer, item);

                valueInfo.Key = index;
                valueInfo.Type = typeof(object);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        IDataReader<TKey> IDataReader.As<TKey>()
        {
            if (this is IDataReader<TKey>)
            {
                return (IDataReader<TKey>)(object)this;
            }

            return new AsDataReader<int, TKey>(this);
        }

        IDataWriter<TKey> IDataWriter.As<TKey>()
        {
            if (this is IDataWriter<TKey>)
            {
                return (IDataWriter<TKey>)(object)this;
            }

            return new AsDataWriter<int, TKey>(this);
        }
    }

    internal sealed class CollectionInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            var type = typeof(T);

            if (type.IsArray)
            {
                return null;
            }

            var item = type;

            var interfaces = type.GetInterfaces();

            var index = 0;

            Loop:

            if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                var genericArguments = item.GetGenericArguments();

                return (IValueInterface<T>)Activator.CreateInstance(typeof(CollectionInterface<,>).MakeGenericType(type, genericArguments[0]));
            }

            if (index < interfaces.Length)
            {
                item = interfaces[index];

                ++index;

                goto Loop;
            }

            if (typeof(ICollection).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(CollectionInterface<>).MakeGenericType(type));
            }

            return null;
        }
    }

    internal sealed class CollectionInterface<T, TValue> : IValueInterface<T> where T : ICollection<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var collectionRW = new CollectionRW<T, TValue>();

            valueReader.ReadArray(collectionRW);

            return collectionRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var collectionRW = new CollectionRW<T, TValue>();

            collectionRW.Initialize(value);

            valueWriter.WriteArray(collectionRW);
        }
    }

    internal sealed class CollectionInterface<T> : IValueInterface<T> where T : ICollection
    {
        public T ReadValue(IValueReader valueReader)
        {
            var collectionRW = new CollectionRW<T>();

            valueReader.ReadArray(collectionRW);

            return collectionRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var collectionRW = new CollectionRW<T>();

            collectionRW.Initialize(value);

            valueWriter.WriteArray(collectionRW);
        }
    }
}