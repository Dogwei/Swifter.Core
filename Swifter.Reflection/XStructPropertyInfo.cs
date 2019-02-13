using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XStructPropertyInfo<TStruct, TValue> : XPropertyInfo, IXFieldRW where TStruct : struct
    {
        XStructGetValueHandler<TStruct, TValue> _get;
        XStructSetValueHandler<TStruct, TValue> _set;

        Type declaringType;

        internal override void Initialize(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.Initialize(propertyInfo, flags);

            declaringType = propertyInfo.DeclaringType;

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

            declaringType = propertyInfo.DeclaringType;

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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        ref TStruct GetRef(object obj)
        {
            return ref Tools.Pointer.Box<TStruct>(TypeHelper.GetStructAddress(obj));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        ref TStruct GetRefCheck(object obj)
        {
            if (obj.GetType() != declaringType)
            {
                throw new TargetException(nameof(obj));
            }

            return ref GetRef(obj);
        }

        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            return _get(ref GetRefCheck(obj));
        }

        public override object GetValue(TypedReference typedRef)
        {
            Assert(CanRead, "get");

            if (__reftype(typedRef) != declaringType)
            {
                throw new TargetException(nameof(typedRef));
            }

            return _get(ref __refvalue(typedRef, TStruct));
        }
        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            _set(ref GetRefCheck(obj), (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Assert(CanWrite, "set");

            if (__reftype(typedRef) != declaringType)
            {
                throw new TargetException(nameof(typedRef));
            }

            _set(ref __refvalue(typedRef, TStruct), (TValue)value);
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(CanRead, "get");

            ValueInterface<TValue>.Content.WriteValue(valueWriter, _get(ref GetRef(obj)));
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(CanWrite, "set");

            _set(ref GetRef(obj), ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            Assert(CanRead, "get");

            if (_get is XStructGetValueHandler<TStruct, T> __get)
            {
                return __get(ref GetRef(obj));
            }

            return XConvert<TValue, T>.Convert(_get(ref GetRef(obj)));
        }

        public void WriteValue<T>(object obj, T value)
        {
            Assert(CanWrite, "set");

            if (_set is XStructSetValueHandler<TStruct, T> __set)
            {
                __set(ref GetRef(obj), value);

                return;
            }

            _set(ref GetRef(obj), XConvert<T, TValue>.Convert(value));
        }
    }
}