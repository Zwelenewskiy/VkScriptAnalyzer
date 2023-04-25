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

        /// <summary>
        /// "Закрывает" текущую область видимости. Текущей областью становится предыдущая
        /// </summary>
        public void CloseScope()
        {
            scope = scope.Prev;
        }

        /// <summary>
        /// Ищет символ во всех областях видимости
        /// </summary>
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

        /// <summary>
        /// Возвращает символ из текущей области видимости
        /// </summary>
        public Symbol GetSymbolLocal(string name)
        {
            return scope.GetSymbol(name);
        }

        /// <summary>
        /// Добавляет символ в текущую область видимости
        /// </summary>
        public void AddSymbol(Symbol symbol)
        {
            scope.AddSymbol(symbol);

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

        public void SetSymbolValue(Symbol symbol)
        {
            symbol.Scope.SetSymbolValue(symbol);
        }

        public Scope GetCurrentScope()
        {
            return scope;
        }
    }
}
