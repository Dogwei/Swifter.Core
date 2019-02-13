using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.SimpleScript
{
    sealed class CodeInterpreter
    {
        readonly Dictionary<char, List<ISyntax>> syntaxmap;

        public CodeInterpreter()
        {
            syntaxmap = new Dictionary<char, List<ISyntax>>();
        }

        public void AddSyntax(ISyntax syntax)
        {
            if (syntax == null)
            {
                throw new ArgumentException(nameof(syntax));
            }

            foreach (var item in syntax.BeginChars)
            {
                var list = GetSyntaxes(item);

                list.Add(syntax);

                list.Sort(ISyntaxSort);
            }
        }

        int ISyntaxSort(ISyntax x, ISyntax y)
        {
            return y.Priority - x.Priority;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        List<ISyntax> GetSyntaxes(char c)
        {
            if (!syntaxmap.TryGetValue(c, out var syntaxes))
            {
                syntaxes = new List<ISyntax>();

                syntaxmap.Add(c, syntaxes);
            }

            return syntaxes;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IProcess Interpret(CodeReader reader, Priorities priority)
        {
            if (TryInterpret(reader, priority, out var process))
            {
                return process;
            }

            throw new Exception("Uninterpreted code.");
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal bool TryInterpret(CodeReader reader, Priorities priority, out IProcess process)
        {
            if (reader.CurrentChar(out var c))
            {
                process = reader.Cache(priority);

                if (process != null)
                {
                    return true;
                }

                var backup = reader.Backup();

                foreach (var item in GetSyntaxes(c))
                {
                    if (priority <= item.Priority)
                    {
                        continue;
                    }

                    Continue:

                    if (item.TryInterpret(this, reader, out var result))
                    {
                        if (backup.Cache(item.Priority, result))
                        {
                            if (item.IsContinue)
                            {
                                backup.Restore();

                                goto Continue;
                            }

                            break;
                        }
                    }
                }

                backup.Restore();

                process = reader.Cache(priority);

                if (process != null)
                {
                    return true;
                }
            }

            process = null;

            return false;
        }
    }
}
