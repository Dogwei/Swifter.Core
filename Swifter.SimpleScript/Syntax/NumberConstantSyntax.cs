using Swifter.SimpleScript.Value;
using Swifter.Tools;
using System;
using System.Globalization;

namespace Swifter.SimpleScript.Syntax
{
    sealed class NumberConstantSyntax : ISyntax
    {
        public char[] BeginChars => CodeReader.NumberBeginChars;

        public Priorities Priority => Priorities.Constant;

        public bool IsContinue => false;

        public unsafe bool TryInterpret(CodeInterpreter compiler, CodeReader reader, out IProcess process)
        {
            var backup = reader.Backup();

            process = null;

            if (reader.ReadNumber(out var text))
            {
                fixed (char* pText = text)
                {
                    var numberInfo = NumberHelper.GetNumberInfo(pText, text.Length, 10);

                    if (numberInfo.IsNumber)
                    {
                        if (numberInfo.HaveExponent)
                        {
                            process = new ConstantProcess(new DoubleConstant(numberInfo.ToDouble()));
                        }
                        else if (numberInfo.IsFloat)
                        {
                            if (numberInfo.IsDecimal && numberInfo.IntegerCount + numberInfo.FractionalCount <= 28)
                            {
                                process = new ConstantProcess(new DecimalConstant(numberInfo.ToDecimal()));
                            }
                            else
                            {
                                process = new ConstantProcess(new DoubleConstant(numberInfo.ToDouble()));
                            }
                        }
                        else if (numberInfo.IntegerCount <= 18)
                        {
                            process = new ConstantProcess(new Int64Constant(numberInfo.ToInt64()));
                        }
                        else if (numberInfo.IsDecimal && numberInfo.IntegerCount <= 28)
                        {
                            process = new ConstantProcess(new DecimalConstant(numberInfo.ToDecimal()));
                        }
                        else
                        {
                            process = new ConstantProcess(new DoubleConstant(numberInfo.ToDouble()));
                        }

                        return true;
                    }
                }

                if (long.TryParse(text, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out var int64Value))
                {
                    process = new ConstantProcess(new Int64Constant(int64Value));

                    return true;
                }

                if (decimal.TryParse(text, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out var decimalValue))
                {
                    process = new ConstantProcess(new DecimalConstant(decimalValue));

                    return true;
                }

                if (double.TryParse(text, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out var doubleValue))
                {
                    process = new ConstantProcess(new DoubleConstant(doubleValue));

                    return true;
                }
            }

            backup.Restore();

            return false;
        }
    }
}