using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class DictionaryRW<T, TKey, TValue> : IDataRW<TKey>, IDirectContent, IInitialize<T> where T : IDictionary<TKey, TValue>
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

        public ValueCopyer<TKey> this[TKey key]
        {
            get
            {
                return new ValueCopyer<TKey>(this, key);
            }
        }

        IValueWriter IDataWriter<TKey>.this[TKey key]
        {
            get
            {
                return this[key];
            }
        }

        IValueReader IDataReader<TKey>.this[TKey key]
        {
            get
            {
                return this[key];
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return content.Keys;
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
                if (typeof(T).IsAssignableFrom(typeof(Dictionary<TKey, TValue>)))
                {
                    goto Dictionary;
                }

                // TODO: Others Interface initialize.
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<Dictionary<TKey, TValue>>.Int64TypeHandle)
            {
                goto Dictionary;
            }

            // TODO: Capacity
            content = Activator.CreateInstance<T>();

            return;

            Dictionary:

            content = (T)(object)new Dictionary<TKey, TValue>(capacity);
        }

        public void OnReadAll(IDataWriter<TKey> dataWriter)
        {
            foreach (var item in content)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[item.Key], item.Value);
            }
        }

        public void OnReadValue(TKey key, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.Content.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(TKey key, IValueReader valueReader)
        {
            content[key] = ValueInterface<TValue>.Content.ReadValue(valueReader);
        }

        public void OnReadAll(IDataWriter<TKey> dataWriter, IValueFilter<TKey> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<TKey>();

            foreach (var item in content)
            {
                ValueInterface<TValue>.Content.WriteValue(valueInfo.ValueCopyer, item.Value);

                valueInfo.Key = item.Key;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        IDataReader<TAsKey> IDataReader.As<TAsKey>()
        {
            if (this is IDataReader<TAsKey>)
            {
                return (IDataReader<TAsKey>)(object)this;
            }

            return new AsDataReader<TKey, TAsKey>(this);
        }

        IDataWriter<TAsKey> IDataWriter.As<TAsKey>()
        {
            if (this is IDataWriter<TAsKey>)
            {
                return (IDataWriter<TAsKey>)(object)this;
            }

            return new AsDataWriter<TKey, TAsKey>(this);
        }
    }

    internal sealed class DictionaryRW<T> : IDataRW<object>, IDirectContent where T : IDictionary
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

        public ValueCopyer<object> this[object key]
        {
            get
            {
                return new ValueCopyer<object>(this, key);
            }
        }

        IValueWriter IDataWriter<object>.this[object key]
        {
            get
            {
                return this[key];
            }
        }

        IValueReader IDataReader<object>.this[object key]
        {
            get
            {
                return this[key];
            }
        }

        public IEnumerable<object> Keys
        {
            get
            {
                return (IEnumerable<object>)content.Keys;
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
                if (typeof(T).IsAssignableFrom(typeof(Hashtable)))
                {
                    goto Hashtable;
                }

                // TODO: Others Interface initialize.
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<Hashtable>.Int64TypeHandle)
            {
                goto Hashtable;
            }

            // TODO: Capacity
            content = Activator.CreateInstance<T>();

            return;

            Hashtable:

            content = (T)(object)new Hashtable(capacity);
        }

        public void OnReadAll(IDataWriter<object> dataWriter)
        {
            foreach (IDictionaryEnumerator item in content)
            {
                ValueInterface<object>.Content.WriteValue(dataWriter[item.Key], item.Value);
            }
        }

        public void OnReadValue(object key, IValueWriter valueWriter)
        {
            ValueInterface<object>.Content.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(object key, IValueReader valueReader)
        {
            content[key] = ValueInterface<object>.Content.ReadValue(valueReader);
        }

        public void OnReadAll(IDataWriter<object> dataWriter, IValueFilter<object> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<object>();

            foreach (IDictionaryEnumerator item in content)
            {
                ValueInterface<object>.Content.WriteValue(valueInfo.ValueCopyer, item.Value);

                valueInfo.Key = item.Key;
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

            return new AsDataReader<object, TKey>(this);
        }

        IDataWriter<TKey> IDataWriter.As<TKey>()
        {
            if (this is IDataWriter<TKey>)
            {
                return (IDataWriter<TKey>)(object)this;
            }

            return new AsDataWriter<object, TKey>(this);
        }
    }

    internal sealed class DictionaryInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            var type = typeof(T);

            var item = type;

            var interfaces = type.GetInterfaces();

            var index = 0;

            Loop:

            if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var genericArguments = item.GetGenericArguments();

                return (IValueInterface<T>)Activator.CreateInstance(typeof(DictionaryInterface<,,>).MakeGenericType(type, genericArguments[0], genericArguments[1]));
            }

            if (index < interfaces.Length)
            {
                item = interfaces[index];

                ++index;

                goto Loop;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(DictionaryInterface<>).MakeGenericType(type));
            }

            return null;
        }
    }

    internal sealed class DictionaryInterface<T, TKey, TValue> : IValueInterface<T> where T : IDictionary<TKey, TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dictionaryRW = new DictionaryRW<T, TKey, TValue>();

            valueReader.ReadObject(((IDataWriter)dictionaryRW).As<string>());

            return dictionaryRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var dictionaryRW = new DictionaryRW<T, TKey, TValue>();

            dictionaryRW.Initialize(value);

            valueWriter.WriteObject(((IDataReader)dictionaryRW).As<string>());
        }
    }

    internal sealed class DictionaryInterface<T> : IValueInterface<T> where T : IDictionary
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dictionaryRW = new DictionaryRW<T>();

            valueReader.ReadObject(((IDataWriter)dictionaryRW).As<string>());

            return dictionaryRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var dictionaryRW = new DictionaryRW<T>();

            dictionaryRW.Initialize(value);

            valueWriter.WriteObject(((IDataReader)dictionaryRW).As<string>());
        }
    }
}