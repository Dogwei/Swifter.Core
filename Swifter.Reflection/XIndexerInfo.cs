using Swifter.Tools;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XIndexerInfo 索引器信息。
    /// </summary>
    public sealed class XIndexerInfo
    {
        /// <summary>
        /// 创建索引器信息。
        /// </summary>
        /// <param name="propertyInfo">.Net 自带的索引器信息</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XIndexerInfo 索引器信息</returns>
        public static XIndexerInfo Create(PropertyInfo propertyInfo, XBindingFlags flags = XBindingFlags.Indexer)
        {
            return new XIndexerInfo(propertyInfo, flags);
        }

        readonly Type declaringType;
        readonly IntPtr declaringTypeHandle;
        readonly bool declaringTypeIsValueType;
        readonly XBindingFlags flags;

        XIndexerInfo(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            this.flags = flags;

            declaringType = propertyInfo.DeclaringType;
            declaringTypeHandle = TypeHelper.GetTypeHandle(declaringType);
            declaringTypeIsValueType = declaringType.IsValueType;

            PropertyInfo = propertyInfo;

            var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

            if (getMethod != null)
            {
                GetValueDelegate = MethodHelper.CreateDelegate(getMethod);
            }

            if (setMethod != null)
            {
                SetValueDelegate = MethodHelper.CreateDelegate(getMethod);
            }
        }

        /// <summary>
        /// 获取 .Net 自带的索引器信息。
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// 获取该索引器的 get 方法的委托。
        /// </summary>
        public Delegate GetValueDelegate { get; private set; }

        /// <summary>
        /// 获取该索引器的 set 方法的委托。
        /// </summary>
        public Delegate SetValueDelegate { get; private set; }

        /// <summary>
        /// 获取该索引器指定参数的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <returns>返回该值</returns>
        public object GetValue(object obj, object[] parameters)
        {
            if (GetValueDelegate == null)
            {
                if ((flags & XBindingFlags.RWCannotGetException) != 0)
                {
                    throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method.", PropertyInfo.DeclaringType.Name, PropertyInfo.Name, "get"));
                }

                return null;
            }

            if (GetValueDelegate is IInstanceDynamicDelegate instanceDynamicDelegate)
            {
                if (!declaringType.IsInstanceOfType(obj))
                {
                    throw new TargetException(nameof(obj));
                }

                if (declaringTypeIsValueType)
                {
                    return instanceDynamicDelegate.Invoke(TypeHelper.GetStructAddress(obj), parameters);
                }

                return instanceDynamicDelegate.Invoke(Tools.Pointer.UnBox(obj), parameters);
            }

            return GetValueDelegate.DynamicInvoke(parameters);
        }

        /// <summary>
        /// 获取该索引器指定参数的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <returns>返回该值</returns>
        public object GetValue(TypedReference typedRef, object[] parameters)
        {
            if (GetValueDelegate == null)
            {
                if ((flags & XBindingFlags.RWCannotGetException) != 0)
                {
                    throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method.", PropertyInfo.DeclaringType.Name, PropertyInfo.Name, "get"));
                }

                return null;
            }

            if (GetValueDelegate is IInstanceDynamicDelegate instanceDynamicDelegate)
            {
                if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
                {
                    throw new TargetException(nameof(typedRef));
                }

                if (declaringTypeIsValueType)
                {
                    return instanceDynamicDelegate.Invoke(TypeHelper.GetStructAddress(typedRef), parameters);
                }

                unsafe
                {
                    return instanceDynamicDelegate.Invoke(*(IntPtr*)Tools.Pointer.UnBox(typedRef), parameters);
                }
            }

            return GetValueDelegate.DynamicInvoke(parameters);
        }

        /// <summary>
        /// 获取静态索引器指定参数的值。
        /// </summary>
        /// <param name="parameters">索引器的参数</param>
        /// <returns>返回该值</returns>
        public object GetValue(object[] parameters)
        {
            if (GetValueDelegate == null)
            {
                if ((flags & XBindingFlags.RWCannotGetException) != 0)
                {
                    throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method.", PropertyInfo.DeclaringType.Name, PropertyInfo.Name, "get"));
                }

                return null;
            }

            return GetValueDelegate.DynamicInvoke(parameters);
        }

        /// <summary>
        /// 设置该索引器指定参数的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <param name="value">值</param>
        public void SetValue(object obj, object[] parameters, object value)
        {
            if (SetValueDelegate == null)
            {
                if ((flags & XBindingFlags.RWCannotSetException) != 0)
                {
                    throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method.", PropertyInfo.DeclaringType.Name, PropertyInfo.Name, "set"));
                }

                return;
            }

            if (SetValueDelegate is IInstanceDynamicDelegate instanceDynamicDelegate)
            {
                if (!declaringType.IsInstanceOfType(obj))
                {
                    throw new InvalidCastException();
                }

                switch (parameters.Length)
                {
                    case 1:
                        ((IAction<object, object, object>)instanceDynamicDelegate).Invoke(obj, parameters[0], value);
                        return;
                    case 2:
                        ((IAction<object, object, object, object>)instanceDynamicDelegate).Invoke(obj, parameters[0], parameters[2], value);
                        return;
                }

                Array.Resize(ref parameters, parameters.Length + 1);

                parameters[parameters.Length - 1] = value;

                if (declaringTypeIsValueType)
                {
                    instanceDynamicDelegate.Invoke(TypeHelper.GetStructAddress(obj), parameters);
                }
                else
                {
                    instanceDynamicDelegate.Invoke(Tools.Pointer.UnBox(obj), parameters);
                }

                return;
            }

            SetValue(parameters, value);
        }

        /// <summary>
        /// 设置该索引器指定参数的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <param name="value">值</param>
        public void SetValue(TypedReference typedRef, object[] parameters, object value)
        {
            if (SetValueDelegate == null)
            {
                if ((flags & XBindingFlags.RWCannotSetException) != 0)
                {
                    throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method.", PropertyInfo.DeclaringType.Name, PropertyInfo.Name, "set"));
                }

                return;
            }

            if (SetValueDelegate is IInstanceDynamicDelegate instanceDynamicDelegate)
            {
                if (!declaringType.IsAssignableFrom(__reftype(typedRef)))
                {
                    throw new InvalidCastException();
                }

                Array.Resize(ref parameters, parameters.Length + 1);

                parameters[parameters.Length - 1] = value;

                if (declaringTypeIsValueType)
                {
                    instanceDynamicDelegate.Invoke(TypeHelper.GetStructAddress(typedRef), parameters);
                }
                else
                {
                    unsafe
                    {
                        instanceDynamicDelegate.Invoke(*(IntPtr*)Tools.Pointer.UnBox(typedRef), parameters);
                    }
                }

                return;
            }

            SetValue(parameters, value);
        }

        /// <summary>
        /// 设置静态索引器指定参数的值。
        /// </summary>
        /// <param name="parameters">索引器的参数</param>
        /// <param name="value">值</param>
        public void SetValue(object[] parameters, object value)
        {
            if (SetValueDelegate == null)
            {
                if ((flags & XBindingFlags.RWCannotSetException) != 0)
                {
                    throw new MissingMethodException(StringHelper.Format("Property '{0}.{1}' No define '{2}' method.", PropertyInfo.DeclaringType.Name, PropertyInfo.Name, "set"));
                }

                return;
            }

            switch (parameters.Length)
            {
                case 1:
                    ((IAction<object, object>)SetValueDelegate).Invoke(parameters[0], value);
                    break;
                case 2:
                    ((IAction<object, object, object>)SetValueDelegate).Invoke(parameters[0], parameters[1], value);
                    break;
                default:
                    Array.Resize(ref parameters, parameters.Length + 1);
                    parameters[parameters.Length - 1] = value;
                    SetValueDelegate.DynamicInvoke(parameters);
                    break;
            }
        }
    }
}
