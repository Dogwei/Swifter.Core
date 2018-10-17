using Swifter.Tools;
using System;

namespace Swifter.Tools
{
    /// <summary>
    /// 表示函数的参数签名标识
    /// </summary>
    public sealed class RuntimeParamsSign
    {
        /// <summary>
        /// 构造运行时函数参数签名标识
        /// </summary>
        /// <param name="parameters">输入参数</param>
        public RuntimeParamsSign(object[] parameters)
        {
            this.parameters = parameters;
            hashCode = 46104728 * parameters.Length;

            isInputParameters = true;
        }

        /// <summary>
        /// 构造运行时函数参数签名标识
        /// </summary>
        /// <param name="Types">参数类型</param>
        public RuntimeParamsSign(Type[] Types)
            : this((object[])Types)
        {
            isInputParameters = false;
        }

        private readonly object[] parameters;
        private readonly bool isInputParameters;
        private readonly int hashCode;

        /// <summary>
        /// 返回此方法签名 HashCode。此值只考虑参数生成。
        /// </summary>
        /// <returns>一个 HashCode 值。</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        /// 比较一个对象的实例是否为 RuntimeParamsSign 类型，并且和当前实例的签名相同。
        /// </summary>
        /// <param name="obj">对象的实例</param>
        /// <returns>返回一个 bool 值</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is RuntimeParamsSign))
            {
                return false;
            }

            var Object = (RuntimeParamsSign)obj;

            if (parameters.Length == Object.parameters.Length)
            {
                if (isInputParameters)
                {
                    return TypeHelper.ParametersCompares((Type[])Object.parameters, parameters);
                }
                else if (Object.isInputParameters)
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