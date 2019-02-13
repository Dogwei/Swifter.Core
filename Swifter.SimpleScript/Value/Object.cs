using System.Collections.Generic;

namespace Swifter.SimpleScript.Value
{
    abstract class BaseObject : IValue
    {
        readonly Dictionary<string, IValue> fields;

        public BaseObject(Prototype prototype)
        {
            fields = new Dictionary<string, IValue>();

            Prototype = prototype;
        }

        public virtual IValue this[string name]
        {
            get
            {
                switch (name)
                {
                    case "prototype":
                        return Prototype;
                }

                if (!fields.TryGetValue(name, out var value))
                {
                    value = null;
                }

                return value;
            }
            set
            {
                fields[name] = value;
            }
        }

        public Prototype Prototype { get; private set; }

        public virtual IValue this[int index]
        {
            get
            {
                if (!fields.TryGetValue(index.ToString(), out var value))
                {
                    value = null;
                }

                return value;
            }
            set
            {
                fields[index.ToString()] = value;
            }
        }

        public IEnumerable<string> Keys => fields.Keys;

        public virtual ValueTypes Type => ValueTypes.Object;

        public virtual string Stringify()
        {
            return "[object]";
        }

        public virtual bool Equal(IValue value) => ReferenceEquals(this, value);
    }
}