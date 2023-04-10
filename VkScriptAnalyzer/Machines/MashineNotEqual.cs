using System.Collections.Generic;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer.Mashines
{
    public class MashineNotEqual : Machine
    {
        public MashineNotEqual(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, TokenType.NonEqual, new State[] { State.S2 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            if (symbol == '!')
                return Input_signal.ExclamationMark;
            else if (symbol == '=')
                return Input_signal.Equal;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }
}

