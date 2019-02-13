using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XAttributedFieldRW : IXFieldRW
    {
        internal readonly IXFieldRW fieldRW;
        internal readonly RWFieldAttribute attribute;
        internal readonly bool canRead;
        internal readonly bool canWrite;
        internal readonly string name;

        public XAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute)
        {
            this.fieldRW = fieldRW;
            this.attribute = attribute;

            canRead = !(attribute.Access != RWFieldAccess.RW && attribute.Access != RWFieldAccess.ReadOnly && !fieldRW.CanRead);
            canWrite = !(attribute.Access != RWFieldAccess.RW && attribute.Access != RWFieldAccess.WriteOnly && !fieldRW.CanWrite);

            name = attribute.Name ?? fieldRW.Name;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void Assert(bool err, string name)
        {
            if (!err)
            {
                Throw();
            }

            void Throw()
            {
                throw new MemberAccessException($"Attributed Property '{Name}' Don't access '{name}' method.");
            }
        }

        public bool CanRead => canRead;

        public bool CanWrite => canWrite;

        public Type FieldType => fieldRW.FieldType;

        public string Name => name;

        public int Order => attribute.Order;
        
        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(canRead, "read");

            fieldRW.OnReadValue(obj, valueWriter);
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(canWrite, "write");

            fieldRW.OnWriteValue(obj, valueReader);
        }

        public T ReadValue<T>(object obj)
        {
            Assert(canRead, "read");

            return fieldRW.ReadValue<T>(obj);
        }

        public void WriteValue<T>(object obj, T value)
        {
            Assert(canWrite, "write");

            fieldRW.WriteValue(obj, value);
        }
    }
}