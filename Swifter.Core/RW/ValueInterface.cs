using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// ValueInterface/<T/> 提供在 ValueReader 中读取指定类型的值或在 ValueWriter 中写入指定类型的值。
    /// 此类型提供泛型方法，效率更高。
    /// <typeparam name="T">值的类型</typeparam>
    /// </summary>
    public sealed class ValueInterface<T> : ValueInterface
    {
        /// <summary>
        /// 此类型的具体读写方法实现。
        /// </summary>
        public static IValueInterface<T> Content;

        /// <summary>
        /// 表示是否使用用户自定义的读写方法，如果为 True, FastObjectRW 将不优化基础类型的读写。
        /// 基础类型型请参见枚举 BasicTypes。
        /// </summary>
        public static bool IsNoModify;

        static ValueInterface()
        {
            if (!StaticConstructorInvoked)
            {
                GetInterface(typeof(int));
            }

            IsNoModify = true;

            if (Content != null)
            {
                return;
            }

            if (StaticConstructorInvoking)
            {
                return;
            }

            IValueInterface<T> valueInterface;

            for (int i = Mapers.Count - 1; i >= 0; --i)
            {
                valueInterface = Mapers[i].TryMap<T>();

                if (valueInterface != null)
                {
                    Content = valueInterface;

                    return;
                }
            }

            var type = typeof(T);

            if (typeof(Type).IsAssignableFrom(type))
            {
                Content = (IValueInterface<T>)Activator.CreateInstance((typeof(TypeInfoInterface<>)).MakeGenericType(type));

                return;
            }

            if (typeof(IDataReader).IsAssignableFrom(type))
            {
                foreach (var item in type.GetInterfaces())
                {
                    if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IDataReader<>))
                    {
                        var keyType = item.GetGenericArguments()[0];

                        Content = (IValueInterface<T>)Activator.CreateInstance((typeof(IDataReaderInterface<,>)).MakeGenericType(type, keyType));

                        return;
                    }
                }
            }

            if (type.IsArray)
            {
                Content = (IValueInterface<T>)Activator.CreateInstance(defaultArrayInterfaceType.MakeGenericType(type));

                return;
            }

            if (type.IsValueType && type.IsGenericType)
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(T));

                if (underlyingType != null && !ReferenceEquals(type, underlyingType))
                {
                    Content = (IValueInterface<T>)Activator.CreateInstance(nullableInterfaceType.MakeGenericType(underlyingType));

                    return;
                }
            }

            if (type.IsInterface || type.IsAbstract || typeof(IFormattable).IsAssignableFrom(type))
            {
                Content = (IValueInterface<T>)Activator.CreateInstance(unknowInterfaceType.MakeGenericType(type));

                return;
            }

            Content = (IValueInterface<T>)Activator.CreateInstance(defaultObjectInterfaceType.MakeGenericType(type));
        }

        /// <summary>
        /// 非泛型读取值方法。
        /// </summary>
        /// <param name="valueReader">值读取器。</param>
        /// <returns>返回一个 T 类型的值。</returns>
        public override object ReadValue(IValueReader valueReader)
        {
            return Content.ReadValue(valueReader);
        }

        /// <summary>
        /// 非泛型写入值方法。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">T 类型的值</param>
        public override void WriteValue(IValueWriter valueWriter, object value)
        {
            Content.WriteValue(valueWriter, (T)value);
        }

        /// <summary>
        /// 参数反转写入值方法。
        /// </summary>
        /// <param name="value">T 类型的值</param>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ReverseWriteValue(T value, IValueWriter valueWriter)
        {
            Content.WriteValue(valueWriter, value);
        }
    }

    /// <summary>
    /// ValueInterface 提供在 ValueReader 中读取指定类型的值或在 ValueWriter 中写入指定类型的值。
    /// 此类提供非泛型方法。
    /// </summary>
    public abstract class ValueInterface
    {
        internal static readonly List<IValueInterfaceMaper> Mapers;
        internal static readonly object MapersLock;
        internal static readonly bool StaticConstructorInvoked = false;
        internal static readonly bool StaticConstructorInvoking = false;
        internal static readonly IdCache<ValueInterface> TypedCache;
        internal static readonly object TypedCacheLock;

        internal static Type defaultObjectInterfaceType;
        internal static Type defaultArrayInterfaceType;
        internal static Type nullableInterfaceType;
        internal static Type unknowInterfaceType;

        /// <summary>
        /// 获取或设置默认的对象类型读写接口类型。
        /// 如果要设置此类型要满足以下条件
        /// 1: 类型必须是可以实例化并且具有公开的无参构造函数。
        /// 2: 必须继承 IValueInterface/<T/> 接口。
        /// 3: 必须是泛型类型，有且只有一个泛型参数，泛型参数与 IValueInterface/<T/> 的泛型参数对应。
        /// </summary>
        public static Type DefaultObjectInterfaceType
        {
            get
            {
                return defaultObjectInterfaceType;
            }
            set
            {
                if (value.IsInterface)
                {
                    throw new ArgumentException("Type can't be a interface.", nameof(DefaultObjectInterfaceType));
                }

                if (value.IsAbstract)
                {
                    throw new ArgumentException("Type can't be a abstract class.", nameof(DefaultObjectInterfaceType));
                }

                if (!value.ContainsGenericParameters || value.GetGenericArguments().Length != 1)
                {
                    throw new ArgumentException("Type must only one generic type.", nameof(DefaultObjectInterfaceType));
                }

                if (!typeof(IValueInterface<TestClass>).IsAssignableFrom(value.MakeGenericType(typeof(TestClass))))
                {
                    throw new ArgumentException("Type must extend IValueInterface<T>.", nameof(DefaultObjectInterfaceType));
                }

                defaultObjectInterfaceType = value;
            }
        }

        /// <summary>
        /// 获取或设置默认的数组类型读写接口类型。
        /// 如果要设置此类型要满足以下条件
        /// 1: 类型必须是可以实例化并且具有公开的无参构造函数。
        /// 2: 必须继承 IValueInterface/<T/> 接口。
        /// 3: 必须是泛型类型，有且只有一个泛型参数，泛型参数与 IValueInterface/<T/> 的泛型参数对应。
        /// </summary>
        public static Type DefaultArrayInterfaceType
        {
            get
            {
                return defaultArrayInterfaceType;
            }
            set
            {
                if (value.IsInterface)
                {
                    throw new ArgumentException("Type can't be a interface.", nameof(DefaultArrayInterfaceType));
                }

                if (value.IsAbstract)
                {
                    throw new ArgumentException("Type can't be a abstract class.", nameof(DefaultArrayInterfaceType));
                }

                if (!value.ContainsGenericParameters || value.GetGenericArguments().Length != 1)
                {
                    throw new ArgumentException("Type must only one generic type.", nameof(DefaultArrayInterfaceType));
                }

                if (!typeof(IValueInterface<TestClass[]>).IsAssignableFrom(value.MakeGenericType(typeof(TestClass[]))))
                {
                    throw new ArgumentException("Type must extend IValueInterface<T>.", nameof(DefaultArrayInterfaceType));
                }

                defaultArrayInterfaceType = value;
            }
        }

        private class TestClass
        {

        }


        static ValueInterface()
        {
            StaticConstructorInvoked = true;
            StaticConstructorInvoking = true;

            Mapers = new List<IValueInterfaceMaper>();
            MapersLock = new object();

            DefaultObjectInterfaceType = typeof(FastObjectInterface<>);
            DefaultArrayInterfaceType = typeof(ArrayInterface<>);
            nullableInterfaceType = typeof(NullableInterface<>);
            unknowInterfaceType = typeof(UnknowTypeInterface<>);

            TypedCache = new IdCache<ValueInterface>();
            TypedCacheLock = new object();

            ValueInterface<bool>.Content = new BooleanInterface();
            ValueInterface<sbyte>.Content = new SByteInterface();
            ValueInterface<short>.Content = new Int16Interface();
            ValueInterface<int>.Content = new Int32Interface();
            ValueInterface<long>.Content = new Int64Interface();
            ValueInterface<byte>.Content = new ByteInterface();
            ValueInterface<ushort>.Content = new UInt16Interface();
            ValueInterface<uint>.Content = new UInt32Interface();
            ValueInterface<ulong>.Content = new UInt64Interface();
            ValueInterface<char>.Content = new CharInterface();
            ValueInterface<float>.Content = new SingleInterface();
            ValueInterface<double>.Content = new DoubleInterface();
            ValueInterface<decimal>.Content = new DecimalInterface();
            ValueInterface<string>.Content = new StringInterface();
            ValueInterface<object>.Content = new UnknowTypeInterface<object>();
            ValueInterface<DateTime>.Content = new DateTimeInterface();
            ValueInterface<TimeSpan>.Content = new TimeSpanInterface();
            ValueInterface<IntPtr>.Content = new IntPtrInterface();

            Mapers.Add(new CollectionInterfaceMaper());
            Mapers.Add(new DictionaryInterfaceMaper());
            Mapers.Add(new ListInterfaceMaper());
            Mapers.Add(new TableRWInternalMaper());

            StaticConstructorInvoking = false;
        }

        /// <summary>
        /// 添加一个类型与 ValueInterface 的匹配器。
        /// 此匹配器可以自定义类型的读写方法。
        /// 后加入的匹配器优先级高。
        /// </summary>
        /// <param name="maper">类型与 ValueInterface 的匹配器</param>
        public static void AddMaper(IValueInterfaceMaper maper)
        {
            if (maper == null)
            {
                throw new ArgumentNullException(nameof(maper));
            }

            Mapers.Add(maper);
        }

        /// <summary>
        /// 非泛型方式获取指定类型的 ValueInterface ，此方式效率并不高。
        /// 如果是已知类型，请考虑使用泛型方式 ValueInterface/<TYPE/>.Content 获取。
        /// 如果是未知类型的实例，请考虑使用 ValueInterface.GetInterface(object) 获取。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 ValueInterface 实例。</returns>
        public static ValueInterface GetInterface(Type type)
        {
            ValueInterface result;

            var typeId = (long)type.TypeHandle.Value;

            if (!TypedCache.TryGetValue(typeId, out result))
            {
                lock (TypedCacheLock)
                {
                    if (!TypedCache.TryGetValue(typeId, out result))
                    {
                        var resultType = typeof(ValueInterface<>).MakeGenericType(type);

                        result = (ValueInterface)Activator.CreateInstance(resultType);

                        TypedCache.Add(typeId, result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 非泛型方式获取实例的类型的 ValueInterface ，此方式效率比 ValueInterface.GetInterface(Tyoe) 高，但比 ValueInterface/<T/>.Content 低。
        /// </summary>
        /// <param name="obj">指定一个实例，此实例不能为 Null。</param>
        /// <returns>返回一个 ValueInterface 实例。</returns>
        public static ValueInterface GetInterface(object obj)
        {
            ValueInterface result;

            var typeId = (long)Pointer.GetTypeHandle(obj);

            if (!TypedCache.TryGetValue(typeId, out result))
            {
                lock (TypedCacheLock)
                {
                    if (!TypedCache.TryGetValue(typeId, out result))
                    {
                        var type = obj.GetType();

                        var resultType = typeof(ValueInterface<>).MakeGenericType(type);

                        result = (ValueInterface)Activator.CreateInstance(resultType);

                        TypedCache.Add(typeId, result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 在 IValueReader 中读取该类型的值。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        public abstract object ReadValue(IValueReader valueReader);

        /// <summary>
        /// 在 IValueWriter 中写入该类型的值。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的值</param>
        public abstract void WriteValue(IValueWriter valueWriter, object value);
    }

    /// <summary>
    /// 提供某一类型在 IValueReader 中读取值和在 IValueWriter 写入值的方法。
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public interface IValueInterface<T>
    {
        /// <summary>
        /// 在 IValueReader 中读取该类型的值。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        T ReadValue(IValueReader valueReader);

        /// <summary>
        /// 在 IValueWriter 中写入该类型的值。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的值</param>
        void WriteValue(IValueWriter valueWriter, T value);
    }

    /// <summary>
    /// 提供类型与 IValueInterface 的匹配器。
    /// 实现它，并使用 ValueInterface.AddMaper 添加它的实例即可自定义类型的读写方法。
    /// </summary>
    public interface IValueInterfaceMaper
    {
        /// <summary>
        /// 类型与 IValueInterface 的匹配方法。
        /// 匹配成功则返回实例，不成功则返回 Null。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回一个 IValueInterface/<T/> 实例</returns>
        IValueInterface<T> TryMap<T>();
    }

    internal sealed class BooleanInterface : IValueInterface<bool>
    {
        public bool ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadBoolean();
        }

        public void WriteValue(IValueWriter valueWriter, bool value)
        {
            valueWriter.WriteBoolean(value);
        }
    }

    internal sealed class SByteInterface : IValueInterface<sbyte>
    {
        public sbyte ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadSByte();
        }

        public void WriteValue(IValueWriter valueWriter, sbyte value)
        {
            valueWriter.WriteSByte(value);
        }
    }

    internal sealed class Int16Interface : IValueInterface<short>
    {
        public short ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt16();
        }

        public void WriteValue(IValueWriter valueWriter, short value)
        {
            valueWriter.WriteInt16(value);
        }
    }

    internal sealed class Int32Interface : IValueInterface<int>
    {
        public int ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt32();
        }

        public void WriteValue(IValueWriter valueWriter, int value)
        {
            valueWriter.WriteInt32(value);
        }
    }

    internal sealed class Int64Interface : IValueInterface<long>
    {
        public long ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt64();
        }

        public void WriteValue(IValueWriter valueWriter, long value)
        {
            valueWriter.WriteInt64(value);
        }
    }

    internal sealed class ByteInterface : IValueInterface<byte>
    {
        public byte ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadByte();
        }

        public void WriteValue(IValueWriter valueWriter, byte value)
        {
            valueWriter.WriteByte(value);
        }
    }

    internal sealed class UInt16Interface : IValueInterface<ushort>
    {
        public ushort ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt16();
        }

        public void WriteValue(IValueWriter valueWriter, ushort value)
        {
            valueWriter.WriteUInt16(value);
        }
    }

    internal sealed class UInt32Interface : IValueInterface<uint>
    {
        public uint ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt32();
        }

        public void WriteValue(IValueWriter valueWriter, uint value)
        {
            valueWriter.WriteUInt32(value);
        }
    }

    internal sealed class UInt64Interface : IValueInterface<ulong>
    {

        public ulong ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt64();
        }

        public void WriteValue(IValueWriter valueWriter, ulong value)
        {
            valueWriter.WriteUInt64(value);
        }
    }

    internal sealed class CharInterface : IValueInterface<char>
    {
        public char ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadChar();
        }

        public void WriteValue(IValueWriter valueWriter, char value)
        {
            valueWriter.WriteChar(value);
        }
    }

    internal sealed class SingleInterface : IValueInterface<float>
    {
        public float ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadSingle();
        }

        public void WriteValue(IValueWriter valueWriter, float value)
        {
            valueWriter.WriteSingle(value);
        }
    }

    internal sealed class DoubleInterface : IValueInterface<double>
    {
        public double ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDouble();
        }

        public void WriteValue(IValueWriter valueWriter, double value)
        {
            valueWriter.WriteDouble(value);
        }
    }

    internal sealed class DecimalInterface : IValueInterface<decimal>
    {
        public decimal ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDecimal();
        }

        public void WriteValue(IValueWriter valueWriter, decimal value)
        {
            valueWriter.WriteDecimal(value);
        }
    }

    internal sealed class StringInterface : IValueInterface<string>
    {
        public string ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadString();
        }

        public void WriteValue(IValueWriter valueWriter, string value)
        {
            valueWriter.WriteString(value);
        }
    }

    internal sealed class DateTimeInterface : IValueInterface<DateTime>
    {
        public DateTime ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDateTime();
        }

        public void WriteValue(IValueWriter valueWriter, DateTime value)
        {
            valueWriter.WriteDateTime(value);
        }
    }

    internal sealed class IntPtrInterface : IValueInterface<IntPtr>
    {
        public IntPtr ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<IntPtr> intPtrReader)
            {
                return intPtrReader.ReadValue();
            }

            return (IntPtr)valueReader.ReadInt64();
        }

        public void WriteValue(IValueWriter valueWriter, IntPtr value)
        {
            if (valueWriter is IValueWriter<IntPtr> intPtrWriter)
            {
                intPtrWriter.WriteValue(value);
            }
            else
            {
                valueWriter.WriteInt64((long)value);
            }
        }
    }

    internal sealed class ArrayInterface<T> : IValueInterface<T>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var arrayWriter = ArrayRW<T>.Create();

            valueReader.ReadArray(arrayWriter);

            return arrayWriter.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var arrayWriter = ArrayRW<T>.Create();

            arrayWriter.Initialize(value);

            valueWriter.WriteArray(arrayWriter);
        }
    }

    internal sealed class FastObjectInterface<T> : IValueInterface<T>
    {
        private static readonly bool CheckChildrenInstance = typeof(T).IsClass && (!typeof(T).IsSealed);
        private static readonly long Int64TypeHandle = TypeInfo<T>.Int64TypeHandle;

        public T ReadValue(IValueReader valueReader)
        {
            var fastObjectRW = FastObjectRW<T>.Create();

            valueReader.ReadObject(fastObjectRW);

            return fastObjectRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            if (CheckChildrenInstance && Int64TypeHandle != (long)TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.GetInterface(value).WriteValue(valueWriter, value);

                return;
            }

            var fastObjectRW = FastObjectRW<T>.Create();

            fastObjectRW.Initialize(value);
            
            valueWriter.WriteObject(fastObjectRW);
        }
    }

    internal sealed class UnknowTypeInterface<T> : IValueInterface<T>
    {
        private static readonly long Int64TypeHandle = TypeInfo<T>.Int64TypeHandle;

        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T>)
            {
                return ((IValueReader<T>)valueReader).ReadValue();
            }

            object directValue = valueReader.DirectRead();

            if (directValue is T || directValue == null)
            {
                return (T)directValue;
            }
            
            return DConvert<T>.Convert(directValue);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (value is IFormattable)
            {
                valueWriter.DirectWrite(value);

                return;
            }

            if (value is string)
            {
                valueWriter.WriteString((string)(object)value);

                return;
            }

            if (valueWriter is IValueWriter<T>)
            {
                ((IValueWriter<T>)valueWriter).WriteValue(value);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            if (Int64TypeHandle != (long)TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.GetInterface(value).WriteValue(valueWriter, value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }

    internal sealed class TimeSpanInterface : IValueInterface<TimeSpan>
    {
        public TimeSpan ReadValue(IValueReader valueReader)
        {
            object directValue = valueReader.DirectRead();

            if (directValue is TimeSpan)
            {
                return (TimeSpan)directValue;
            }

            if (directValue is string)
            {
                return TimeSpan.Parse((string)directValue);
            }
            
            return DConvert<TimeSpan>.Convert(directValue);
        }

        public void WriteValue(IValueWriter valueWriter, TimeSpan value)
        {
            valueWriter.DirectWrite(value);
        }
    }

    internal sealed class NullableInterface<T> : IValueInterface<T?> where T : struct
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader.GetBasicType() == BasicTypes.Null)
            {
                var directValue = valueReader.DirectRead();

                if (directValue == null)
                {
                    return null;
                }
                
                return DConvert<T>.Convert(directValue);
            }

            return ValueInterface<T>.Content.ReadValue(valueReader);
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);
            }
            else
            {
                ValueInterface<T>.Content.WriteValue(valueWriter, value.Value);
            }
        }
    }

    internal sealed class IDataReaderInterface<T, Key> : IValueInterface<T> where T : IDataReader<Key>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var value = valueReader.DirectRead();

            if (value is T tValue)
            {
                return tValue;
            }

            throw new NotSupportedException(StringHelper.Format("Cannot read a '{0}', It is a data reader.", typeof(T).Name));
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            switch (TypeInfo<Key>.BasicType)
            {
                case BasicTypes.SByte:
                case BasicTypes.Int16:
                case BasicTypes.Int32:
                case BasicTypes.Int64:
                case BasicTypes.Byte:
                case BasicTypes.UInt16:
                case BasicTypes.UInt32:
                case BasicTypes.UInt64:
                    valueWriter.WriteArray(value.As<int>());
                    break;
                case BasicTypes.Char:
                case BasicTypes.String:
                    valueWriter.WriteObject(value.As<string>());
                    break;
                default:
                    throw new NotSupportedException(StringHelper.Format("Cannot write a '{0}', It's Key not supported.", typeof(T).Name));
            }
        }
    }

    internal sealed class TypeInfoInterface<T> : IValueInterface<T> where T : Type
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            if (valueReader is IValueReader<Type> typeReader)
            {
                return (T)typeReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value == null)
            {
                return null;
            }

            if (value is T tValue)
            {
                return tValue;
            }

            if (value is string sValue)
            {
                return (T)Type.GetType(sValue);
            }

            throw new NotSupportedException(StringHelper.Format("Cannot Read a 'TypeInfo' by '{0}'.", value.ToString()));
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            if (valueWriter is IValueWriter<Type> typeWriter)
            {
                typeWriter.WriteValue(value);

                return;
            }

            valueWriter.WriteString(value.AssemblyQualifiedName);
        }
    }
}