using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.SimpleScript.Syntax
{
    sealed class BracketSyntax : ISyntax
    {
        static readonly char[] _BeginChars = { '(' };

        public Priorities Priority => Priorities.InvokeMethod;

        public char[] BeginChars => _BeginChars;

        public bool IsContinue => false;

        public bool TryInterpret(CodeInterpreter compiler, CodeReader reader, out IProcess process)
        {
            var backup = reader.Backup();

            if (reader.ReadChar(out var bracketBegin) && bracketBegin == '(')
            {
                process = compiler.Interpret(reader, Priorities.None);

                if (reader.ReadChar(out var bracketEnd) && bracketEnd == ')')
                {
                    return true;
                }
            }

            process = null;

            backup.Restore();

            return false;
        }
    }
}
