using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供 Emit 帮助方法。
    /// </summary>
    public static class EmitHelper
    {
        private const int DifferenceSwitchMaxDepth = 2;

        /// <summary>
        /// 加载静态字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">静态字段信息</param>
        public static void LoadStaticField(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            ilGen.Emit(OpCodes.Ldsfld, fieldInfo);
        }

        /// <summary>
        /// 加载静态字段地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">静态字段信息</param>
        public static void LoadStaticFieldAddress(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            ilGen.Emit(OpCodes.Ldsflda, fieldInfo);
        }

        /// <summary>
        /// 加载字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static void LoadField(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                ilGen.Emit(OpCodes.Ldsfld, fieldInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldfld, fieldInfo);
            }
        }

        /// <summary>
        /// 加载字段地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static void LoadFieldAddress(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                ilGen.Emit(OpCodes.Ldsflda, fieldInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldflda, fieldInfo);
            }
        }

        /// <summary>
        /// 加载本地变量值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">变量信息</param>
        public static void LoadLocal(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Ldloc, localBuilder);
        }

        /// <summary>
        /// 加载本地变量地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">变量信息</param>
        public static void LoadLocalAddress(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Ldloca, localBuilder);
        }

        /// <summary>
        /// 加载参数地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="index">参数序号</param>
        public static void LoadArgumentAddress(this ILGenerator ilGen, int index)
        {
            if ((uint)index <= 255)
            {
                ilGen.Emit(OpCodes.Ldarga_S, (byte)index);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldarga, index);
            }
        }

        /// <summary>
        /// 加载 Int32 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static void LoadConstant(this ILGenerator ilGen, int value)
        {
            switch (value)
            {
                case -1:
                    ilGen.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    ilGen.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    ilGen.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    ilGen.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    ilGen.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    ilGen.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    ilGen.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if ((uint)value <= 255)
                    {
                        ilGen.Emit(OpCodes.Ldc_I4_S, (byte)value);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }

        /// <summary>
        /// 加载 Int64 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static void LoadConstant(this ILGenerator ilGen, long value)
        {
            ilGen.Emit(OpCodes.Ldc_I8, value);
        }

        /// <summary>
        /// 加载 String 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static void LoadString(this ILGenerator ilGen, string value)
        {
            ilGen.Emit(OpCodes.Ldstr, value);
        }

        /// <summary>
        /// 设置本地变量值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">本地变量信息</param>
        public static void StoreLocal(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Stloc, localBuilder);
        }

        /// <summary>
        /// 设置类型已提供的值到提供的内存上。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">值的类型</param>
        public static void StoreValue(this ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        ilGen.Emit(OpCodes.Stind_I1);
                        break;
                    case TypeCode.Char:
                        ilGen.Emit(OpCodes.Stind_I2);
                        break;
                    case TypeCode.SByte:
                        ilGen.Emit(OpCodes.Stind_I1);
                        break;
                    case TypeCode.Byte:
                        ilGen.Emit(OpCodes.Stind_I1);
                        break;
                    case TypeCode.Int16:
                        ilGen.Emit(OpCodes.Stind_I2);
                        break;
                    case TypeCode.UInt16:
                        ilGen.Emit(OpCodes.Stind_I2);
                        break;
                    case TypeCode.Int32:
                        ilGen.Emit(OpCodes.Stind_I4);
                        break;
                    case TypeCode.UInt32:
                        ilGen.Emit(OpCodes.Stind_I4);
                        break;
                    case TypeCode.Int64:
                        ilGen.Emit(OpCodes.Stind_I8);
                        break;
                    case TypeCode.UInt64:
                        ilGen.Emit(OpCodes.Stind_I8);
                        break;
                    case TypeCode.Single:
                        ilGen.Emit(OpCodes.Stind_R4);
                        break;
                    case TypeCode.Double:
                        ilGen.Emit(OpCodes.Stind_R8);
                        break;
                    default:
                        ilGen.Emit(OpCodes.Stobj, type);
                        break;
                }
            }
            else
            {
                ilGen.Emit(OpCodes.Stind_Ref);
            }
        }

        /// <summary>
        /// 在提供的内存上加载一个类型的值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">值的类型</param>
        public static void LoadValue(this ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        ilGen.Emit(OpCodes.Ldind_I1);
                        break;
                    case TypeCode.Char:
                        ilGen.Emit(OpCodes.Ldind_U2);
                        break;
                    case TypeCode.SByte:
                        ilGen.Emit(OpCodes.Ldind_I1);
                        break;
                    case TypeCode.Byte:
                        ilGen.Emit(OpCodes.Ldind_U1);
                        break;
                    case TypeCode.Int16:
                        ilGen.Emit(OpCodes.Ldind_I2);
                        break;
                    case TypeCode.UInt16:
                        ilGen.Emit(OpCodes.Ldind_U2);
                        break;
                    case TypeCode.Int32:
                        ilGen.Emit(OpCodes.Ldind_I4);
                        break;
                    case TypeCode.UInt32:
                        ilGen.Emit(OpCodes.Ldind_U4);
                        break;
                    case TypeCode.Int64:
                        ilGen.Emit(OpCodes.Ldind_I8);
                        break;
                    case TypeCode.UInt64:
                        ilGen.Emit(OpCodes.Ldind_I8);
                        break;
                    case TypeCode.Single:
                        ilGen.Emit(OpCodes.Ldind_R4);
                        break;
                    case TypeCode.Double:
                        ilGen.Emit(OpCodes.Ldind_R8);
                        break;
                    default:
                        ilGen.Emit(OpCodes.Ldobj, type);
                        break;
                }
            }
            else
            {
                ilGen.Emit(OpCodes.Ldind_Ref);
            }
        }

        /// <summary>
        /// 加载参数值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="index">参数序号</param>
        public static void LoadArgument(this ILGenerator ilGen, int index)
        {
            switch (index)
            {
                case 0:
                    ilGen.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if ((uint)index <= 255)
                    {
                        ilGen.Emit(OpCodes.Ldarg_S, (byte)index);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Ldarg, index);
                    }
                    break;
            }
        }

        /// <summary>
        /// 加载提供的数组位于提供索引出的元素的地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="elementType">元素的类型</param>
        public static void LoadElementAddress(this ILGenerator ilGen, Type elementType)
        {
            ilGen.Emit(OpCodes.Ldelema, elementType);
        }

        /// <summary>
        /// 加载类型值的大小。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static void SizeOf(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Sizeof, type);
        }

        /// <summary>
        /// 加载一个 Null 值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void LoadNull(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ldnull);
        }

        /// <summary>
        /// 当提供的值为 False 时跳到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchFalse(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Brfalse, label);
        }

        /// <summary>
        /// 当提供的值为 True 时跳到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchTrue(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Brtrue, label);
        }

        /// <summary>
        /// 当提供的值为该类型的默认值时跳到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        /// <param name="tempLocals">已声明的临时变量集合</param>
        /// <param name="label">代码块</param>
        public static void BranchDefaultValue(this ILGenerator ilGen, Type type, Dictionary<Type, LocalBuilder> tempLocals, Label label)
        {
            if (type.IsClass ||
                type.IsInterface ||
                type.IsPointer ||
                type.IsByRef ||
                type == typeof(IntPtr) ||
                type == typeof(UIntPtr))
            {
                ilGen.Emit(OpCodes.Ldnull);
                ilGen.Emit(OpCodes.Ceq);
                ilGen.Emit(OpCodes.Brtrue, label);

                return;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Single:
                    ilGen.Emit(OpCodes.Brfalse, label);
                    return;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilGen.Emit(OpCodes.Ldc_I4_0);
                    ilGen.Emit(OpCodes.Ceq);
                    ilGen.Emit(OpCodes.Brtrue, label);
                    return;
                case TypeCode.Double:
                    ilGen.Emit(OpCodes.Ldc_R8, 0D);
                    ilGen.Emit(OpCodes.Ceq);
                    ilGen.Emit(OpCodes.Brtrue, label);
                    return;
            }

            var size = (int)TypeHelper.SizeOf(type);

            if (!tempLocals.TryGetValue(type, out var local))
            {
                local = ilGen.DeclareLocal(type);

                tempLocals.Add(type, local);
            }

            var labNotEmpty = ilGen.DefineLabel();

            ilGen.StoreLocal(local);

            while (size >= 4)
            {
                size -= 4;
                ilGen.LoadLocalAddress(local);

                if (size != 0)
                {
                    ilGen.LoadConstant(size);
                    ilGen.Emit(OpCodes.Add);
                }

                ilGen.Emit(OpCodes.Ldind_I4);
                ilGen.Emit(OpCodes.Brtrue, labNotEmpty);
            }

            if (size >= 2)
            {
                size -= 2;
                ilGen.LoadLocalAddress(local);

                if (size != 0)
                {
                    ilGen.LoadConstant(size);
                    ilGen.Emit(OpCodes.Add);
                }

                ilGen.Emit(OpCodes.Ldind_I2);
                ilGen.Emit(OpCodes.Brtrue, labNotEmpty);
            }

            if (size >= 1)
            {
                ilGen.LoadLocalAddress(local);
                ilGen.Emit(OpCodes.Ldind_I1);
                ilGen.Emit(OpCodes.Brtrue, labNotEmpty);
            }

            ilGen.Emit(OpCodes.Br, label);

            ilGen.MarkLabel(labNotEmpty);
        }

        /// <summary>
        /// 设置参数值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="index">参数序号</param>
        public static void StoreArgument(this ILGenerator ilGen, int index)
        {
            if ((uint)index <= 255)
            {
                ilGen.Emit(OpCodes.Starg_S, (byte)index);
            }
            else
            {
                ilGen.Emit(OpCodes.Starg, index);
            }
        }

        /// <summary>
        /// 调用方法。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="methodBase">方法信息</param>
        public static void Call(this ILGenerator ilGen, MethodBase methodBase)
        {
            if (methodBase is ConstructorInfo constructor)
            {
                ilGen.Emit(OpCodes.Call, constructor);
            }
            else
            {
                ilGen.Emit(OpCodes.Call, (MethodInfo)methodBase);
            }
        }

        /// <summary>
        /// 将值转换为指针类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ConvertPointer(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I);
        }

        /// <summary>
        /// 将值转换为 Int32 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ConvertInt32(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I4);
        }

        /// <summary>
        /// 将值转换为 Int64 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ConvertInt64(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I8);
        }

        /// <summary>
        /// 将两个值相减。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Subtract(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Sub);
        }

        /// <summary>
        /// 分配本地内存。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void LocalAllocate(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Localloc);
        }

        /// <summary>
        /// 方法返回。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Return(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// 生成 Switch(String) 代码块。
        /// </summary>
        /// <param name="iLGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLdcValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        public static void Switch(this ILGenerator iLGen,
            Action<ILGenerator> emitLdcValue,
            CaseInfo<string>[] cases,
            Label defaultLabel, bool ignoreCase = false)
        {
            try
            {
                DifferenceSwitch(iLGen, emitLdcValue, cases, defaultLabel, ignoreCase);
            }
            catch (Exception)
            {
                HashSwitch(iLGen, emitLdcValue, cases, defaultLabel, ignoreCase);
            }
        }

        /// <summary>
        /// 生成 Switch(String) 代码块。
        /// </summary>
        /// <param name="iLGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLdcValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        private static void HashSwitch(this ILGenerator iLGen,
            Action<ILGenerator> emitLdcValue,
            CaseInfo<string>[] cases,
            Label defaultLabel, bool ignoreCase = false)
        {
            Action<ILGenerator, string> itemLdcValue = (tILGen, value) =>
            {
                tILGen.Emit(OpCodes.Ldstr, value);
            };

            if (ignoreCase)
            {
                cases = (CaseInfo<string>[])cases.Clone();

                for (int i = 0; i < cases.Length; i++)
                {
                    cases[i] = new CaseInfo<string>(cases[i].Value.ToUpper(), cases[i].Label);
                }

                Switch(
                    iLGen,
                    emitLdcValue,
                    ((Func<string, int>)StringHelper.IgnoreCaseGetHashCode).Method,
                    ((Func<string, string, bool>)StringHelper.IgnoreCaseEquals).Method,
                    cases,
                    defaultLabel,
                    itemLdcValue);
            }
            else
            {
                Switch(
                    iLGen,
                    emitLdcValue,
                    ((Func<string, int>)StringHelper.GetHashCode).Method,
                    ((Func<string, string, bool>)StringHelper.Equals).Method,
                    cases,
                    defaultLabel,
                    itemLdcValue);
            }
        }

        /// <summary>
        /// 生成 Switch(int) 代码块。
        /// </summary>
        /// <param name="ILGen">ILGenerator IL 指令生成器</param>
        /// <param name="EmitLdcValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="Cases">case 标签块集合</param>
        /// <param name="DefaultLabel">默认标签块</param>
        public static void Switch(this ILGenerator ILGen,
            Action<ILGenerator> EmitLdcValue,
            CaseInfo<int>[] Cases,
            Label DefaultLabel)
        {
            Cases = (CaseInfo<int>[])Cases.Clone();

            Array.Sort(Cases, (Before, After)=> { return Before.Value - After.Value; });

            SwitchNumber(ILGen, EmitLdcValue, Cases, DefaultLabel, (tILGen, value) => { tILGen.LoadConstant(value); }, 0, (Cases.Length - 1) / 2, Cases.Length - 1);
        }

        /// <summary>
        /// 生成 Switch(String) 代码块。字符串差异位置比较，通常情况下这比 Hash 比较要快。
        /// </summary>
        /// <param name="ILGen">ILGenerator IL 指令生成器</param>
        /// <param name="EmitLdcValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="Cases">case 标签块集合</param>
        /// <param name="DefaultLabel">默认标签块</param>
        /// <param name="IgnoreCase">是否忽略大小写</param>
        private static void DifferenceSwitch(this ILGenerator ILGen,
            Action<ILGenerator> EmitLdcValue,
            CaseInfo<string>[] Cases,
            Label DefaultLabel, bool IgnoreCase = false)
        {
            Action<ILGenerator, string> ItemLdcValue = (tILGen, value) =>
            {
                tILGen.Emit(OpCodes.Ldstr, value);
            };

            if (IgnoreCase)
            {
                Cases = (CaseInfo<string>[])Cases.Clone();

                for (int i = 0; i < Cases.Length; i++)
                {
                    Cases[i] = new CaseInfo<string>(Cases[i].Value.ToUpper(), Cases[i].Label);
                }

                DifferenceSwitch(
                    ILGen,
                    EmitLdcValue,
                    ((Func<string, string, bool>)StringHelper.IgnoreCaseEquals).Method,
                    StringUpperCharArMethod,
                    Cases,
                    DefaultLabel,
                    ItemLdcValue);
            }
            else
            {
                DifferenceSwitch(
                    ILGen,
                    EmitLdcValue,
                    ((Func<string, string, bool>)StringHelper.Equals).Method,
                    StringCharAtMethod,
                    Cases,
                    DefaultLabel,
                    ItemLdcValue);
            }
        }

        private static readonly MethodInfo StringCharAtMethod = ArrayHelper.Filter(
            typeof(string).GetProperties(),
            (item) => { var indexParameters = item.GetIndexParameters(); return indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(int); },
            item => item)
            [0].GetGetMethod();

        private static readonly MethodInfo StringUpperCharArMethod = typeof(StringHelper).GetMethod(nameof(StringHelper.UpperCharAr), BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo StringGetLengthMethod = typeof(string).GetProperty(nameof(string.Length)).GetGetMethod();

        private static void DifferenceCasesProcess(
            ILGenerator iLGen, 
            Action<ILGenerator> emitLdcValue, 
            MethodInfo stringEqualsMethod,
            MethodInfo stringCharAtMethod,
            Action<ILGenerator, string> itemLdcValue,
            Label defaultLabel, 
            CaseInfo<int>[] differenceCases)
        {
            foreach (var item in differenceCases)
            {
                iLGen.MarkLabel(item.Label);

                if (item.Tag is StringSingleGroup<CaseInfo<string>> singleGroup)
                {
                    emitLdcValue(iLGen);
                    itemLdcValue(iLGen, singleGroup.Value.Value);
                    iLGen.Call(stringEqualsMethod);
                    iLGen.Emit(OpCodes.Brtrue, singleGroup.Value.Label);
                }
                else if (item.Tag is StringDifferenceGroup<CaseInfo<string>> differenceGroup)
                {
                    var charCases = new CaseInfo<int>[differenceGroup.Groups.Count];

                    for (int i = 0; i < charCases.Length; i++)
                    {
                        charCases[i] = new CaseInfo<int>(differenceGroup.Groups[i].Key, iLGen.DefineLabel()) { Tag = differenceGroup.Groups[i].Value };
                    }

                    Switch(iLGen, (ilGen2) =>
                    {
                        emitLdcValue(ilGen2);
                        ilGen2.LoadConstant(differenceGroup.Index);
                        ilGen2.Call(stringCharAtMethod);
                    }, charCases, defaultLabel);

                    DifferenceCasesProcess(iLGen, emitLdcValue, stringEqualsMethod, stringCharAtMethod, itemLdcValue,defaultLabel, charCases);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static void DifferenceSwitch(ILGenerator iLGen,
            Action<ILGenerator> emitLdcValue,
            MethodInfo stringEqualsMethod,
            MethodInfo stringCharAtMethod,
            CaseInfo<string>[] cases,
            Label defaultLabel,
            Action<ILGenerator, string> itemLdcValue)
        {

            var lengthGroup = new StringLengthGroup<CaseInfo<string>>(cases, item => item.Value);

            if (lengthGroup.GetDepth() > DifferenceSwitchMaxDepth)
            {
                throw new ArgumentException("Groups too deep.");
            }

            var lengthCases = new CaseInfo<int>[lengthGroup.Groups.Count];

            for (int i = 0; i < lengthCases.Length; i++)
            {
                lengthCases[i] = new CaseInfo<int>(lengthGroup.Groups[i].Key, iLGen.DefineLabel()) { Tag = lengthGroup.Groups[i].Value };
            }

            Switch(iLGen, (ilGen2) =>
            {
                emitLdcValue(ilGen2);
                ilGen2.Call(StringGetLengthMethod);
            }, lengthCases, defaultLabel);

            DifferenceCasesProcess(iLGen, emitLdcValue, stringEqualsMethod, stringCharAtMethod, itemLdcValue, defaultLabel, lengthCases);
        }

        /// <summary>
        /// 生成指定类型的 Switch 代码块。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="iLGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLdcValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="getHashCodeMethod">获取 HashCode 值的方法，参数签名: int(T)</param>
        /// <param name="equalsMethod">比例两个值的方法，参数签名: bool (T, T)</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ldcCaseValue">生成加载指定 Case 块值的指定的委托</param>
        public static void Switch<T>(this ILGenerator iLGen,
            Action<ILGenerator> emitLdcValue,
            MethodInfo getHashCodeMethod,
            MethodInfo equalsMethod,
            CaseInfo<T>[] cases,
            Label defaultLabel,
            Action<ILGenerator, T> ldcCaseValue)
        {
            cases = (CaseInfo<T>[])cases.Clone();

            if (getHashCodeMethod.IsStatic)
            {
                for (int i = 0; i < cases.Length; i++)
                {
                    cases[i].HashCode = (int)getHashCodeMethod.Invoke(null, new object[] { cases[i].Value });
                }
            }
            else
            {
                for (int i = 0; i < cases.Length; i++)
                {
                    cases[i].HashCode = (int)getHashCodeMethod.Invoke(cases[i].Value, null);
                }
            }

            Array.Sort(cases);

            var GroupedCases = new Dictionary<int, List<CaseInfo<T>>>();

            foreach (var Item in cases)
            {
                List<CaseInfo<T>> Items;

                GroupedCases.TryGetValue(Item.HashCode, out Items);

                if (Items == null)
                {
                    Items = new List<CaseInfo<T>>();

                    Items.Add(Item);

                    GroupedCases.Add(Item.HashCode, Items);
                }
                else
                {
                    Items.Add(Item);
                }
            }

            var HashCodeLocal = iLGen.DeclareLocal(typeof(int));

            emitLdcValue(iLGen);
            iLGen.Emit(OpCodes.Call, getHashCodeMethod);
            iLGen.StoreLocal(HashCodeLocal);

            SwitchObject(
                iLGen,
                emitLdcValue,
                HashCodeLocal,
                (EqILGen) =>
                {
                    EqILGen.Emit(OpCodes.Call, equalsMethod);
                },
                GroupedCases.ToList(),
                defaultLabel,
                ldcCaseValue,
                0,
                (GroupedCases.Count - 1) / 2,
                GroupedCases.Count - 1);
        }


        private static void SwitchNumber<T>(this ILGenerator ILGen,
            Action<ILGenerator> LdValue,
            CaseInfo<T>[] Cases,
            Label DefaultLabel,
            Action<ILGenerator, T> ldcCase,
            int Begin,
            int Index,
            int End)
        {

            if (Begin > End)
            {
                ILGen.Emit(OpCodes.Br, DefaultLabel);

                return;
            }

            if (Begin + 1 == End)
            {
                LdValue(ILGen);
                ldcCase(ILGen, Cases[Begin].Value);
                ILGen.Emit(OpCodes.Beq, Cases[Begin].Label);

                LdValue(ILGen);
                ldcCase(ILGen, Cases[End].Value);
                ILGen.Emit(OpCodes.Beq, Cases[End].Label);

                ILGen.Emit(OpCodes.Br, DefaultLabel);

                return;
            }

            if (Begin == End)
            {
                LdValue(ILGen);
                ldcCase(ILGen, Cases[Begin].Value);
                ILGen.Emit(OpCodes.Beq, Cases[Begin].Label);

                ILGen.Emit(OpCodes.Br, DefaultLabel);

                return;
            }

            var GtLabel = ILGen.DefineLabel();

            LdValue(ILGen);
            ldcCase(ILGen, Cases[Index].Value);
            ILGen.Emit(OpCodes.Bgt, GtLabel);

            SwitchNumber(ILGen, LdValue, Cases, DefaultLabel, ldcCase, Begin, (Begin + Index) / 2, Index);

            ILGen.MarkLabel(GtLabel);

            SwitchNumber(ILGen, LdValue, Cases, DefaultLabel, ldcCase, Index + 1, (Index + 1 + End) / 2, End);
        }

        private static void SwitchObject<T>(this ILGenerator ILGen,
            Action<ILGenerator> LdcValue,
            LocalBuilder HashCodeLocal,
            Action<ILGenerator> CallEquals,
            List<KeyValuePair<int, List<CaseInfo<T>>>> Cases,
            Label DefaultLabel,
            Action<ILGenerator, T> ldcCase,
            int Begin,
            int Index,
            int End)
        {
            if (Begin > End)
            {
                return;
            }

            if (Begin == End)
            {
                ILGen.LoadLocal(HashCodeLocal);
                ILGen.LoadConstant(Cases[Begin].Key);
                ILGen.Emit(OpCodes.Bne_Un, DefaultLabel);

                foreach (var Item in Cases[Begin].Value)
                {
                    LdcValue(ILGen);
                    ldcCase(ILGen, Item.Value);
                    CallEquals(ILGen);
                    ILGen.Emit(OpCodes.Brtrue, Item.Label);
                }

                ILGen.Emit(OpCodes.Br, DefaultLabel);

                return;
            }

            if (Begin + 1 == End)
            {

                var EndLabel = ILGen.DefineLabel();

                ILGen.LoadLocal(HashCodeLocal);
                ILGen.LoadConstant(Cases[Begin].Key);
                ILGen.Emit(OpCodes.Bne_Un, EndLabel);

                foreach (var Item in Cases[Begin].Value)
                {
                    LdcValue(ILGen);
                    ldcCase(ILGen, Item.Value);
                    CallEquals(ILGen);
                    ILGen.Emit(OpCodes.Brtrue, Item.Label);
                }


                ILGen.MarkLabel(EndLabel);

                ILGen.LoadLocal(HashCodeLocal);
                ILGen.LoadConstant(Cases[End].Key);
                ILGen.Emit(OpCodes.Bne_Un, DefaultLabel);

                foreach (var Item in Cases[End].Value)
                {
                    LdcValue(ILGen);
                    ldcCase(ILGen, Item.Value);
                    CallEquals(ILGen);
                    ILGen.Emit(OpCodes.Brtrue, Item.Label);
                }

                ILGen.Emit(OpCodes.Br, DefaultLabel);

                return;
            }

            var GtLabel = ILGen.DefineLabel();

            ILGen.LoadLocal(HashCodeLocal);
            ILGen.LoadConstant(Cases[Index].Key);
            ILGen.Emit(OpCodes.Bgt, GtLabel);

            SwitchObject(
                ILGen, 
                LdcValue,
                HashCodeLocal,
                CallEquals,
                Cases,
                DefaultLabel,
                ldcCase,
                Begin, 
                (Begin + Index) / 2,
                Index);

            ILGen.MarkLabel(GtLabel);

            SwitchObject(
                ILGen,
                LdcValue,
                HashCodeLocal,
                CallEquals,
                Cases,
                DefaultLabel,
                ldcCase,
                Index + 1,
                (Index + 1 + End) / 2,
                End);
        }
    }

    /// <summary>
    /// 表示 Switch 的 Case 块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CaseInfo<T> : IComparable<CaseInfo<T>>
    {
        /// <summary>
        /// 获取 Case 块的值。
        /// </summary>
        public readonly T Value;
        /// <summary>
        /// 获取 Case 块的指令标签。
        /// </summary>
        public readonly Label Label;
        /// <summary>
        /// 获取或设置值的 HashCode 值。
        /// </summary>
        public int HashCode;

        /// <summary>
        /// 辅助变量。
        /// </summary>
        internal object Tag;

        /// <summary>
        /// 实例化 Case 块。
        /// </summary>
        /// <param name="Value">Case 块的值</param>
        /// <param name="Label">ase 块的指令标签</param>
        public CaseInfo(T Value, Label Label)
        {
            this.Value = Value;
            this.Label = Label;
        }

        /// <summary>
        /// 与另一个 Case 块信息比较 HashCode 的大小。
        /// </summary>
        /// <param name="other">Case 块信息</param>
        /// <returns>返回大于 0 则比它大，小于 0 则比它小，否则一样大</returns>
        public int CompareTo(CaseInfo<T> other)
        {
            return HashCode.CompareTo(other.HashCode);
        }
    }

}