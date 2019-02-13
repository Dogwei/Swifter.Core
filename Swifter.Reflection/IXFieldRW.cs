using Swifter.Readers;
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

        void OnReadValue(object obj, IValueWriter valueWriter);

        void OnWriteValue(object obj, IValueReader valueReader);

        T ReadValue<T>(object obj);

        void WriteValue<T>(object obj, T value);
    }
}
