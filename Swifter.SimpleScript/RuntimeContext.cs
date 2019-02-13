using Swifter.SimpleScript.Value;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.SimpleScript
{
    public sealed class RuntimeContext
    {
        readonly Dictionary<string, IValue> fields = new Dictionary<string, IValue>();

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IValue GetField(string name)
        {
            if (fields.TryGetValue(name, out var value))
            {
                return value;
            }

            return null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void DefineField(string name, IValue value)
        {
            fields[name] = value;
        }
    }

    sealed class AddFunction : BaseFunction
    {
        public override IValue Invoke(IValue[] parameters)
        {
            return new DoubleConstant(((BaseNumber)parameters[0]).ReadFloat() + ((BaseNumber)parameters[1]).ReadFloat());
        }
    }
}
