using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.NewScript
{
    public sealed unsafe class Engine
    {
        private static readonly Dictionary<string, IOperator> globalOperators;

        static Engine()
        {
            var basicOperators = new IOperator[] {
                new AddOperator(),
                new SubtractOperator(),
                new MultiplicationOperator(),
                new DivisionOperator()
            };

            globalOperators = new Dictionary<string, IOperator>();

            foreach (var item in basicOperators)
            {
                globalOperators.Add(item.Code, item);
            }
        }

        public static IExpression Compile(string expression)
        {
            fixed(char* pExpression = expression)
            {
                return Compile(pExpression, pExpression + expression.Length);
            }
        }

        private static IExpression Compile(char* pIndex, char* pEnd)
        {
            var engine = new Engine();

            engine.pIndex = pIndex;
            engine.pEnd = pEnd;

            IExpression expression = null;

            while (engine.pIndex < engine.pEnd)
            {
                expression = engine.CompileExpression(expression);
            }

            return expression;
        }


        private char* pIndex;
        private char* pEnd;
        
        private int length
        {
            get
            {
                return (int)(pEnd - pIndex);
            }
        }

        private Constant CompileStringConstant()
        {
            char textChar = *pIndex;

            int textLength = 0;

            ++pIndex;

            var tExpression = pIndex;

            while (pIndex < pEnd)
            {
                if (*pIndex == textChar)
                {
                    goto String;
                }
                else if (*pIndex == '\\')
                {
                    pIndex += 2;
                }
                else
                {
                    ++pIndex;
                }

                ++textLength;
            }

            throw new Exception("ReadString");

        String:

            if (pIndex - tExpression == textLength)
            {
                ++pIndex;

                return new Constant(new string(tExpression, 0, textLength));
            }

            var result = new string('\0', textLength);

            fixed (char* pResult = result)
            {
                for (int i = 0; tExpression < pIndex; ++i, ++tExpression)
                {
                    if (*tExpression == '\\')
                    {
                        ++tExpression;

                        if (tExpression >= pIndex)
                        {
                            throw new Exception("ReadString");
                        }

                        switch (*tExpression)
                        {
                            case 'b':
                                pResult[i] = '\b';
                                continue;
                            case 'f':
                                pResult[i] = '\f';
                                continue;
                            case 'n':
                                pResult[i] = '\n';
                                continue;
                            case 't':
                                pResult[i] = '\t';
                                continue;
                            case 'r':
                                pResult[i] = '\r';
                                continue;
                        }
                    }

                    pResult[i] = *tExpression;
                }
            }

            return new Constant(result);
        }

        private Constant CompileNumberConstant()
        {
            var numberInfo = NumberHelper.Decimal.GetNumberInfo(pIndex, (int)(pEnd - pIndex));

            if (numberInfo.IsNumber)
            {
                pIndex += numberInfo.End;

                if (numberInfo.HaveExponent)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16 && numberInfo.ExponentCount <= 2)
                    {
                        return new Constant(NumberHelper.Decimal.ToDouble(numberInfo));
                    }

                    // Out double range.
                    return new Constant(NumberHelper.Decimal.ToDouble(numberInfo));
                }

                if (numberInfo.IsFloat)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16)
                    {
                        return new Constant(NumberHelper.Decimal.ToDouble(numberInfo));
                    }

                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 28)
                    {
                        return new Constant(NumberHelper.ToDecimal(numberInfo));
                    }

                    // Out double range.
                    return new Constant(NumberHelper.Decimal.ToDouble(numberInfo));
                }

                if (numberInfo.IntegerCount <= 18)
                {
                    var int64 = NumberHelper.Decimal.ToInt64(numberInfo);

                    return new Constant(int64);
                }

                if (numberInfo.IntegerCount <= 28)
                {
                    return new Constant(NumberHelper.ToDecimal(numberInfo));
                }

                // Out double range.
                return new Constant(NumberHelper.Decimal.ToDouble(numberInfo));
            }

            throw new Exception("ReadNumber");
        }

        private IExpression CompileExpression(IExpression before)
        {
        Loop:

            switch (GetCompileType())
            {
                case CompileTypes.Undefined:
                case CompileTypes.Null:
                case CompileTypes.String:
                case CompileTypes.Number:
                case CompileTypes.True:
                case CompileTypes.False:
                    return CompileValue(before);
                case CompileTypes.Operator:
                    return CompileOperator(0, before);
                case CompileTypes.Name:
                    return CompileName();
                case CompileTypes.BracketBegin:
                    return CompileBracket(before);
                case CompileTypes.BracketEnd:
                    ++pIndex;
                    return before;
                case CompileTypes.ArrayBegin:
                    return CompileArray();
                case CompileTypes.ArrayEnd:
                    throw new Exception("无效的中括号结束。");
                case CompileTypes.ScopeBegin:
                    return CompileScope();
                case CompileTypes.ScopeEnd:
                    throw new Exception("无效的大括号结束。");
                case CompileTypes.Dot:
                    throw new Exception("无效的点。");
                case CompileTypes.Comma:
                    throw new Exception("无效的逗号。");
                case CompileTypes.Semicolon:
                    ++pIndex;
                    goto Loop;
                default:
                    return null;
            }
        }

        private IExpression CompileArray()
        {
            return null;
        }

        private IExpression CompileScope()
        {
            return null;
        }

        private IExpression CompileBracket(IExpression before)
        {
            ++pIndex;

            return CompileExpression(before);
        }

        private IExpression CompileName()
        {
            return null;
        }

        private IExpression CompileOperator(Priorities priority, IExpression before)
        {
            var length = 0;

            var tIndex = pIndex;

            while (pIndex < pEnd && IsOperatorChar())
            {
                ++pIndex;
                ++length;
            }

            for (var len = length; len >= 1; --len,--pIndex)
            {
                if (globalOperators.TryGetValue(new string(tIndex, 0, len), out var value) && value.Priority > priority)
                {
                    var expression = new ComputionExpression(value);

                    var valueBefore = before;

                    if (valueBefore is ComputionExpression)
                    {
                        valueBefore = null;
                    }

                    if (value.IsMiddle && valueBefore != null)
                    {
                        var after = CompileAfterValue(expression);

                        if (after != null)
                        {
                            expression.before = valueBefore;
                            expression.after = after;

                            return expression;
                        }
                    }

                    if (value.IsAfter && valueBefore != null)
                    {
                        expression.before = valueBefore;

                        return expression;
                    }

                    if (value.IsBefore)
                    {
                        var after = CompileAfterValue(expression);

                        if (after != null)
                        {
                            expression.after = after;

                            return expression;
                        }
                    }

                    throw new Exception("无效的运算符 -- " + value.Code);
                }
            }

            throw new Exception("无效的运算符 -- " + new string(tIndex, 0, length));
        }

        private bool IsOperatorChar()
        {
            switch (*pIndex)
            {
                case '~':
                case '!':
                case '@':
                case '%':
                case '^':
                case '&':
                case '*':
                case '-':
                case '+':
                case '=':
                case ':':
                case '|':
                case '\\':
                case '/':
                case '?':
                case '<':
                case '>':
                    return true;
            }

            return false;
        }

        private IExpression CompileValue(IExpression before)
        {
            IExpression expression = null;

            switch (GetCompileType())
            {
                case CompileTypes.Undefined:
                    pIndex += GetNameLength();
                    expression = new GetConstantExpression(Constant.Undefined);
                    break;
                case CompileTypes.Null:
                    pIndex += GetNameLength();
                    expression = new GetConstantExpression(Constant.Null);
                    break;
                case CompileTypes.String:
                    expression = new GetConstantExpression(CompileStringConstant());
                    break;
                case CompileTypes.Number:
                    expression = new GetConstantExpression(CompileNumberConstant());
                    break;
                case CompileTypes.True:
                    pIndex += GetNameLength();
                    expression = new GetConstantExpression(Constant.True);
                    break;
                case CompileTypes.False:
                    pIndex += GetNameLength();
                    expression = new GetConstantExpression(Constant.False);
                    break;
            }

            switch (GetCompileType())
            {
                case CompileTypes.Undefined:
                case CompileTypes.Null:
                case CompileTypes.String:
                case CompileTypes.Number:
                case CompileTypes.True:
                case CompileTypes.False:
                case CompileTypes.Name:
                case CompileTypes.BracketEnd:
                case CompileTypes.ArrayEnd:
                case CompileTypes.ScopeBegin:
                case CompileTypes.ScopeEnd:
                case CompileTypes.Comma:
                case CompileTypes.Semicolon:
                    throw new Exception("无效的指令。");
                case CompileTypes.Operator:
                    return CompileOperator(0, expression);
                case CompileTypes.BracketBegin:
                    break;
                case CompileTypes.ArrayBegin:
                    break;
                case CompileTypes.Dot:
                    break;
                case CompileTypes.Done:
                    return expression;
            }

            throw new Exception("未正常结束。");
        }

        private IExpression CompileAfterValue(IExpression before)
        {
            IExpression expression = null;

            switch (GetCompileType())
            {
                case CompileTypes.Undefined:
                    pIndex += GetNameLength();
                    expression = new GetConstantExpression(Constant.Undefined);
                    break;
                case CompileTypes.Null:
                    pIndex += GetNameLength();
                    expression = new GetConstantExpression(Constant.Null);
                    break;
                case CompileTypes.String:
                    expression = new GetConstantExpression(CompileStringConstant());
                    break;
                case CompileTypes.Number:
                    expression = new GetConstantExpression(CompileNumberConstant());
                    break;
                case CompileTypes.True:
                    pIndex += GetNameLength();
                    expression = new GetConstantExpression(Constant.True);
                    break;
                case CompileTypes.False:
                    pIndex += GetNameLength();
                    expression = new GetConstantExpression(Constant.False);
                    break;
                case CompileTypes.Operator:
                    return CompileOperator(0, before);
                case CompileTypes.Name:
                    expression = CompileName();
                    break;
                case CompileTypes.BracketBegin:
                    return CompileBracket(before);
                case CompileTypes.BracketEnd:
                    throw new Exception("无效的括号结束。");
                case CompileTypes.ArrayBegin:
                    return CompileArray();
                case CompileTypes.ArrayEnd:
                    throw new Exception("无效的中括号结束。");
                case CompileTypes.ScopeBegin:
                    return CompileScope();
                case CompileTypes.ScopeEnd:
                    throw new Exception("无效的大括号结束。");
                case CompileTypes.Dot:
                    throw new Exception("无效的点。");
                case CompileTypes.Comma:
                    throw new Exception("无效的逗号。");
                case CompileTypes.Semicolon:
                    throw new Exception("无效的分号。");
            }

            var compution = before as ComputionExpression;

            Priorities priority = 0;

            if (compution != null)
            {
                priority = compution.@operator.Priority;
            }

            switch (GetCompileType())
            {
                case CompileTypes.Undefined:
                case CompileTypes.Null:
                case CompileTypes.String:
                case CompileTypes.Number:
                case CompileTypes.True:
                case CompileTypes.False:
                case CompileTypes.Name:
                case CompileTypes.BracketEnd:
                case CompileTypes.ArrayEnd:
                case CompileTypes.ScopeBegin:
                case CompileTypes.ScopeEnd:
                case CompileTypes.Comma:
                case CompileTypes.Semicolon:
                    throw new Exception("无效的指令。");
                case CompileTypes.Operator:
                    return CompileOperator(priority, expression);
                case CompileTypes.BracketBegin:
                    break;
                case CompileTypes.ArrayBegin:
                    break;
                case CompileTypes.Dot:
                    break;
                case CompileTypes.Done:
                    return expression;
            }

            throw new Exception("未正常结束。");
        }

        private int GetNameLength()
        {
            var length = 0;

            for (var i = pIndex; i < pEnd; ++i, ++length)
            {
                var c = *i;

                if (c >= 'A' || c <= 'Z') continue;
                else if (c >= 'a' || c <= 'z') continue;
                else if ((c >= '0' || c <= '9') && length != 0) continue;
                else if (c == '_') continue;
                else break;
            }

            return length;
        }

        private CompileTypes GetCompileType()
        {
            Loop:

            if (pIndex >= pEnd)
            {
                return CompileTypes.Done;
            }
            
            if (IsOperatorChar())
            {
                return CompileTypes.Operator;
            }

            switch (*pIndex)
            {
                case '(':
                    return CompileTypes.BracketBegin;
                case ')':
                    return CompileTypes.BracketEnd;
                case '\'':
                case '"':
                    return CompileTypes.String;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return CompileTypes.Number;
                case 't':
                    if (StringHelper.Equals(pIndex, 0, GetNameLength(), "true"))
                    {
                        return CompileTypes.True;
                    }
                    break;
                case 'f':
                    if (StringHelper.Equals(pIndex, 0, GetNameLength(), "false"))
                    {
                        return CompileTypes.False;
                    }
                    break;
                case 'n':
                    if (StringHelper.Equals(pIndex, 0, GetNameLength(), "null"))
                    {
                        return CompileTypes.Null;
                    }
                    break;
                case 'u':
                    if (StringHelper.Equals(pIndex, 0, GetNameLength(), "undefined"))
                    {
                        return CompileTypes.Undefined;
                    }
                    break;
                case ';':
                    return CompileTypes.Semicolon;
                case ',':
                    return CompileTypes.Comma;
                case '.':
                    return CompileTypes.Dot;
                case '[':
                    return CompileTypes.ArrayBegin;
                case ']':
                    return CompileTypes.ArrayEnd;
                case '{':
                    return CompileTypes.ScopeBegin;
                case '}':
                    return CompileTypes.ScopeEnd;
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    ++pIndex;
                    goto Loop;
            }

            return CompileTypes.Name;
        }
    }

    internal enum CompileTypes
    {
        Undefined,
        Null,
        String,
        Number,
        True,
        False,
        Operator,
        Name,
        BracketBegin,
        BracketEnd,
        ArrayBegin,
        ArrayEnd,
        ScopeBegin,
        ScopeEnd,
        Dot,
        Comma,
        Semicolon,
        Done
    }
}