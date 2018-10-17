using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 读写器帮助类。
    /// </summary>
    public static partial class RWHelper
    {
        [ThreadStatic]
        private static HelperValueRW helperValueRW;

        private static HelperValueRW HelperValueRW
        {
            get
            {
                if (helperValueRW == null)
                {
                    helperValueRW = new HelperValueRW();
                }

                return helperValueRW;
            }
        }

        /// <summary>
        /// 为实例创建读取器。
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataReader CreateReader<T>(T obj)
        {
            var helperValueRW = HelperValueRW;

            ValueInterface<T>.Content.WriteValue(helperValueRW, obj);

            var value = helperValueRW.Content;

            if (value == null)
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataReader for BasicTypes '{0}'.", helperValueRW.GetBasicType().ToString()));
            }

            if (!(value is IDataReader))
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataReader for '{0}'.", typeof(T).Name));
            }

            if (value is IAsDataReader)
            {
                value = ((IAsDataReader)value).Content;
            }

            return (IDataReader)value;
        }

        /// <summary>
        /// 为实例创建读写器。
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataRW CreateRW<T>(T obj)
        {
            var helperValueRW = HelperValueRW;

            ValueInterface<T>.Content.WriteValue(helperValueRW, obj);

            var value = helperValueRW.Content;

            if (value == null)
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataRW for BasicTypes '{0}'.", helperValueRW.GetBasicType().ToString()));
            }

            if (value is IAsDataReader)
            {
                value = ((IAsDataReader)value).Content;
            }

            if (value is IAsDataWriter)
            {
                value = ((IAsDataWriter)value).Content;
            }

            if (!(value is IDataRW))
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataRW for '{0}'.", typeof(T).Name));
            }

            return (IDataRW)value;
        }

        /// <summary>
        /// 为类型创建一个写入器。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter CreateWriter<T>()
        {
            var helperValueRW = HelperValueRW;

            ValueInterface<T>.Content.ReadValue(helperValueRW);

            var value = helperValueRW.Content;

            if (value == null)
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataWriter for BasicTypes '{0}'.", helperValueRW.GetBasicType().ToString()));
            }

            if (!(value is IDataWriter))
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataWriter for '{0}'.", typeof(T).Name));
            }

            if (value is IAsDataWriter)
            {
                value = ((IAsDataWriter)value).Content;
            }

            return (IDataWriter)value;
        }

        /// <summary>
        /// 为实例创建一个读取器。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataReader CreateReader(object obj)
        {
            var helperValueRW = HelperValueRW;

            ValueInterface.GetInterface(obj).WriteValue(helperValueRW, obj);

            var value = helperValueRW.Content;

            if (value == null)
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataReader for BasicTypes '{0}'.", helperValueRW.GetBasicType().ToString()));
            }

            if (!(value is IDataReader))
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataReader for '{0}'.", obj.GetType().Name));
            }

            if (value is IAsDataReader)
            {
                value = ((IAsDataReader)value).Content;
            }

            return (IDataReader)value;
        }

        /// <summary>
        /// 为实例创建一个读写器。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataRW CreateRW(object obj)
        {
            var helperValueRW = HelperValueRW;

            ValueInterface.GetInterface(obj).WriteValue(helperValueRW, obj);

            var value = helperValueRW.Content;

            if (value == null)
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataRW for BasicTypes '{0}'.", helperValueRW.GetBasicType().ToString()));
            }

            if (value is IAsDataReader)
            {
                value = ((IAsDataReader)value).Content;
            }

            if (value is IAsDataWriter)
            {
                value = ((IAsDataWriter)value).Content;
            }

            if (!(value is IDataRW))
            {
                throw new NotSupportedException(StringHelper.Format("Failure to create IDataRW for '{0}'.", obj.GetType().Name));
            }

            return (IDataRW)value;
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader<T> dataReader, IDataWriter<T> dataWriter)
        {
            dataReader.OnReadAll(dataWriter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader<T> dataReader, IDataWriter dataWriter)
        {
            Copy(dataReader, dataWriter.As<T>());
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader dataReader, IDataWriter<T> dataWriter)
        {
            Copy(dataReader.As<T>(), dataWriter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        /// <param name="valueFilter">筛选器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader<T> dataReader, IDataWriter<T> dataWriter, IValueFilter<T> valueFilter)
        {
            dataReader.OnReadAll(dataWriter, valueFilter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        /// <param name="valueFilter">筛选器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader<T> dataReader, IDataWriter dataWriter, IValueFilter<T> valueFilter)
        {
            Copy(dataReader, dataWriter.As<T>(), valueFilter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        /// <param name="valueFilter">筛选器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader dataReader, IDataWriter<T> dataWriter, IValueFilter<T> valueFilter)
        {
            Copy(dataReader.As<T>(), dataWriter, valueFilter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy(IDataReader dataReader, IDataWriter dataWriter)
        {
            Copy(dataReader.As<object>(), dataWriter.As<object>());
        }
        
        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <param name="tableReader">数据源</param>
        /// <param name="tableWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy(ITableReader tableReader, ITableWriter tableWriter)
        {
            while (tableReader.Read())
            {
                tableWriter.Next();

                Copy<string>(tableReader, tableWriter);
            }
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <param name="tableReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy(ITableReader tableReader, IDataWriter dataWriter)
        {
            Copy(new TableToArrayReader(tableReader), dataWriter.As<int>());
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <param name="dataReader">数据源</param>
        /// <param name="tableWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy(IDataReader dataReader, ITableWriter tableWriter)
        {
            Copy(dataReader.As<int>(), new TableToArrayWriter(tableWriter));
        }

        /// <summary>
        /// 获取数据读取器的数据源。
        /// </summary>
        /// <typeparam name="T">数据源类型</typeparam>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>返回该数据源</returns>
        public static T GetContent<T>(IDataReader dataReader)
        {
            if (dataReader is IInitialize<T> initialize)
            {
                return initialize.Content;
            }

            if (dataReader is IDirectContent directContent)
            {
                return (T)directContent.DirectContent;
            }

            throw new NotSupportedException(StringHelper.Format("Unable Get Content By '{0}' RW.", dataReader.GetType().FullName));
        }


        /// <summary>
        /// 获取数据写入器的数据源。
        /// </summary>
        /// <typeparam name="T">数据源类型</typeparam>
        /// <param name="dataWriter">数据写入器</param>
        /// <returns>返回该数据源</returns>
        public static T GetContent<T>(IDataWriter dataWriter)
        {
            if (dataWriter is IInitialize<T> initialize)
            {
                return initialize.Content;
            }

            if (dataWriter is IDirectContent directContent)
            {
                return (T)directContent.DirectContent;
            }

            throw new NotSupportedException(StringHelper.Format("Unable Get Content By '{0}' RW.", dataWriter.GetType().FullName));
        }

        /// <summary>
        /// 获取数据读取器的数据源。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>返回该数据源</returns>
        public static object GetContent(IDataReader dataReader)
        {
            if (dataReader is IDirectContent directContent)
            {
                return directContent.DirectContent;
            }

            throw new NotSupportedException(StringHelper.Format("Unable Get Content By '{0}' RW.", dataReader.GetType().FullName));
        }


        /// <summary>
        /// 获取数据写入器的数据源。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <returns>返回该数据源</returns>
        public static object GetContent(IDataWriter dataWriter)
        {
            if (dataWriter is IDirectContent directContent)
            {
                return directContent.DirectContent;
            }

            throw new NotSupportedException(StringHelper.Format("Unable Get Content By '{0}' RW.", dataWriter.GetType().FullName));
        }
    }
}
