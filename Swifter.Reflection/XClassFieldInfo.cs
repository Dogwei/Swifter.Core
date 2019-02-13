using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XClassFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        private long offset;
        private Type declaringType;

        public int Order => RWFieldAttribute.DefaultOrder;

        public bool CanRead => true;

        public bool CanWrite => true;

        public Type FieldType => typeof(TValue);

        internal override void Initialize(System.Reflection.FieldInfo fieldInfo, XBindingFlags flags)
        {
            offset = TypeHelper.OffsetOf(fieldInfo) + TypeHelper.ObjectHandleSize;
            declaringType = fieldInfo.DeclaringType;

            base.Initialize(fieldInfo, flags);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        IntPtr GetAddress(object obj) => (IntPtr)((long)Pointer.UnBox(obj) + offset);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        IntPtr GetAddressCheck(object obj)
        {
            if (declaringType.IsInstanceOfType(obj))
            {
                throw new System.Reflection.TargetException(nameof(obj));
            }

            return GetAddress(obj);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        unsafe IntPtr GetAddress(TypedReference typedRef)
        {
            if (declaringType != __reftype(typedRef))
            {
                throw new System.Reflection.TargetException(nameof(typedRef));
            }

            return (IntPtr)((long)*(IntPtr*)Pointer.UnBox(typedRef) + offset);
        }
        
        public override object GetValue(object obj)
        {
            return Pointer.GetValue<TValue>(GetAddressCheck(obj));
        }

        public override unsafe object GetValue(TypedReference typedRef)
        {

            return Pointer.GetValue<TValue>(GetAddress(typedRef));
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.Content.WriteValue(valueWriter, Pointer.GetValue<TValue>(GetAddress(obj)));
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Pointer.SetValue(GetAddress(obj), ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TValue>.Int64TypeHandle)
            {
                return Pointer.GetValue<T>(GetAddress(obj));
            }

            return XConvert<TValue, T>.Convert(Pointer.GetValue<TValue>(GetAddress(obj)));
        }

        public override void SetValue(object obj, object value)
        {
            Pointer.SetValue(GetAddressCheck(obj), (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Pointer.SetValue(GetAddress(typedRef), (TValue)value);
        }

        public void WriteValue<T>(object obj, T value)
        {
            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TValue>.Int64TypeHandle)
            {
                Pointer.SetValue(GetAddress(obj), value);

                return;
            }

            Pointer.SetValue(GetAddress(obj), XConvert<T, TValue>.Convert(value));
        }
    }
}