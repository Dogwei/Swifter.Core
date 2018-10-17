using System;
using System.Runtime.InteropServices;

namespace Swifter.Json
{
    /// <summary>
    /// 提供字符串的缓存
    /// </summary>
    public sealed unsafe class HGlobalChars
    {
        [ThreadStatic]
        private static HGlobalChars threadInstance;

        /// <summary>
        /// 当前线程的实例
        /// </summary>
        public static HGlobalChars ThreadInstance
        {
            get
            {
                var value = threadInstance;

                if (value == null)
                {
                    value = new HGlobalChars();

                    threadInstance = value;
                }

                return value;
            }
        }

        /// <summary>
        /// 全局字符串内存地址
        /// </summary>
        public char* chars;
        /// <summary>
        /// 字符串长度
        /// </summary>
        public int count;

        /// <summary>
        /// 释放全局内存。
        /// </summary>
        ~HGlobalChars()
        {
            if (chars != null)
            {
                Marshal.FreeHGlobal((IntPtr)chars);
            }
        }

        /// <summary>
        /// 扩展字符串长度。
        /// </summary>
        /// <param name="expandMinSize">最小扩展长度</param>
        public void Expand(int expandMinSize)
        {
            count = count * 3 + expandMinSize;

            if (chars == null)
            {
                chars = (char*)Marshal.AllocHGlobal(count * sizeof(char));
            }
            else
            {
                chars = (char*)Marshal.ReAllocHGlobal((IntPtr)chars, (IntPtr)(count * sizeof(char)));
            }
        }
    }
}