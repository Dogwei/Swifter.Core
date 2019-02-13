using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.RW
{
    /// <summary>
    /// FastObjectRW 基于 Emit 实现的几乎完美效率的对象读写器。
    /// </summary>
    public static class FastObjectRW
    {
        static readonly TypeCache<FastObjectRWOptions> TypeOptions;

        static FastObjectRW()
        {
            TypeOptions = new TypeCache<FastObjectRWOptions>();

            FastObjectRW<ValueType>.CurrentOptions |= FastObjectRWOptions.Field;
        }

        internal static FastObjectRWOptions GetCurrentOptions(Type currentType)
        {
            if (TypeOptions.TryGetValue(currentType, out var value))
            {
                return value;
            }

            return TypeOptions.LockGetOrAdd(currentType, type =>
            {
                if (type == typeof(object))
                {
                    return DefaultOptions;
                }

                return GetCurrentOptions(type.BaseType);
            });
        }

        internal static void SetCurrentOptions(Type currentType, FastObjectRWOptions fastObjectRWOptions)
        {
            TypeOptions.LockSetOrAdd(currentType, fastObjectRWOptions);
        }

        /// <summary>
        /// FastObjectRW 全局默认配置。
        /// </summary>
        public static 
            FastObjectRWOptions DefaultOptions { get; set; } = 
            FastObjectRWOptions.NotFoundException | 
            FastObjectRWOptions.CannotGetException | 
            FastObjectRWOptions.CannotSetException | 
            FastObjectRWOptions.BasicTypeDirectCallMethod |
            FastObjectRWOptions.Property | 
            FastObjectRWOptions.InheritedMembers;
    }

    /// <summary>
    /// FastObjectRW 基于 Emit 实现的几乎完美效率的对象读写器。
    /// </summary>
    /// <typeparam name="T">数据源对象的类型</typeparam>
    public abstract class FastObjectRW<T> : IDataRW<string>, IDirectContent, IInitialize<T>
    {
        internal static bool isInitialized = false;
        internal static FastObjectRWOptions currentOptions = FastObjectRW.GetCurrentOptions(typeof(T));

        /// <summary>
        /// 读取或设置该类型的 FastObjectRWOptions 枚举配置项。
        /// 如果该类型已经初始化完成，则无法设置该值，且发生异常。
        /// </summary>
        public static FastObjectRWOptions CurrentOptions
        {
            get
            {
                return currentOptions;
            }
            set
            {
                if (currentOptions == value)
                {
                    return;
                }

                if (isInitialized)
                {
                    throw new InvalidOperationException("Invalid modification option After type has been initialized.");
                }

                FastObjectRW.SetCurrentOptions(typeof(T), value);

                currentOptions = value;
            }
        }

        /// <summary>
        /// 创建 FastObjectRW 实例。
        /// </summary>
        /// <returns>返回 FastObjectRW 实例</returns>
        public static FastObjectRW<T> Create()
        {
            return StaticFastObjectRW<T>.Creater.Create();
        }

        /// <summary>
        /// 数据源，此字段提供给 Emit 实现类使用。
        /// </summary>
        internal protected T content;

        /// <summary>
        /// 获取数据源。
        /// </summary>
        public T Content => content;

        /// <summary>
        /// 调用默认无参的构造函数初始化数据源。
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 设置数据源。
        /// </summary>
        /// <param name="content">数据源</param>
        public void Initialize(T content)
        {
            this.content = content;
        }

        /// <summary>
        /// 调用默认无参的构造函数初始化数据源。
        /// </summary>
        /// <param name="capacity">不使用此参数</param>
        public void Initialize(int capacity)
        {
            Initialize();
        }

        /// <summary>
        /// 将指定名称的成员的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueWriter">值写入器</param>
        public abstract void OnReadValue(string key, IValueWriter valueWriter);

        /// <summary>
        /// 将值读取器中的值写入到指定名称的成员中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueReader">值读取器</param>
        public abstract void OnWriteValue(string key, IValueReader valueReader);

        /// <summary>
        /// 将数据源中的所有成员写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public abstract void OnReadAll(IDataWriter<string> dataWriter);

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public abstract void OnWriteAll(IDataReader<string> dataReader);

        /// <summary>
        /// 对数据源中的所有成员进行筛选，并将满足筛选的结果写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public abstract void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter);

        /// <summary>
        /// 获取指定名称的成员的值读写器。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <returns>返回值读写器</returns>
        public ValueCopyer<string> this[string key] => new ValueCopyer<string>(this, key);

        /// <summary>
        /// 获取该类型所有的成员。
        /// </summary>
        public IEnumerable<string> Keys => ArrayHelper.CreateArrayIterator(StaticFastObjectRW<T>.StringKeys);

        /// <summary>
        /// 获取该类型所有的成员的数量。
        /// </summary>
        public int Count => StaticFastObjectRW<T>.StringKeys.Length;

        object IDirectContent.DirectContent { get => content; set => content = (T)value; }

        /// <summary>
        /// 获取数据源的引用根，全局唯一。如果数据源是值类型或 Null，则返回 Null。
        /// </summary>
        public object ReferenceToken => TypeInfo<T>.IsValueType ? null : (object)content;

        IValueRW IDataRW<string>.this[string key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        /// <summary>
        /// 获取该读写器的名称。
        /// </summary>
        /// <returns>返回该读写器的名称</returns>
        public override string ToString() => $"{nameof(FastObjectRW)}<{typeof(T).FullName}>";
    }

    /// <summary>
    /// FastObjectRW 初始化配置。
    /// </summary>
    public enum FastObjectRWOptions
    {
        /// <summary>
        /// 忽略大小写。
        /// </summary>
        IgnoreCase = 0x1,

        /// <summary>
        /// 字段未找到发生异常。
        /// </summary>
        NotFoundException = 0x2,

        /// <summary>
        /// 不能读取发生异常。
        /// </summary>
        CannotGetException = 0x4,

        /// <summary>
        /// 不能写入发送异常。
        /// </summary>
        CannotSetException = 0x8,

        /// <summary>
        /// 基础类型直接调用方法读写，不经过 ValueInterface。
        /// </summary>
        BasicTypeDirectCallMethod = 0x10,

        /// <summary>
        /// 读写器包含属性。
        /// </summary>
        Property = 0x20,

        /// <summary>
        /// 读写器包含字段。
        /// </summary>
        Field = 0x40,

        /// <summary>
        /// 读写器包含继承的成员。
        /// </summary>
        InheritedMembers = 0x80,

        /// <summary>
        /// 在 OnReadAll 中跳过具有类型默认值的成员。
        /// </summary>
        SkipDefaultValue = 0x100
    }

    #region -- Internal Classes --

    /// <summary>
    /// FastObjectRW 创建接口。
    /// 此接口由 Emit 实现。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFastObjectRWCreater<T>
    {
        /// <summary>
        /// 创建该类型的对象读写器。
        /// </summary>
        /// <returns>返回该类型</returns>
        FastObjectRW<T> Create();
    }

    internal interface IFastField
    {
        string Name { get; }

        Type Type { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        bool IsPublic { get; }

        int Order { get; }

        void BeforeValueRead(ILGenerator ilGen);

        void AfterValueRead(ILGenerator ilGen);

        void BeforeValueWrite(ILGenerator ilGen);

        void AfterValueWrite(ILGenerator ilGen);
    }

    internal sealed class CallbackEvents
    {
        public event Action<Type> TypeCreated;

        public void OnTypeCreated(Type type)
        {
            TypeCreated?.Invoke(type);
        }
    }

    internal static class StaticFastObjectRW<T>
    {
        public static readonly string[] StringKeys;

        public static readonly IFastField[] Fields;

        public static readonly IFastObjectRWCreater<T> Creater;

        public static readonly FieldInfo ContentField;

        public static readonly bool HaveNonPublicMember;

        private static void GetFields(Type type, Dictionary<string, IFastField> dicFields, FastObjectRWOptions options, ref bool haveNonPublicMember)
        {
            if ((options & FastObjectRWOptions.InheritedMembers) != 0)
            {
                var baseType = type.BaseType;

                if (baseType != null && baseType != typeof(object))
                {
                    GetFields(baseType, dicFields, options, ref haveNonPublicMember);
                }
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);


            if (fields != null && fields.Length != 0)
            {
                foreach (var item in fields)
                {
                    var attributes = item.GetCustomAttributes(typeof(RWFieldAttribute), true);

                    if (attributes != null && attributes.Length != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedField = new FastField(item, (RWFieldAttribute)attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                dicFields[attributedField.Name] = attributedField;

                                if (!attributedField.IsPublic)
                                {
                                    haveNonPublicMember = true;
                                }
                            }
                        }
                    }
                    else if((options & FastObjectRWOptions.Field) != 0 && item.IsPublic)
                    {
                        var field = new FastField(item, null);

                        dicFields[field.Name] = field;
                    }
                }
            }


            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (properties != null && properties.Length != 0)
            {
                foreach (var item in properties)
                {
                    var indexParameters = item.GetIndexParameters();

                    if (indexParameters != null && indexParameters.Length != 0)
                    {
                        /* Ignore Indexer. */
                        continue;
                    }

                    var attributes = item.GetCustomAttributes(typeof(RWFieldAttribute), true);

                    if (attributes != null && attributes.Length != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedField = new FastProperty(item, (RWFieldAttribute)attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                dicFields[attributedField.Name] = attributedField;

                                if (!attributedField.IsPublic)
                                {
                                    haveNonPublicMember = true;
                                }
                            }
                        }
                    }
                    else if ((options & FastObjectRWOptions.Property) != 0)
                    {
                        var field = new FastProperty(item, null);

                        if (field.CanRead || field.CanWrite)
                        {
                            dicFields[field.Name] = field;
                        }
                    }
                }
            }
        }

        static StaticFastObjectRW()
        {
            try
            {
                FastObjectRW<T>.isInitialized = true;

                var options = FastObjectRW<T>.currentOptions;

                var fields = new Dictionary<string, IFastField>();

                GetFields(typeof(T), fields, options, ref HaveNonPublicMember);

                Fields = new IFastField[fields.Count];

                fields.Values.CopyTo(Fields, 0);

                Array.Sort(Fields, (x, y) =>
                {
                    var com = x.Order.CompareTo(y.Order);

                    if (com != 0)
                    {
                        return com;
                    }

                    return x.Name.CompareTo(y.Name);
                });

                StringKeys = ArrayHelper.Filter(Fields, (Item) => { return true; }, (Item) => { return Item.Name; });

                ContentField = typeof(FastObjectRW<T>).GetField(nameof(FastObjectRW<T>.content), BindingFlags.Instance | BindingFlags.NonPublic);

                Creater = FastObjectRWFactory.CreateCreater<T>(options);
            }
            catch (Exception e)
            {
                Creater = new ErrorFastObjectRWCreater<T>(e);
            }
        }
        
        internal sealed class FastProperty : IFastField
        {
            readonly PropertyInfo property;
            readonly bool isStatic;
            readonly bool isByRef;
            readonly RWFieldAttribute attribute;
            readonly Type propertyType;
            readonly MethodInfo getMethod;
            readonly MethodInfo setMethod;

            public FastProperty(PropertyInfo property, RWFieldAttribute attribute)
            {
                this.property = property;
                this.attribute = attribute;

                getMethod = property.GetGetMethod(true);
                setMethod = property.GetSetMethod(true);

                isStatic = (getMethod != null && getMethod.IsStatic) || (setMethod != null && setMethod.IsStatic);

                propertyType = property.PropertyType;

                if (propertyType.IsByRef)
                {
                    isByRef = true;

                    propertyType = propertyType.GetElementType();
                }
                else if (propertyType.IsPointer)
                {
                    propertyType = typeof(IntPtr);
                }
            }

            public string Name
            {
                get
                {
                    if (attribute != null && attribute.Name != null)
                    {
                        return attribute.Name;
                    }

                    return property.Name;
                }
            }

            public Type Type
            {
                get
                {
                    return propertyType;
                }
            }

            public bool CanRead
            {
                get
                {
                    if (!property.CanRead)
                    {
                        return false;
                    }

                    var method = property.GetGetMethod(true);

                    if (method == null)
                    {
                        return false;
                    }

                    if (attribute == null)
                    {
                        return method.IsPublic;
                    }

                    return attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.ReadOnly;
                }
            }

            public bool CanWrite
            {
                get
                {
                    MethodInfo method;

                    if (isByRef)
                    {
                        if (!property.CanRead)
                        {
                            return false;
                        }

                        method = property.GetGetMethod(true);
                    }
                    else
                    {
                        if (!property.CanWrite)
                        {
                            return false;
                        }

                        method = property.GetSetMethod(true);
                    }

                    if (method == null)
                    {
                        return false;
                    }

                    if (attribute == null)
                    {
                        return method.IsPublic;
                    }

                    return attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.WriteOnly;
                }
            }

            public bool IsPublic
            {
                get
                {
                    var getMethod = property.GetGetMethod(true);
                    var setMethod = property.GetSetMethod(true);

                    return (getMethod == null || getMethod.IsPublic) && (setMethod == null || setMethod.IsPublic);
                }
            }

            public int Order
            {
                get
                {
                    if (attribute != null)
                    {
                        return attribute.Order;
                    }

                    return RWFieldAttribute.DefaultOrder;
                }
            }

            public void AfterValueRead(ILGenerator ilGen)
            {
                if (isByRef)
                {
                    ilGen.Call(getMethod);

                    ilGen.LoadValue(propertyType);
                }
                else
                {
                    ilGen.Call(getMethod);
                }
            }

            public void AfterValueWrite(ILGenerator ilGen)
            {
                if (isByRef)
                {
                    ilGen.StoreValue(propertyType);
                }
                else
                {
                    ilGen.Call(setMethod);
                }
            }

            public void BeforeValueRead(ILGenerator ilGen)
            {
                LoadContent(ilGen);
            }

            public void BeforeValueWrite(ILGenerator ilGen)
            {
                LoadContent(ilGen);

                if (isByRef)
                {
                    ilGen.Call(getMethod);
                }
            }

            void LoadContent(ILGenerator ilGen)
            {
                if (isStatic)
                {
                    return;
                }

                ilGen.LoadArgument(0);

                if (typeof(T).IsValueType)
                {
                    ilGen.LoadFieldAddress(ContentField);
                }
                else
                {
                    ilGen.LoadField(ContentField);
                }
            }
        }

        internal sealed class FastField : IFastField
        {
            readonly FieldInfo field;
            readonly RWFieldAttribute attribute;

            public FastField(FieldInfo field, RWFieldAttribute attribute)
            {
                this.field = field;
                this.attribute = attribute;
            }

            public string Name
            {
                get
                {
                    if (attribute != null && attribute.Name != null)
                    {
                        return attribute.Name;
                    }

                    return field.Name;
                }
            }

            public Type Type
            {
                get
                {

                    if (field.FieldType.IsPointer || field.FieldType.IsByRef)
                    {
                        return typeof(IntPtr);
                    }

                    return field.FieldType;
                }
            }

            public bool CanRead
            {
                get
                {
                    if (attribute != null)
                    {
                        return attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.ReadOnly;
                    }

                    return field.IsPublic;
                }
            }

            public bool CanWrite
            {
                get
                {
                    if (attribute != null)
                    {
                        return attribute.Access == RWFieldAccess.RW || attribute.Access == RWFieldAccess.WriteOnly;
                    }

                    return field.IsPublic;
                }
            }

            public bool IsPublic
            {
                get
                {
                    return field.IsPublic;
                }
            }

            public int Order
            {
                get
                {
                    if (attribute != null)
                    {
                        return attribute.Order;
                    }

                    return RWFieldAttribute.DefaultOrder;
                }
            }

            public void AfterValueRead(ILGenerator ilGen)
            {
                ilGen.Emit(OpCodes.Ldfld, field);
            }

            public void AfterValueWrite(ILGenerator ilGen)
            {
                ilGen.Emit(OpCodes.Stfld, field);
            }

            public void BeforeValueRead(ILGenerator ilGen)
            {
                LoadContent(ilGen);
            }

            public void BeforeValueWrite(ILGenerator ilGen)
            {
                LoadContent(ilGen);
            }

            void LoadContent(ILGenerator ilGen)
            {
                if (field.IsStatic)
                {
                    return;
                }

                ilGen.LoadArgument(0);

                if (typeof(T).IsValueType)
                {
                    ilGen.Emit(OpCodes.Ldflda, ContentField);
                }
                else
                {
                    ilGen.Emit(OpCodes.Ldfld, ContentField);
                }
            }
        }
    }

    internal sealed class ErrorFastObjectRWCreater<T> : IFastObjectRWCreater<T>
    {
        public Exception InnerException { get; set; }

        public ErrorFastObjectRWCreater(Exception innerException)
        {
            InnerException = innerException;
        }

        public FastObjectRW<T> Create()
        {
            throw new TargetException(StringHelper.Format("Failed to create FastObjectRW of '{0}' type.", typeof(T).FullName), InnerException);
        }
    }

    internal static class FastObjectRWFactory
    {
        public const string RWName = "EmitFastObjectRW";

        public static readonly FieldInfo ValueFilterInfoValueCopyerField = typeof(ValueFilterInfo<string>).GetField(nameof(ValueFilterInfo<string>.ValueCopyer));
        public static readonly FieldInfo ValueFilterInfoKeyField = typeof(ValueFilterInfo<string>).GetField(nameof(ValueFilterInfo<string>.Key));
        public static readonly FieldInfo ValueFilterInfoTypeField = typeof(ValueFilterInfo<string>).GetField(nameof(ValueFilterInfo<string>.Type));

        public static readonly MethodInfo CreateInstanceFromTypeMethod = typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new Type[] { typeof(Type) });
        public static readonly MethodInfo GetTypeFromHandleMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) });

        public static readonly MethodInfo TypeGetTypeFromHandleMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));
        public static readonly MethodInfo IValueFilterFilterMethod = typeof(IValueFilter<string>).GetMethod(nameof(IValueFilter<string>.Filter));
        public static readonly MethodInfo ValueCopyerWriteToMethod = typeof(ValueCopyer).GetMethod(nameof(ValueCopyer.WriteTo));
        public static readonly MethodInfo IDataReaderIndexerGetMethod = typeof(IDataReader<string>).GetProperty(TypeHelper.DefaultIndexerName, new Type[] { typeof(string) }).GetGetMethod(true);
        public static readonly MethodInfo IDataWriterIndexerGetMethod = typeof(IDataWriter<string>).GetProperty(TypeHelper.DefaultIndexerName, new Type[] { typeof(string) }).GetGetMethod(true);

        public static readonly ConstructorInfo MemberAccessException_String_Constructor = typeof(MemberAccessException).GetConstructor(new Type[] { typeof(string) });
        public static readonly ConstructorInfo MissingMemberException_String_String_Constructor = typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string), typeof(string) });

        public static readonly AssemblyBuilder AssBuilder;
        public static readonly ModuleBuilder ModBuilder;

        public static readonly bool DynamicAssemblyCanAccessNonPublicTypes;
        public static readonly bool DynamicAssemblyCanAccessNonPublicMembers;

        static FastObjectRWFactory()
        {
            AssemblyName AssName = new AssemblyName(
                RWName
                );

            AssBuilder =
                VersionDifferences.DefineDynamicAssembly(
                    AssName,
                    AssemblyBuilderAccess.Run);

            ModBuilder = AssBuilder.DefineDynamicModule(
                RWName);

            if (VersionDifferences.DynamicAssemblyCanAccessNonPublicTypes == null)
            {
                try
                {
                    ModBuilder.DefineType("TestType", TypeAttributes.Public, typeof(TestClass)).CreateType();

                    DynamicAssemblyCanAccessNonPublicTypes = true;
                }
                catch (Exception)
                {
                    DynamicAssemblyCanAccessNonPublicTypes = false;
                }
            }
            else
            {
                DynamicAssemblyCanAccessNonPublicTypes = VersionDifferences.DynamicAssemblyCanAccessNonPublicTypes.Value;
            }

            if (VersionDifferences.DynamicAssemblyCanAccessNonPublicMembers == null)
            {
                try
                {
                    var dynamicMethodName = "TestMethod";

                    var typeBuilder = ModBuilder.DefineType("TestType2", TypeAttributes.Public);

                    var methodBuilder = typeBuilder.DefineMethod(
                        dynamicMethodName,
                        MethodAttributes.Public | MethodAttributes.Static,
                        CallingConventions.Standard,
                        typeof(void),
                        Type.EmptyTypes);

                    var ilGen = methodBuilder.GetILGenerator();

                    ilGen.Call(typeof(TestClass).GetMethod(nameof(TestClass.TestMethod), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
                    ilGen.Return();

                    var method = typeBuilder.CreateType().GetMethod(dynamicMethodName);

                    method.Invoke(null, null);

                    DynamicAssemblyCanAccessNonPublicMembers = true;
                }
                catch (Exception)
                {
                    DynamicAssemblyCanAccessNonPublicMembers = false;
                }
            }
            else
            {
                DynamicAssemblyCanAccessNonPublicMembers = VersionDifferences.DynamicAssemblyCanAccessNonPublicMembers.Value;
            }
        }

        private class TestClass
        {
            internal static void TestMethod()
            {

            }
        }

        public static IFastObjectRWCreater<T> CreateCreater<T>(FastObjectRWOptions options)
        {
            string typeName = RWName + "_" + typeof(T).Name + "_" + Guid.NewGuid().ToString("N");

            var typeBuilder = ModBuilder.DefineType(
                typeName,
                TypeAttributes.Sealed | TypeAttributes.Public);

            if (typeof(T).IsVisible || DynamicAssemblyCanAccessNonPublicTypes)
            {
                typeBuilder.SetParent(typeof(FastObjectRW<T>));
            }
            else
            {
                /* Use generics to skip visibility check. */

                var genericParameters = typeBuilder.DefineGenericParameters("T");

                var baseType = typeof(FastObjectRW<>).MakeGenericType(genericParameters);
                
                typeBuilder.SetParent(baseType);
            }

            var callbackEvents = new CallbackEvents();

            ImplInitialize<T>(typeBuilder, options, callbackEvents);

            ImplOnWriteValue<T>(typeBuilder, options, callbackEvents);

            ImplOnReadValue<T>(typeBuilder, options, callbackEvents);

            ImplOnReadAll<T>(typeBuilder, options, callbackEvents);

            ImplOnWriteAll<T>(typeBuilder, options, callbackEvents);

            ImplOnReadAllWithFilter<T>(typeBuilder, options, callbackEvents);
            


            Type rtType = typeBuilder.CreateType();

            if (rtType.IsGenericTypeDefinition)
            {
                rtType = rtType.MakeGenericType(typeof(T));
            }

            callbackEvents.OnTypeCreated(rtType);





            typeBuilder = ModBuilder.DefineType(
                typeName + "_Creater",
                TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.Public,
                typeof(object)
            );

            callbackEvents = new CallbackEvents();

            ImplCreate<T>(typeBuilder, rtType, options, callbackEvents);

            rtType = typeBuilder.CreateType();

            if (rtType.IsGenericTypeDefinition)
            {
                rtType = rtType.MakeGenericType(typeof(T));
            }

            callbackEvents.OnTypeCreated(rtType);

            return (IFastObjectRWCreater<T>)Activator.CreateInstance(rtType);
        }

        public static void ImplInitialize<T>(TypeBuilder typeBuilder, FastObjectRWOptions options, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.Initialize),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                Type.EmptyTypes);


            if (typeof(T).IsVisible)
            {
                ImplInitialize<T>(methodBuilder.GetILGenerator(), options);
            }
            else
            {
                var delegateType = typeof(Action<IntPtr>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplInitialize) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                if (typeof(T).IsValueType)
                {
                    ilGen.Emit(OpCodes.Ret);

                    return;
                }

                FixedArg0<T>(ilGen);

                ilGen.Emit(OpCodes.Ldsfld, delegateField);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Call, delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Emit(OpCodes.Ret);

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.Initialize),
                        typeof(void),
                        new Type[] { typeof(IntPtr) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplInitialize<T>(dynamicMethod.GetILGenerator(), options);

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnWriteValue<T>(TypeBuilder typeBuilder, FastObjectRWOptions options, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteValue),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(string), typeof(IValueReader) });

            if (typeof(T).IsVisible && (!StaticFastObjectRW<T>.HaveNonPublicMember))
            {
                ImplOnWriteValue<T>(methodBuilder.GetILGenerator(), options);
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, string, IValueReader>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnWriteValue) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0<T>(ilGen);

                ilGen.Emit(OpCodes.Ldsfld, delegateField);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);
                ilGen.Emit(OpCodes.Call, delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Emit(OpCodes.Ret);

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.OnWriteValue),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(string), typeof(IValueReader) },
                        typeof(FastObjectRW<T>).Module, true);
                    
                    ImplOnWriteValue<T>(dynamicMethod.GetILGenerator(), options);

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnReadAll<T>(TypeBuilder typeBuilder, FastObjectRWOptions options, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadAll),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataWriter<string>) });


            if (typeof(T).IsVisible && (!StaticFastObjectRW<T>.HaveNonPublicMember))
            {
                ImplOnReadAll<T>(methodBuilder.GetILGenerator(), options);
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, IDataWriter<string>>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnReadAll) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0<T>(ilGen);

                ilGen.Emit(OpCodes.Ldsfld, delegateField);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Call, delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Emit(OpCodes.Ret);

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.OnReadAll),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(IDataWriter<string>) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplOnReadAll<T>(dynamicMethod.GetILGenerator(), options);

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }



        public static void ImplOnWriteAll<T>(TypeBuilder typeBuilder, FastObjectRWOptions options, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteAll),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataReader<string>) });


            if (typeof(T).IsVisible && (!StaticFastObjectRW<T>.HaveNonPublicMember))
            {
                ImplOnWriteAll<T>(methodBuilder.GetILGenerator(), options);
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, IDataReader<string>>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnWriteAll) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0<T>(ilGen);

                ilGen.Emit(OpCodes.Ldsfld, delegateField);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Call, delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Emit(OpCodes.Ret);

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.OnWriteAll),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(IDataReader<string>) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplOnWriteAll<T>(dynamicMethod.GetILGenerator(), options);

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnReadAllWithFilter<T>(TypeBuilder typeBuilder, FastObjectRWOptions options, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadAll),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataWriter<string>), typeof(IValueFilter<string>) });

            if (typeof(T).IsVisible && (!StaticFastObjectRW<T>.HaveNonPublicMember))
            {
                ImplOnReadAllWithFilter<T>(methodBuilder.GetILGenerator(), options);
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, IDataWriter<string>, IValueFilter<string>>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnReadAllWithFilter) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0<T>(ilGen);

                ilGen.Emit(OpCodes.Ldsfld, delegateField);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);
                ilGen.Emit(OpCodes.Call, delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Emit(OpCodes.Ret);

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(ImplOnReadAllWithFilter),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(IDataWriter<string>), typeof(IValueFilter<string>) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplOnReadAllWithFilter<T>(dynamicMethod.GetILGenerator(), options);

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnReadValue<T>(TypeBuilder typeBuilder, FastObjectRWOptions options, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadValue),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(string), typeof(IValueWriter) });

            if (typeof(T).IsVisible && (!StaticFastObjectRW<T>.HaveNonPublicMember))
            {
                ImplOnReadValue<T>(methodBuilder.GetILGenerator(), options);
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, string, IValueWriter>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnReadValue) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0<T>(ilGen);

                ilGen.Emit(OpCodes.Ldsfld, delegateField);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);
                ilGen.Emit(OpCodes.Call, delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Emit(OpCodes.Ret);

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(ImplOnReadValue),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(string), typeof(IValueWriter) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplOnReadValue<T>(dynamicMethod.GetILGenerator(), options);

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplCreate<T>(TypeBuilder typeBuilder, Type rtType, FastObjectRWOptions options, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
               nameof(IFastObjectRWCreater<T>.Create),
               MethodAttributes.Public | MethodAttributes.HideBySig |
               MethodAttributes.NewSlot | MethodAttributes.Virtual |
               MethodAttributes.Final,
               CallingConventions.HasThis);

            if (typeof(T).IsVisible || DynamicAssemblyCanAccessNonPublicTypes)
            {
                typeBuilder.AddInterfaceImplementation(typeof(IFastObjectRWCreater<T>));

                methodBuilder.SetReturnType(typeof(FastObjectRW<T>));
            }
            else
            {
                var genericParameters = typeBuilder.DefineGenericParameters("T");

                typeBuilder.AddInterfaceImplementation(typeof(IFastObjectRWCreater<>).MakeGenericType(genericParameters));

                methodBuilder.SetReturnType(typeof(FastObjectRW<>).MakeGenericType(genericParameters));
            }

            if (typeof(T).IsVisible || DynamicAssemblyCanAccessNonPublicMembers)
            {
                ImplCreate<T>(methodBuilder.GetILGenerator(), rtType, options);
            }
            else
            {
                var delegateType = typeof(Func<object>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplCreate) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldsfld, delegateField);
                ilGen.Emit(OpCodes.Call, delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Emit(OpCodes.Ret);

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(ImplOnReadValue),
                        typeof(object),
                        Type.EmptyTypes,
                        typeof(FastObjectRW<T>).Module, true);

                    ImplCreate<T>(dynamicMethod.GetILGenerator(), rtType, options);

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }





        private static void FixedArg0<T>(ILGenerator ilGen)
        {
            var arg0 = ilGen.DeclareLocal(typeof(IntPtr).MakeByRefType(), true);
            ilGen.LoadArgument(0);
            ilGen.ConvertPointer();
            ilGen.StoreLocal(arg0);
        }

        public static void ImplInitialize<T>(ILGenerator ilGen, FastObjectRWOptions options)
        {
            if (typeof(T).IsValueType)
            {
                ilGen.Emit(OpCodes.Ret);
            }
            else
            {
                ilGen.LoadArgument(0);

                var Constructor = typeof(T).GetConstructor(Type.EmptyTypes);

                if (Constructor == null)
                {
                    ilGen.Emit(OpCodes.Ldtoken, typeof(T));
                    ilGen.Emit(OpCodes.Call, GetTypeFromHandleMethod);
                    ilGen.Emit(OpCodes.Call, CreateInstanceFromTypeMethod);
                }
                else
                {
                    ilGen.Emit(OpCodes.Newobj, Constructor);
                }

                ilGen.Emit(OpCodes.Stfld, StaticFastObjectRW<T>.ContentField);
                ilGen.Emit(OpCodes.Ret);
            }
        }

        public static void ImplOnWriteValue<T>(ILGenerator ilGen, FastObjectRWOptions options)
        {
            var fields = StaticFastObjectRW<T>.Fields;

            var cases = new CaseInfo<string>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                cases[i] = new CaseInfo<string>(fields[i].Name, ilGen.DefineLabel());
            }

            var DefaultLabel = ilGen.DefineLabel();
            
            ilGen.Switch((iLGen) => { iLGen.LoadArgument(1); }, cases, DefaultLabel, (options & FastObjectRWOptions.IgnoreCase) != 0);

            ilGen.MarkLabel(DefaultLabel);

            if ((options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.Emit(OpCodes.Ldstr, typeof(T).Name);
                ilGen.LoadArgument(1);
                ilGen.Emit(OpCodes.Newobj, MissingMemberException_String_String_Constructor);
                ilGen.Emit(OpCodes.Throw);
            }
            else
            {
                ilGen.LoadArgument(2);
                ilGen.Emit(OpCodes.Call, typeof(IValueReader).GetMethod(nameof(IValueReader.DirectRead)));
                ilGen.Emit(OpCodes.Pop);
                ilGen.Emit(OpCodes.Ret);
            }

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var @case = cases[i];

                ilGen.MarkLabel(@case.Label);

                if (field.CanWrite)
                {
                    var valueInterfaceType = typeof(ValueInterface<>).MakeGenericType(field.Type);
                    var valueInterfaceIsNoModify = TypeHelper.SlowGetValue<bool>(valueInterfaceType, nameof(ValueInterface<object>.IsNoModify)); //(bool)valueInterfaceType.GetField(nameof(ValueInterface<object>.IsNoModify)).GetValue(null);

                    if (valueInterfaceIsNoModify && (options & FastObjectRWOptions.BasicTypeDirectCallMethod) != 0)
                    {
                        string methodName = null;

                        switch (Type.GetTypeCode(field.Type))
                        {
                            case TypeCode.Boolean:
                                methodName = nameof(IValueReader.ReadBoolean);
                                break;
                            case TypeCode.Char:
                                methodName = nameof(IValueReader.ReadChar);
                                break;
                            case TypeCode.SByte:
                                methodName = nameof(IValueReader.ReadSByte);
                                break;
                            case TypeCode.Byte:
                                methodName = nameof(IValueReader.ReadByte);
                                break;
                            case TypeCode.Int16:
                                methodName = nameof(IValueReader.ReadInt16);
                                break;
                            case TypeCode.UInt16:
                                methodName = nameof(IValueReader.ReadUInt16);
                                break;
                            case TypeCode.Int32:
                                methodName = nameof(IValueReader.ReadInt32);
                                break;
                            case TypeCode.UInt32:
                                methodName = nameof(IValueReader.ReadUInt32);
                                break;
                            case TypeCode.Int64:
                                methodName = nameof(IValueReader.ReadInt64);
                                break;
                            case TypeCode.UInt64:
                                methodName = nameof(IValueReader.ReadUInt64);
                                break;
                            case TypeCode.Single:
                                methodName = nameof(IValueReader.ReadSingle);
                                break;
                            case TypeCode.Double:
                                methodName = nameof(IValueReader.ReadDouble);
                                break;
                            case TypeCode.Decimal:
                                methodName = nameof(IValueReader.ReadDecimal);
                                break;
                            case TypeCode.DateTime:
                                methodName = nameof(IValueReader.ReadDateTime);
                                break;
                            case TypeCode.String:
                                methodName = nameof(IValueReader.ReadString);
                                break;
                        }

                        if (methodName != null)
                        {
                            field.BeforeValueWrite(ilGen);

                            ilGen.LoadArgument(2);
                            ilGen.Call(typeof(IValueReader).GetMethod(methodName));

                            field.AfterValueWrite(ilGen);

                            ilGen.Return();

                            continue;
                        }
                    }

                    var valueInterfaceContentField = valueInterfaceType.GetField(nameof(ValueInterface<object>.Content));
                    var valueInterfaceContentReadValueMethod = valueInterfaceContentField.FieldType.GetMethod(nameof(ValueInterface<object>.Content.ReadValue));

                    field.BeforeValueWrite(ilGen);

                    ilGen.LoadStaticField(valueInterfaceContentField);

                    ilGen.LoadArgument(2);

                    ilGen.Call(valueInterfaceContentReadValueMethod);

                    field.AfterValueWrite(ilGen);

                    ilGen.Return();
                }
                else
                {
                    if ((options & FastObjectRWOptions.CannotSetException) != 0)
                    {
                        ilGen.Emit(OpCodes.Ldstr, StringHelper.Format("This member '{0}' no set method or cannot access.", field.Name));
                        ilGen.Emit(OpCodes.Newobj, MemberAccessException_String_Constructor);
                        ilGen.Emit(OpCodes.Throw);
                    }
                    else
                    {
                        ilGen.LoadArgument(2);
                        ilGen.Emit(OpCodes.Call, typeof(IValueReader).GetMethod(nameof(IValueReader.DirectRead)));
                        ilGen.Emit(OpCodes.Pop);
                        ilGen.Emit(OpCodes.Ret);
                    }
                }
            }
        }

        public static void ImplOnReadAll<T>(ILGenerator ilGen, FastObjectRWOptions options)
        {
            var fields = StaticFastObjectRW<T>.Fields;
           
            if ((FastObjectRW<T>.currentOptions & FastObjectRWOptions.SkipDefaultValue) != 0)
            {
                var tempLocals = new Dictionary<Type, LocalBuilder>();

                foreach (var field in fields)
                {
                    if (field.CanRead)
                    {
                        var isDefaultValue = ilGen.DefineLabel();

                        var endCurrent = ilGen.DefineLabel();

                        field.BeforeValueRead(ilGen);
                        field.AfterValueRead(ilGen);

                        ilGen.Emit(OpCodes.Dup);

                        ilGen.BranchDefaultValue(field.Type, tempLocals, isDefaultValue);

                        ilGen.LoadArgument(1);
                        ilGen.LoadString(field.Name);
                        ilGen.Call(IDataWriterIndexerGetMethod);

                        var valueInterfaceType = typeof(ValueInterface<>).MakeGenericType(field.Type);

                        var valueInterfaceWriteValueMethod = valueInterfaceType.GetMethod(nameof(ValueInterface<object>.WriteValue));

                        ilGen.Emit(OpCodes.Call, valueInterfaceWriteValueMethod);

                        ilGen.Emit(OpCodes.Br, endCurrent);

                        ilGen.MarkLabel(isDefaultValue);

                        ilGen.Emit(OpCodes.Pop);

                        ilGen.MarkLabel(endCurrent);
                    }
                }
            }
            else
            {
                foreach (var field in fields)
                {
                    if (field.CanRead)
                    {
                        var valueInterfaceType = typeof(ValueInterface<>).MakeGenericType(field.Type);
                        var isNoModify = TypeHelper.SlowGetValue<bool>(valueInterfaceType, nameof(ValueInterface<object>.IsNoModify)); // (bool)valueInterfaceType.GetField(nameof(ValueInterface<object>.IsNoModify)).GetValue(null);
                        
                        if (isNoModify && (options & FastObjectRWOptions.BasicTypeDirectCallMethod) != 0)
                        {
                            string methodName = null;

                            switch (Type.GetTypeCode(field.Type))
                            {
                                case TypeCode.Boolean:
                                    methodName = nameof(IValueWriter.WriteBoolean);
                                    break;
                                case TypeCode.Char:
                                    methodName = nameof(IValueWriter.WriteChar);
                                    break;
                                case TypeCode.SByte:
                                    methodName = nameof(IValueWriter.WriteSByte);
                                    break;
                                case TypeCode.Byte:
                                    methodName = nameof(IValueWriter.WriteByte);
                                    break;
                                case TypeCode.Int16:
                                    methodName = nameof(IValueWriter.WriteInt16);
                                    break;
                                case TypeCode.UInt16:
                                    methodName = nameof(IValueWriter.WriteUInt16);
                                    break;
                                case TypeCode.Int32:
                                    methodName = nameof(IValueWriter.WriteInt32);
                                    break;
                                case TypeCode.UInt32:
                                    methodName = nameof(IValueWriter.WriteUInt32);
                                    break;
                                case TypeCode.Int64:
                                    methodName = nameof(IValueWriter.WriteInt64);
                                    break;
                                case TypeCode.UInt64:
                                    methodName = nameof(IValueWriter.WriteUInt64);
                                    break;
                                case TypeCode.Single:
                                    methodName = nameof(IValueWriter.WriteSingle);
                                    break;
                                case TypeCode.Double:
                                    methodName = nameof(IValueWriter.WriteDouble);
                                    break;
                                case TypeCode.Decimal:
                                    methodName = nameof(IValueWriter.WriteDecimal);
                                    break;
                                case TypeCode.DateTime:
                                    methodName = nameof(IValueWriter.WriteDateTime);
                                    break;
                                case TypeCode.String:
                                    methodName = nameof(IValueWriter.WriteString);
                                    break;
                            }

                            if (methodName != null)
                            {
                                ilGen.LoadArgument(1);
                                ilGen.LoadString(field.Name);
                                ilGen.Call(IDataWriterIndexerGetMethod);

                                field.BeforeValueRead(ilGen);
                                field.AfterValueRead(ilGen);

                                ilGen.Call(typeof(IValueWriter).GetMethod(methodName));

                                continue;
                            }
                        }

                        var valueInterfaceContentField = valueInterfaceType.GetField(nameof(ValueInterface<object>.Content));
                        var valueInterfaceContentWriteValueMethod = valueInterfaceContentField.FieldType.GetMethod(nameof(ValueInterface<object>.Content.WriteValue));

                        ilGen.LoadStaticField(valueInterfaceContentField);

                        ilGen.LoadArgument(1);
                        ilGen.LoadString(field.Name);
                        ilGen.Call(IDataWriterIndexerGetMethod);

                        field.BeforeValueRead(ilGen);
                        field.AfterValueRead(ilGen);

                        ilGen.Emit(OpCodes.Call, valueInterfaceContentWriteValueMethod);
                    }
                }
            }

            ilGen.Emit(OpCodes.Ret);
        }

        public static void ImplOnWriteAll<T>(ILGenerator ilGen, FastObjectRWOptions options)
        {
            var fields = StaticFastObjectRW<T>.Fields;

            foreach (var field in fields)
            {
                if (field.CanWrite)
                {
                    var valueInterfaceType = typeof(ValueInterface<>).MakeGenericType(field.Type);
                    var isNoModify = TypeHelper.SlowGetValue<bool>(valueInterfaceType, nameof(ValueInterface<object>.IsNoModify)); // (bool)valueInterfaceType.GetField(nameof(ValueInterface<object>.IsNoModify)).GetValue(null);

                    if (isNoModify && (options & FastObjectRWOptions.BasicTypeDirectCallMethod) != 0)
                    {
                        string methodName = null;

                        switch (Type.GetTypeCode(field.Type))
                        {
                            case TypeCode.Boolean:
                                methodName = nameof(IValueReader.ReadBoolean);
                                break;
                            case TypeCode.Char:
                                methodName = nameof(IValueReader.ReadChar);
                                break;
                            case TypeCode.SByte:
                                methodName = nameof(IValueReader.ReadSByte);
                                break;
                            case TypeCode.Byte:
                                methodName = nameof(IValueReader.ReadByte);
                                break;
                            case TypeCode.Int16:
                                methodName = nameof(IValueReader.ReadInt16);
                                break;
                            case TypeCode.UInt16:
                                methodName = nameof(IValueReader.ReadUInt16);
                                break;
                            case TypeCode.Int32:
                                methodName = nameof(IValueReader.ReadInt32);
                                break;
                            case TypeCode.UInt32:
                                methodName = nameof(IValueReader.ReadUInt32);
                                break;
                            case TypeCode.Int64:
                                methodName = nameof(IValueReader.ReadInt64);
                                break;
                            case TypeCode.UInt64:
                                methodName = nameof(IValueReader.ReadUInt64);
                                break;
                            case TypeCode.Single:
                                methodName = nameof(IValueReader.ReadSingle);
                                break;
                            case TypeCode.Double:
                                methodName = nameof(IValueReader.ReadDouble);
                                break;
                            case TypeCode.Decimal:
                                methodName = nameof(IValueReader.ReadDecimal);
                                break;
                            case TypeCode.DateTime:
                                methodName = nameof(IValueReader.ReadDateTime);
                                break;
                            case TypeCode.String:
                                methodName = nameof(IValueReader.ReadString);
                                break;
                        }

                        if (methodName != null)
                        {
                            field.BeforeValueWrite(ilGen);

                            ilGen.LoadArgument(1);
                            ilGen.LoadString(field.Name);
                            ilGen.Call(IDataReaderIndexerGetMethod);
                            ilGen.Call(typeof(IValueReader).GetMethod(methodName));

                            field.AfterValueWrite(ilGen);

                            continue;
                        }
                    }

                    var valueInterfaceContentField = valueInterfaceType.GetField(nameof(ValueInterface<object>.Content));
                    var valueInterfaceContentReadValueMethod = valueInterfaceContentField.FieldType.GetMethod(nameof(ValueInterface<object>.Content.ReadValue));


                    field.BeforeValueWrite(ilGen);

                    ilGen.LoadStaticField(valueInterfaceContentField);

                    ilGen.LoadArgument(1);
                    ilGen.LoadString(field.Name);
                    ilGen.Call(IDataReaderIndexerGetMethod);

                    ilGen.Call(valueInterfaceContentReadValueMethod);

                    field.AfterValueWrite(ilGen);
                }
            }

            ilGen.Emit(OpCodes.Ret);
        }

        public static void ImplOnReadAllWithFilter<T>(ILGenerator ilGen, FastObjectRWOptions options)
        {
            var fields = StaticFastObjectRW<T>.Fields;

            var valueInfo = ilGen.DeclareLocal(typeof(ValueFilterInfo<string>));
            
            ilGen.Emit(OpCodes.Newobj, typeof(ValueFilterInfo<string>).GetConstructor(Type.EmptyTypes));

            ilGen.StoreLocal(valueInfo);
            
            if ((FastObjectRW<T>.currentOptions & FastObjectRWOptions.SkipDefaultValue) != 0)
            {
                var tempLocals = new Dictionary<Type, LocalBuilder>();

                foreach (var field in fields)
                {
                    if (field.CanRead)
                    {
                        var isDefaultValue = ilGen.DefineLabel();

                        var endLabel = ilGen.DefineLabel();

                        field.BeforeValueRead(ilGen);

                        field.AfterValueRead(ilGen);

                        ilGen.Emit(OpCodes.Dup);

                        ilGen.BranchDefaultValue(field.Type, tempLocals, isDefaultValue);

                        ilGen.LoadLocal(valueInfo);

                        ilGen.Emit(OpCodes.Ldfld, ValueFilterInfoValueCopyerField);

                        var valueInterfaceType = typeof(ValueInterface<>).MakeGenericType(field.Type);

                        var valueInterfaceWriteValueMethod = valueInterfaceType.GetMethod(nameof(ValueInterface<object>.WriteValue));

                        ilGen.Call(valueInterfaceWriteValueMethod);

                        ImplOnReadAllWithFilterItem(ilGen, valueInfo, field, endLabel);

                        ilGen.Emit(OpCodes.Br, endLabel);

                        ilGen.MarkLabel(isDefaultValue);

                        ilGen.Emit(OpCodes.Pop);

                        ilGen.MarkLabel(endLabel);
                    }
                }
            }
            else
            {
                foreach (var field in fields)
                {
                    if (field.CanRead)
                    {
                        var endLabel = ilGen.DefineLabel();

                        var interfaceType = typeof(ValueInterface<>).MakeGenericType(field.Type);

                        var interfaceContentField = interfaceType.GetField(nameof(ValueInterface<object>.Content));

                        var interfaceContentWriteValueMethod = interfaceContentField.FieldType.GetMethod(nameof(ValueInterface<object>.Content.WriteValue));

                        ilGen.Emit(OpCodes.Ldsfld, interfaceContentField);

                        ilGen.LoadLocal(valueInfo);

                        ilGen.Emit(OpCodes.Ldfld, ValueFilterInfoValueCopyerField);

                        field.BeforeValueRead(ilGen);

                        field.AfterValueRead(ilGen);

                        ilGen.Call(interfaceContentWriteValueMethod);

                        ImplOnReadAllWithFilterItem(ilGen, valueInfo, field, endLabel);

                        ilGen.MarkLabel(endLabel);
                    }
                }
            }


            ilGen.Emit(OpCodes.Ret);
        }

        public static void ImplOnReadValue<T>(ILGenerator ilGen, FastObjectRWOptions options)
        {
            var fields = StaticFastObjectRW<T>.Fields;

            var Cases = new CaseInfo<string>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                Cases[i] = new CaseInfo<string>(fields[i].Name, ilGen.DefineLabel());
            }

            var DefaultLabel = ilGen.DefineLabel();

            ilGen.Switch((iLGen) => { iLGen.LoadArgument(1); }, Cases, DefaultLabel, (options & FastObjectRWOptions.IgnoreCase) != 0);

            ilGen.MarkLabel(DefaultLabel);

            if ((options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.Emit(OpCodes.Ldstr, typeof(T).Name);

                ilGen.LoadArgument(1);

                ilGen.Emit(OpCodes.Newobj, MissingMemberException_String_String_Constructor);

                ilGen.Emit(OpCodes.Throw);
            }
            else
            {
                ilGen.LoadArgument(2);

                ilGen.Emit(OpCodes.Ldnull);

                ilGen.Emit(OpCodes.Call, typeof(IValueWriter).GetMethod(nameof(IValueWriter.DirectWrite)));

                ilGen.Emit(OpCodes.Ret);
            }

            for (int i = 0; i < fields.Length; i++)
            {
                var PropertyInfo = fields[i];
                var CaseInfo = Cases[i];

                ilGen.MarkLabel(CaseInfo.Label);

                if (PropertyInfo.CanRead)
                {
                    var valueInterfaceType = typeof(ValueInterface<>).MakeGenericType(PropertyInfo.Type);
                    var isNoModify = TypeHelper.SlowGetValue<bool>(valueInterfaceType, nameof(ValueInterface<object>.IsNoModify)); //(bool)valueInterfaceType.GetField(nameof(ValueInterface<object>.IsNoModify)).GetValue(null);

                    if (isNoModify && (options & FastObjectRWOptions.BasicTypeDirectCallMethod) != 0)
                    {
                        string methodName = null;

                        switch (Type.GetTypeCode(PropertyInfo.Type))
                        {
                            case TypeCode.Boolean:
                                methodName = nameof(IValueWriter.WriteBoolean);
                                break;
                            case TypeCode.Char:
                                methodName = nameof(IValueWriter.WriteChar);
                                break;
                            case TypeCode.SByte:
                                methodName = nameof(IValueWriter.WriteSByte);
                                break;
                            case TypeCode.Byte:
                                methodName = nameof(IValueWriter.WriteByte);
                                break;
                            case TypeCode.Int16:
                                methodName = nameof(IValueWriter.WriteInt16);
                                break;
                            case TypeCode.UInt16:
                                methodName = nameof(IValueWriter.WriteUInt16);
                                break;
                            case TypeCode.Int32:
                                methodName = nameof(IValueWriter.WriteInt32);
                                break;
                            case TypeCode.UInt32:
                                methodName = nameof(IValueWriter.WriteUInt32);
                                break;
                            case TypeCode.Int64:
                                methodName = nameof(IValueWriter.WriteInt64);
                                break;
                            case TypeCode.UInt64:
                                methodName = nameof(IValueWriter.WriteUInt64);
                                break;
                            case TypeCode.Single:
                                methodName = nameof(IValueWriter.WriteSingle);
                                break;
                            case TypeCode.Double:
                                methodName = nameof(IValueWriter.WriteDouble);
                                break;
                            case TypeCode.Decimal:
                                methodName = nameof(IValueWriter.WriteDecimal);
                                break;
                            case TypeCode.DateTime:
                                methodName = nameof(IValueWriter.WriteDateTime);
                                break;
                            case TypeCode.String:
                                methodName = nameof(IValueWriter.WriteString);
                                break;
                        }

                        if (methodName != null)
                        {
                            ilGen.LoadArgument(2);

                            PropertyInfo.BeforeValueRead(ilGen);
                            PropertyInfo.AfterValueRead(ilGen);

                            ilGen.Call(typeof(IValueWriter).GetMethod(methodName));

                            ilGen.Return();

                            continue;
                        }
                    }

                    var valueInterfaceContentField = valueInterfaceType.GetField(nameof(ValueInterface<object>.Content));
                    var valueInterfaceContentWriteValue = valueInterfaceContentField.FieldType.GetMethod(nameof(ValueInterface<object>.Content.WriteValue));

                    ilGen.LoadStaticField(valueInterfaceContentField);

                    ilGen.LoadArgument(2);

                    PropertyInfo.BeforeValueRead(ilGen);
                    PropertyInfo.AfterValueRead(ilGen);

                    ilGen.Call(valueInterfaceContentWriteValue);

                    ilGen.Return();
                }
                else
                {
                    if ((options & FastObjectRWOptions.CannotGetException) != 0)
                    {
                        ilGen.Emit(OpCodes.Ldstr, StringHelper.Format("This member '{0}' no get method or cannot access.", PropertyInfo.Name));

                        ilGen.Emit(OpCodes.Newobj, MemberAccessException_String_Constructor);

                        ilGen.Emit(OpCodes.Throw);
                    }
                    else
                    {
                        ilGen.LoadArgument(2);

                        ilGen.Emit(OpCodes.Ldnull);

                        ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.DirectWrite)));

                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplCreate<T>(ILGenerator ilGen, Type rtType, FastObjectRWOptions options)
        {
            ilGen.Emit(OpCodes.Newobj, rtType.GetConstructor(Type.EmptyTypes));
            ilGen.Emit(OpCodes.Ret);
        }
        




        private static void ImplOnReadAllWithFilterItem(ILGenerator ilGen, LocalBuilder valueInfo, IFastField field, Label endLabel)
        {
            ilGen.LoadLocal(valueInfo);

            ilGen.Emit(OpCodes.Ldstr, field.Name);

            ilGen.Emit(OpCodes.Stfld, ValueFilterInfoKeyField);

            ilGen.LoadLocal(valueInfo);

            ilGen.Emit(OpCodes.Ldtoken, field.Type);

            ilGen.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);

            ilGen.Emit(OpCodes.Stfld, ValueFilterInfoTypeField);

            ilGen.LoadArgument(2);

            ilGen.LoadLocal(valueInfo);

            ilGen.Emit(OpCodes.Call, IValueFilterFilterMethod);

            ilGen.Emit(OpCodes.Brfalse, endLabel);

            ilGen.LoadLocal(valueInfo);

            ilGen.Emit(OpCodes.Ldfld, ValueFilterInfoValueCopyerField);

            ilGen.LoadArgument(1);

            ilGen.LoadLocal(valueInfo);

            ilGen.Emit(OpCodes.Ldfld, ValueFilterInfoKeyField);

            ilGen.Emit(OpCodes.Call, IDataWriterIndexerGetMethod);

            ilGen.Emit(OpCodes.Call, ValueCopyerWriteToMethod);
        }
    }

#endregion
}