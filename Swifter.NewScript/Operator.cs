using Swifter.Tools;
using System;

namespace Swifter.NewScript
{
    public sealed class AddOperator : IOperator
    {
        public string Code => "+";

        public Priorities Priority => Priorities.Ordinary;

        public bool IsBefore => true;

        public bool IsAfter => false;

        public bool IsMiddle => true;

        public IValue After(IValue before)
        {
            throw new NotSupportedException();
        }

        public IValue Before(IValue after)
        {
            if (after.ValueType == ValueTypes.Integer ||
                after.ValueType == ValueTypes.Number ||
                after.ValueType == ValueTypes.Float)
            {
                return after;
            }

            throw new Exception("+ 无法应用于" + after.ValueType);
        }

        public IValue Middle(IValue before, IValue after)
        {
            var beforeType = before.ValueType;
            var afterType = after.ValueType;

            if (beforeType == ValueTypes.String || afterType == ValueTypes.String)
            {
                return new Constant(before.GetString() + after.GetString());
            }

            if (beforeType == ValueTypes.Number || afterType == ValueTypes.Number)
            {
                return new Constant(before.GetNumber() + after.GetNumber());
            }

            if (beforeType == ValueTypes.Float || afterType == ValueTypes.Float)
            {
                return new Constant(before.GetFloat() + after.GetFloat());
            }

            if (beforeType == ValueTypes.Integer || afterType == ValueTypes.Integer)
            {
                return new Constant(before.GetInteger() + after.GetInteger());
            }

            throw new Exception(StringHelper.Format("{0} and {1} cannot {2}.", beforeType.ToString(), afterType.ToString(), Code));
        }
    }

    public sealed class SubtractOperator : IOperator
    {
        public string Code => "-";

        public Priorities Priority => Priorities.Ordinary;

        public bool IsBefore => true;

        public bool IsAfter => false;

        public bool IsMiddle => true;

        public IValue After(IValue before)
        {
            throw new NotSupportedException();
        }

        public IValue Before(IValue after)
        {
            switch (after.ValueType)
            {
                case ValueTypes.Integer:
                    return new Constant(-after.GetInteger());
                case ValueTypes.Float:
                    return new Constant(-after.GetFloat());
                case ValueTypes.Number:
                    return new Constant(-after.GetNumber());
                default:
                    throw new Exception("- 无法应用于" + after.ValueType);
            }

        }

        public IValue Middle(IValue before, IValue after)
        {
            var beforeType = before.ValueType;
            var afterType = after.ValueType;
            
            if (beforeType == ValueTypes.Number || afterType == ValueTypes.Number)
            {
                return new Constant(before.GetNumber() - after.GetNumber());
            }

            if (beforeType == ValueTypes.Float || afterType == ValueTypes.Float)
            {
                return new Constant(before.GetFloat() - after.GetFloat());
            }

            if (beforeType == ValueTypes.Integer || afterType == ValueTypes.Integer)
            {
                return new Constant(before.GetInteger() - after.GetInteger());
            }

            throw new Exception(StringHelper.Format("{0} and {1} cannot {2}.", beforeType.ToString(), afterType.ToString(), Code));
        }
    }

    public sealed class MultiplicationOperator : IOperator
    {
        public string Code => "*";

        public Priorities Priority => Priorities.High;

        public bool IsBefore => false;

        public bool IsAfter => false;

        public bool IsMiddle => true;

        public IValue After(IValue before)
        {
            throw new NotSupportedException();
        }

        public IValue Before(IValue after)
        {
            throw new NotSupportedException();
        }

        public IValue Middle(IValue before, IValue after)
        {
            var beforeType = before.ValueType;
            var afterType = after.ValueType;

            if (beforeType == ValueTypes.Number || afterType == ValueTypes.Number)
            {
                return new Constant(before.GetNumber() * after.GetNumber());
            }

            if (beforeType == ValueTypes.Float || afterType == ValueTypes.Float)
            {
                return new Constant(before.GetFloat() * after.GetFloat());
            }

            if (beforeType == ValueTypes.Integer || afterType == ValueTypes.Integer)
            {
                return new Constant(before.GetInteger() * after.GetInteger());
            }

            throw new Exception(StringHelper.Format("{0} and {1} cannot {2}.", beforeType.ToString(), afterType.ToString(), Code));
        }
    }

    public sealed class DivisionOperator : IOperator
    {
        public string Code => "/";

        public Priorities Priority => Priorities.High;

        public bool IsBefore => false;

        public bool IsAfter => false;

        public bool IsMiddle => true;

        public IValue After(IValue before)
        {
            throw new NotSupportedException();
        }

        public IValue Before(IValue after)
        {
            throw new NotSupportedException();
        }

        public IValue Middle(IValue before, IValue after)
        {
            var beforeType = before.ValueType;
            var afterType = after.ValueType;

            if (beforeType == ValueTypes.Number || afterType == ValueTypes.Number)
            {
                return new Constant(before.GetNumber() / after.GetNumber());
            }

            if (beforeType == ValueTypes.Float || afterType == ValueTypes.Float)
            {
                return new Constant(before.GetFloat() / after.GetFloat());
            }

            if (beforeType == ValueTypes.Integer || afterType == ValueTypes.Integer)
            {
                return new Constant(before.GetInteger() / after.GetInteger());
            }

            throw new Exception(StringHelper.Format("{0} and {1} cannot {2}.", beforeType.ToString(), afterType.ToString(), Code));
        }
    }
}