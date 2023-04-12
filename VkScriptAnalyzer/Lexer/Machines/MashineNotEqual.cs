using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MashineNotEqual : Machine
    {
        public MashineNotEqual() :
            base(
                    state_table: new Dictionary<Input_signal, Dictionary<State, State>>()
                    {
                        { Input_signal.Equal_1,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S1 },
                            {  State.S1, State.S_error },
                            {  State.S2, State.S_error }
                        } },
                        { Input_signal.Equal_2,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S_error },
                            {  State.S1, State.S2 },
                            {  State.S2, State.S_error }
                        } },
                        { Input_signal.Other,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S_error },
                            {  State.S1, State.S_error },
                            {  State.S2, State.S_error }
                        } },
                    },
                    type: TokenType.NonEqual,
                    finished_states: new State[] { State.S2 }
                )
        {

        }

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

