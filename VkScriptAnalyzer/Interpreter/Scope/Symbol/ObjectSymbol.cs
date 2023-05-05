namespace VkScriptAnalyzer.Interpreter
{
    public class ObjectSymbol : Symbol
    {
        public System.Collections.Hashtable Fields { get; set; }

        public ObjectSymbol(string name, Scope scope, System.Collections.Generic.List<VariableSymbol> fields) : base(name, scope)
        {
            Fields = new System.Collections.Hashtable(fields.Count);

            foreach (VariableSymbol field in fields)
            {
                if (Fields.ContainsKey(field.Name))
                {
                    Fields[field.Name] = field.Value;
                }
                else
                {
                    Fields.Add(field.Name, field.Value);
                }
            }
        }
    }
}
