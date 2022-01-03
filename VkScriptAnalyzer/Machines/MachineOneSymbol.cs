using System.Collections.Generic;
using System.Linq;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer.Mashines
{
    public class MashineOneSymbol : Machine
	{
		private const string enable_one_symbols = "+-=<>()";
		public MashineOneSymbol(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
			base(next_state, TokenType.OneSymbol, new State[] { State.S1 })
		{ }

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
