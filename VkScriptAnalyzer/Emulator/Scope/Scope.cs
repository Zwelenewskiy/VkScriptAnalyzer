namespace VkScriptAnalyzer.Emulator
{
    public class Scope
    {
        private System.Collections.Hashtable _symbolTable;

        public Scope Prev { get; set; }

        public Scope()
        {
            _symbolTable = new System.Collections.Hashtable();
        }

        public Symbol GetSymbol(string name)
        {
            if (_symbolTable.ContainsKey(name))
                return (Symbol)_symbolTable[name];
            else
                return null;
        }

        public bool ContainsName(string name)
        {
            return _symbolTable.ContainsKey(name);
        }

        public void AddSymbol(Symbol symbol)
        {
            _symbolTable.Add(symbol.Name, symbol);
        }

        public void UpdateSymbolValue(Symbol symbol)
        {
            _symbolTable[symbol.Name] = symbol;
        }
    }
}
