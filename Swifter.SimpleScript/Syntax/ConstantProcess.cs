namespace Swifter.SimpleScript.Syntax
{
    sealed class ConstantProcess : IProcess
    {
        readonly IValue value;

        public ConstantProcess(IValue value)
        {
            this.value = value;
        }

        public IValue Execute(RuntimeContext runtime)
        {
            return value;
        }
    }
}