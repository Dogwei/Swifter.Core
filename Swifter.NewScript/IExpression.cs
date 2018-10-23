using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.NewScript
{
    public interface IExpression
    {
        ExpressionTypes ExpressionType { get; }

        IValue Eval(Environment environment);
    }
}
