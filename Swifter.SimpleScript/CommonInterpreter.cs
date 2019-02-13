using Swifter.SimpleScript.Syntax;
using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.SimpleScript
{
    public static class CommonInterpreter
    {
        static readonly CodeInterpreter CodeInterpreter = new CodeInterpreter();
        static readonly NameCache<CodeReader> ReaderCaches = new NameCache<CodeReader>();

        static CommonInterpreter()
        {
            CodeInterpreter.AddSyntax(new InvokeFunctionSyntax());
            CodeInterpreter.AddSyntax(new GetFieldSyntax());
            CodeInterpreter.AddSyntax(new NumberConstantSyntax());
            CodeInterpreter.AddSyntax(new AddOperatorSyntax());
            CodeInterpreter.AddSyntax(new SubOperatorSyntax());
            CodeInterpreter.AddSyntax(new MulOperatorSyntax());
            CodeInterpreter.AddSyntax(new DivOperatorSyntax());
            CodeInterpreter.AddSyntax(new ModOperatorSyntax());
            CodeInterpreter.AddSyntax(new RightOperatorSyntax());
            CodeInterpreter.AddSyntax(new LeftOperatorSyntax());
            CodeInterpreter.AddSyntax(new LessOperatorSyntax());
            CodeInterpreter.AddSyntax(new GreaterOperatorSyntax());
            CodeInterpreter.AddSyntax(new LessEqualOperatorSyntax());
            CodeInterpreter.AddSyntax(new GreaterEqualOperatorSyntax());
            CodeInterpreter.AddSyntax(new EqualOperatorSyntax());
            CodeInterpreter.AddSyntax(new NotEqualOperatorSyntax());
            CodeInterpreter.AddSyntax(new ByBitAndOperatorSyntax());
            CodeInterpreter.AddSyntax(new ByBitOrOperatorSyntax());
            CodeInterpreter.AddSyntax(new ByBitNonOperatorSyntax());
            CodeInterpreter.AddSyntax(new AndOperatorSyntax());
            CodeInterpreter.AddSyntax(new OrOperatorSyntax()); 
            CodeInterpreter.AddSyntax(new BracketSyntax());
            CodeInterpreter.AddSyntax(new VarSyntax());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IProcess Interpret(string text)
        {
            unsafe
            {
                fixed (char* pText = text)
                {
                    var value = ReaderCaches.LockGetOrAdd(text, (text2) => {

                        fixed (char* pText2 = text2)
                        {
                            return new CodeReader(pText2, 0, text2.Length);
                        }
                    });

                    var reader = new CodeReader(pText, 0, text.Length, value);

                    UnionProcess unionProcess = new UnionProcess();
                    
                    while (CodeInterpreter.TryInterpret(reader, Priorities.None, out var process))
                    {
                        while (reader.SkipEnd())
                        {

                        }

                        unionProcess.AddProcess(process);

                        if (!reader.SkipBlank())
                        {
                            return unionProcess;
                        }
                    }

                    throw new Exception("Uninterpreted code.");
                }
            }
        }
    }
}