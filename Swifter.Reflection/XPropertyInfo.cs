using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XPropertyInfo 属性信息。
    /// 此属性信息提供的读写方法比 .Net 自带的 PropertyInfo 属性信息快很多。
    /// </summary>
    public abstract class XPropertyInfo
    {
        /// <summary>
        /// 创建 XPropertyInfo 属性信息。
        /// </summary>
        /// <param name="propertyInfo">.Net 自带的 PropertyInfo 属性</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XPropertyInfo 属性信息。</returns>
        public static XPropertyInfo Create(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

            var isStatic = (getMethod == null || getMethod.IsStatic) && (setMethod == null || setMethod.IsStatic);
            var declaringType = propertyInfo.DeclaringType;
            var propertyType = propertyInfo.PropertyType;

            if (propertyType.IsPointer)
            {
                propertyType = typeof(IntPtr);
            }
            else if (propertyType.IsByRef)
            {
                propertyType = propertyType.GetElementType();
            }

            Type targetType;

            if (isStatic)
            {
                targetType = typeof(XStaticPropertyInfo<>).MakeGenericType(propertyType);
            }
            else if (declaringType.IsValueType)
            {
                targetType = typeof(XStructPropertyInfo<,>).MakeGenericType(declaringType, propertyType);
            }
            else
            {
                targetType = typeof(XClassPropertyInfo<>).MakeGenericType(propertyType);
            }

            var result = (XPropertyInfo)Activator.CreateInstance(targetType);

            if (propertyInfo.PropertyType.IsByRef)
            {
                result.InitializeByRef(propertyInfo, flags);
            }
            else
            {
                result.Initialize(propertyInfo, flags);
            }

            return result;
        }
        
        internal string name;
        internal XBindingFlags flags;

        internal virtual void Initialize(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            PropertyInfo = propertyInfo;

            name = propertyInfo.Name;
            this.flags = flags;
        }

        internal virtual void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            PropertyInfo = propertyInfo;

            name = propertyInfo.Name;
            this.flags = flags;
        }
        
        internal void assert(bool err, string name)
        {
            if (!err)
            {
                throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method or cannot access.", PropertyInfo.DeclaringType.Name, PropertyInfo.Name, name));
            }
        }

        /// <summary>
        /// 获取 .Net 自带的 PropertyInfo 属性
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// 获取此属性的名称。
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 获取该属性的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该属性是静态的，则指定为 Null</param>
        /// <returns>返回该属性的值</returns>
        public abstract object GetValue(object obj);

        /// <summary>
        /// 设置该属性的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该属性是静态的，则指定为 Null</param>
        /// <param name="value">值</param>
        public abstract void SetValue(object obj, object value);

        /// <summary>
        /// 获取该属性的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该属性是静态的，则指定为 Null</param>
        /// <returns>返回该属性的值</returns>
        public abstract object GetValue(TypedReference typedRef);

        /// <summary>
        /// 设置该属性的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该属性是静态的，则指定为 Null</param>
        /// <param name="value">值</param>
        public abstract void SetValue(TypedReference typedRef, object value);
    }

    delegate ref TValue XClassRefValueHandler<TValue>(object obj);
    delegate TValue XClassGetValueHandler<TValue>(object obj);
    delegate void XClassSetValueHandler<TValue>(object obj, TValue value);

    delegate ref TValue XStructRefValueHandler<TStruct, TValue>(ref TStruct obj);
    delegate TValue XStructGetValueHandler<TStruct, TValue>(ref TStruct obj);
    delegate void XStructSetValueHandler<TStruct, TValue>(ref TStruct obj, TValue value);

    delegate ref TValue XStaticRefValueHandler<TValue>();
    delegate TValue XStaticGetValueHandler<TValue>();
    delegate void XStaticSetValueHandler<TValue>(TValue value);

    sealed class XClassPropertyInfo<TValue> : XPropertyInfo, IXFieldRW
    {
        XClassGetValueHandler<TValue> _get;
        XClassSetValueHandler<TValue> _set;

        Type declaringType;
        IntPtr declaringTypeHandle;

        internal override void Initialize(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.Initialize(propertyInfo, flags);

            declaringType = propertyInfo.DeclaringType;
            declaringTypeHandle = TypeHelper.GetTypeHandle(declaringType);
            
            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

            if (getMethod != null)
            {
                _get = MethodHelper.CreateDelegate<XClassGetValueHandler<TValue>>(getMethod, SignatureLevels.Cast);
            }

            if (setMethod != null)
            {
                _set = MethodHelper.CreateDelegate<XClassSetValueHandler<TValue>>(setMethod, SignatureLevels.Cast);
            }
        }

        internal override void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByRef(propertyInfo, flags);

            declaringType = propertyInfo.DeclaringType;
            declaringTypeHandle = TypeHelper.GetTypeHandle(declaringType);

            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

            if (getMethod != null)
            {
                var _ref = MethodHelper.CreateDelegate<XClassRefValueHandler<TValue>>(getMethod, SignatureLevels.Cast);

                _get = (obj) =>
                {
                    return _ref(obj);
                };

                _set = (obj, value) =>
                {
                    _ref(obj) = value;
                };
            }
        }

        public bool CanRead => _get != null;

        public bool CanWrite => _set != null;

        public int Order => 999;

        public Type FieldType => typeof(TValue);

        public BasicTypes BasicType => TypeInfo<TValue>.BasicType;

        public XFieldValueRW CreateRW(XObjectRW objectRW)
        {
            return new XFieldValueRW<TValue>(objectRW, this);
        }

        public override object GetValue(object obj)
        {
            assert(CanRead, "get");

            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            return _get(obj);
        }

        public override object GetValue(TypedReference typedRef)
        {
            assert(CanRead, "get");

            if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
            {
                throw new TargetException(nameof(typedRef));
            }

            unsafe
            {
                return _get(Tools.Pointer.Box(*(IntPtr*)Tools.Pointer.UnBox(typedRef)));
            }
        }

        public void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter)
        {
            assert(CanRead, "get");

            ValueInterface<TValue>.Content.WriteValue(valueWriter, _get(objectRW.Object));
        }

        public void OnWriteValue(XObjectRW objectRW, IValueReader valueReader)
        {
            assert(CanWrite, "set");

            _set(objectRW.Object, ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(XObjectRW objectRW)
        {
            assert(CanRead, "get");

            if ((object)this is XClassPropertyInfo<T> typedThis)
            {
                return typedThis._get(objectRW.Object);
            }
            else
            {
                return XConvert<TValue, T>.Convert(_get(objectRW.Object));
            }
        }

        public override void SetValue(object obj, object value)
        {
            assert(CanWrite, "set");

            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            _set(obj, (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            assert(CanWrite, "set");

            if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
            {
                throw new TargetException(nameof(typedRef));
            }

            unsafe
            {
                _set(Tools.Pointer.Box(*(IntPtr*)Tools.Pointer.UnBox(typedRef)), (TValue)value);
            }
        }

        public void WriteValue<T>(XObjectRW objectRW, T value)
        {
            assert(CanWrite, "set");

            if ((object)this is XClassPropertyInfo<T> typedThis)
            {
                typedThis._set(objectRW.Object, value);
            }
            else
            {
                _set(objectRW.Object, XConvert<T, TValue>.Convert(value));
            }
        }

        public void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter)
        {
            if ((flags & XBindingFlags.SkipDefaultValue) == 0)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[name], _get(objectRW.Object));
            }
            else
            {
                var value = _get(objectRW.Object);

                if (!TypeHelper.IsEmptyValue(value))
                {
                    ValueInterface<TValue>.Content.WriteValue(dataWriter[name], value);
                }
            }
        }
    }

    sealed class XStructPropertyInfo<TStruct,TValue> : XPropertyInfo, IXFieldRW
    {
        XStructGetValueHandler<TStruct, TValue> _get;
        XStructSetValueHandler<TStruct, TValue> _set;

        IntPtr declaringTypeHandle;

        internal override void Initialize(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.Initialize(propertyInfo, flags);

            declaringTypeHandle = TypeHelper.GetTypeHandle(propertyInfo.DeclaringType);

            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

            if (getMethod != null)
            {
                _get = MethodHelper.CreateDelegate<XStructGetValueHandler<TStruct, TValue>>(getMethod, SignatureLevels.Cast);
            }

            if (setMethod != null)
            {
                _set = MethodHelper.CreateDelegate<XStructSetValueHandler<TStruct, TValue>>(setMethod, SignatureLevels.Cast);
            }
        }

        internal override void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByRef(propertyInfo, flags);

            declaringTypeHandle = TypeHelper.GetTypeHandle(propertyInfo.DeclaringType);

            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

            if (getMethod != null)
            {
                var _ref = MethodHelper.CreateDelegate<XStructRefValueHandler<TStruct, TValue>>(getMethod, SignatureLevels.Cast);

                _get = (ref TStruct obj) =>
                {
                    return _ref(ref obj);
                };

                _set = (ref TStruct obj, TValue value) =>
                {
                    _ref(ref obj) = value;
                };
            }
        }

        public bool CanRead => _get != null;

        public bool CanWrite => _set != null;

        public int Order => 999;

        public Type FieldType => typeof(TValue);

        public BasicTypes BasicType => TypeInfo<TValue>.BasicType;

        public XFieldValueRW CreateRW(XObjectRW objectRW)
        {
            return new XFieldValueRW<TValue>(objectRW, this);
        }

        public override object GetValue(object obj)
        {
            assert(CanRead, "get");

            if (declaringTypeHandle != TypeHelper.GetTypeHandle(obj))
            {
                throw new TargetException(nameof(obj));
            }

            return _get(ref Tools.Pointer.Box<TStruct>(TypeHelper.GetStructAddress(obj)));
        }
        
        public unsafe override object GetValue(TypedReference typedRef)
        {
            assert(CanRead, "get");

            if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
            {
                throw new TargetException(nameof(typedRef));
            }

            return _get(ref __refvalue(typedRef, TStruct));
        }

        public void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter)
        {
            assert(CanRead, "get");

            ValueInterface<TValue>.Content.WriteValue(valueWriter, _get(ref ((XObjectRW<TStruct>)objectRW).Reference));
        }

        public void OnWriteValue(XObjectRW objectRW, IValueReader valueReader)
        {
            assert(CanWrite, "set");

            var value = ValueInterface<TValue>.Content.ReadValue(valueReader);

            _set(ref ((XObjectRW<TStruct>)objectRW).Reference, value);
        }

        public T ReadValue<T>(XObjectRW objectRW)
        {
            assert(CanRead, "get");

            if ((object)this is XStructPropertyInfo<TStruct,T> typedThis)
            {
                return typedThis._get(ref Tools.Pointer.Box<TStruct>(objectRW.Address));
            }
            else
            {
                return XConvert<TValue, T>.Convert(_get(ref ((XObjectRW<TStruct>)objectRW).Reference));
            }
        }

        public override void SetValue(object obj, object value)
        {
            assert(CanWrite, "set");

            if (declaringTypeHandle != TypeHelper.GetTypeHandle(obj))
            {
                throw new TargetException(nameof(obj));
            }

            _set(ref Tools.Pointer.Box<TStruct>(TypeHelper.GetStructAddress(obj)), (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            assert(CanWrite, "set");

            if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
            {
                throw new TargetException(nameof(typedRef));
            }

            _set(ref __refvalue(typedRef, TStruct), (TValue)value);
        }

        public void WriteValue<T>(XObjectRW objectRW, T value)
        {
            assert(CanWrite, "set");

            if ((object)this is XStructPropertyInfo<TStruct,T> typedThis)
            {
                typedThis._set(ref ((XObjectRW<TStruct>)objectRW).Reference, value);
            }
            else
            {
                var tValue = XConvert<T, TValue>.Convert(value);

                _set(ref ((XObjectRW<TStruct>)objectRW).Reference, tValue);
            }
        }

        public void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter)
        {
            if ((flags & XBindingFlags.SkipDefaultValue) == 0)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[name], _get(ref ((XObjectRW<TStruct>)objectRW).Reference));
            }
            else
            {
                var value = _get(ref ((XObjectRW<TStruct>)objectRW).Reference);

                if (!TypeHelper.IsEmptyValue(value))
                {
                    ValueInterface<TValue>.Content.WriteValue(dataWriter[name], value);
                }
            }
        }
    }

    sealed class XStaticPropertyInfo<TValue> : XPropertyInfo, IXFieldRW
    {
        XStaticGetValueHandler<TValue> _get;
        XStaticSetValueHandler<TValue> _set;

        internal override void Initialize(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.Initialize(propertyInfo, flags);

            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

            if (getMethod != null)
            {
                _get = MethodHelper.CreateDelegate<XStaticGetValueHandler<TValue>>(getMethod, SignatureLevels.Cast);
            }

            if (setMethod != null)
            {
                _set = MethodHelper.CreateDelegate<XStaticSetValueHandler<TValue>>(setMethod, SignatureLevels.Cast);
            }
        }

        internal override void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByRef(propertyInfo, flags);

            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

            if (getMethod != null)
            {
                var _ref = MethodHelper.CreateDelegate<XStaticRefValueHandler<TValue>>(getMethod, SignatureLevels.Cast);

                _get = () =>
                {
                    return _ref();
                };

                _set = (value) =>
                {
                    _ref() = value;
                };
            }
        }

        public bool CanRead => _get != null;

        public bool CanWrite => _set != null;

        public int Order => 999;

        public Type FieldType => typeof(TValue);

        public BasicTypes BasicType => TypeInfo<TValue>.BasicType;

        public XFieldValueRW CreateRW(XObjectRW objectRW)
        {
            return new XFieldValueRW<TValue>(objectRW, this);
        }

        public override object GetValue(object obj)
        {
            assert(CanRead, "get");

            return _get();
        }

        public override object GetValue(TypedReference typedRef)
        {
            assert(CanRead, "get");

            return _get();
        }

        public void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter)
        {
            assert(CanRead, "get");

            ValueInterface<TValue>.Content.WriteValue(valueWriter, _get());
        }

        public void OnWriteValue(XObjectRW objectRW, IValueReader valueReader)
        {
            assert(CanWrite, "set");

            _set(ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(XObjectRW objectRW)
        {
            assert(CanRead, "get");

            if ((object)this is XStaticPropertyInfo<T> typedThis)
            {
                return typedThis._get();
            }
            else
            {
                return XConvert<TValue, T>.Convert(_get());
            }
        }

        public override void SetValue(object obj, object value)
        {
            assert(CanWrite, "set");

            _set((TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            assert(CanWrite, "set");

            _set((TValue)value);
        }

        public void WriteValue<T>(XObjectRW objectRW, T value)
        {
            assert(CanWrite, "set");

            if ((object)this is XStaticPropertyInfo<T> typedThis)
            {
                typedThis._set(value);
            }
            else
            {
                _set(XConvert<T, TValue>.Convert(value));
            }
        }

        public void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter)
        {
            if ((flags & XBindingFlags.SkipDefaultValue) == 0)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[name], _get());
            }
            else
            {
                var value = _get();

                if (!TypeHelper.IsEmptyValue(value))
                {
                    ValueInterface<TValue>.Content.WriteValue(dataWriter[name], value);
                }
            }
        }
    }
}