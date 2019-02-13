namespace Swifter.SimpleScript.Value
{
    abstract class BaseNumber : IValue
    {
        public ValueTypes Type => ValueTypes.Number;

        public abstract bool IsFloat { get; }

        public abstract bool IsExact { get; }

        public abstract double ReadFloat();

        public abstract decimal ReadExact();

        public abstract long ReadInteger();

        public string Stringify()
        {
            if (IsFloat)
            {
                return ReadFloat().ToString();
            }

            if (IsExact)
            {
                return ReadExact().ToString();
            }

            return ReadInteger().ToString();
        }

        public bool Equal(IValue value)
        {
            if (value is BaseNumber @base)
            {
                if (IsFloat)
                {
                    return @base.IsFloat && @base.ReadFloat() == ReadFloat();
                }

                if (IsExact)
                {
                    return @base.IsExact && @base.ReadExact() == ReadExact();
                }

                return @base.ReadInteger() == ReadInteger();
            }

            return false;
        }
    }

    sealed class DoubleConstant : BaseNumber
    {
        readonly double value;

        public DoubleConstant(double value)
        {
            this.value = value;
        }

        public override bool IsFloat => true;

        public override bool IsExact => false;

        public override decimal ReadExact()
        {
            return (decimal)value;
        }

        public override double ReadFloat()
        {
            return value;
        }

        public override long ReadInteger()
        {
            return (long)value;
        }
    }

    sealed class DecimalConstant : BaseNumber
    {
        readonly decimal value;

        public DecimalConstant(decimal value)
        {
            this.value = value;
        }

        public override bool IsFloat => false;

        public override bool IsExact => true;

        public override decimal ReadExact()
        {
            return value;
        }

        public override double ReadFloat()
        {
            return (double)value;
        }

        public override long ReadInteger()
        {
            return (long)value;
        }
    }

    sealed class Int64Constant : BaseNumber
    {
        readonly long value;

        public Int64Constant(long value)
        {
            this.value = value;
        }

        public override bool IsFloat => false;

        public override bool IsExact => false;

        public override decimal ReadExact()
        {
            return value;
        }

        public override double ReadFloat()
        {
            return value;
        }

        public override long ReadInteger()
        {
            return value;
        }
    }
}