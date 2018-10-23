namespace Swifter.NewScript
{
    public sealed class Scope
    {
        private Scope baseScope;

        public IValue GetField(string name)
        {
            return null;
        }

        public void SetField(string name, IValue value)
        {

        }

        public bool SetExistField(string name, IValue value)
        {
            return false;
        }
    }
}