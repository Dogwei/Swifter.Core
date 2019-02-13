using System.Collections.Generic;

namespace Swifter.SimpleScript
{
    sealed class UnionProcess : IProcess
    {
        readonly LinkedList<IProcess> Processes;

        public UnionProcess()
        {
            Processes = new LinkedList<IProcess>();
        }

        internal void AddProcess(IProcess process)
        {
            Processes.AddLast(process);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            IValue result = null;

            foreach (var item in Processes)
            {
                result = item.Execute(runtime);
            }

            return result;
        }
    }
}
