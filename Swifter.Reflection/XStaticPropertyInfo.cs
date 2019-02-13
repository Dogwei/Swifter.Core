using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
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

        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            return _get();
        }

        public override object GetValue(TypedReference typedRef)
        {
            Assert(CanRead, "get");

            return _get();
        }

        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            _set((TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Assert(CanWrite, "set");

            _set((TValue)value);
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(CanRead, "get");

            ValueInterface<TValue>.Content.WriteValue(valueWriter, _get());
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(CanWrite, "set");

            _set(ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            Assert(CanRead, "get");

            if (_get is XStaticGetValueHandler<T> __get)
            {
                return __get();
            }

            return XConvert<TValue, T>.Convert(_get());
        }

        public void WriteValue<T>(object obj, T value)
        {
            Assert(CanWrite, "set");

            if (_set is XStaticSetValueHandler<T> __set)
            {
                __set(value);

                return;
            }

            _set(XConvert<T, TValue>.Convert(value));
        }
    }
}