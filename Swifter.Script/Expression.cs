using System.Collections.Generic;

namespace Swifter.Script
{
    public sealed class GetConstantExpression : IExpression
    {
        private readonly Constant constant;

        public GetConstantExpression(Constant constant)
        {
            this.constant = constant;
        }

        public ExpressionTypes ExpressionType => ExpressionTypes.GetConstant;

        public IValue Eval(Environment environment)
        {
            return constant;
        }
    }

    public sealed class ComputionExpression : IExpression
    {
        internal IExpression before;
        internal IExpression after;

        internal IOperator @operator;

        internal ComputionExpression(IOperator @operator)
        {
            this.@operator = @operator;
        }

        public ExpressionTypes ExpressionType => ExpressionTypes.Compution;

        public IValue Eval(Environment environment)
        {
            if (before != null)
            {
                if (after != null)
                {
                    return @operator.Middle(before.Eval(environment), after.Eval(environment));
                }

                return @operator.After(before.Eval(environment));
            }

            return @operator.Before(after.Eval(environment));
        }
    }

    public sealed class GetGlobalExpression : IExpression
    {
        internal string name;

        public GetGlobalExpression(string name)
        {
            this.name = name;
        }

        public ExpressionTypes ExpressionType => ExpressionTypes.GetField;

        public IValue Eval(Environment environment)
        {
            return environment.CurrentScope.GetField(name);
        }
    }

    public sealed class DefineFieldExpression : IExpression
    {
        private string name;

        private IExpression initialization;

        public DefineFieldExpression(string name, IExpression initialization)
        {
            this.name = name;
            this.initialization = initialization;
        }

        public ExpressionTypes ExpressionType => ExpressionTypes.DefineField;

        public IValue Eval(Environment environment)
        {
            IValue initialization = Constant.Undefined;

            if (this.initialization != null)
            {
                initialization = this.initialization.Eval(environment);
            }

            environment.CurrentScope.DefinedField(name, initialization);

            return initialization;
        }
    }

    public sealed class UnionExpression : IExpression
    {
        private IExpression[] expressions;

        public UnionExpression(IExpression[] expressions)
        {
            this.expressions = expressions;
        }

        public ExpressionTypes ExpressionType => ExpressionTypes.Union;

        public IValue Eval(Environment environment)
        {
            IValue result = Constant.Undefined;

            foreach (var item in expressions)
            {
                result = item.Eval(environment);
            }

            return result;
        }
    }
}