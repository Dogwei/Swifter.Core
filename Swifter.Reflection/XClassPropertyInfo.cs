using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XClassPropertyInfo<TValue> : XPropertyInfo, IXFieldRW
    {
        XClassGetValueHandler<TValue> _get;
        XClassSetValueHandler<TValue> _set;

        Type declaringType;

        internal override void Initialize(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.Initialize(propertyInfo, flags);

            declaringType = propertyInfo.DeclaringType;
            
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

        public int Order => RWFieldAttribute.DefaultOrder;

        public Type FieldType => typeof(TValue);
        
        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            return _get(obj);
        }

        public override object GetValue(TypedReference typedRef)
        {
            Assert(CanRead, "get");

            if (declaringType != __reftype(typedRef))
            {
                throw new TargetException(nameof(typedRef));
            }

            unsafe
            {
                return _get(Tools.Pointer.Box(*(IntPtr*)Tools.Pointer.UnBox(typedRef)));
            }
        }

        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            _set(obj, (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Assert(CanWrite, "set");

            if (declaringType != __reftype(typedRef))
            {
                throw new TargetException(nameof(typedRef));
            }

            unsafe
            {
                _set(Tools.Pointer.Box(*(IntPtr*)Tools.Pointer.UnBox(typedRef)), (TValue)value);
            }
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(CanRead, "get");

            ValueInterface<TValue>.Content.WriteValue(valueWriter, _get(obj));
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(CanWrite, "set");

            _set(obj, ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            Assert(CanRead, "get");

            if (_get is XClassGetValueHandler<T> __get)
            {
                return __get(obj);
            }

            return XConvert<TValue, T>.Convert(_get(obj));
        }
        
        public void WriteValue<T>(object obj, T value)
        {
            Assert(CanWrite, "set");

            if (_set is XClassSetValueHandler<T> __set)
            {
                __set(obj, value);

                return;
            }

            _set(obj, XConvert<T, TValue>.Convert(value));
        }
    }
}