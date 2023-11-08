namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MaсhineOneSymbol : Machine
    {
        private const string EnableOneSymbols = "+-=<>()";

        public MaсhineOneSymbol() :
            base(
                stateTable: new()
                {
                    {
                        InputSignal.Letter, new()
                        {
                            {
                                State.S0, State.S1
                            },
                            {
                                State.S1, State.S1
                            },
                            {
                                State.SError, State.S1
                            }
                        }
                    },
                    {
                        InputSignal.Other, new()
                        {
                            {
                                State.S0, State.SError
                            },
                            {
                                State.S1, State.SError
                            }
                        }
                    }
                },
                type: TokenType.OneSymbol,
                finishedStates: new State[]
                {
                    State.S1
                }
            )
        {
        }

        public override InputSignal DefineSignal(char symbol)
        {
            if (EnableOneSymbols.Contains(symbol))
            {
                return InputSignal.Letter;
            }

            return symbol == ' ' ? InputSignal.End : InputSignal.Other;
        }
    }
}