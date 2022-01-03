using System.Collections.Generic;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer.Mashines
{
    public class MashineString : Machine
    {
        public MashineString(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, TokenType.String, new State[] { State.S2 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            if (symbol == '\'')
                return Input_signal.Quote;
            else if (symbol >= 'a' && symbol <= 'z' || symbol >= 'A' && symbol <= 'Z' || symbol >= '0' && symbol <= '9')
                return Input_signal.Letter;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }
}
