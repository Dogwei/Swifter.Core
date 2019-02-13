using Swifter.SimpleScript.Value;
using System;
using System.Collections.Generic;

namespace Swifter.SimpleScript.Syntax
{
    sealed class InvokeFunctionSyntax : ISyntax
    {
        public Priorities Priority => Priorities.InvokeMethod;

        public char[] BeginChars => CodeReader.NameBeginChars;

        public bool IsContinue => false;

        public bool TryInterpret(CodeInterpreter compiler, CodeReader reader, out IProcess process)
        {
            var backup = reader.Backup();

            process = null;

            if (reader.ReadName(out var name) && reader.ReadChar(out var bracketBegin) && bracketBegin == '(')
            {
                var parameters = new List<IProcess>();

            Loop:

                if (reader.CurrentChar(out var bracketEnd) && bracketEnd == ')')
                {
                    reader.Next();

                    process = new InvokeFunctionProcess(name, parameters.ToArray());

                    return true;
                }
                else
                {
                    if (parameters.Count == 0 || (reader.ReadChar(out var comma) && comma == ','))
                    {
                        parameters.Add(compiler.Interpret(reader, Priorities.ParameterSeparator));

                        goto Loop;
                    }
                }
            }

            backup.Restore();

            return false;
        }
    }

    sealed class InvokeFunctionProcess : IProcess
    {
        public InvokeFunctionProcess(string name, IProcess[] parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public readonly string Name;

        public readonly IProcess[] Parameters;

        public IValue Execute(RuntimeContext runtime)
        {
            var func = runtime.GetField(Name);

            if (!(func is BaseFunction function))
            {
                throw new ArgumentException(string.Format($"'{Name}' is not a function."));
            }

            var parameters = new IValue[Parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = Parameters[i].Execute(runtime);
            }

            return function.Invoke(parameters);
        }
    }
}