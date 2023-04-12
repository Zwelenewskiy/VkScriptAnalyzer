using System.Collections.Generic;
using System.Linq;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MashineOneSymbol : Machine
	{
		private const string enable_one_symbols = "+-=<>()";

        public MashineOneSymbol() :
            base(
                    state_table: new Dictionary<Input_signal, Dictionary<State, State>>()
					{
						{ Input_signal.Letter,
							new Dictionary<State, State>() {
							{  State.S0, State.S1 },
							{  State.S1, State.S1 },
							{  State.S_error, State.S1 }
						} },
						{ Input_signal.Other,
							new Dictionary<State, State>() {
							{  State.S0, State.S_error },
							{  State.S1, State.S_error }
						} }
					},
                    type: TokenType.OneSymbol,
                    finished_states: new State[] { State.S1 }
                )
        {

        }

        public override Input_signal DefineSignal(char symbol)
		{
			if (enable_one_symbols.Contains(symbol))
				return Input_signal.Letter;
			else if (symbol == ' ')
				return Input_signal.End;
			else return Input_signal.Other;
		}
	}
}
