using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;

namespace Swifter.Reflection
{
    /// <summary>
    /// XObjectRW 一个强大，高效，内存小的对象读写器。
    /// </summary>
    public sealed class XObjectRW : IDataRW<string>, IDirectContent
    {
        /// <summary>
        /// 读取或设置默认的绑定标识。
        /// </summary>
        public static XBindingFlags DefaultBindingFlags { get; set; } =
            XBindingFlags.Property |
            XBindingFlags.Field |
            XBindingFlags.Public |
            XBindingFlags.Instance |
            XBindingFlags.RWCannotGetException |
            XBindingFlags.RWCannotSetException |
            XBindingFlags.RWNoFoundException |
            XBindingFlags.RWIgnoreCase;

        /// <summary>
        /// 创建 XObjectRW 对象读写器。
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XObjectRW 对象读写器</returns>
        public static XObjectRW Create(Type type, XBindingFlags flags = XBindingFlags.None)
        {
            if (flags == XBindingFlags.None)
            {
                flags = DefaultBindingFlags;
            }

            return new XObjectRW(XTypeInfo.Create(type, flags), flags);
        }

        internal readonly NameCache<IXFieldRW> fieldsCache;
        internal readonly IXFieldRW[] fields;
        internal readonly XBindingFlags flags;
        internal readonly Type type;

        internal object obj;

        internal XObjectRW(XTypeInfo typeInfo, XBindingFlags flags)
        {
            this.flags = flags;

            fieldsCache = typeInfo.rwFieldsCache;
            fields = typeInfo.rwFields;
            type = typeInfo.Type;
        }

        /// <summary>
        /// 获取指定成员名称的成员值的读写器。
        /// </summary>
        /// <param name="key">成员名称</param>
        /// <returns>返回值的读写器</returns>
        public XFieldValueRW this[string key]
        {
            get
            {
                if (fieldsCache.TryGetValue(key, out var field))
                {
                    return new XFieldValueRW(obj, field);
                }

                if ((flags & XBindingFlags.RWNoFoundException) != 0)
                {
                    throw new MissingMemberException(type.Name, key);
                }

                return null;
            }
        }

        /// <summary>
        /// 将指定成员名称的值写入到值写入器中。
        /// </summary>
        /// <param name="key">成员的名称</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            if (fieldsCache.TryGetValue(key, out var field))
            {
                if (field.CanRead)
                {
                    field.OnReadValue(obj, valueWriter);
                }
                else if ((flags & XBindingFlags.RWCannotGetException) != 0)
                {
                    throw new MissingMethodException($"Property '{type.Name}.{key}' No define '{"get"}' method or cannot access.");
                }
            }
            else if ((flags & XBindingFlags.RWNoFoundException) != 0)
            {
                throw new MissingMemberException(type.Name, key);
            }
            else
            {
                valueWriter.DirectWrite(null);
            }
        }

        /// <summary>
        /// 将数据源中的所有成员的名称和值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            if ((flags & XBindingFlags.SkipDefaultValue) != 0)
            {
                var valueCopyer = new ValueCopyer();

                foreach (var item in fields)
                {
                    item.OnReadValue(obj, valueCopyer);

                    if (!valueCopyer.IsEmptyValue())
                    {
                        valueCopyer.WriteTo(dataWriter[item.Name]);
                    }
                }
            }
            else
            {
                foreach (var item in fields)
                {
                    item.OnReadValue(obj, dataWriter[item.Name]);
                }
            }
        }

        /// <summary>
        /// 对数据源中的原有成员的名称和值进行筛选，并将满足筛选的结果写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
        {
            var filter = new DataFilterWriter<string>(dataWriter, valueFilter);

            if ((flags & XBindingFlags.SkipDefaultValue) != 0)
            {
                var valueCopyer = new ValueCopyer();

                foreach (var item in fields)
                {
                    item.OnReadValue(obj, valueCopyer);

                    if (!valueCopyer.IsEmptyValue())
                    {
                        valueCopyer.WriteTo(filter[item.Name]);
                    }
                }
            }
            else
            {
                foreach (var item in fields)
                {
                    item.OnReadValue(obj, filter[item.Name]);
                }
            }
        }

        /// <summary>
        /// 将数据读取器中的值设置到指定名称的成员中。
        /// </summary>
        /// <param name="key">成员的名称</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(string key, IValueReader valueReader)
        {
            if (fieldsCache.TryGetValue(key, out var field))
            {
                if (field.CanWrite)
                {
                    field.OnWriteValue(obj, valueReader);
                }
                else if ((flags & XBindingFlags.RWCannotSetException) != 0)
                {
                    throw new MissingMethodException($"Property '{type.Name}.{key}' No define '{"set"}' method or cannot access.");
                }
            }
            else if ((flags & XBindingFlags.RWNoFoundException) != 0)
            {
                throw new MissingMemberException(type.Name, key);
            }
            else
            {
                valueReader.DirectRead();
            }
        }

        /// <summary>
        /// 调用默认构造函数初始化数据源对象。
        /// </summary>
        public void Initialize()
        {
            obj = Activator.CreateInstance(type);
        }

        /// <summary>
        /// 调用默认构造函数初始化数据源对象。
        /// </summary>
        /// <param name="capacity">不处理此参数</param>
        public void Initialize(int capacity)
        {
            Initialize();
        }

        /// <summary>
        /// 初始化数据源。
        /// </summary>
        /// <param name="obj">数据源。</param>
        public void Initialize(object obj)
        {
            this.obj = type.IsInstanceOfType(obj) ? obj : throw new InvalidCastException();
        }

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void OnWriteAll(IDataReader<string> dataReader)
        {
            foreach (var item in fields)
            {
                if (item.CanWrite)
                {
                    item.OnWriteValue(obj, dataReader[item.Name]);
                }
            }
        }


        /// <summary>
        /// 获取该对象读写器的成员名称集合。
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                foreach (var item in fields)
                {
                    yield return item.Name;
                }
            }
        }

        /// <summary>
        /// 获取该对象读写器的成员名称的数量
        /// </summary>
        public int Count => fields.Length;

        /// <summary>
        /// 获取数据源的引用根，全局唯一。如果数据源是值类型或 Null，则返回 Null。
        /// </summary>
        public object ReferenceToken => type.IsValueType ? null : obj;

        /// <summary>
        /// 获取数据源。
        /// </summary>
        public object Content => obj;

        object IDirectContent.DirectContent
        {
            get => obj;
            set => Initialize(value);
        }

        IValueRW IDataRW<string>.this[string key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];
    }
}