using System.Collections.Generic;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer.Mashines
{
    public class MashineNumber : Machine
    {
        public MashineNumber(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, TokenType.Number, new State[] { State.S2, State.S4 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            switch (symbol)
            {
                case '-': return Input_signal.Minus;
                case '.': return Input_signal.Dot;
                case ',': return Input_signal.Comma;
            }

            if (symbol >= '0' && symbol <= '9')
                return Input_signal.Digit;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }
}
