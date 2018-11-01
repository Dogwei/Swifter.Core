using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.Script
{
    public interface IOperator
    {
        bool IsBefore { get; }

        bool IsAfter { get; }

        bool IsMiddle { get; }

        string Code { get; }

        IValue Before(IValue after);

        IValue After(IValue before);

        IValue Middle(IValue before, IValue after);

        Priorities Priority { get; }
    }
}