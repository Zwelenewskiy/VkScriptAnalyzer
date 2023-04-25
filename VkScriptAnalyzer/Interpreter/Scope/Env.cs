namespace VkScriptAnalyzer.Interpreter
{
    public class Env
    {
        private Scope scope;

        public void CreateScope()
        {
            if(scope == null)
            {
                scope = new Scope();
            }
            else
            {
                var new_scope = new Scope();

                new_scope.Prev = scope;
                scope = new_scope;
            }
        }

        public void CloseScope()
        {
            scope = scope.Prev;
        }

        public Symbol GetSymbol(string name)
        {
            var tmp_scope = scope;
            while (tmp_scope != null)
            {
                var symbol = tmp_scope.GetSymbol(name);
                if (symbol == null)
                {
                    tmp_scope = tmp_scope.Prev;
                    continue;
                }
                else
                {
                    return symbol;
                }
            }

            return null;
        }

        public void AddSymbol(Symbol symbol)
        {
            scope.ContainsName(symbol.Name);

            /*if (scope.ContainsName(symbol.Name))
            {
                return false;
            }
            else
            {
                scope.AddSymbol(symbol);
                return true;
            }*/
        }
    }
}
