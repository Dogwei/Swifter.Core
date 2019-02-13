using Swifter.Tools;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// XPropertyInfo 属性信息。
    /// 此属性信息提供的读写方法比 .Net 自带的 PropertyInfo 属性信息快很多。
    /// </summary>
    public abstract class XPropertyInfo
    {
        /// <summary>
        /// 创建 XPropertyInfo 属性信息。
        /// </summary>
        /// <param name="propertyInfo">.Net 自带的 PropertyInfo 属性</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XPropertyInfo 属性信息。</returns>
        public static XPropertyInfo Create(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

            var isStatic = (getMethod == null || getMethod.IsStatic) && (setMethod == null || setMethod.IsStatic);
            var declaringType = propertyInfo.DeclaringType;
            var propertyType = propertyInfo.PropertyType;

            if (propertyType.IsPointer)
            {
                propertyType = typeof(IntPtr);
            }
            else if (propertyType.IsByRef)
            {
                propertyType = propertyType.GetElementType();
            }

            Type targetType;

            if (isStatic)
            {
                targetType = typeof(XStaticPropertyInfo<>).MakeGenericType(propertyType);
            }
            else if (declaringType.IsValueType)
            {
                targetType = typeof(XStructPropertyInfo<,>).MakeGenericType(declaringType, propertyType);
            }
            else
            {
                targetType = typeof(XClassPropertyInfo<>).MakeGenericType(propertyType);
            }

            var result = (XPropertyInfo)Activator.CreateInstance(targetType);

            if (propertyInfo.PropertyType.IsByRef)
            {
                result.InitializeByRef(propertyInfo, flags);
            }
            else
            {
                result.Initialize(propertyInfo, flags);
            }

            return result;
        }
        
        internal string name;
        internal XBindingFlags flags;
        internal PropertyInfo propertyInfo;

        internal virtual void Initialize(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            this.propertyInfo = propertyInfo;

            name = propertyInfo.Name;
            this.flags = flags;
        }

        internal virtual void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            this.propertyInfo = propertyInfo;

            name = propertyInfo.Name;
            this.flags = flags;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void Assert(bool err, string name)
        {
            if (!err)
            {
                Throw();
            }

            void Throw()
            {
                throw new MissingMethodException($"Property '{PropertyInfo.DeclaringType.Name}.{PropertyInfo.Name}' No define '{name}' method or cannot access.");
            }
        }

        /// <summary>
        /// 获取 .Net 自带的 PropertyInfo 属性
        /// </summary>
        public PropertyInfo PropertyInfo => propertyInfo;

        /// <summary>
        /// 获取此属性的名称。
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 获取该属性的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该属性是静态的，则指定为 Null</param>
        /// <returns>返回该属性的值</returns>
        public abstract object GetValue(object obj);

        /// <summary>
        /// 设置该属性的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该属性是静态的，则指定为 Null</param>
        /// <param name="value">值</param>
        public abstract void SetValue(object obj, object value);

        /// <summary>
        /// 获取该属性的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该属性是静态的，则指定为 Null</param>
        /// <returns>返回该属性的值</returns>
        public abstract object GetValue(TypedReference typedRef);

        /// <summary>
        /// 设置该属性的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该属性是静态的，则指定为 Null</param>
        /// <param name="value">值</param>
        public abstract void SetValue(TypedReference typedRef, object value);
    }

    delegate ref TValue XClassRefValueHandler<TValue>(object obj);
    delegate TValue XClassGetValueHandler<TValue>(object obj);
    delegate void XClassSetValueHandler<TValue>(object obj, TValue value);

    delegate ref TValue XStructRefValueHandler<TStruct, TValue>(ref TStruct obj);
    delegate TValue XStructGetValueHandler<TStruct, TValue>(ref TStruct obj);
    delegate void XStructSetValueHandler<TStruct, TValue>(ref TStruct obj, TValue value);

    delegate ref TValue XStaticRefValueHandler<TValue>();
    delegate TValue XStaticGetValueHandler<TValue>();
    delegate void XStaticSetValueHandler<TValue>(TValue value);
}