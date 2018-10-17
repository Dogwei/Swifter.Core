namespace Swifter.Tools
{
    /// <summary>
    /// 基础类型枚举，此枚举不能按位合并值。
    /// </summary>
    public enum BasicTypes : byte
    {
        /// <summary>
        /// Boolean, bool
        /// </summary>
        Boolean = 1,
        /// <summary>
        /// SByte, sbyte
        /// </summary>
        SByte = 2,
        /// <summary>
        /// Int16, short
        /// </summary>
        Int16 = 3,
        /// <summary>
        /// Int32, int
        /// </summary>
        Int32 = 4,
        /// <summary>
        /// Int64, long
        /// </summary>
        Int64 = 5,
        /// <summary>
        /// Byte, byte
        /// </summary>
        Byte = 6,
        /// <summary>
        /// UInt16, ushort
        /// </summary>
        UInt16 = 7,
        /// <summary>
        /// UInt32, uint
        /// </summary>
        UInt32 = 8,
        /// <summary>
        /// UInt64, ulong
        /// </summary>
        UInt64 = 9,
        /// <summary>
        /// Single, float
        /// </summary>
        Single = 10,
        /// <summary>
        /// Double, double
        /// </summary>
        Double = 11,
        /// <summary>
        /// Decimal, decimal
        /// </summary>
        Decimal = 12,
        /// <summary>
        /// Char, char
        /// </summary>
        Char = 13,
        /// <summary>
        /// DateTime
        /// </summary>
        DateTime = 14,
        /// <summary>
        /// String, string
        /// </summary>
        String = 15,
        /// <summary>
        /// Direct
        /// 
        /// 表示可以直接读写值的类型。
        /// 通常是可以用字符串表示的值的类型。
        /// 
        /// Represents a type that can read and write value directly.
        /// is typically the type of a value that can be represented by a string.
        /// </summary>
        Direct = 16,
        /// <summary>
        /// Array
        /// </summary>
        Array = 17,
        /// <summary>
        /// Object
        /// 其他类型
        /// Other types
        /// </summary>
        Object = 18,
        /// <summary>
        /// Null, DBNull
        /// </summary>
        Null = 0
    }
}