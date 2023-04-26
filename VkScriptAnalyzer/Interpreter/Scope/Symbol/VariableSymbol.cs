﻿namespace VkScriptAnalyzer.Interpreter
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

        public VariableSymbol(string name, object value, DataType type, Scope scope) : base(name, scope)
        {
            Name     = name;
            Value    = value;
            DataType = type;
        }
    }
}