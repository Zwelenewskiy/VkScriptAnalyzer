namespace VkScriptAnalyzer.Emulator
{
    public class Env
    {
        private Scope _scope;

        public void CreateScope()
        {
            if(_scope == null)
            {
                _scope = new Scope();
            }
            else
            {
                var newScope = new Scope
                {
                    Prev = _scope
                };

                _scope = newScope;
            }
        }

        /// <summary>
        /// "Закрывает" текущую область видимости. Текущей областью становится предыдущая
        /// </summary>
        public void CloseScope()
        {
            _scope = _scope.Prev;
        }

        /// <summary>
        /// Ищет символ во всех областях видимости
        /// </summary>
        public Symbol GetSymbol(string name)
        {
            var tempScope = _scope;
            while (tempScope != null)
            {
                var symbol = tempScope.GetSymbol(name);

                if (symbol != null)
                {
                    return symbol;
                }

                tempScope = tempScope.Prev;
            }

            return null;
        }

        /// <summary>
        /// Возвращает символ из текущей области видимости
        /// </summary>
        public Symbol GetSymbolLocal(string name)
        {
            return _scope.GetSymbol(name);
        }

        /// <summary>
        /// Добавляет символ в текущую область видимости
        /// </summary>
        public void AddSymbol(Symbol symbol)
        {
            _scope.AddSymbol(symbol);
        }

        public void UpdateSymbolValue(Symbol symbol)
        {
            symbol.Scope.UpdateSymbolValue(symbol);
        }

        public Scope GetCurrentScope()
        {
            return _scope;
        }
    }
}
