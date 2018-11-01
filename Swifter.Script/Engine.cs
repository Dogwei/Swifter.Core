using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.Script
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
                new DivisionOperator(),
                new AssignValueOperator()
            };

            globalOperators = new Dictionary<string, IOperator>();

            foreach (var item in basicOperators)
            {
                globalOperators.Add(item.Code, item);
            }
        }

        public static IExpression Compile(string expression)
        {
            fixed (char* pExpression = expression)
            {
                return new Engine() { pIndex = pExpression, pEnd = pExpression + expression.Length }.CompileCompution();
            }
        }

        private char* pIndex;
        private char* pEnd;

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

        private void Offset(int i)
        {
            pIndex += i;
        }

        private Constant CompileConstant()
        {
            switch (GetCompileType())
            {
                case CompileTypes.Undefined:
                    Offset(GetNameLength());
                    return Constant.Undefined;
                case CompileTypes.Null:
                    Offset(GetNameLength());
                    return Constant.Null;
                case CompileTypes.String:
                    return CompileStringConstant();
                case CompileTypes.Number:
                    return CompileNumberConstant();
                case CompileTypes.True:
                    Offset(GetNameLength());
                    return Constant.True;
                case CompileTypes.False:
                    Offset(GetNameLength());
                    return Constant.False;
            }

            throw new Exception("不是一个常量.");
        }

        private static bool IsOperatorChar(char c)
        {
            switch (c)
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
                case '.':
                    return true;
            }

            return false;
        }

        private int GetNameLength()
        {
            var length = 0;

            for (var i = pIndex; i < pEnd; ++i, ++length)
            {
                var c = *i;

                if (c >= 'A' && c <= 'Z') continue;
                else if (c >= 'a' && c <= 'z') continue;
                else if ((c >= '0' && c <= '9') && length != 0) continue;
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

            var c = *pIndex;

            if (IsOperatorChar(c))
            {
                return CompileTypes.Operator;
            }

            switch (c)
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

        private IExpression CompileValue()
        {
            switch (GetCompileType())
            {
                case CompileTypes.Undefined:
                case CompileTypes.Null:
                case CompileTypes.String:
                case CompileTypes.Number:
                case CompileTypes.True:
                case CompileTypes.False:
                    return new GetConstantExpression(CompileConstant());
                case CompileTypes.Name:
                    return CompileName();
                case CompileTypes.BracketBegin:
                    break;
                case CompileTypes.ArrayBegin:
                    break;
                case CompileTypes.ScopeBegin:
                    break;
            }

            throw new Exception("不是一个值。");
        }

        private string ReadName()
        {
            var name = new string(pIndex, 0, GetNameLength());

            Offset(GetNameLength());

            return name;
        }

        private IExpression CompileVar()
        {
            Offset(GetNameLength());

            List<IExpression> expressions = null;

            Comma:

            if (GetCompileType() != CompileTypes.Name)
            {
                throw new Exception("var 后面必须是变量名。");
            }

            var name = ReadName();

            IExpression initialization = null;

            IOperator @operator = null;

            if (GetCompileType() == CompileTypes.Operator && (@operator = CompileOperator()).Code == "=")
            {
                initialization = CompileCompution();
            }

            var defineField = new DefineFieldExpression(name, initialization);

            if (expressions != null)
            {
                expressions.Add(defineField);
            }
            
            if (GetCompileType() == CompileTypes.Comma)
            {
                if (expressions == null)
                {
                    expressions = new List<IExpression>();
                }

                expressions.Add(defineField);

                Offset(1);

                goto Comma;
            }

            if (expressions != null)
            {
                return new UnionExpression(expressions.ToArray());
            }

            return defineField;
        }

        private IExpression CompileName()
        {
            var name = ReadName();


            switch (name)
            {
                case "var":
                    return CompileVar();
                case "function":
                case "typeof":
                case "if":
                case "while":
                case "for":
                case "do":
                case "arguments":
                case "break":
                case "case":
                case "catch":
                case "class":
                case "continue":
                case "debugger":
                case "default":
                case "delete":
                case "else":
                case "extends":
                case "false":
                case "finally":
                case "goto":
                case "in":
                case "instanceof":
                case "new":
                case "null":
                case "return":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "void":
                    throw new NotImplementedException("关键字未实现");
            }

            // TODO: Invoke.
            return new GetGlobalExpression(name);
        }

        private IOperator CompileOperator()
        {
            int operatorLength = 0;

            for (var tIndex = pIndex; tIndex < pEnd && IsOperatorChar(*tIndex); ++tIndex, ++operatorLength) ;

            while (operatorLength >= 1)
            {
                if (globalOperators.TryGetValue(new string(pIndex, 0, operatorLength), out var value))
                {
                    pIndex += operatorLength;

                    return value;
                }

                --operatorLength;
            }

            throw new Exception("无效的操作符。");
        }

        private IExpression AsExpression(object obj)
        {
            if (obj is Constant constant)
            {
                return new GetConstantExpression(constant);
            }

            if (obj is IExpression expression)
            {
                return expression;
            }

            throw new NotImplementedException();
        }

        private IExpression CompileCompution(bool isBracket = false)
        {
            LinkedList<object> list = new LinkedList<object>();

            while (true)
            {
                switch (GetCompileType())
                {
                    case CompileTypes.Undefined:
                    case CompileTypes.Null:
                    case CompileTypes.String:
                    case CompileTypes.Number:
                    case CompileTypes.True:
                    case CompileTypes.False:
                    case CompileTypes.Name:
                    case CompileTypes.ArrayBegin:
                    case CompileTypes.ScopeBegin:
                        list.AddLast(CompileValue());
                        break;
                    case CompileTypes.Operator:
                        list.AddLast(CompileOperator());
                        break;
                    case CompileTypes.BracketBegin:
                        Offset(1);

                        list.AddLast(CompileCompution(true));
                        break;
                    case CompileTypes.BracketEnd:
                        if (isBracket)
                        {
                            Offset(1);

                            goto Done;
                        }
                        throw new Exception("无效的字符 )");
                    default:
                        goto Done;
                }
            }

        Done:

            while (list.Count >= 2)
            {
                object first, middle, last;

                var node = list.First;

                ComputionExpression compution;

                if (list.Count == 2)
                {
                    first = node.Value;
                    last = (node = node.Next).Value;

                    goto Two;
                }
                else
                {
                    first = node.Value;
                    middle = (node = node.Next).Value;
                    last = (node = node.Next).Value;

                    Loop:

                    if (first is IOperator)
                    {
                        node = node.Previous;

                        last = middle;

                        goto Two;
                    }

                    if (middle is IOperator middleOperator)
                    {
                        if (last is IOperator lastOperator)
                        {
                            if (middleOperator.IsAfter)
                            {
                                node = node.Previous;

                                last = middle;

                                goto Two;
                            }
                            else if (lastOperator.IsBefore && list.Count >= 4)
                            {
                                first = last;

                                last = (node = node.Next).Value;

                                goto Two;
                            }

                        }

                        if (node.Next != null &&
                            node.Next.Value is IOperator nextOperator &&
                            (nextOperator.IsAfter || (node.Next.Next != null && nextOperator.Priority > middleOperator.Priority)))
                        {
                            first = last;
                            middle = (node = node.Next).Value;
                            last = (node = node.Next).Value;

                            goto Loop;
                        }

                        compution = new ComputionExpression(middleOperator);

                        compution.before = AsExpression(first);
                        compution.after = AsExpression(last);

                        list.AddAfter(node, compution);

                        list.Remove((node = node.Previous).Next);
                        list.Remove((node = node.Previous).Next);
                        list.Remove(node);

                        continue;
                    }

                    throw new Exception("缺少运算符。");
                }

            Two:
                
                if (first is IOperator firstOperator)
                {
                    if (last is IOperator)
                    {
                        throw new Exception("缺少值。");
                    }

                    if (!firstOperator.IsBefore)
                    {
                        throw new Exception("运算符不支持后置参数。");
                    }

                    compution = new ComputionExpression(firstOperator);

                    compution.after = AsExpression(last);

                }
                else if (last is IOperator lastOperator)
                {
                    if (!lastOperator.IsAfter)
                    {
                        throw new Exception("运算符不支持前置参数。");
                    }

                    compution = new ComputionExpression(lastOperator);

                    compution.before = AsExpression(first);
                }
                else
                {
                    throw new Exception("缺少运算符。");
                }

                list.AddAfter(node, compution);

                list.Remove((node = node.Previous).Next);
                list.Remove(node);
            }

            return AsExpression(list.First.Value);
        }
    }

    internal enum CompileTypes
    {
        /// <summary>
        /// 未定义
        /// </summary>
        Undefined,
        /// <summary>
        /// Null
        /// </summary>
        Null,
        /// <summary>
        /// 字符串
        /// </summary>
        String,
        /// <summary>
        /// 数字
        /// </summary>
        Number,
        /// <summary>
        /// True
        /// </summary>
        True,
        /// <summary>
        /// False
        /// </summary>
        False,
        /// <summary>
        /// 运算符
        /// </summary>
        Operator,
        /// <summary>
        /// 一个名称
        /// </summary>
        Name,
        /// <summary>
        /// 括号 (
        /// </summary>
        BracketBegin,
        /// <summary>
        /// 括号 )
        /// </summary>
        BracketEnd,
        /// <summary>
        /// 数组 [
        /// </summary>
        ArrayBegin,
        /// <summary>
        /// 数组 ]
        /// </summary>
        ArrayEnd,
        /// <summary>
        /// 大括号 }
        /// </summary>
        ScopeBegin,
        /// <summary>
        /// 大括号 }
        /// </summary>
        ScopeEnd,
        /// <summary>
        /// 逗号
        /// </summary>
        Comma,
        /// <summary>
        /// 分号
        /// </summary>
        Semicolon,
        /// <summary>
        /// 结束
        /// </summary>
        Done
    }
}