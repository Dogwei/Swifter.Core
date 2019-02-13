namespace Swifter.SimpleScript.Value
{
    abstract class BaseString : BaseObject, IValue
    {
        static readonly Prototype StringPrototype = new Prototype();

        public BaseString() : base(StringPrototype)
        {
        }

        public override IValue this[string name]
        {
            get
            {

                switch (name)
                {
                    case "length":
                        return new Int64Constant(Value.Length);
                }

                return base[name];
            }
            set => base[name] = value;
        }

        public override ValueTypes Type => ValueTypes.String;

        public abstract string Value { get; }

        public override string Stringify() => Value;

        public override bool Equal(IValue value) => value is BaseString @base && @base.Value == Value;

    }

    sealed class StringConstant : BaseString
    {
        readonly string value;

        public StringConstant(string value)
        {
            this.value = value;
        }

        public override string Value => value;
    }
}