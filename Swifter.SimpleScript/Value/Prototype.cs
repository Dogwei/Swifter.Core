using System.Collections.Generic;

namespace Swifter.SimpleScript.Value
{
    sealed class Prototype: IValue
    {
        readonly Dictionary<string, BaseFunction> items;

        public Prototype()
        {
            items = new Dictionary<string, BaseFunction>();
        }

        public BaseFunction this[string name]
        {
            get
            {
                if (!items.TryGetValue(name, out var value))
                {
                    value = null;
                }

                return value;
            }
            set
            {
                items[name] = value;
            }
        }

        public ValueTypes Type => ValueTypes.Object;

        public bool Equal(IValue value) => ReferenceEquals(this, value);

        public string Stringify() => "[prototype]";
    }
}