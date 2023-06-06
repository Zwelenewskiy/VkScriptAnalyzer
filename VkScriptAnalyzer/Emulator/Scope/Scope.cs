namespace VkScriptAnalyzer.Emulator
{
    public class Scope
    {
        private System.Collections.Hashtable symbol_table;

        public Scope Prev { get; set; }

        public Scope()
        {
            symbol_table = new System.Collections.Hashtable();
        }

        public Symbol GetSymbol(string name)
        {
            if (symbol_table.ContainsKey(name))
                return (Symbol)symbol_table[name];
            else
                return null;
        }

        public bool ContainsName(string name)
        {
            return symbol_table.ContainsKey(name);
        }

        public void AddSymbol(Symbol symbol)
        {
            symbol_table.Add(symbol.Name, symbol);
        }

        public void UpdateSymbolValue(Symbol symbol)
        {
            symbol_table[symbol.Name] = symbol;
        }
    }
}
