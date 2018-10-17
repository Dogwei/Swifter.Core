using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Swifter
{
    /// <summary>
    /// 该文档用于解决版本差异性。
    /// </summary>
    public static class VersionDifferences
    {
#if NETSTANDARD2_0
        internal static readonly bool? DynamicAssemblyCanAccessNonPublicTypes = null;
        internal static readonly bool? DynamicAssemblyCanAccessNonPublicMembers = null;
#else
        internal static readonly bool? DynamicAssemblyCanAccessNonPublicTypes = false;
        internal static readonly bool? DynamicAssemblyCanAccessNonPublicMembers = false;
#endif
        /// <summary>
        /// 获取对象的 TypeHandle 值。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回一个 IntPtr 值。</returns>

#if NET451 || NET45 || NET40 || NET35 || NET30 || NET20 || NETCOREAPP2_0 || NETCOREAPP2_1
        [MethodImpl(AggressiveInlining)]
        public static IntPtr GetTypeHandle(object obj)
        {
            return Tools.Pointer.GetTypeHandle(obj);
        }
#else
        [MethodImpl(AggressiveInlining)]
        public static IntPtr GetTypeHandle(object obj)
        {
            if (ObjectHandleEqualsTypeHandle)
            {
                // Faster
                return Tools.Pointer.GetTypeHandle(obj);
            }
            else
            {
                // Unstable
                return obj.GetType().TypeHandle.Value;
            }
        }
#endif
        /// <summary>
        /// 获取对象 Handle 和 TypeHandle 是否一致。
        /// </summary>
        public static readonly bool ObjectHandleEqualsTypeHandle =
            typeof(DBNull).TypeHandle.Value == Tools.Pointer.GetTypeHandle(DBNull.Value) && 
            typeof(string).TypeHandle.Value == Tools.Pointer.GetTypeHandle(string.Empty);

#if NET451 || NET45 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETSTANDARD2_0
        /// <summary>
        /// 表示该方法尽量内敛。
        /// </summary>
        public const MethodImplOptions AggressiveInlining = MethodImplOptions.AggressiveInlining;
        
        /// <summary>
        /// 定义动态程序集。
        /// </summary>
        /// <param name="assName">程序集名称</param>
        /// <param name="access">程序集的可访问性</param>
        /// <returns>返回动态程序集生成器</returns>
        [MethodImpl(AggressiveInlining)]
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName assName, AssemblyBuilderAccess access)
        {
            return AssemblyBuilder.DefineDynamicAssembly(assName, access);
        }
#else
        /// <summary>
        /// 表示该方法尽量内敛。
        /// </summary>
        public const MethodImplOptions AggressiveInlining = (MethodImplOptions)256;

        /// <summary>
        /// 定义动态程序集。
        /// </summary>
        /// <param name="assName">程序集名称</param>
        /// <param name="access">程序集的可访问性</param>
        /// <returns>返回动态程序集生成器</returns>
        [MethodImpl(AggressiveInlining)]
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName assName, AssemblyBuilderAccess access)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(assName, access);
        }
#endif
    }
}

#if NETSTANDARD2_0

/// <summary>
/// 提供类型的扩展方法。
/// </summary>
public static class TypeMethods
{
    /// <summary>
    /// 类型生成器的 CreateType 扩展方法，返回 TypeInfo。
    /// </summary>
    /// <param name="typeBuilder">类型生成器</param>
    /// <returns>返回一个 TypeInfo。</returns>
    public static TypeInfo CreateType(this TypeBuilder typeBuilder)
    {
        return typeBuilder.CreateTypeInfo();
    }
}

#endif

#if NET35 || NET30 || NET20
namespace System
{
    /// <summary>
    /// 类型访问异常。
    /// </summary>
    public class TypeAccessException : TypeLoadException
    {
        /// <summary>
        /// 初始化类型访问异常。
        /// </summary>
        public TypeAccessException()
        {

        }


        /// <summary>
        /// 初始化具有指定消息的类型访问异常。
        /// </summary>
        /// <param name="message">指定消息</param>
        public TypeAccessException(string message) : base(message)
        {

        }
    }
}
#endif

#if NET30 || NET20
namespace System
{
    /// <summary>
    /// 为 .Net 2.0 和 3.0 提供必要的委托。
    /// </summary>
    public delegate void Action();

    // public delegate void Action<T1>(T1 arg1);

    /// <summary>
    /// 为 .Net 2.0 和 3.0 提供必要的委托。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    /// <summary>
    /// 为 .Net 2.0 和 3.0 提供必要的委托。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    /// <summary>
    /// 为 .Net 2.0 和 3.0 提供必要的委托。
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public delegate TResult Func<TResult>();
    /// <summary>
    /// 为 .Net 2.0 和 3.0 提供必要的委托。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="arg1"></param>
    /// <returns></returns>
    public delegate TResult Func<T1, TResult>(T1 arg1);
    /// <summary>
    /// 为 .Net 2.0 和 3.0 提供必要的委托。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <returns></returns>
    public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
    /// <summary>
    /// 为 .Net 2.0 和 3.0 提供必要的委托。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <returns></returns>
    public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
}


namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 扩展方法的特性
    /// </summary>
    public class ExtensionAttribute : Attribute
    {
    }
}
#endif