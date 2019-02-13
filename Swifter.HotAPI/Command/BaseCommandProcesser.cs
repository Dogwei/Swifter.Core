using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Swifter.HotAPI.Command
{
    public abstract class BaseCommandProcesser
    {
        /// <summary> 匹配参数验证器 </summary>
        private static readonly Regex ValidationRegex;

        static BaseCommandProcesser()
        {
            /* 匹配 Name = Value Description */
            ValidationRegex = new Regex("^// *valid *(?<Name>[A-Za-z_]+[A-Za-z0-9_]*) +(?<Mode>[A-Za-z_]+[A-Za-z0-9_]*)(( +(?<Value>([A-Za-z0-9,./\\\\_]+)|(\"((\\\\\")|[^\"])+\")|(\\(((\\\\\\))|[^)])+\\))))?)(( +(?<Description>.+);?)?)$");
        }

        public static List<CommandFieldValidation> GetValidations(ref string code)
        {
            var list = new List<CommandFieldValidation>();

            using (var codeReader = new StringReader(code))
            {
                code = string.Empty;

                while (true)
                {
                    var line = codeReader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    var match = ValidationRegex.Match(line);

                    if (!match.Success)
                    {
                        code = line + codeReader.ReadToEnd();

                        break;
                    }

                    var name = match.Groups["Value"].Value;
                    var mode = match.Groups["Mode"].Value;
                    var value = match.Groups["Value"].Value;
                    var description = match.Groups["Description"].Value;

                    if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("(") && value.EndsWith(")")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    list.Add(new CommandFieldValidation(name, mode, value, description));
                }
            }

            return list;
        }
    }
}
