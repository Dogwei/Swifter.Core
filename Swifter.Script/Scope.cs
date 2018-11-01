using System;
using System.Collections.Generic;

namespace Swifter.Script
{
    public sealed class Scope
    {
        private readonly Dictionary<string, object> fields;

        public Scope()
        {
            fields = new Dictionary<string, object>();
        }

        public Scope BaseScope { get; private set; }

        public IValue GetField(string name)
        {
            if (fields.TryGetValue(name, out var value))
            {
                if (value is IValue iValue)
                {
                    return iValue;
                }

                throw new Exception("不是一个值。");
            }
            else if (BaseScope != null)
            {
                return BaseScope.GetField(name);
            }

            throw new Exception("没有找到字段。");
        }

        public void SetField(string name, IValue value)
        {
            var field = GetField(name) as Field;

            if (field == null)
            {
                throw new Exception("无法设置值");
            }

            if (value is Field fieldValue)
            {
                value = fieldValue.GetValue();
            }

            field.SetValue(value);
        }

        public void DefinedField(string name, IValue value)
        {
            if (value is Field fieldValue)
            {
                value = fieldValue.GetValue();
            }

            var field = new Field(value);

            fields.Add(name, field);
        }
    }
}