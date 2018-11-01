using Swifter.Tools;
using System;

namespace Swifter.Script
{
    public sealed class Environment
    {
        public Environment()
        {
            RootScope = new Scope();

            CurrentScope = RootScope;
        }

        public Scope RootScope { get; private set; }

        public Scope CurrentScope { get; private set; }

        public void EnterScope(string name)
        {

        }

        public void LeaveLastScope()
        {

        }
    }
}