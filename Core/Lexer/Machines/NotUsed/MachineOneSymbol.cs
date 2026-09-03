using System.Collections.Generic;
using System.Linq;
using Entities.Lexer;

namespace Core.Lexer.Mashines
{
    public class MashineOneSymbol : Machine
	{
		private const string enableOneSymbols = "+-=<>()";

        public MashineOneSymbol() :
            base(
                    stateTable: new Dictionary<InputSignal, Dictionary<State, State>>()
					{
						{ InputSignal.Letter,
							new Dictionary<State, State>() {
							{  State.S0, State.S1 },
							{  State.S1, State.S1 },
							{  State.Error, State.S1 }
						} },
						{ InputSignal.Other,
							new Dictionary<State, State>() {
							{  State.S0, State.Error },
							{  State.S1, State.Error }
						} }
					},
                    type: TokenType.OneSymbol,
                    finishedStates: new State[] { State.S1 }
                )
        {

        }

        public override InputSignal DefineSignal(char symbol)
		{
			if (enableOneSymbols.Contains(symbol))
				return InputSignal.Letter;
			else if (symbol == ' ')
				return InputSignal.End;
			else return InputSignal.Other;
		}
	}
}
