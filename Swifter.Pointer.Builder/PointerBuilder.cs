using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter
{
    public static class PointerBuilder
    {
        public const string AssemblyName = "Swifter.Pointer";
        public const string ModuleName = "Swifter.Pointer.dll";
        public const string ClassName = "Swifter.Tools.Pointer";
        public const MethodImplAttributes AggressiveInlining = (MethodImplAttributes)256;

        public unsafe static void BuildPointerDLL()
        {
            var AssName = new AssemblyName(
                AssemblyName);

            
            var AssBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    AssName,
                    AssemblyBuilderAccess.RunAndSave);

            var ModBuilder = AssBuilder.DefineDynamicModule(
                AssemblyName,
                ModuleName);

            var PointerTypeBuilder = ModBuilder.DefineType(
                ClassName,
                TypeAttributes.Public | TypeAttributes.Abstract |
                TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            #region -- UnBox(object)。

            {
                var UnBoxMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "UnBox",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(IntPtr),
                    new Type[] { typeof(object) });

                UnBoxMethodBuilder.DefineParameter(1, ParameterAttributes.None, "obj");

                UnBoxMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var UnBoxILGenerator = UnBoxMethodBuilder.GetILGenerator();

                UnBoxILGenerator.Emit(OpCodes.Ldarga_S, 0);
                UnBoxILGenerator.Emit(OpCodes.Ldind_I);
                UnBoxILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- UnBox(TypedReference)。

            {
                var UnBoxMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "UnBox",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(IntPtr),
                    new Type[] { typeof(TypedReference) });

                UnBoxMethodBuilder.DefineParameter(1, ParameterAttributes.None, "typedRef");

                UnBoxMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var UnBoxILGenerator = UnBoxMethodBuilder.GetILGenerator();

                UnBoxILGenerator.Emit(OpCodes.Ldarga_S, 0);
                UnBoxILGenerator.Emit(OpCodes.Ldind_I);
                UnBoxILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- Box(IntPtr)。

            {
                var BoxMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "Box",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(object),
                    new Type[] { typeof(IntPtr) });

                BoxMethodBuilder.DefineParameter(1, ParameterAttributes.None, "ptr");

                BoxMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var BoxILGenerator = BoxMethodBuilder.GetILGenerator();


                BoxILGenerator.Emit(OpCodes.Ldarga_S, 0);
                BoxILGenerator.Emit(OpCodes.Ldind_Ref);
                BoxILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- GetTypeHandle(IntPtr)。

            {
                var GetTypeHandleMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "GetTypeHandle",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(IntPtr),
                    new Type[] { typeof(IntPtr) });

                GetTypeHandleMethodBuilder.DefineParameter(1, ParameterAttributes.None, "ptr");

                GetTypeHandleMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var GetTypeHandleILGenerator = GetTypeHandleMethodBuilder.GetILGenerator();

                GetTypeHandleILGenerator.Emit(OpCodes.Ldarg_0);
                GetTypeHandleILGenerator.Emit(OpCodes.Ldind_I);
                GetTypeHandleILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- GetTypeHandle(object)。

            {
                var GetTypeHandleMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "GetTypeHandle",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(IntPtr),
                    new Type[] { typeof(object) });

                GetTypeHandleMethodBuilder.DefineParameter(1, ParameterAttributes.None, "obj");

                GetTypeHandleMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var GetTypeHandleILGenerator = GetTypeHandleMethodBuilder.GetILGenerator();

                GetTypeHandleILGenerator.Emit(OpCodes.Ldarg_0);
                GetTypeHandleILGenerator.Emit(OpCodes.Ldind_I);
                GetTypeHandleILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- SizeOf<Type>()。

            {
                var SizeOfMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "SizeOf",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(uint),
                    new Type[] { });

                var SizeOfGenericsParameters = SizeOfMethodBuilder.DefineGenericParameters("T");

                SizeOfMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var SizeOfILGenerator = SizeOfMethodBuilder.GetILGenerator();

                SizeOfILGenerator.Emit(OpCodes.Sizeof, SizeOfGenericsParameters[0]);
                SizeOfILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- CopyMemory(IntPtr, IntPtr, uint)。

            {
                var CopyMemoryMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "CopyMemory",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(void),
                    new Type[] { typeof(IntPtr), typeof(IntPtr), typeof(uint) });

                CopyMemoryMethodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                CopyMemoryMethodBuilder.DefineParameter(2, ParameterAttributes.None, "destination");
                CopyMemoryMethodBuilder.DefineParameter(3, ParameterAttributes.None, "size");

                CopyMemoryMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var CopyMemoryILGenerator = CopyMemoryMethodBuilder.GetILGenerator();

                CopyMemoryILGenerator.Emit(OpCodes.Ldarg_1);
                CopyMemoryILGenerator.Emit(OpCodes.Ldarg_0);
                CopyMemoryILGenerator.Emit(OpCodes.Ldarg_2);
                CopyMemoryILGenerator.Emit(OpCodes.Cpblk);
                CopyMemoryILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- GetValue<TValue>(IntPtr)。

            {
                var GetValueMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "GetValue",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(void),
                    new Type[] { typeof(IntPtr) });

                var GetValueGenericParameters = GetValueMethodBuilder.DefineGenericParameters(new string[] { "TValue" });

                GetValueMethodBuilder.SetReturnType(GetValueGenericParameters[0]);

                GetValueMethodBuilder.DefineParameter(1, ParameterAttributes.None, "ptr");

                GetValueMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var GetValueILGenerator = GetValueMethodBuilder.GetILGenerator();

                GetValueILGenerator.Emit(OpCodes.Ldarg_0);
                GetValueILGenerator.Emit(OpCodes.Ldobj, GetValueGenericParameters[0]);
                GetValueILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- SetValue<TValue>(IntPtr, TValue)。

            {
                var SetValueMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "SetValue",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard);

                var SetValueGenericParameters = SetValueMethodBuilder.DefineGenericParameters(new string[] { "TValue" });

                SetValueMethodBuilder.SetParameters(new Type[] { typeof(IntPtr), SetValueGenericParameters[0] });

                SetValueMethodBuilder.SetReturnType(typeof(void));

                SetValueMethodBuilder.DefineParameter(1, ParameterAttributes.None, "ptr");
                SetValueMethodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

                SetValueMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var SetValueILGenerator = SetValueMethodBuilder.GetILGenerator();

                SetValueILGenerator.Emit(OpCodes.Ldarg_0);
                SetValueILGenerator.Emit(OpCodes.Ldarg_1);
                SetValueILGenerator.Emit(OpCodes.Stobj, SetValueGenericParameters[0]);
                SetValueILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            #region -- UnBox<TValue>(ref TValue)。

            {
                var UnBoxMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "UnBox",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard,
                    typeof(IntPtr),
                    Type.EmptyTypes);

                var UnBoxGenericParameters = UnBoxMethodBuilder.DefineGenericParameters(new string[] { "TValue" });

                UnBoxMethodBuilder.SetParameters(UnBoxGenericParameters[0].MakeByRefType());

                UnBoxMethodBuilder.DefineParameter(1, ParameterAttributes.None, "value");

                UnBoxMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var UnBoxILGenerator = UnBoxMethodBuilder.GetILGenerator();

                UnBoxILGenerator.Emit(OpCodes.Ldarg_0);
                UnBoxILGenerator.Emit(OpCodes.Conv_I);
                UnBoxILGenerator.Emit(OpCodes.Ret);
            }

            #endregion
            
            #region -- Box<TValue>(IntPtr)。

            {
                var BoxMethodBuilder = PointerTypeBuilder.DefineMethod(
                    "Box",
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.Static, CallingConventions.Standard);

                var BoxGenericParameters = BoxMethodBuilder.DefineGenericParameters("TValue");

                BoxMethodBuilder.SetReturnType(BoxGenericParameters[0].MakeByRefType());

                BoxMethodBuilder.SetParameters(typeof(IntPtr));

                BoxMethodBuilder.DefineParameter(1, ParameterAttributes.None, "obj");

                BoxMethodBuilder.SetImplementationFlags(AggressiveInlining);

                var BoxILGenerator = BoxMethodBuilder.GetILGenerator();

                BoxILGenerator.Emit(OpCodes.Ldarg_0);
                BoxILGenerator.Emit(OpCodes.Ret);
            }

            #endregion

            PointerTypeBuilder.CreateType();

            AssBuilder.Save(ModuleName);
        }
    }
}