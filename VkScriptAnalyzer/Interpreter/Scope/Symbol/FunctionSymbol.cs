namespace VkScriptAnalyzer.Interpreter
{
    public class FunctionSymbol : Symbol
    {
        public string Section { get; set; }
        public System.Collections.Generic.IEnumerable<VariableSymbol> Parameters { get; set; }

        public FunctionSymbol(string section, string name, System.Collections.Generic.IEnumerable<VariableSymbol> parameters, Scope scope) : base(name, scope)
        {
            Name       = name;
            Section    = section;
            Parameters = parameters;
        }
    }
}
