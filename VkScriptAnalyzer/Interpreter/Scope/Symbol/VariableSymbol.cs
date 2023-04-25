namespace VkScriptAnalyzer.Interpreter
{
    public enum DataType
    {
        Double,
        Bool
    }

    public class VariableSymbol : Symbol
    {
        public object Value { get; set; }
        public DataType DataType { get; set; }

        public VariableSymbol(string name, object value, DataType type) : base(name)
        {
            Name     = name;
            Value    = value;
            DataType = type;
        }
    }
}
