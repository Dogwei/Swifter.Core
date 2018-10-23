using System;

namespace Swifter.NewScript
{
    public sealed class Constant : IValue
    {
        public static readonly Constant Null;
        public static readonly Constant Undefined;
        public static readonly Constant True;
        public static readonly Constant False;

        static Constant()
        {
            Null = new Constant();
            Undefined = new Constant();
            True = new Constant();
            False = new Constant();

            Null.ValueType = ValueTypes.Number;
            Undefined.ValueType = ValueTypes.Undefined;
            True.ValueType = ValueTypes.Boolean;
            False.ValueType = ValueTypes.Boolean;

            True.booleanValue = true;
        }

        private bool booleanValue;
        private long integerValue;
        private double floatValue;
        private decimal numberValue;
        private object value;

        private Constant()
        {

        }

        public Constant(long integerValue)
        {
            this.integerValue = integerValue;

            ValueType = ValueTypes.Integer;
        }

        public Constant(double floatValue)
        {
            this.floatValue = floatValue;

            ValueType = ValueTypes.Float;
        }

        public Constant(decimal numberValue)
        {
            this.numberValue = numberValue;

            ValueType = ValueTypes.Number;
        }

        public Constant(string stringValue)
        {
            this.value = stringValue;

            ValueType = ValueTypes.String;
        }

        public ValueTypes ValueType { get; private set; }

        public bool CanRead => true;

        public bool CanWrite => false;

        public bool GetBoolean()
        {
            switch (ValueType)
            {
                case ValueTypes.Undefined:
                case ValueTypes.Null:
                    return false;
                case ValueTypes.Integer:
                    return integerValue != 0;
                case ValueTypes.Float:
                    return floatValue != 0;
                case ValueTypes.Number:
                    return numberValue != 0;
                case ValueTypes.Boolean:
                    return booleanValue;
            }

            return value != null;
        }

        public IValue GetElement(int index)
        {
            throw new NotImplementedException();
        }

        public IValue GetField(string name)
        {
            throw new NotImplementedException();
        }

        public double GetFloat()
        {
            switch (ValueType)
            {
                case ValueTypes.Undefined:
                    throw new Exception("Cannot convert undefined to number.");
                case ValueTypes.Null:
                    throw new Exception("Cannot convert null to number.");
                case ValueTypes.Function:
                    throw new Exception("Cannot convert function to number.");
                case ValueTypes.Integer:
                    return integerValue;
                case ValueTypes.Float:
                    return floatValue;
                case ValueTypes.Number:
                    return (double)numberValue;
                case ValueTypes.Boolean:
                    throw new Exception("Cannot convert boolean to number.");
                case ValueTypes.String:
                    return (double)value;
                case ValueTypes.Array:
                    throw new Exception("Cannot convert array to number.");
            }

            throw new Exception("Cannot convert object to number.");
        }

        public long GetInteger()
        {
            switch (ValueType)
            {
                case ValueTypes.Undefined:
                    throw new Exception("Cannot convert undefined to number.");
                case ValueTypes.Null:
                    throw new Exception("Cannot convert null to number.");
                case ValueTypes.Function:
                    throw new Exception("Cannot convert function to number.");
                case ValueTypes.Integer:
                    return integerValue;
                case ValueTypes.Float:
                    return (long)floatValue;
                case ValueTypes.Number:
                    return (long)numberValue;
                case ValueTypes.Boolean:
                    throw new Exception("Cannot convert boolean to number.");
                case ValueTypes.String:
                    return (long)value;
                case ValueTypes.Array:
                    throw new Exception("Cannot convert array to number.");
            }

            throw new Exception("Cannot convert object to number.");
        }

        public decimal GetNumber()
        {
            switch (ValueType)
            {
                case ValueTypes.Undefined:
                    throw new Exception("Cannot convert undefined to number.");
                case ValueTypes.Null:
                    throw new Exception("Cannot convert null to number.");
                case ValueTypes.Function:
                    throw new Exception("Cannot convert function to number.");
                case ValueTypes.Integer:
                    return integerValue;
                case ValueTypes.Float:
                    return (decimal)floatValue;
                case ValueTypes.Number:
                    return numberValue;
                case ValueTypes.Boolean:
                    throw new Exception("Cannot convert boolean to number.");
                case ValueTypes.String:
                    return (decimal)value;
                case ValueTypes.Array:
                    throw new Exception("Cannot convert array to number.");
            }

            throw new Exception("Cannot convert object to number.");
        }

        public string GetString()
        {
            switch (ValueType)
            {
                case ValueTypes.Undefined:
                    throw new Exception("Cannot convert undefined to string.");
                case ValueTypes.Null:
                    throw new Exception("Cannot convert null to string.");
                case ValueTypes.Function:
                    throw new Exception("Cannot convert function to string.");
                case ValueTypes.Integer:
                    return integerValue.ToString();
                case ValueTypes.Float:
                    return floatValue.ToString();
                case ValueTypes.Number:
                    return numberValue.ToString();
                case ValueTypes.Boolean:
                    return booleanValue.ToString();
                case ValueTypes.String:
                    return (string)value;
                case ValueTypes.Array:
                    throw new Exception("Cannot convert array to string.");
            }

            throw new Exception("Cannot convert object to string.");
        }

        public void SetElement(int index, IValue value)
        {
            throw new Exception("Cannot set element of the constant.");
        }

        public void SetField(string name, IValue value)
        {
            throw new Exception("Cannot set field of the constant.");
        }
    }
}