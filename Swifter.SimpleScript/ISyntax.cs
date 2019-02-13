using System.Runtime.CompilerServices;

namespace Swifter.SimpleScript
{
    interface ISyntax
    {
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool TryInterpret(CodeInterpreter compiler, CodeReader reader, out IProcess process);
        
        Priorities Priority { get; }
        
        char[] BeginChars { get; }
        
        bool IsContinue { get; }
    }
}