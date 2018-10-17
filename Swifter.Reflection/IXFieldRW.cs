using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.Reflection
{
    interface IXFieldRW
    {
        int Order { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        string Name { get; }

        Type FieldType { get; }

        BasicTypes BasicType { get; }

        void WriteTo(XObjectRW objectRW, IDataWriter<string> dataWriter);

        void OnReadValue(XObjectRW objectRW, IValueWriter valueWriter);

        void OnWriteValue(XObjectRW objectRW, IValueReader valueReader);

        T ReadValue<T>(XObjectRW objectRW);

        void WriteValue<T>(XObjectRW objectRW, T value);
        
        XFieldValueRW CreateRW(XObjectRW objectRW);
    }
}
