namespace Swifter.NewScript
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
}
