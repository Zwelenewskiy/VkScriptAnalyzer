namespace VkScriptAnalyzer.Emulator
{
    public class ObjectSymbol : Symbol
    {
        public System.Collections.Hashtable Fields { get; set; }

        public ObjectSymbol(string name, Scope scope, System.Collections.Generic.List<VariableSymbol> fields) : base(name, scope)
        {
            Fields = new System.Collections.Hashtable(fields.Count);

            foreach (var field in fields)
            {
                if (Fields.ContainsKey(field.Name))
                {
                    Fields[field.Name] = field;
                }
                else
                {
                    Fields.Add(field.Name, field);
                }
            }
        }

        public object GetMember(string name)
        {
            return Fields.ContainsKey(name) ? Fields[name] : null;
        }
    }
}
