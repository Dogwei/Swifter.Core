using Swifter.Tools;
using System;

namespace Swifter.NewScript
{
    public sealed class Environment
    {
        private Scope rootScope;
        private Scope currentScope;

        public IValue GetField(string name)
        {
            var value = currentScope.GetField(name);

            if (value == null)
            {
                throw new Exception(StringHelper.Format("{0} is undefined.", name));
            }

            return value;
        }

        public void SetField(string name, IValue value)
        {
            if (!currentScope.SetExistField(name, value))
            {
                throw new Exception(StringHelper.Format("{0} is undefined.", name));
            }
        }

        public void EnterScope(string name)
        {

        }

        public void LeaveLastScope()
        {

        }
    }
}