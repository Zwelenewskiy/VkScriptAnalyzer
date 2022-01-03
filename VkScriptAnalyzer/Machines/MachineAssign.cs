using System.Collections.Generic;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer.Mashines
{
    public class MashineAssign : Machine
    {
        public MashineAssign(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, TokenType.Assign, new State[] { State.S2 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            if (symbol == ':')
                return Input_signal.Colon;
            else if (symbol == '=')
                return Input_signal.Equals;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }
}
