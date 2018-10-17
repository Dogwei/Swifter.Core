using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.Reflection
{
    sealed class XAttributedFieldRW : IXFieldRW
    {
        private readonly IXFieldRW fieldRW;
        private readonly RWFieldAttribute attribute;

        public XAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute)
        {
            this.fieldRW = fieldRW;
            this.attribute = attribute;
        }

        void assert(bool err, string name)
        {
            if (!err)
            {
                throw new MemberAccessException(StringHelper.Format("Attributed Property '{0}' Don't access '{1}' method.", Name, name));
            }
        }

        public bool CanRead
        {
            get
            {
                if (attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.ReadOnly)
                {
                    return fieldRW.CanRead;
                }

                return false;
            }
        }

        public bool CanWrite
        {
            get
            {
                if (attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.WriteOnly)
                {
                    return fieldRW.CanWrite;
                }

                return false;
            }
        }

        public Type FieldType => fieldRW.FieldType;

        public BasicTypes BasicType => fieldRW.BasicType;

        public string Name
        {
            get
            {
                if (attribute.Name != null)
                {
                    return attribute.Name;
                }

                return fieldRW.Name;
            }
        }

        public int Order => attribute.Order;

        public XFieldValueRW CreateRW(XObjectRW objectRW)
        {
            return fieldRW.CreateRW(objectRW).SetFieldRW(this);
        }

        public void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter)
        {
            assert(attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.ReadOnly, "read");

            fieldRW.OnReadValue(objectRW, valueWriter);
        }

        public void OnWriteValue(XObjectRW objectRW, IValueReader valueReader)
        {
            assert(attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.WriteOnly, "write");

            fieldRW.OnWriteValue(objectRW, valueReader);
        }

        public T ReadValue<T>(XObjectRW objectRW)
        {
            assert(attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.ReadOnly, "get");

            return fieldRW.ReadValue<T>(objectRW);
        }

        public void WriteValue<T>(XObjectRW objectRW, T value)
        {
            assert(attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.WriteOnly, "write");

            fieldRW.WriteValue(objectRW, value);
        }

        public void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter)
        {
            fieldRW.WriteTo(objectRW, dataWriter);
        }
    }
}