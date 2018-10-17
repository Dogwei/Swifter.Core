using System;

namespace Swifter.Tools
{
    /// <summary>
    /// 运行时函数参数签名标识
    /// </summary>
    public sealed class RuntimeMethodSign
    {
        /// <summary>
        /// 构造运行时函数参数签名标识
        /// </summary>
        /// <param name="Name">函数名称</param>
        /// <param name="Params">函数的参数</param>
        public RuntimeMethodSign(string Name, object[] Params)
        {
            name = Name;
            parameters = Params;
            hashCode = Name.GetHashCode() ^ (46104728 * Params.Length);

            isInputParams = true;
        }

        /// <summary>
        /// 构造运行时函数参数签名标识
        /// </summary>
        /// <param name="Name">函数名称</param>
        /// <param name="Types">函数的参数类型</param>
        public RuntimeMethodSign(string Name, Type[] Types):this(Name, (object[])Types)
        {
            isInputParams = false;
        }

        private readonly bool isInputParams;
        private readonly int hashCode;
        private readonly string name;
        private readonly object[] parameters;

        /// <summary>
        /// 返回此方法签名 HashCode。此值考虑方法名和参数生成。
        /// </summary>
        /// <returns>一个 HashCode 值。</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }


        /// <summary>
        /// 比较一个对象的实例是否为 RuntimeMethodSign 类型，并且和当前实例的签名相同。
        /// </summary>
        /// <param name="obj">对象的实例</param>
        /// <returns>返回一个 bool 值</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is RuntimeMethodSign))
            {
                return false;
            }

            var Object = (RuntimeMethodSign)obj;

            if (name == Object.name && parameters.Length == Object.parameters.Length)
            {
                if (isInputParams)
                {
                    return TypeHelper.ParametersCompares((Type[])Object.parameters, parameters);
                }
                else if (Object.isInputParams)
                {
                    return TypeHelper.ParametersCompares((Type[])parameters, Object.parameters);
                }
                else
                {
                    return TypeHelper.ParametersCompares((Type[])Object.parameters, (Type[])parameters);
                }
            }

            return false;


        }
    }
}