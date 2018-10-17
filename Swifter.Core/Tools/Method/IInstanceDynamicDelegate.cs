using System;

namespace Swifter.Tools
{
    /// <summary>
    /// 第一个参数是 class 类型或是 ref 类型的动态委托将实现此接口。
    /// </summary>
    public interface IInstanceDynamicDelegate
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="pObject">第一个参数</param>
        /// <param name="parameters">后续的参数</param>
        /// <returns>返回值，无返回值则为 Null。</returns>
        object Invoke(IntPtr pObject, object[] parameters);
    }
}