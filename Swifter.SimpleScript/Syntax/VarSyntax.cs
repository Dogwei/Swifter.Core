namespace Swifter.SimpleScript.Syntax
{
    sealed class VarSyntax : ISyntax
    {
        static readonly char[] _BeginChars = { 'v' };

        public Priorities Priority => Priorities.DefindVar;

        public char[] BeginChars => _BeginChars;

        public bool IsContinue => false;

        public bool TryInterpret(CodeInterpreter compiler, CodeReader reader, out IProcess process)
        {
            var backup = reader.Backup();

            process = null;

            if (reader.ReadName(out var _var) && _var == "var")
            {
                Loop:

                if (reader.ReadName(out var name))
                {
                    IProcess initValue = null;

                    if (reader.CurrentChar(out var c) && c == '=')
                    {
                        reader.Next();

                        initValue = compiler.Interpret(reader, Priorities.AssignValueOperator);
                    }

                    if (reader.CurrentChar(out var c1) && c1 == ',')
                    {
                        reader.Next();

                        process = new VarProcess(name, initValue, process);

                        goto Loop;
                    }

                    process = new VarProcess(name, initValue, process);

                    return true;
                }
            }

            backup.Restore();

            return false;
        }
    }

    sealed class VarProcess : IProcess
    {
        readonly IProcess Previous;
        readonly string Name;
        readonly IProcess InitValue;

        public VarProcess(string name, IProcess initValue, IProcess previous)
        {
            Name = name;
            InitValue = initValue;
            Previous = previous;
        }

        public IValue Execute(RuntimeContext runtime)
        {
            if (Previous != null)
            {
                Previous.Execute(runtime);
            }

            var value = InitValue.Execute(runtime);

            runtime.DefineField(Name, value);

            return value;
        }
    }
}