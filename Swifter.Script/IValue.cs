namespace Swifter.Script
{
    public interface IValue
    {
        ValueTypes ValueType { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        long GetInteger();

        double GetFloat();

        decimal GetNumber();

        string GetString();

        bool GetBoolean();

        IValue GetField(string name);

        IValue GetElement(int index);

        void SetField(string name, IValue value);

        void SetElement(int index, IValue value);
    }

    public interface IField
    {
        void SetValue(IValue value);
    }
}