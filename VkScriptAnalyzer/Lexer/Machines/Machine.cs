using System.Collections.Generic;
using System.Linq;

namespace VkScriptAnalyzer.Lexer.Mashines
{   
    public enum InputSignal
    {
        Digit,
        Letter,
        Dot,
        Comma,
        Minus,
        LetterI,
        LetterF,
        Colon,
        Equal,
        Equal1,
        Equal2,
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
        SError
    }

    public abstract class Machine
    {
        public TokenType Type { get; set; }
        public State State { get; set; }
        public string LexValue { get; set; }

        private readonly Dictionary<InputSignal, Dictionary<State, State>> _nextState;
        private State[] _finishedStates;

        public abstract InputSignal DefineSignal(char symbol);

        protected Machine() { }

        protected Machine(Dictionary<InputSignal, Dictionary<State, State>> stateTable, TokenType type, State[] finishedStates)
        {
            this._nextState = stateTable;
            this.Type = type;
            this._finishedStates = finishedStates;
            State = State.S0;
            LexValue = string.Empty;
        }

        public void Parse(char symbol)
        {
            InputSignal signal = DefineSignal(symbol);

            if (signal != InputSignal.End)
            {
                if (!_nextState.ContainsKey(signal))
                {
                    State = State.SError;
                }
                else if (State != State.SError)
                {
                    State = _nextState[signal][State];
                }

                LexValue += symbol;
                
                if(State != State.SError)
                {
                    if (signal == InputSignal.Other)
                    {
                        State = State.S0;
                    }
                }

                /*if (signal != Input_signal.Other)
                    lex_value += symbol;
                else
                    state = State.S0;*/
            }
        }

        public bool InError()
        {
            return State == State.SError;
        }

        public bool IsEnd()
        {
            return State != State.SError && _finishedStates.Contains(State);
        }

        public void Reset()
        {
            State = State.S0;
            LexValue = null;
        }
    }
}
