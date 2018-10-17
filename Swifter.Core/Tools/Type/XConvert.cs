using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供任意类型转换到任意类型的方法。
    /// 
    /// 此方法考虑以下方式转换
    /// (0) : 内部实现的快速方法。
    /// (1) : System.Convert 的方法。
    /// (2) : 隐式转换和显式转换方法。
    /// (3) : 静态的 Parse 和 ValueOf 方法。
    /// (4) : To, Get, get_ 开头并且后续与目标类型名称一致的实例方法。
    /// (5) : 目标类型名称一致的实例方法。
    /// </summary>
    public sealed class XConvert
    {
        internal static readonly string[] ExplicitNames = { "op_Explicit", "op_Implicit", "Parse", "ValueOf" };
        internal static readonly string[] InstanceToBefores = { "To", "Get", "get_", "" };

        internal static bool IsInitialize = false;
        internal static bool IsInitializing = false;

        static XConvert()
        {
            IsInitialize = true;
            IsInitializing = true;

            foreach (var item in typeof(Convert).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (item.IsGenericMethodDefinition)
                {
                    continue;
                }

                if (item.ReturnType == typeof(void))
                {
                    continue;
                }

                ParameterInfo[] parameters = item.GetParameters();

                if (parameters.Length != 1)
                {
                    continue;
                }

                var convertType = typeof(XConvert<,>).MakeGenericType(parameters[0].ParameterType, item.ReturnType);

                var setConvertMethod = convertType.GetMethod(nameof(XConvert<object, object>.SetConvert), new Type[] { typeof(MethodInfo) });

                setConvertMethod.Invoke(null, new object[] { item });
            }

            IsInitializing = false;
        }

        internal static void Initialize()
        {
        }

        /// <summary>
        /// 明确双向类型的转换方法。
        /// </summary>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="source">值</param>
        /// <returns>返回新值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination Convert<TSource, TDestination>(TSource source)
        {
            return XConvert<TSource, TDestination>.Convert(source);
        }

        /// <summary>
        /// 明确目标类型的转换方法。
        /// </summary>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="source">值</param>
        /// <returns>返回新值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination Convert<TDestination>(object source)
        {
            return DConvert<TDestination>.Convert(source);
        }

        /// <summary>
        /// 明确原类型的转换方法。
        /// </summary>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <param name="source">值</param>
        /// <param name="type">目标类型</param>
        /// <returns>返回新值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object Convert<TSource>(TSource source, Type type)
        {
            return SConvert<TSource>.Convert(source, type);
        }

        /// <summary>
        /// 不明确类型的转换方法。
        /// </summary>
        /// <param name="source">值</param>
        /// <param name="type">目标类型</param>
        /// <returns>返回新值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object Convert(object source, Type type)
        {
            return NConvert.Convert(source, type);
        }
    }

    internal interface IDConvert<TDestination>
    {
        TDestination Convert(object source);
    }

    internal interface ISConvert<TSource>
    {
        object Convert(TSource source);
    }

    internal interface INConvert
    {
        object Convert(object source);
    }

    /// <summary>
    /// 提供任意类型转换到任意类型的方法。
    /// 
    /// 此方法考虑以下方式转换
    /// (0) : 内部实现的快速方法。
    /// (1) : System.Convert 的方法。
    /// (2) : 隐式转换和显式转换方法。
    /// (3) : 静态的 Parse 和 ValueOf 方法。
    /// (4) : To, Get, get_ 开头并且后续与目标类型名称一致的实例方法。
    /// (5) : 目标类型名称一致的实例方法。
    /// </summary>
    /// <typeparam name="TSource">原类型</typeparam>
    /// <typeparam name="TDestination">目标目标</typeparam>
    public sealed class XConvert<TSource, TDestination> : IDConvert<TDestination>, ISConvert<TSource>, INConvert
    {
        private delegate TDestination ConvertDelegate(TSource source);
        private delegate TDestination ObjectConvertDelegate(object source);
        private delegate TDestination RefConvertDelegate(ref TSource source);

        private static ConvertDelegate convert;
        private static RefConvertDelegate refConvert;
        private static ObjectConvertDelegate objectConvert;

        static XConvert()
        {
            if (XConvert.IsInitializing)
            {
                return;
            }

            if (!XConvert.IsInitialize)
            {
                XConvert.Initialize();
            }

            if (convert != null)
            {
                return;
            }

            var tSource = typeof(TSource);

            var tDestination = typeof(TDestination);

            if (ReferenceEquals(tSource, tDestination))
            {
                convert = (ConvertDelegate)(XConvert<TSource, TSource>.ConvertDelegate)NoConvert;

                return;
            }

            if (tSource.IsValueType && tSource.IsGenericType)
            {
                var underlyingType = Nullable.GetUnderlyingType(tSource);

                if (underlyingType != null && !ReferenceEquals(tSource, underlyingType))
                {
                    var convertMethod = typeof(NullableConvert).GetMethod(nameof(NullableConvert.Convert));

                    convertMethod = convertMethod.MakeGenericMethod(underlyingType, tDestination);

                    convert = MethodHelper.CreateDelegate<ConvertDelegate>(convertMethod);

                    return;
                }
            }

            if (tDestination.IsValueType && tDestination.IsGenericType)
            {
                var underlyingType = Nullable.GetUnderlyingType(tDestination);

                if (underlyingType != null && !ReferenceEquals(tDestination, underlyingType))
                {
                    var convertMethod = typeof(NullableConvert).GetMethod(nameof(NullableConvert.ConvertTo));

                    convertMethod = convertMethod.MakeGenericMethod(tSource, underlyingType);

                    convert = MethodHelper.CreateDelegate<ConvertDelegate>(convertMethod);

                    return;
                }
            }

            var tTemp = tSource;

            do
            {
                foreach (var item in tTemp.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    ParameterInfo[] parameters;

                    if (Array.IndexOf(XConvert.ExplicitNames, item.Name) >= 0
                        && item.ReturnType == tDestination
                        && (parameters = item.GetParameters()).Length == 1
                        && parameters[0].ParameterType == tTemp)
                    {
                        convert = MethodHelper.CreateDelegate<ConvertDelegate>(item, SignatureLevels.Cast);

                        return;
                    }
                }

                tTemp = tTemp.BaseType;

            } while (tTemp != null);


            foreach (var item in tDestination.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                ParameterInfo[] parameters;

                if (Array.IndexOf(XConvert.ExplicitNames, item.Name) >= 0
                    && item.ReturnType == tDestination
                    && (parameters = item.GetParameters()).Length == 1
                    && parameters[0].ParameterType.IsAssignableFrom(tSource))
                {
                    convert = MethodHelper.CreateDelegate<ConvertDelegate>(item, SignatureLevels.Cast);

                    return;
                }
            }

            var destinationNames = new List<string>();

            destinationNames.Add(tDestination.Name);

            destinationNames.Add(tDestination.Name + "Value");

            if (tDestination.IsArray)
            {
                destinationNames.Add("Array");

                destinationNames.Add(GetName(tDestination.GetElementType()) + "Array");
            }

            if (tDestination.IsPointer)
            {
                destinationNames.Add("Pointer");

                destinationNames.Add(GetName(tDestination.GetElementType()) + "Pointer");
            }

            destinationNames.Add("Value");

            foreach (var item in tSource.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var parameters = item.GetParameters();

                if (item.IsStatic)
                {
                    if (parameters.Length == 1 && parameters[0].ParameterType == tDestination)
                    {
                        goto ParameterIsMatch;
                    }
                }
                else if (parameters.Length == 0)
                {
                    goto ParameterIsMatch;
                }

                continue;

                ParameterIsMatch:

                if (item.ReturnType == tDestination)
                {
                    foreach (var before in XConvert.InstanceToBefores)
                    {
                        if (item.Name.StartsWith(before))
                        {
                            foreach (var name in destinationNames)
                            {
                                if (item.Name.EndsWith(name) && item.Name == before + name)
                                {
                                    goto SetConvert;
                                }
                            }
                        }
                    }

                    continue;

                    SetConvert:

                    if (tSource != item.DeclaringType)
                    {
                        objectConvert = MethodHelper.CreateDelegate<ObjectConvertDelegate>(item, SignatureLevels.Cast);
                    }
                    else if (tSource.IsValueType)
                    {
                        refConvert = MethodHelper.CreateDelegate<RefConvertDelegate>(item, SignatureLevels.Cast);
                    }
                    else
                    {
                        convert = MethodHelper.CreateDelegate<ConvertDelegate>(item);
                    }

                    return;
                }
            }

            convert = DefaultConvert;
        }

        private static string GetName(Type type)
        {
            if (type.IsGenericType)
            {
                return type.Name.Substring(0, type.Name.IndexOf('`'));
            }

            return type.Name;
        }

        /// <summary>
        /// 设置该映射的转换方法。
        /// </summary>
        /// <param name="methodInfo">转换方法</param>
        public static void SetConvert(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            convert = MethodHelper.CreateDelegate<ConvertDelegate>(methodInfo);
        }

        /// <summary>
        /// 转换方法。
        /// </summary>
        /// <param name="source">值</param>
        /// <returns>返回新值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination Convert(TSource source)
        {
            if (convert != null)
            {
                return convert(source);
            }

            if (refConvert != null)
            {
                return refConvert(ref source);
            }

            return objectConvert(source);
        }

        /// <summary>
        /// 值类型的优先转换方法。
        /// </summary>
        /// <param name="source">值</param>
        /// <returns>返回新值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination Convert(ref TSource source)
        {
            if (refConvert != null)
            {
                return refConvert(ref source);
            }

            if (convert != null)
            {
                return convert(source);
            }

            return objectConvert(source);
        }

        private static TSource NoConvert(TSource source)
        {
            return source;
        }

        private static TDestination DefaultConvert(TSource source)
        {
            if (source is DBNull)
            {
                return (TDestination)(object)null;
            }

            return (TDestination)System.Convert.ChangeType(source, typeof(TDestination));
        }

        TDestination IDConvert<TDestination>.Convert(object obj)
        {
            return Convert((TSource)obj);
        }

        object ISConvert<TSource>.Convert(TSource source)
        {
            return Convert(source);
        }

        object INConvert.Convert(object source)
        {
            return Convert((TSource)source);
        }
    }

    /// <summary>
    /// 明确目标类型的转换类。
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    public static class DConvert<TDestination>
    {
        private static readonly IdCache<IDConvert<TDestination>> Cache;
        private static readonly object CacheLock;

        static DConvert()
        {
            Cache = new IdCache<IDConvert<TDestination>>();

            CacheLock = new object();
        }

        /// <summary>
        /// 转换方法。
        /// </summary>
        /// <param name="obj">值</param>
        /// <returns>返回新值</returns>
        public static TDestination Convert(object obj)
        {
            if (obj == null)
            {
                return XConvert<object, TDestination>.Convert(null);
            }

            if (obj is TDestination)
            {
                return (TDestination)obj;
            }

            var typeKey = (long)Pointer.GetTypeHandle(obj);

            IDConvert<TDestination> value;

            if (!Cache.TryGetValue(typeKey, out value))
            {
                lock (CacheLock)
                {
                    if (!Cache.TryGetValue(typeKey, out value))
                    {
                        var type = typeof(XConvert<,>).MakeGenericType(obj.GetType(), typeof(TDestination));

                        value = (IDConvert<TDestination>)Activator.CreateInstance(type);

                        Cache.Add(typeKey, value);
                    }
                }
            }

            return value.Convert(obj);
        }
    }

    /// <summary>
    /// 明确原类型的转换方法。
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public static class SConvert<TSource>
    {
        private static readonly IdCache<ISConvert<TSource>> Cache;
        private static readonly object CacheLock;

        static SConvert()
        {
            Cache = new IdCache<ISConvert<TSource>>();

            CacheLock = new object();
        }

        /// <summary>
        /// 转换方法。
        /// </summary>
        /// <param name="source">值</param>
        /// <param name="type">目标类型</param>
        /// <returns>返回新值</returns>
        public static object Convert(TSource source, Type type)
        {
            var typeKey = type.TypeHandle.Value.ToInt64();

            ISConvert<TSource> value;

            if (!Cache.TryGetValue(typeKey, out value))
            {
                lock (CacheLock)
                {
                    if (!Cache.TryGetValue(typeKey, out value))
                    {
                        var convertType = typeof(XConvert<,>).MakeGenericType(typeof(TSource), type);

                        value = (ISConvert<TSource>)Activator.CreateInstance(convertType);

                        Cache.Add(typeKey, value);
                    }
                }
            }

            return value.Convert(source);
        }
    }

    /// <summary>
    /// 未知类型的转换类。
    /// </summary>
    public static class NConvert
    {
        internal sealed class CacheKey : IEquatable<CacheKey>
        {
            public readonly long source;
            public readonly long destination;

            public CacheKey(long source, long destination)
            {
                this.source = source;
                this.destination = destination;
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public bool Equals(CacheKey other)
            {
                return other.source == source && other.destination == destination;
            }

            public override bool Equals(object obj)
            {
                return Equals((CacheKey)obj);
            }

            public override int GetHashCode()
            {
                return (source ^ destination).GetHashCode();
            }
        }

        private static readonly Dictionary<CacheKey, INConvert> Cache;
        private static readonly object CacheLock;

        static NConvert()
        {
            Cache = new Dictionary<CacheKey, INConvert>();

            CacheLock = new object();
        }

        /// <summary>
        /// 转换方法。
        /// </summary>
        /// <param name="source">值</param>
        /// <param name="type">目标类型</param>
        /// <returns>返回新值</returns>
        public static object Convert(object source, Type type)
        {
            if (source == null)
            {
                return System.Convert.ChangeType(null, type);
            }

            var typeKey = new CacheKey(Pointer.GetTypeHandle(source).ToInt64(), type.TypeHandle.Value.ToInt64());

            INConvert value;

            if (!Cache.TryGetValue(typeKey, out value))
            {
                lock (CacheLock)
                {
                    if (!Cache.TryGetValue(typeKey, out value))
                    {
                        var convertType = typeof(XConvert<,>).MakeGenericType(source.GetType(), type);

                        value = (INConvert)Activator.CreateInstance(convertType);

                        Cache.Add(typeKey, value);
                    }
                }
            }

            return value.Convert(source);
        }
    }

    internal static class NullableConvert
    {
        public static TDestination Convert<TSource, TDestination>(TSource? source) where TSource : struct
        {
            if (source == null)
            {
                object nullValue = null;

                return (TDestination)nullValue;
            }

            return XConvert<TSource, TDestination>.Convert(source.Value);
        }

        public static TDestination? ConvertTo<TSource, TDestination>(TSource source) where TDestination : struct
        {
            if (source == null)
            {
                return null;
            }

            return XConvert<TSource, TDestination>.Convert(source);
        }
    }
}