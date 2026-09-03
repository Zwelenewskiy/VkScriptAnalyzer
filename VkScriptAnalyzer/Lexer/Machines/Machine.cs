using System.Collections.Generic;
using System.Linq;

namespace VkScriptAnalyzer.Lexer.Mashines
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
        public string lexValue { get; set; }

        private readonly Dictionary<Input_signal, Dictionary<State, State>> nextState;
        private State[] finishedStates;

        public abstract Input_signal DefineSignal(char symbol);

        protected Machine() { }

        protected Machine(Dictionary<Input_signal, Dictionary<State, State>> stateTable, TokenType type, State[] finishedStates)
        {
            this.nextState = stateTable;
            this.type = type;
            this.finishedStates = finishedStates;
            state = State.S0;
            lexValue = string.Empty;
        }

        public void Parse(char symbol)
        {
            Input_signal signal = DefineSignal(symbol);

            if (signal != Input_signal.End)
            {
                if (!nextState.ContainsKey(signal))
                {
                    state = State.S_error;
                }
                else if (state != State.S_error)
                {
                    state = nextState[signal][state];
                }

                lexValue += symbol;
                
                if(state != State.S_error)
                {
                    if (signal == Input_signal.Other)
                        state = State.S0;
                }

                /*if (signal != Input_signal.Other)
                    lexValue += symbol;
                else
                    state = State.S0;*/
            }
        }

        public bool InError()
        {
            return state == State.S_error;
        }

        public bool IsEnd()
        {
            return state != State.S_error && finishedStates.Contains(state);
        }

        public void Reset()
        {
            state = State.S0;
            lexValue = null;
        }
    }
}
