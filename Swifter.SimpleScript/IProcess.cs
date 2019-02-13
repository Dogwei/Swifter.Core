using Swifter.SimpleScript.Value;
using System.Runtime.CompilerServices;

namespace Swifter.SimpleScript
{
    public interface IProcess
    {
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        IValue Execute(RuntimeContext runtime);
    }
}
