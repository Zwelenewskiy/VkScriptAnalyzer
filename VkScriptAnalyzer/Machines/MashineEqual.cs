using System.Collections.Generic;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer.Mashines
{
    public class MashineEqual : Machine
    {
        public MashineEqual(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, TokenType.Equal, new State[] { State.S2 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            if(state == State.S0 && symbol == '=')
                return Input_signal.Equal_1;
            else if (state == State.S1 && symbol == '=')
                return Input_signal.Equal_2;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }
}

