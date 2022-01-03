using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer.Mashines
{
    public class MashineIdentifier : Machine
    {
        public MashineIdentifier(Dictionary<Input_signal, Dictionary<State, State>> next_state) :
            base(next_state, TokenType.Identifier, new State[] { State.S1 })
        { }

        public override Input_signal DefineSignal(char symbol)
        {
            if (symbol >= 'a' && symbol <= 'z' || symbol >= 'A' && symbol <= 'Z')
                return Input_signal.Letter;
            else if (symbol >= '0' && symbol <= '9')
                return Input_signal.Digit;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }
}
