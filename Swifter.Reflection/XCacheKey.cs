using System;

namespace Swifter.Reflection
{
    sealed class XCacheKey
    {
        public XCacheKey(Type type, XBindingFlags flags)
        {
            this.type = type;
            this.flags = flags;
        }

        public readonly Type type;
        public readonly XBindingFlags flags;
    }
}