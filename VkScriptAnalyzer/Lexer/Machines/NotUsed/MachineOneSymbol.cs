using System.Collections.Generic;
using System.Linq;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MashineOneSymbol : Machine
	{
		private const string EnableOneSymbols = "+-=<>()";

        public MashineOneSymbol() :
            base(
                    stateTable: new Dictionary<InputSignal, Dictionary<State, State>>()
					{
						{ InputSignal.Letter,
							new Dictionary<State, State>() {
							{  State.S0, State.S1 },
							{  State.S1, State.S1 },
							{  State.SError, State.S1 }
						} },
						{ InputSignal.Other,
							new Dictionary<State, State>() {
							{  State.S0, State.SError },
							{  State.S1, State.SError }
						} }
					},
                    type: TokenType.OneSymbol,
                    finishedStates: new State[] { State.S1 }
                )
        {

        }

        public override InputSignal DefineSignal(char symbol)
		{
			if (EnableOneSymbols.Contains(symbol))
			{
				return InputSignal.Letter;
			}

			if (symbol == ' ')
			{
				return InputSignal.End;
			}
			return InputSignal.Other;
		}
	}
}
