using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
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

        public Type FieldType => typeof(TValue);
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void ThrowCannotSetException()
        {
            throw new InvalidOperationException(StringHelper.Format("Cannot set value of the const field '{0}.{1}'.", FieldInfo.DeclaringType.Name, FieldInfo.Name));
        }

        public override object GetValue(object obj)
        {
            return value;
        }

        public override void SetValue(object obj, object value)
        {
            ThrowCannotSetException();
        }

        public override object GetValue(TypedReference typedRef)
        {
            return value;
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            ThrowCannotSetException();
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.Content.WriteValue(valueWriter, value);
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            ThrowCannotSetException();
        }

        public T ReadValue<T>(object obj)
        {
            if (this is XLiteralFieldInfo<T> me)
            {
                return me.value;
            }

            return XConvert<TValue, T>.Convert(value);
        }

        public void WriteValue<T>(object obj, T value)
        {
            ThrowCannotSetException();
        }
    }
}