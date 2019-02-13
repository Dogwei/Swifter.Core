namespace Swifter.SimpleScript.Value
{
    abstract class BaseBoolean : IValue
    {
        public ValueTypes Type => ValueTypes.Boolean;

        public abstract bool Value { get; }

        public bool Equal(IValue value) => (value is BaseBoolean @base) && @base.Value == this.Value;

        public string Stringify() => Value.ToString();
    }

    sealed class BooleanConstant : BaseBoolean
    {
        readonly bool value;

        public BooleanConstant(bool value)
        {
            this.value = value;
        }

        public override bool Value => value;
    }
}