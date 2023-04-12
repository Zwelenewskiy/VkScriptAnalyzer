using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MashineString : Machine
    {
        public MashineString() :
            base(
                    state_table: new Dictionary<Input_signal, Dictionary<State, State>>()
                    {
                        { Input_signal.Quote,
                            new Dictionary<State, State>() {
                                { State.S0, State.S1 },
                                { State.S1, State.S2 },
                                { State.S2, State.S_error }
                        } },
                        { Input_signal.Letter,
                            new Dictionary<State, State>() {
                                { State.S0, State.S_error },
                                { State.S1, State.S1 },
                                { State.S2, State.S_error }
                        } },
                        { Input_signal.Other,
                            new Dictionary<State, State>() {
                                { State.S0, State.S_error },
                                { State.S1, State.S_error },
                                { State.S2, State.S_error }
                        } },
                    },
                    type: TokenType.String,
                    finished_states: new State[] { State.S2 }
                )
        {

        }

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
