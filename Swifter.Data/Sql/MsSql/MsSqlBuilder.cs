using Swifter.Tools;
using System.Text.RegularExpressions;
using System;

namespace Swifter.Data.Sql.MsSql
{
    sealed class MsSqlBuilder : SqlBuilder
    {
        const string Code_Global_Parameter_Identity = "@@IDENTITY";

        static readonly Regex StandardName = new Regex("^(@*[A-Za-z0-9_.]+)|\\*$");
        static readonly Regex ErrorName = new Regex("[\\[\\]']");

        public override string ProviderName => "System.Data.SqlClient";

        bool IsStandardName(string name) => StandardName.IsMatch(name);

        bool IsErrorName(string name) => ErrorName.IsMatch(name);

        string InjectCheck(string value) => value?.Replace("'", "''");

        protected override void BuildName(string name)
        {
            if (IsStandardName(name))
            {
                BuildCode(name);
            }
            else if (IsErrorName(name))
            {
                throw new ArgumentException($"Object name format error -- [{name}].", nameof(name));
            }
            else
            {
                BuildCode(Code_Square_Brackets_Begin);

                BuildCode(name);

                BuildCode(Code_Square_Brackets_End);
            }
        }

        protected override void BuildSelectLimit(int? offset, int? limit)
        {
            BuildCode(Code_Space);

            BuildCode(Code_Offset);

            BuildCode(Code_Space);

            BuildCode(NumberHelper.Decimal.ToString(offset ?? 0));

            BuildCode(Code_Space);

            BuildCode(Code_Rows);

            BuildCode(Code_Space);

            BuildCode(Code_Fetch);

            BuildCode(Code_Space);

            BuildCode(Code_Next);

            BuildCode(Code_Space);

            BuildCode(NumberHelper.Decimal.ToString(limit ?? 999999999));

            BuildCode(Code_Space);

            BuildCode(Code_Rows);

            BuildCode(Code_Space);

            BuildCode(Code_Only);
        }
        
        protected override void BuildValue(ConstantValue<string> value) => BuildCode($"N'{InjectCheck(value.Value)}'");

        protected override OrderBy GetDefaultOrderBy(SelectStatement selectStatement)
        {
            return new OrderBy(new Column(null, Code_One), OrderByDirections.None);
        }

        public override void BuildGetIdentity(InsertStatement insertStatement)
        {
            BuildCode(Code_Select);

            BuildCode(Code_Space);

            BuildCode(Code_Global_Parameter_Identity);
        }

        protected override void BiildStatementEnd()
        {
            BuildCode(Code_Semicolon);

            BuildCode(Code_WrapLine);
        }

        protected internal override SqlBuilder CreateInstance() => new MsSqlBuilder();
    }
}