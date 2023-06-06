namespace VkScriptAnalyzer.Emulator
{
    public class FunctionSymbol : Symbol
    {
        public string SectionName { get; set; }
        public System.Collections.Generic.IEnumerable<VariableSymbol> Parameters { get; set; }

        public FunctionSymbol(string section, string name, System.Collections.Generic.IEnumerable<VariableSymbol> parameters, Scope scope) : base(name, scope)
        {
            Name       = name;
            SectionName    = section;
            Parameters = parameters;
        }
    }
}
