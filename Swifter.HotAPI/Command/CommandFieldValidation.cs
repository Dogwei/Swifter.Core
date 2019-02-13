namespace Swifter.HotAPI.Command
{
    /// <summary>
    /// 命令字段验证器
    /// </summary>
    public class CommandFieldValidation
    {
        public readonly string Name;
        public readonly string Mode;
        public readonly string Value;
        public readonly string Description;


        public CommandFieldValidation(string name, string mode, string value, string description)
        {
            Name = name;
            Mode = mode;
            Value = value;
            Description = description;
        }
    }
}
