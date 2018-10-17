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
    public abstract class XObjectRW : IDataRW<string>
    {
        /// <summary>
        /// 读取或设置默认的绑定标识。
        /// </summary>
        public static XBindingFlags DefaultBindingFlags { get; set; } =
            XBindingFlags.Property |
            XBindingFlags.Public |
            XBindingFlags.Instance |
            XBindingFlags.RWCannotGetException |
            XBindingFlags.RWCannotSetException |
            XBindingFlags.RWNoFoundException |
            XBindingFlags.RWIgnoreCase;
        
        readonly Dictionary<string, IXFieldRW> fields;
        readonly IXFieldRW[] rFields;
        readonly XBindingFlags flags;
        internal readonly bool typeIsValueType;

        internal XObjectRW(XTypeInfo typeInfo, XBindingFlags flags)
        {
            this.flags = flags;

            fields = typeInfo.rwFields;
            rFields = typeInfo.rFields;
            Type = typeInfo.Type;
            typeIsValueType = Type.IsValueType;
        }
        
        internal abstract object Object { get; }

        internal abstract IntPtr Pointer { get; }

        internal abstract IntPtr Address { get; }

        /// <summary>
        /// 将指定成员名称的值写入到值写入器中。
        /// </summary>
        /// <param name="key">成员的名称</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            if (fields.TryGetValue(key, out var field))
            {
                if (field.CanRead)
                {
                    field.OnReadValue(this, valueWriter);

                    return;
                }
                else if ((flags & XBindingFlags.RWCannotGetException) != 0)
                {
                    throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method or cannot access.", Type.Name, key, "get"));
                }
            }
            else if ((flags & XBindingFlags.RWNoFoundException) != 0)
            {
                throw new MissingMemberException(Type.Name, key);
            }

            valueWriter.DirectWrite(null);
        }

        /// <summary>
        /// 将数据源中的所有成员的名称和值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            foreach (var item in rFields)
            {
                item.WriteTo(this, dataWriter);
            }
        }

        /// <summary>
        /// 对数据源中的原有成员的名称和值进行筛选，并将满足筛选的结果写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
        {
            var dataFilterWriter = new DataFilterWriter<string>(dataWriter, valueFilter);

            foreach (var item in rFields)
            {
                item.WriteTo(this, dataFilterWriter);
            }
        }

        /// <summary>
        /// 将数据读取器中的值设置到指定名称的成员中。
        /// </summary>
        /// <param name="key">成员的名称</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(string key, IValueReader valueReader)
        {
            if (fields.TryGetValue(key, out var field))
            {
                if (field.CanWrite)
                {
                    field.OnWriteValue(this, valueReader);

                    return;
                }
                else if ((flags & XBindingFlags.RWCannotSetException) != 0)
                {
                    throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method or cannot access.", Type.Name, key, "set"));
                }
            }

            else if ((flags & XBindingFlags.RWNoFoundException) != 0)
            {
                throw new MissingMemberException(Type.Name, key);
            }

            valueReader.DirectRead();
        }

        /// <summary>
        /// 调用默认构造函数初始化数据源对象。
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 调用默认构造函数初始化数据源对象。
        /// </summary>
        /// <param name="capacity">不处理此参数</param>
        public void Initialize(int capacity)
        {
            Initialize();
        }


        /// <summary>
        /// 获取该对象读写器的成员名称集合。
        /// </summary>
        public IEnumerable<string> Keys => fields.Keys;

        /// <summary>
        /// 获取该对象读写器的成员名称的数量
        /// </summary>
        public int Count => fields.Count;

        /// <summary>
        /// 获取该数据源对象的 Id。如果时值类型或 Null，则返回 0。
        /// </summary>
        public long ObjectId => typeIsValueType ? 0 : (long)Pointer;

        /// <summary>
        /// 获取数据源的类型。
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// 获取指定成员名称的成员值的读写器。
        /// </summary>
        /// <param name="key">成员名称</param>
        /// <returns>返回值的读写器</returns>
        public XFieldValueRW this[string key]
        {
            get
            {
                if (fields.TryGetValue(key, out var field))
                {
                    return field.CreateRW(this);
                }

                if ((flags & XBindingFlags.RWNoFoundException) != 0)
                {
                    throw new MissingMemberException(Type.Name, key);
                }

                return null;
            }
        }

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        IDataReader<T> IDataReader.As<T>()
        {
            if (this is IDataReader<T>)
            {
                return (IDataReader<T>)(object)this;
            }

            return new AsDataReader<string, T>(this);
        }

        IDataWriter<T> IDataWriter.As<T>()
        {
            if (this is IDataWriter<T>)
            {
                return (IDataWriter<T>)(object)this;
            }

            return new AsDataWriter<string, T>(this);
        }
    }

    /// <summary>
    /// XObjectRW 一个强大，高效，内存小的对象读写器。
    /// </summary>
    public sealed class XObjectRW<T> : XObjectRW, IInitialize<T>, IDirectContent
    {
        T content;

        /// <summary>
        /// 读取或设置默认的绑定标识。
        /// </summary>
        public static XBindingFlags BindingFlags { get; set; } = DefaultBindingFlags;


        internal override IntPtr Pointer => Tools.Pointer.UnBox(content);

        internal override IntPtr Address => Tools.Pointer.UnBox(ref content);
        
        internal override object Object => content;

        internal ref T Reference => ref content;
        
        /// <summary>
        /// 获取或设置该读写器的数据源对象。
        /// </summary>
        public T Content => content;

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


        /// <summary>
        /// 创建 XObjectRW 对象读写器。
        /// </summary>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XObjectRW 对象读写器</returns>
        public static XObjectRW<T> Create(XBindingFlags flags = XBindingFlags.None)
        {
            if (flags == 0)
            {
                flags = BindingFlags;
            }

            return new XObjectRW<T>(XTypeInfo.Create(typeof(T), flags), flags);
        }


        private XObjectRW(XTypeInfo xTypeInfo, XBindingFlags flags) : base(xTypeInfo, flags)
        {
        }
        
        /// <summary>
        /// 设置数据源对象。
        /// </summary>
        /// <param name="obj">数据源对象</param>
        public void Initialize(T obj)
        {
            content = obj;
        }

        /// <summary>
        /// 初始化数据源。
        /// </summary>
        public override void Initialize()
        {
            content = Activator.CreateInstance<T>();
        }

        /// <summary>
        /// 获取 XObjectRW 的名称。
        /// </summary>
        /// <returns>返回 XObjectRW 的名称</returns>
        public override string ToString()
        {
            return typeof(XObjectRW).FullName + "<" + typeof(T).FullName + ">";
        }
    }
}