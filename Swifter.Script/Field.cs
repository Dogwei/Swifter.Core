using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.Script
{
    public sealed class Field : IValue, IField
    {
        private IValue value;

        public Field(IValue value)
        {
            this.value = value;
        }

        public ValueTypes ValueType => value.ValueType;

        public bool CanRead => value.CanRead;

        public bool CanWrite => value.CanWrite;

        public bool GetBoolean()
        {
            return value.GetBoolean();
        }

        public IValue GetElement(int index)
        {
            return value.GetElement(index);
        }

        public IValue GetField(string name)
        {
            return value.GetField(name);
        }

        public double GetFloat()
        {
            return value.GetFloat();
        }

        public long GetInteger()
        {
            return value.GetInteger();
        }

        public decimal GetNumber()
        {
            return value.GetNumber();
        }

        public string GetString()
        {
            return value.GetString();
        }

        public void SetElement(int index, IValue value)
        {
            value.SetElement(index, value);
        }

        public void SetField(string name, IValue value)
        {
            value.SetField(name, value);
        }

        public void SetValue(IValue value)
        {
            if (value is Field field)
            {
                value = field.GetValue();
            }

            this.value = value;
        }

        public IValue GetValue()
        {
            return value;
        }
    }
}