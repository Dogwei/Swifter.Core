using System.Runtime.CompilerServices;

namespace Swifter.SimpleScript
{
    public interface IValue
    {
        ValueTypes Type { get; }
        
        string Stringify();
        
        bool Equal(IValue value);
    }
}
