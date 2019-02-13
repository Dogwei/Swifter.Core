using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.Reflection
{
    sealed class XStaticFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        private IntPtr address;

        public bool CanRead => true;

        public bool CanWrite => true;

        public int Order => 999;

        public Type FieldType => typeof(TValue);

        internal override void Initialize(System.Reflection.FieldInfo fieldInfo, XBindingFlags flags)
        {
            base.Initialize(fieldInfo, flags);

            address = (IntPtr)((long)TypeHelper.GetTypeStaticMemoryAddress(fieldInfo.DeclaringType) + TypeHelper.OffsetOf(fieldInfo));
        }

        public override object GetValue(object obj)
        {
            return Pointer.GetValue<TValue>(address);
        }

        public override void SetValue(object obj, object value)
        {
            Pointer.SetValue(address, (TValue)value);
        }

        public override object GetValue(TypedReference typedRef)
        {
            return Pointer.GetValue<TValue>(address);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Pointer.SetValue(address, (TValue)value);
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.Content.WriteValue(valueWriter, Pointer.GetValue<TValue>(address));
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Pointer.SetValue(address, ValueInterface<TValue>.Content.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TValue>.Int64TypeHandle)
            {
                return Pointer.GetValue<T>(address);
            }

            return XConvert<TValue, T>.Convert(Pointer.GetValue<TValue>(address));
        }

        public void WriteValue<T>(object obj, T value)
        {
            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TValue>.Int64TypeHandle)
            {
                Pointer.SetValue(address, value);

                return;
            }

            Pointer.SetValue(address, XConvert<T, TValue>.Convert(value));
        }
    }
}