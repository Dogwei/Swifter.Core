namespace Swifter.SimpleScript.Value
{
    abstract class BaseFunction : BaseObject
    {
        static readonly Prototype FunctionPrototype = new Prototype();

        public BaseFunction() : base(FunctionPrototype)
        {
        }

        public override ValueTypes Type => ValueTypes.Function;

        public abstract IValue Invoke(IValue[] parameters);

        public override string Stringify() => "function()";
    }

    sealed class ExternFunction : BaseFunction
    {
        public override IValue Invoke(IValue[] parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}