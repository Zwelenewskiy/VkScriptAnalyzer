using System.Collections.Generic;
using System.Linq;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer.Mashines
{   
    public enum Input_signal
    {
        Digit,
        Letter,
        Dot,
        Comma,
        Minus,
        Letter_i,
        Letter_f,
        Colon,
        Equal,
        Equal_1,
        Equal_2,
        Quote,
        ExclamationMark,// !
        Other,
        End,

    }
    public enum State
    {
        S0,
        S1,
        S2,
        S3,
        S4,
        S5,
        S_error
    }

    public abstract class Machine
    {
        public TokenType type { get; set; }
        public State state { get; set; }
        public string lex_value { get; set; }

        private readonly Dictionary<Input_signal, Dictionary<State, State>> next_state;
        private State[] finished_states;

        public abstract Input_signal DefineSignal(char symbol);

        public Machine(Dictionary<Input_signal, Dictionary<State, State>> next_state, TokenType type, State[] finished_states)
        {
            this.next_state = next_state;
            this.type = type;
            this.finished_states = finished_states;
            state = State.S0;
            lex_value = string.Empty;
        }

        public void Parse(char symbol)
        {
            Input_signal signal = DefineSignal(symbol);

            if (signal != Input_signal.End)
            {
                if (!next_state.ContainsKey(signal))
                {
                    state = State.S_error;
                }
                else if (state != State.S_error)
                {
                    state = next_state[signal][state];
                }

                lex_value += symbol;
                
                if(state != State.S_error)
                {
                    if (signal == Input_signal.Other)
                        state = State.S0;
                }

                /*if (signal != Input_signal.Other)
                    lex_value += symbol;
                else
                    state = State.S0;*/
            }
        }

        public bool IsEnd()
        {
            return state != State.S_error && finished_states.Contains(state);
        }

        public void Reset()
        {
            state = State.S0;
            lex_value = null;
        }
    }
}
