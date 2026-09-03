using System.Collections.Generic;
using System.Linq;
using Entities.Lexer;

namespace Core.Lexer.Mashines
{   
    public enum InputSignal
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
        End
    }
    public enum State
    {
        S0,
        S1,
        S2,
        S3,
        S4,
        S5,
        Error
    }

    public abstract class Machine
    {
        public TokenType Type { get; set; }
        public string LexValue { get; set; }
        protected State CurrentState { get; set; }

        private readonly Dictionary<InputSignal, Dictionary<State, State>> _nextState;
        private readonly State[] _finishedStates;

        public abstract InputSignal DefineSignal(char symbol);

        protected Machine() { }

        protected Machine(Dictionary<InputSignal, Dictionary<State, State>> stateTable, TokenType type, State[] finishedStates)
        {
            this._nextState = stateTable;
            this.Type = type;
            this._finishedStates = finishedStates;
            CurrentState = State.S0;
            LexValue = string.Empty;
        }

        public void Parse(char symbol)
        {
            InputSignal signal = DefineSignal(symbol);

            if (signal != InputSignal.End)
            {
                if (!_nextState.ContainsKey(signal))
                {
                    CurrentState = State.Error;
                }
                else if (CurrentState != State.Error)
                {
                    CurrentState = _nextState[signal][CurrentState];
                }

                LexValue += symbol;
                
                if(CurrentState != State.Error)
                {
                    if (signal == InputSignal.Other)
                        CurrentState = State.S0;
                }

                /*if (signal != Input_signal.Other)
                    lexValue += symbol;
                else
                    state = State.S0;*/
            }
        }

        public bool InError()
        {
            return CurrentState == State.Error;
        }

        public bool IsEnd()
        {
            return CurrentState != State.Error && _finishedStates.Contains(CurrentState);
        }

        public void Reset()
        {
            CurrentState = State.S0;
            LexValue = null;
        }
    }
}
