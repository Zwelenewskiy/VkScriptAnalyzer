namespace VkScriptAnalyzer.Emulator
{
    public class Symbol
    {
        public string Name { get; set; }
        public Scope Scope { get; set; }

        public Symbol(string name, Scope scope)
        {
            Name  = name;
            Scope = scope;
        }
    }
}
