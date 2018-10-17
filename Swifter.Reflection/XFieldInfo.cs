using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XFieldInfo 字段信息。
    /// 此 XFieldInfo 的提供读写方法比 .Net 自带的 FieldInfo 快很多。
    /// </summary>
    public abstract class XFieldInfo
    {
        /// <summary>
        /// 创建 XFieldInfo 字段信息。
        /// </summary>
        /// <param name="fieldInfo">.Net 自带的 FieldInfo 字段信息。</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回一个 XFieldInfo 字段信息。</returns>
        public static XFieldInfo Create(FieldInfo fieldInfo, XBindingFlags flags)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }

            var declaringType = fieldInfo.DeclaringType;
            var fieldType = fieldInfo.FieldType;

            Type targetType;

            if (fieldInfo.IsLiteral)
            {
                targetType = typeof(XLiteralFieldInfo<>);
            }
            else if (fieldInfo.IsStatic)
            {
                targetType = typeof(XStaticFieldInfo<>);
            }
            else if (declaringType.IsValueType)
            {
                targetType = typeof(XStructFieldInfo<>);
            }
            else
            {
                targetType = typeof(XClassFieldInfo<>);
            }

            if (fieldType.IsPointer || fieldType.IsByRef)
            {
                fieldType = typeof(IntPtr);
            }

            targetType = targetType.MakeGenericType(fieldType);

            var result = (XFieldInfo)Activator.CreateInstance(targetType);

            result.Initialize(fieldInfo, flags);

            return result;
        }

        internal string name;
        internal XBindingFlags flags;

        internal virtual void Initialize(FieldInfo fieldInfo, XBindingFlags flags)
        {
            FieldInfo = fieldInfo;

            name = fieldInfo.Name;
            this.flags = flags;
        }

        /// <summary>
        /// 获取 .Net 自带的 FieldInfo 字段信息。
        /// </summary>
        public FieldInfo FieldInfo { get; private set; }

        /// <summary>
        /// 获取此字段的名称。
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 获取该字段的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该字段时静态的，则指定为 Null</param>
        /// <returns>返回该字段的值</returns>
        public abstract object GetValue(object obj);

        /// <summary>
        /// 设置该字段的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该字段时静态的，则指定为 Null</param>
        /// <param name="value">该字段的值</param>
        public abstract void SetValue(object obj, object value);

        /// <summary>
        /// 获取该字段的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该字段时静态的，则指定为 Null</param>
        /// <returns>返回该字段的值</returns>
        public abstract object GetValue(TypedReference typedRef);

        /// <summary>
        /// 设置该字段的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该字段时静态的，则指定为 Null</param>
        /// <param name="value">返回该字段的值</param>
        public abstract void SetValue(TypedReference typedRef, object value);
    }

    sealed class XClassFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        uint offset;

        Type declaringType;
        IntPtr declaringTypeHandle;

        internal override void Initialize(FieldInfo fieldInfo, XBindingFlags flags)
        {
            base.Initialize(fieldInfo, flags);

            declaringType = fieldInfo.DeclaringType;
            declaringTypeHandle = TypeHelper.GetTypeHandle(declaringType);

            offset = TypeHelper.OffsetOf(fieldInfo);
        }
        
        TValue _get(IntPtr pObject)
        {
            return Tools.Pointer.GetValue<TValue>((IntPtr)((long)pObject + TypeHelper.ObjectHandleSize + offset));
        }

        void _set(IntPtr pObject, TValue value)
        {
            Tools.Pointer.SetValue((IntPtr)((long)pObject + TypeHelper.ObjectHandleSize + offset), value);
        }

        TValue _get(XObjectRW objectRW)
        {
            return Tools.Pointer.GetValue<TValue>((IntPtr)((long)objectRW.Pointer + TypeHelper.ObjectHandleSize + offset));
        }

        void _set(XObjectRW objectRW, TValue value)
        {
            Tools.Pointer.SetValue((IntPtr)((long)objectRW.Pointer + TypeHelper.ObjectHandleSize + offset), value);
        }


        public bool CanRead => true;

        public bool CanWrite => true;

        public Type FieldType => typeof(TValue);

        public int Order => 999;

        public BasicTypes BasicType => TypeInfo<TValue>.BasicType;
        
        public XFieldValueRW CreateRW(XObjectRW objectRW)
        {
            return new XFieldValueRW<TValue>(objectRW, this);
        }

        public override object GetValue(object obj)
        {
            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            return _get(Tools.Pointer.UnBox(obj));
        }

        public override object GetValue(TypedReference typedRef)
        {
            if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
            {
                throw new TargetException(nameof(typedRef));
            }

            unsafe
            {
                return _get(*(IntPtr*)Tools.Pointer.UnBox(typedRef));
            }
        }

        public void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.Content.WriteValue(valueWriter, _get(objectRW));
        }

        public void OnWriteValue(XObjectRW objectRW, IValueReader valueReader)
        {
            _set(objectRW, ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(XObjectRW objectRW)
        {
            if ((object)this is XClassFieldInfo<T> typedThis)
            {
                return typedThis._get(objectRW);
            }
            else
            {
                return XConvert<TValue, T>.Convert(_get(objectRW));
            }
        }

        public override void SetValue(object obj, object value)
        {
            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            _set(Tools.Pointer.UnBox(obj), (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
            {
                throw new TargetException(nameof(typedRef));
            }

            unsafe
            {
                _set(*(IntPtr*)Tools.Pointer.UnBox(typedRef), (TValue)value);
            }
        }

        public void WriteValue<T>(XObjectRW objectRW, T value)
        {
            if ((object)this is XClassFieldInfo<T> typedThis)
            {
                typedThis._set(objectRW, value);
            }
            else
            {
                _set(objectRW, XConvert<T, TValue>.Convert(value));
            }
        }

        public void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter)
        {
            if ((flags & XBindingFlags.SkipDefaultValue) == 0)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[name], _get(objectRW));
            }
            else
            {
                var value = _get(objectRW);

                if (!TypeHelper.IsEmptyValue(value))
                {
                    ValueInterface<TValue>.Content.WriteValue(dataWriter[name], value);
                }
            }
        }
    }

    sealed class XStructFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        uint offset;

        IntPtr declaringTypeHandle;

        internal override void Initialize(FieldInfo fieldInfo, XBindingFlags flags)
        {
            base.Initialize(fieldInfo, flags);

            declaringTypeHandle = TypeHelper.GetTypeHandle(fieldInfo.DeclaringType);

            offset = TypeHelper.OffsetOf(fieldInfo);
        }

        TValue _get(IntPtr pObject)
        {
            return Tools.Pointer.GetValue<TValue>((IntPtr)((long)pObject + offset));
        }

        void _set(IntPtr pObject, TValue value)
        {
            Tools.Pointer.SetValue((IntPtr)((long)pObject + offset), value);
        }

        TValue _get(XObjectRW objectRW)
        {
            return Tools.Pointer.GetValue<TValue>((IntPtr)((long)objectRW.Address + offset));
        }

        void _set(XObjectRW objectRW, TValue value)
        {
            Tools.Pointer.SetValue((IntPtr)((long)objectRW.Address + offset), value);
        }

        public bool CanRead => true;

        public bool CanWrite => true;

        public int Order => 999;

        public Type FieldType => typeof(TValue);

        public BasicTypes BasicType => TypeInfo<TValue>.BasicType;

        public XFieldValueRW CreateRW(XObjectRW objectRW)
        {
            return new XFieldValueRW<TValue>(objectRW, this);
        }

        public override object GetValue(object obj)
        {
            if (declaringTypeHandle != TypeHelper.GetTypeHandle(obj))
            {
                throw new TargetException(nameof(obj));
            }

            return _get(TypeHelper.GetStructAddress(obj));
        }

        public override object GetValue(TypedReference typedRef)
        {
            if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
            {
                throw new TargetException(nameof(typedRef));
            }

            return _get(TypeHelper.GetStructAddress(typedRef));
        }

        public void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.Content.WriteValue(valueWriter, _get(objectRW));
        }

        public void OnWriteValue(XObjectRW objectRW, IValueReader valueReader)
        {
            _set(objectRW, ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(XObjectRW objectRW)
        {
            if ((object)this is XStructFieldInfo<T> typedThis)
            {
                return typedThis._get(objectRW);
            }
            else
            {
                return XConvert<TValue, T>.Convert(_get(objectRW));
            }
        }

        public override void SetValue(object obj, object value)
        {
            if (declaringTypeHandle != TypeHelper.GetTypeHandle(obj))
            {
                throw new TargetException(nameof(obj));
            }

            _set(TypeHelper.GetStructAddress(obj), (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
            {
                throw new TargetException(nameof(typedRef));
            }

            _set(TypeHelper.GetStructAddress(typedRef), (TValue)value);
        }

        public void WriteValue<T>(XObjectRW objectRW, T value)
        {
            if ((object)this is XStructFieldInfo<T> typedThis)
            {
                typedThis._set(objectRW, value);
            }
            else
            {
                _set(objectRW, XConvert<T, TValue>.Convert(value));
            }
        }

        public void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter)
        {
            if ((flags & XBindingFlags.SkipDefaultValue) == 0)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[name], _get(objectRW));
            }
            else
            {
                var value = _get(objectRW);

                if (!TypeHelper.IsEmptyValue(value))
                {
                    ValueInterface<TValue>.Content.WriteValue(dataWriter[name], value);
                }
            }
        }
    }

    sealed class XStaticFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        IntPtr address;

        internal override void Initialize(FieldInfo fieldInfo, XBindingFlags flags)
        {
            base.Initialize(fieldInfo, flags);

            address = (IntPtr)((long)TypeHelper.GetTypeStaticMemoryAddress(fieldInfo.DeclaringType) + TypeHelper.OffsetOf(fieldInfo));
        }

        public bool CanRead => true;

        public bool CanWrite => true;

        public int Order => 999;

        public Type FieldType => FieldInfo.FieldType;

        public BasicTypes BasicType => TypeInfo<TValue>.BasicType;

        public XFieldValueRW CreateRW(XObjectRW objectRW)
        {
            return new XFieldValueRW<TValue>(objectRW, this);
        }

        public override object GetValue(object obj)
        {
            return Tools.Pointer.GetValue<TValue>(address);
        }

        public override object GetValue(TypedReference typedRef)
        {
            return Tools.Pointer.GetValue<TValue>(address);
        }

        public void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.Content.WriteValue(valueWriter, Tools.Pointer.GetValue<TValue>(address));
        }

        public void OnWriteValue(XObjectRW objectRW, IValueReader valueReader)
        {
            Tools.Pointer.SetValue(address, ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(XObjectRW objectRW)
        {
            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TValue>.Int64TypeHandle)
            {
                return Tools.Pointer.GetValue<T>(address);
            }
            else
            {
                return XConvert<TValue, T>.Convert(Tools.Pointer.GetValue<TValue>(address));
            }
        }

        public override void SetValue(object obj, object value)
        {
            Tools.Pointer.SetValue(address, (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Tools.Pointer.SetValue(address, (TValue)value);
        }

        public void WriteValue<T>(XObjectRW objectRW, T value)
        {
            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TValue>.Int64TypeHandle)
            {
                Tools.Pointer.SetValue(address, value);
            }
            else
            {
                Tools.Pointer.SetValue(address, XConvert<T, TValue>.Convert(value));
            }
        }

        public void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter)
        {
            if ((flags & XBindingFlags.SkipDefaultValue) == 0)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[name], Tools.Pointer.GetValue<TValue>(address));
            }
            else
            {
                var value = Tools.Pointer.GetValue<TValue>(address);

                if (!TypeHelper.IsEmptyValue(value))
                {
                    ValueInterface<TValue>.Content.WriteValue(dataWriter[name], value);
                }
            }
        }
    }

    sealed class XLiteralFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        TValue value;

        internal override void Initialize(FieldInfo fieldInfo, XBindingFlags flags)
        {
            base.Initialize(fieldInfo, flags);

            value = (TValue)FieldInfo.GetRawConstantValue();
        }

        public bool CanRead => true;

        public bool CanWrite => false;

        public int Order => 999;

        public Type FieldType => FieldInfo.FieldType;

        public BasicTypes BasicType => TypeInfo<TValue>.BasicType;

        public XFieldValueRW CreateRW(XObjectRW objectRW)
        {
            return new XFieldValueRW<TValue>(objectRW, this);
        }

        public override object GetValue(object obj)
        {
            return value;
        }

        public override object GetValue(TypedReference typedRef)
        {
            return value;
        }

        public void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.Content.WriteValue(valueWriter, value);
        }

        void _throw()
        {
            throw new InvalidOperationException(StringHelper.Format("Cannot set value of the const field '{0}.{1}'.", FieldInfo.DeclaringType.Name, FieldInfo.Name));
        }

        public void OnWriteValue(XObjectRW objectRW, IValueReader valueReader)
        {
            _throw();
        }

        public T ReadValue<T>(XObjectRW objectRW)
        {
            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TValue>.Int64TypeHandle)
            {
                return ((XLiteralFieldInfo<T>)(object)this).value;
            }
            else
            {
                return XConvert<TValue, T>.Convert(value);
            }
        }

        public override void SetValue(object obj, object value)
        {
            _throw();
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            _throw();
        }

        public void WriteValue<T>(XObjectRW objectRW, T value)
        {
            _throw();
        }

        public void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter)
        {
            if ((flags & XBindingFlags.SkipDefaultValue) == 0)
            {
                ValueInterface<TValue>.Content.WriteValue(dataWriter[name], value);
            }
            else
            {
                if (!TypeHelper.IsEmptyValue(value))
                {
                    ValueInterface<TValue>.Content.WriteValue(dataWriter[name], value);
                }
            }
        }
    }
}