using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供类型信息的一些方法。
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 获取默认索引器的名称。
        /// </summary>
        public const string DefaultIndexerName = "Item";

        private static readonly bool offsetOfByHandleIsAvailable;

        static TypeHelper()
        {
            try
            {
                offsetOfByHandleIsAvailable =
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field1)) == OffsetOfByHandle(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field1))) &&
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field2)) == OffsetOfByHandle(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field2))) &&
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field3)) == OffsetOfByHandle(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field3)));
            }
            catch (Exception)
            {
                offsetOfByHandleIsAvailable = false;
            }
        }

        /// <summary>
        /// 获取一个字段的偏移量。如果是 Class 的字段则不包括 ObjectHandle 的大小。
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        /// <returns>返回偏移量</returns>
        public static uint OffsetOf(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }

            if (fieldInfo.IsLiteral)
            {
                throw new ArgumentException("Unable get offset of a const field.", nameof(fieldInfo));
            }

            if (offsetOfByHandleIsAvailable)
            {
                return OffsetOfByHandle(fieldInfo);
            }

            return OffsetOfByDynamic(fieldInfo);
        }

        private static uint OffsetOfByHandle(FieldInfo fieldInfo)
        {
            unsafe
            {
                var pFieldHandleInfo = (FieldHandleStruct*)fieldInfo.FieldHandle.Value;

                return pFieldHandleInfo->Offset;
            }
        }

        private static IdCache<uint> fieldOffsetCache;
        private static object fieldOffsetCacheLock;

        private static uint OffsetOfByDynamic(FieldInfo fieldInfo)
        {
            if (fieldOffsetCache == null)
            {
                lock (typeof(TypeHelper))
                {
                    if (fieldOffsetCache == null)
                    {
                        fieldOffsetCache = new IdCache<uint>();
                        fieldOffsetCacheLock = new object();
                    }
                }
            }

            uint offset;

            var fieldId = (long)fieldInfo.FieldHandle.Value;

            if (fieldOffsetCache.TryGetValue(fieldId, out offset))
            {
                return offset;
            }

            lock (fieldOffsetCacheLock)
            {
                if (fieldOffsetCache.TryGetValue(fieldId, out offset))
                {
                    return offset;
                }

                GetAllFieldOffsetByDynamic(fieldInfo.DeclaringType);

                return fieldOffsetCache[fieldId];
            }
        }

        private static IntPtr ChooseMin(IntPtr ptr1, IntPtr ptr2)
        {
            return ((ulong)ptr1 < (ulong)ptr2) ? ptr1 : ptr2;
        }

        private static MethodInfo idCacheUInt32AddMethod;
        private static MethodInfo intPtrChooseMinMethod;

        private static void GetAllFieldOffsetByDynamic(Type type)
        {
            if (intPtrChooseMinMethod == null)
            {
                lock (typeof(TypeHelper))
                {
                    if (intPtrChooseMinMethod == null)
                    {
                        intPtrChooseMinMethod = ((Func<IntPtr, IntPtr, IntPtr>)ChooseMin).Method;

                        idCacheUInt32AddMethod = typeof(IdCache<uint>).GetMethod(
                            nameof(IdCache<uint>.Add),
                            new Type[] { typeof(long), typeof(uint) });
                    }
                }
            }

            var dynamicMethod = new DynamicMethod(
                StringHelper.Format("{0}_{1}_{2}", nameof(OffsetOf), type.Name, Guid.NewGuid().ToString("N")),
                null, new Type[] { typeof(IdCache<uint>) },
                type.Module,
                true);

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            var ilGen = dynamicMethod.GetILGenerator();

            var instance = ilGen.DeclareLocal(typeof(IntPtr));
            var first = ilGen.DeclareLocal(typeof(IntPtr));

            /* 分配一个模拟实例 */
            ilGen.LoadConstant(8);
            ilGen.LocalAllocate();
            ilGen.StoreLocal(instance);

            /* 找到最小的静态字段地址 */
            var isFirstStatic = true;
            foreach (var item in fields)
            {
                /* 跳过常量 */
                if (item.IsLiteral)
                {
                    continue;
                }

                if (item.IsStatic)
                {
                    ilGen.LoadStaticFieldAddress(item);

                    if (isFirstStatic)
                    {
                        isFirstStatic = false;
                    }
                    else
                    {
                        ilGen.LoadLocal(first);
                        ilGen.Call(intPtrChooseMinMethod);
                    }

                    ilGen.StoreLocal(first);
                }
            }

            foreach (var item in fields)
            {
                /* 跳过常量 */
                if (item.IsLiteral)
                {
                    continue;
                }

                /* 加载 IdCache 实例 */
                ilGen.LoadArgument(0);
                /* 加载字段 Id */
                ilGen.LoadConstant((long)item.FieldHandle.Value);

                if (item.IsStatic)
                {
                    // Offset = Address - First Address.
                    ilGen.LoadStaticFieldAddress(item);
                    ilGen.LoadLocal(first);
                    ilGen.Subtract();
                }
                else
                {
                    /* Offset = Address - Instance. */
                    ilGen.LoadLocal(instance);
                    ilGen.LoadFieldAddress(item);
                    ilGen.LoadLocal(instance);
                    ilGen.Subtract();

                    /* if class then Offset -= sizeof TypeHandle. */
                    if (type.IsClass)
                    {
                        ilGen.LoadConstant(IntPtr.Size);
                        ilGen.Subtract();
                    }
                }

                ilGen.ConvertInt32();
                ilGen.Call(idCacheUInt32AddMethod);
            }

            ilGen.Return();

            var action = (Action<IdCache<uint>>)dynamicMethod.CreateDelegate(typeof(Action<IdCache<uint>>));

            /* Call the DynamicMethod. */
            action(fieldOffsetCache);
        }

        private const string MarshalSizeOfHelperName = "SizeOfHelper";
        private delegate int SizeOfHelperDelegate(Type t, bool throwIfNotMarshalable);
        private static IdCache<uint> typeSizeCache;
        private static object typeSizeCacheLock;
        private static SizeOfHelperDelegate sizeOfHelper;
        private static MethodInfo pointerSizeOfMethod;

        /// <summary>
        /// 获取一个类型占用的内存大小。如果是 Class 则不包括 ObjectHandle 的大小。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存大小。</returns>
        public unsafe static uint SizeOf(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type is TypeBuilder)
            {
                throw new ArgumentException("Unable get size of a TypeBuilder.", nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Unable get size of a generic definition type.", nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                return 0;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return sizeof(bool);
                case TypeCode.Char:
                    return sizeof(char);
                case TypeCode.SByte:
                    return sizeof(sbyte);
                case TypeCode.Byte:
                    return sizeof(byte);
                case TypeCode.Int16:
                    return sizeof(short);
                case TypeCode.UInt16:
                    return sizeof(ushort);
                case TypeCode.Int32:
                    return sizeof(int);
                case TypeCode.UInt32:
                    return sizeof(uint);
                case TypeCode.Int64:
                    return sizeof(long);
                case TypeCode.UInt64:
                    return sizeof(ulong);
                case TypeCode.Single:
                    return sizeof(float);
                case TypeCode.Double:
                    return sizeof(double);
                case TypeCode.Decimal:
                    return sizeof(decimal);
                case TypeCode.DateTime:
                    return (uint)sizeof(DateTime);
            }

            if (type.IsPointer || 
                type.IsInterface || 
                type.IsByRef || 
                typeof(object) == type || 
                typeof(IntPtr) == type)
            {
                return (uint)IntPtr.Size;
            }

            if (typeSizeCache == null)
            {
                lock (typeof(TypeHelper))
                {
                    if (typeSizeCache == null)
                    {
                        typeSizeCache = new IdCache<uint>();
                        typeSizeCacheLock = new object();
                    }

                    var sizeOfHelperMethod = typeof(Marshal).GetMethod(MarshalSizeOfHelperName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                    if (sizeOfHelperMethod != null)
                    {
                        sizeOfHelper = MethodHelper.CreateDelegate<SizeOfHelperDelegate>(sizeOfHelperMethod, SignatureLevels.Cast);
                    }

                    pointerSizeOfMethod = typeof(Pointer).GetMethod(nameof(Pointer.SizeOf), Type.EmptyTypes);
                }
            }

            if (type.IsValueType)
            {
                if (sizeOfHelper != null)
                {
                    return (uint)sizeOfHelper(type, false);
                }

                try
                {
                    return (uint)Marshal.SizeOf(type);
                }
                catch (Exception)
                {
                }
            }

            var typeId = (long)type.TypeHandle.Value;

            uint size;

            if (typeSizeCache.TryGetValue(typeId, out size))
            {
                return size;
            }

            lock (typeSizeCacheLock)
            {
                if (typeSizeCache.TryGetValue(typeId, out size))
                {
                    return size;
                }

                if (type.IsValueType)
                {
                    size = (uint)pointerSizeOfMethod.MakeGenericMethod(type).Invoke(null, null);
                }
                else
                {
                    var fields = type.GetFields(BindingFlags.NonPublic | 
                        BindingFlags.Public | 
                        BindingFlags.Instance | 
                        BindingFlags.DeclaredOnly);

                    if (fields.Length == 0)
                    {
                        size = (uint)IntPtr.Size;
                    }
                    else
                    {
                        int right = fields.Length - 1;
                        var maxOffset = OffsetOf(fields[right]);
                        var maxOffsetFieldType = fields[right].FieldType;

                        while (right > 0)
                        {
                            --right;

                            var offset = OffsetOf(fields[right]);

                            if (offset > maxOffset)
                            {
                                maxOffset = offset;
                                maxOffsetFieldType = fields[right].FieldType;
                            }
                        }

                        // = last field offset + last field size + type handle size;
                        size = maxOffset + (maxOffsetFieldType.IsValueType ? SizeOf(maxOffsetFieldType) : (uint)IntPtr.Size) + ((uint)IntPtr.Size);
                    }
                }

                typeSizeCache[typeId] = size;
            }

            return size;
        }

        /// <summary>
        /// 克隆一个值或对象。
        /// </summary>
        /// <typeparam name="T">值或对象的类型</typeparam>
        /// <param name="obj">值或对象</param>
        /// <returns>返回一个新的值或对象</returns>
        public static T Clone<T>(T obj)
        {
            if (typeof(T).IsValueType)
            {
                return obj;
            }

            if (obj == null)
            {
                return default(T);
            }

            if (obj is string)
            {
                return obj;
            }

            if (obj is ICloneable)
            {
                return (T)((ICloneable)obj).Clone();
            }

            return (T)Clone((object)obj);
        }

        private const string MemberwiseCloneName = "MemberwiseClone";
        private delegate object MemberwiseCloneDelegate(object obj);
        private static MemberwiseCloneDelegate memberwiseClone;

        private static object Clone(object obj)
        {
            if (memberwiseClone == null)
            {
                lock (typeof(TypeHelper))
                {
                    if (memberwiseClone == null)
                    {
                        var memberwiseCloneMethod = typeof(object).GetMethod(MemberwiseCloneName, BindingFlags.NonPublic | BindingFlags.Instance);
                        if (memberwiseCloneMethod != null)
                        {
                            memberwiseClone = MethodHelper.CreateDelegate<MemberwiseCloneDelegate>(memberwiseCloneMethod, SignatureLevels.Cast);
                        }
                    }
                }
            }

            return memberwiseClone(obj);
        }

        private const string AllocateMethodName = "Allocate";
        private const string StubHelpersTypeName = "System.StubHelpers.StubHelpers";
        private const string AllocateInternalName = "AllocateInternal";
        private const string ActivationServicesTypeName = "System.Runtime.Remoting.Activation.ActivationServices";
        private const string AllocateUninitializedClassInstanceName = "AllocateUninitializedClassInstance";

        private delegate object AllocateDelegate(Type t);
        private static AllocateDelegate allocate;
        private delegate object AllocateInternalDelegate(IntPtr typeHandle);
        private static AllocateInternalDelegate allocateInternal;
        private delegate object AllocateUninitializedClassInstanceDelegate(Type type);
        private static AllocateUninitializedClassInstanceDelegate allocateUninitializedClassInstance;
        private static bool allocateIsInitialized;

        private static object AllocateByInternal(Type type)
        {
            if (!allocateIsInitialized)
            {
                lock (typeof(TypeHelper))
                {
                    if (!allocateIsInitialized)
                    {
                        allocateIsInitialized = true;

                        var allocateMethod = typeof(RuntimeTypeHandle).GetMethod(AllocateMethodName, BindingFlags.NonPublic | BindingFlags.Static);
                        if (allocateMethod != null)
                        {
                            allocate = MethodHelper.CreateDelegate<AllocateDelegate>(allocateMethod, SignatureLevels.Cast);
                        }

                        var stubHelpersType = Type.GetType(StubHelpersTypeName);
                        if (stubHelpersType != null)
                        {
                            var allocateInternalMethod = stubHelpersType.GetMethod(AllocateInternalName, BindingFlags.Static | BindingFlags.NonPublic);
                            if (allocateInternalMethod != null)
                            {
                                allocateInternal = MethodHelper.CreateDelegate<AllocateInternalDelegate>(allocateInternalMethod, SignatureLevels.Cast);
                            }
                        }

                        var activationServicesType = Type.GetType(ActivationServicesTypeName);
                        if (activationServicesType != null)
                        {
                            var allocateUninitializedClassInstanceMethod = activationServicesType.GetMethod(AllocateUninitializedClassInstanceName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                            if (allocateUninitializedClassInstanceMethod != null)
                            {
                                allocateUninitializedClassInstance = MethodHelper.CreateDelegate<AllocateUninitializedClassInstanceDelegate>(allocateUninitializedClassInstanceMethod, SignatureLevels.Cast);
                            }
                        }
                    }
                }
            }

            if (allocate != null)
            {
                try
                {
                    return allocate(type);
                }
                catch (Exception)
                {
                }
            }

            if (allocateInternal != null)
            {
                try
                {
                    return allocateInternal(type.TypeHandle.Value);
                }
                catch (Exception)
                {
                }
            }

            if (allocateUninitializedClassInstance != null)
            {
                try
                {
                    return allocateUninitializedClassInstance(type);
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        /// <summary>
        /// 分配一个类型的实例。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Allocate(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Unable allocate instance of a generic definition type.", nameof(type));
            }

            if (type.IsInterface)
            {
                throw new ArgumentException("Unable allocate instance of a interface type.", nameof(type));
            }

            if (type.IsAbstract)
            {
                throw new ArgumentException("Unable allocate instance of a abstract type.", nameof(type));
            }

            if (type.IsByRef)
            {
                throw new ArgumentException("Unable allocate instance of a byRef type.", nameof(type));
            }

            if (type.IsArray)
            {
                var lengths = new int[type.GetArrayRank()];

                return Array.CreateInstance(type.GetElementType(), lengths);
            }

            var tObject = AllocateByInternal(type);

            if (tObject != null)
            {
                return tObject;
            }

            unsafe
            {
                var pObject = stackalloc byte[(int)SizeOf(type) + sizeof(IntPtr)];

                *(IntPtr*)pObject = type.TypeHandle.Value;

                tObject = Pointer.Box((IntPtr)pObject);

                return Clone(tObject);
            }
        }

        /// <summary>
        /// 获取实例的 ObjectHandle 值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetObjectHandle(object obj)
        {
            return Pointer.GetTypeHandle(obj);
        }

        /// <summary>
        /// 获取实例的 TypeHandle 值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetTypeHandle(object obj)
        {
            return VersionDifferences.GetTypeHandle(obj);
        }

        /// <summary>
        /// 获取类型的 TypeHandle 值。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回 TypeHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetTypeHandle(Type type)
        {
            return type.TypeHandle.Value;
        }

        /// <summary>
        /// 获取类型的 ObjectHandle 值。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetObjectHandle(Type type)
        {
            if (VersionDifferences.ObjectHandleEqualsTypeHandle && !type.IsArray)
            {
                return type.TypeHandle.Value;
            }

            var obj = AllocateByInternal(type);

            if (obj == null)
            {
                VersionNotSupport(nameof(GetObjectHandle));
            }

            return GetObjectHandle(obj);
        }

        /// <summary>
        /// 获取 ObjectHandle 占用的内存大小。
        /// </summary>
        public static int ObjectHandleSize
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return IntPtr.Size;
            }
        }

        private static IdCache<IntPtr> StaticFieldAddressCache;
        private static object StaticFieldAddressCacheLock;

        /// <summary>
        /// 获取类型的静态内存地址位置。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存地址。</returns>
        public static IntPtr GetTypeStaticMemoryAddress(Type type)
        {
            if (StaticFieldAddressCache == null)
            {
                lock (typeof(TypeHelper))
                {
                    if (StaticFieldAddressCache == null)
                    {
                        StaticFieldAddressCache = new IdCache<IntPtr>();
                        StaticFieldAddressCacheLock = new object();
                    }
                }
            }

            var typeId = (long)GetTypeHandle(type);

            if (StaticFieldAddressCache.TryGetValue(typeId, out var value))
            {
                return value;
            }

            lock (StaticFieldAddressCacheLock)
            {
                if (StaticFieldAddressCache.TryGetValue(typeId, out value))
                {
                    return value;
                }

                value = GetTypeStaticMemoryAddressByDynamic(type);

                StaticFieldAddressCache.Add(typeId, value);

                return value;
            }
        }

        private static IntPtr GetTypeStaticMemoryAddressByDynamic(Type type)
        {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            FieldInfo fieldInfo = null;

            foreach (var item in fields)
            {
                if (item.IsLiteral)
                {
                    continue;
                }

                fieldInfo = item;

                break;
            }

            if (fieldInfo == null)
            {
                return IntPtr.Zero;
            }

            var getFieldPointerMethod = new DynamicMethod(
                StringHelper.Format("{0}_{1}_{2}", nameof(GetTypeStaticMemoryAddressByDynamic), type.Name, Guid.NewGuid().ToString("N")),
                typeof(IntPtr),
                Type.EmptyTypes,
                fieldInfo.Module,
                true);

            var ilGen = getFieldPointerMethod.GetILGenerator();

            ilGen.Emit(OpCodes.Ldsflda, fieldInfo);
            ilGen.Emit(OpCodes.Ret);

            var pAddress = (IntPtr)getFieldPointerMethod.Invoke(null, null);

            var pResult = (IntPtr)((ulong)pAddress - OffsetOf(fieldInfo));

            return pResult;
        }

        /// <summary>
        /// 获取已装箱的值类型实例的结构地址。
        /// </summary>
        /// <param name="obj">已装箱的值类型实例</param>
        /// <returns>返回结构地址</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetStructAddress(object obj)
        {
            return (IntPtr)((long)Pointer.UnBox(obj) + ObjectHandleSize);
        }

        /// <summary>
        /// 获取引用的结构地址。
        /// </summary>
        /// <param name="typedRef">引用</param>
        /// <returns>返回结构地址</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetStructAddress(TypedReference typedRef)
        {
            return Pointer.UnBox(typedRef);
        }

        /// <summary>
        /// 判断一个值是否为空。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="value">值</param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsEmptyValue<T>(T value)
        {
            var size = Pointer.SizeOf<T>();

            unsafe
            {
                switch (size)
                {
                    case 0:
                        return true;
                    case 1:
                        return *(byte*)Pointer.UnBox(ref value) == 0;
                    case 2:
                        return *(short*)Pointer.UnBox(ref value) == 0;
                    case 4:
                        return *(int*)Pointer.UnBox(ref value) == 0;
                    case 8:
                        return *(long*)Pointer.UnBox(ref value) == 0;
                }

                var pValue = (byte*)Pointer.UnBox(ref value);

                while (size >= 8)
                {
                    size -= 8;

                    if (*(long*)(pValue - size) != 0)
                    {
                        return false;
                    }
                }

                if (size >= 4)
                {
                    size -= 4;

                    if (*(int*)(pValue - size) != 0)
                    {
                        return false;
                    }
                }

                if (size >= 2)
                {
                    size -= 2;

                    if (*(short*)(pValue - size) != 0)
                    {
                        return false;
                    }
                }

                if (size >= 1)
                {
                    if (*pValue != 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static void VersionNotSupport(string methodName)
        {
            throw new PlatformNotSupportedException(StringHelper.Format("current .net version not support '{0}' method.", methodName));
        }

        /// <summary>
        /// 获取类型的 BasicTypes 值。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回 BasicTypes 值。</returns>
        public static BasicTypes GetBasicType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return BasicTypes.Boolean;
                case TypeCode.Byte:
                    return BasicTypes.Byte;
                case TypeCode.Char:
                    return BasicTypes.Char;
                case TypeCode.DateTime:
                    return BasicTypes.DateTime;
                case TypeCode.DBNull:
                    return BasicTypes.Null;
                case TypeCode.Decimal:
                    return BasicTypes.Decimal;
                case TypeCode.Double:
                    return BasicTypes.Double;
                case TypeCode.Int16:
                    return BasicTypes.Int16;
                case TypeCode.Int32:
                    return BasicTypes.Int32;
                case TypeCode.Int64:
                    return BasicTypes.Int64;
                case TypeCode.SByte:
                    return BasicTypes.SByte;
                case TypeCode.Single:
                    return BasicTypes.Single;
                case TypeCode.String:
                    return BasicTypes.String;
                case TypeCode.UInt16:
                    return BasicTypes.UInt16;
                case TypeCode.UInt32:
                    return BasicTypes.UInt32;
                case TypeCode.UInt64:
                    return BasicTypes.UInt64;
            }

            if (type.IsArray)
            {
                return BasicTypes.Array;
            }

            return BasicTypes.Object;
        }

        /// <summary>
        /// 比较参数类型集合和参数集合是否兼容。
        /// </summary>
        /// <param name="parametersTypes">参数类型集合。</param>
        /// <param name="inputParameters">参数集合。</param>
        /// <returns>返回兼容或不兼容。</returns>
        public static bool ParametersCompares(Type[] parametersTypes, object[] inputParameters)
        {
            for (int i = 0; i < parametersTypes.Length; i++)
            {
                if (parametersTypes[i].IsInstanceOfType(inputParameters[i]) || (parametersTypes[i].IsByRef && parametersTypes[i].GetElementType().IsInstanceOfType(inputParameters[i])))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 比较两个参数类型集合是否一致。
        /// </summary>
        /// <param name="parametersTypes">参数类型集合。</param>
        /// <param name="inputParameters">参数类型集合。</param>
        /// <returns>返回一致或不一致。</returns>
        public static bool ParametersCompares(Type[] parametersTypes, Type[] inputParameters)
        {
            for (int i = 0; i < parametersTypes.Length; i++)
            {
                if (parametersTypes[i] != inputParameters[i])
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void CreateTempObjectFormValueTypeRef(Type valueType, TypedReference typedRef, Action<IntPtr> tempObjectScope)
        {
            unsafe
            {
                var pType = (IntPtr*)((*(byte**)&typedRef) - sizeof(IntPtr));

                var pBackup = *pType;

                *pType = GetObjectHandle(valueType);

                tempObjectScope((IntPtr)pType);

                *pType = pBackup;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct OffsetOfTest
        {
            public byte Field1;
            public int Field2;
            public long Field3;
        }
    }
}