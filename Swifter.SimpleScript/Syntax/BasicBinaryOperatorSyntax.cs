using Swifter.SimpleScript.Value;
using System;

namespace Swifter.SimpleScript.Syntax
{
    abstract class BasicBinaryOperatorSyntax : ISyntax
    {
        protected readonly IProcess before;
        protected readonly IProcess after;

        protected BasicBinaryOperatorSyntax(IProcess before, IProcess after)
        {
            this.before = before;
            this.after = after;
        }

        protected BasicBinaryOperatorSyntax()
        {

        }

        public abstract Priorities Priority { get; }

        public abstract string OperatorText { get; }

        public abstract IProcess CreateProcess(IProcess before, IProcess after);

        public char[] BeginChars => CodeReader.AnyBeginChars;

        public bool IsContinue => true;

        public bool TryInterpret(CodeInterpreter compiler, CodeReader reader, out IProcess process)
        {
            var backup = reader.Backup();

            process = null;

            if (compiler.TryInterpret(reader, Priority, out var before))
            {
                if (reader.ReadOperator(out var operatorText) && OperatorText == operatorText)
                {
                    if (compiler.TryInterpret(reader, Priority, out var after))
                    {
                        process = CreateProcess(before, after);

                        return true;
                    }
                }
            }

            backup.Restore();

            return false;
        }
    }

    sealed class AddOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public AddOperatorSyntax()
        {
        }

        public AddOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.BinaryOperatorMedium;

        public override string OperatorText => "+";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new AddOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before.Type == ValueTypes.String || after.Type == ValueTypes.String)
            {
                return new StringConstant(before.Stringify() + after.Stringify());
            }

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new DoubleConstant(nBefore.ReadFloat() + nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new DecimalConstant(nBefore.ReadExact() + nAfter.ReadExact());
                }

                return new Int64Constant(nBefore.ReadInteger() + nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compute.");
        }
    }
    
    sealed class SubOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public SubOperatorSyntax()
        {
        }

        public SubOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.BinaryOperatorMedium;

        public override string OperatorText => "-";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new SubOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);
            
            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new DoubleConstant(nBefore.ReadFloat() - nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new DecimalConstant(nBefore.ReadExact() - nAfter.ReadExact());
                }

                return new Int64Constant(nBefore.ReadInteger() - nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compute.");
        }
    }

    sealed class MulOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public MulOperatorSyntax()
        {
        }

        public MulOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.BinaryOperatorHigh;

        public override string OperatorText => "*";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new MulOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new DoubleConstant(nBefore.ReadFloat() * nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new DecimalConstant(nBefore.ReadExact() * nAfter.ReadExact());
                }

                return new Int64Constant(nBefore.ReadInteger() * nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compute.");
        }
    }

    sealed class DivOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public DivOperatorSyntax()
        {
        }

        public DivOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.BinaryOperatorHigh;

        public override string OperatorText => "/";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new DivOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);
            
            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new DoubleConstant(nBefore.ReadFloat() / nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new DecimalConstant(nBefore.ReadExact() / nAfter.ReadExact());
                }

                return new Int64Constant(nBefore.ReadInteger() / nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compute.");
        }
    }

    sealed class ModOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public ModOperatorSyntax()
        {
        }

        public ModOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.BinaryOperatorHigh;

        public override string OperatorText => "%";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new ModOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new DoubleConstant(nBefore.ReadFloat() % nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new DecimalConstant(nBefore.ReadExact() % nAfter.ReadExact());
                }

                return new Int64Constant(nBefore.ReadInteger() % nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compute.");
        }
    }
    
    sealed class RightOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public RightOperatorSyntax()
        {
        }

        public RightOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.BinaryOperatorLow;

        public override string OperatorText => ">>";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new RightOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter && !nAfter.IsExact && !nAfter.IsFloat)
            {
                return new Int64Constant(nBefore.ReadInteger() >> (int)nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compute.");
        }
    }

    sealed class LeftOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public LeftOperatorSyntax()
        {
        }

        public LeftOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.BinaryOperatorLow;

        public override string OperatorText => "<<";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new LeftOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter && !nAfter.IsExact && !nAfter.IsFloat)
            {
                return new Int64Constant(nBefore.ReadInteger() << (int)nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compute.");
        }
    }

    sealed class LessOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public LessOperatorSyntax()
        {
        }

        public LessOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.CompareOperator;

        public override string OperatorText => "<";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new LessOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new BooleanConstant(nBefore.ReadFloat() < nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new BooleanConstant(nBefore.ReadExact() < nAfter.ReadExact());
                }

                return new BooleanConstant(nBefore.ReadInteger() < nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compare.");
        }
    }

    sealed class GreaterOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public GreaterOperatorSyntax()
        {
        }

        public GreaterOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.CompareOperator;

        public override string OperatorText => ">";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new GreaterOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new BooleanConstant(nBefore.ReadFloat() > nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new BooleanConstant(nBefore.ReadExact() > nAfter.ReadExact());
                }

                return new BooleanConstant(nBefore.ReadInteger() > nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compare.");
        }
    }
    
    sealed class LessEqualOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public LessEqualOperatorSyntax()
        {
        }

        public LessEqualOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.CompareOperator;

        public override string OperatorText => "<=";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new LessEqualOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new BooleanConstant(nBefore.ReadFloat() <= nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new BooleanConstant(nBefore.ReadExact() <= nAfter.ReadExact());
                }

                return new BooleanConstant(nBefore.ReadInteger() <= nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compare.");
        }
    }

    sealed class GreaterEqualOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public GreaterEqualOperatorSyntax()
        {
        }

        public GreaterEqualOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.CompareOperator;

        public override string OperatorText => ">=";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new GreaterEqualOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                if (nBefore.IsFloat || nAfter.IsFloat)
                {
                    return new BooleanConstant(nBefore.ReadFloat() >= nAfter.ReadFloat());
                }

                if (nBefore.IsExact || nAfter.IsExact)
                {
                    return new BooleanConstant(nBefore.ReadExact() >= nAfter.ReadExact());
                }

                return new BooleanConstant(nBefore.ReadInteger() >= nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compare.");
        }
    }
    
    sealed class EqualOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public EqualOperatorSyntax()
        {
        }

        public EqualOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.EqualsOperator;

        public override string OperatorText => "==";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new EqualOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before == null)
            {
                return new BooleanConstant(after == null);
            }

            return new BooleanConstant(before.Equal(after));
        }
    }
    
    sealed class NotEqualOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public NotEqualOperatorSyntax()
        {
        }

        public NotEqualOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.EqualsOperator;

        public override string OperatorText => "!=";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new NotEqualOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before == null)
            {
                return new BooleanConstant(after != null);
            }

            return new BooleanConstant(!before.Equal(after));
        }
    }
    
    sealed class ByBitAndOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public ByBitAndOperatorSyntax()
        {
        }

        public ByBitAndOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.ByBitAndOperator;

        public override string OperatorText => "&";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new ByBitAndOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                return new Int64Constant(nBefore.ReadInteger() & nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compare.");
        }
    }
    
    sealed class ByBitOrOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public ByBitOrOperatorSyntax()
        {
        }

        public ByBitOrOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.ByBitOrOperator;

        public override string OperatorText => "|";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new ByBitOrOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                return new Int64Constant(nBefore.ReadInteger() | nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compare.");
        }
    }
    
    sealed class ByBitNonOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public ByBitNonOperatorSyntax()
        {
        }

        public ByBitNonOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.ByBitNonOperator;

        public override string OperatorText => "^";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new ByBitNonOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);
            var after = this.after.Execute(runtime);

            if (before is BaseNumber nBefore && after is BaseNumber nAfter)
            {
                return new Int64Constant(nBefore.ReadInteger() ^ nAfter.ReadInteger());
            }

            throw new ArgumentException("Unable to compare.");
        }
    }
    
    sealed class AndOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public AndOperatorSyntax()
        {
        }

        public AndOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.AndOperator;

        public override string OperatorText => "&&";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new AndOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);

            if (before is BaseBoolean nBefore)
            {
                if (nBefore.Value)
                {
                    var after = this.after.Execute(runtime);

                    if (after is BaseBoolean nAfter)
                    {
                        return new BooleanConstant(nAfter.Value);
                    }
                }
                else
                {
                    return new BooleanConstant(false);
                }
            }

            throw new ArgumentException("Unable to compare.");
        }
    }
    
    sealed class OrOperatorSyntax : BasicBinaryOperatorSyntax, IProcess
    {
        public OrOperatorSyntax()
        {
        }

        public OrOperatorSyntax(IProcess before, IProcess after) : base(before, after)
        {
        }

        public override Priorities Priority => Priorities.OrOperator;

        public override string OperatorText => "||";

        public override IProcess CreateProcess(IProcess before, IProcess after)
        {
            return new OrOperatorSyntax(before, after);
        }

        public IValue Execute(RuntimeContext runtime)
        {
            var before = this.before.Execute(runtime);

            if (before is BaseBoolean nBefore)
            {
                if (nBefore.Value)
                {
                    return new BooleanConstant(true);
                }
                else
                {
                    var after = this.after.Execute(runtime);

                    if (after is BaseBoolean nAfter)
                    {
                        return new BooleanConstant(nAfter.Value);
                    }
                }
            }

            throw new ArgumentException("Unable to compare.");
        }
    }
}