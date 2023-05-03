namespace VkScriptAnalyzer.Interpreter
{
    public class ObjectSymbol : Symbol
    {
        public System.Collections.Hashtable Fields { get; set; }

        public ObjectSymbol(string name, Scope scope) : base(name, scope)
        {
        }
    }
}
