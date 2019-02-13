using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

#pragma warning disable 1591

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
                // Stable
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

        /// <summary>
        /// 往字符串写入器中写入一个字符串。
        /// </summary>
        /// <param name="textWriter">字符串写入器</param>
        /// <param name="chars">字符串地址</param>
        /// <param name="length">字符串长度</param>
        [MethodImpl(AggressiveInlining)]
        public static unsafe void WriteChars(TextWriter textWriter, char* chars, int length)
        {
#if NETCOREAPP2_1
            textWriter.Write(new ReadOnlySpan<char>(chars, length));
#else
            const int bufferLength = 800;
            
            var buffer = new char[bufferLength];

            for (int index = 0; index < length;)
            {
                var count = 0;

                for (; count < bufferLength && index < length; ++count, ++index)
                {
                    buffer[count] = chars[index];
                }

                textWriter.Write(buffer, 0, count);
            }
#endif
        }
    }
}

#if NETSTANDARD2_0

public static class TypeMethods
{
    public static TypeInfo CreateType(this TypeBuilder typeBuilder)
    {
        return typeBuilder.CreateTypeInfo();
    }
}

#endif

#if NET451 || NET45 || NET40 || NET35 || NET30 || NET20
namespace System
{
    [Serializable]
    public struct ValueTuple
    {
    }

    [Serializable]
    public struct ValueTuple<T1>
    {
        public T1 Item1;
        
        public ValueTuple(T1 item1)
        {
            Item1 = item1;
        }
    }
    
    [Serializable]
    public struct ValueTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;
        
        public ValueTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
    
    [Serializable]
    public struct ValueTuple<T1, T2, T3, T4>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3, T4, T5>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3,T4,T5,T6>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3,T4,T5,T6,T7>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;
        public T7 Item7;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;
        public T7 Item7;
        public TRest Rest;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Rest = rest;
        }
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;

        public ValueTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 指示应将所使用的成员上的值元组视为具有元素名称的元组。
    /// </summary>
    public sealed class TupleElementNamesAttribute : Attribute
    {
        /// <summary>
        /// 指示在类型构造的深度优先前序遍历中，哪个值元组元素应具有元素名称。
        /// </summary>
        public IList<string> TransformNames { get; }

        /// <summary>
        /// 初始化 System.Runtime.CompilerServices.TupleElementNamesAttribute 类的新实例。
        /// </summary>
        /// <param name="transformNames">一个字符串数组，该数组指示在类型构造的深度优先前序遍历中，哪个值元组事件应具有元素名称。</param>
        public TupleElementNamesAttribute(string[] transformNames)
        {
            TransformNames = transformNames;
        }
    }
}
#endif

#if NET30 || NET20
namespace System
{
    public delegate void Action();
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult Func<TResult>();
    public delegate TResult Func<T1, TResult>(T1 arg1);
    public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}


namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute
    {
    }
}
#endif