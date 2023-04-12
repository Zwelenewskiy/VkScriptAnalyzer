using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MashineIdentifier : Machine
    {
        public MashineIdentifier() :
            base(
                    state_table: new Dictionary<Input_signal, Dictionary<State, State>>()
                    {
                        { Input_signal.Digit,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S_error },
                            {  State.S1, State.S1 }
                        } },
                        { Input_signal.Letter,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S1 },
                            {  State.S1, State.S1 }
                        } },
                        { Input_signal.Other,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S_error },
                            {  State.S1, State.S_error }
                        } },
                    },
                    type: TokenType.Identifier,
                    finished_states: new State[] { State.S1 }
                )
        {

        }

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
