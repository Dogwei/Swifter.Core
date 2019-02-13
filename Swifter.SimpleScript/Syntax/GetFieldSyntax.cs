namespace Swifter.SimpleScript.Syntax
{
    sealed class GetFieldSyntax : ISyntax
    {
        public Priorities Priority => Priorities.GetField;

        public char[] BeginChars => CodeReader.NameBeginChars;

        public bool IsContinue => false;

        public bool TryInterpret(CodeInterpreter compiler, CodeReader reader, out IProcess process)
        {
            process = null;

            var backup = reader.Backup();

            if (reader.ReadName(out var name))
            {
                process = new GetFieldProcess(name);

                return true;
            }

            backup.Restore();

            return false;
        }
    }

    sealed class GetFieldProcess : IProcess
    {
        public readonly string Name;

        public GetFieldProcess(string name)
        {
            Name = name;
        }

        public IValue Execute(RuntimeContext runtime)
        {
            return runtime.GetField(Name);
        }
    }
}
