using System.Collections.Generic;
using Entities.Lexer;

namespace Core.Lexer.Mashines
{
    public class MashineString : Machine
    {
        public MashineString() :
            base(
                    stateTable: new Dictionary<InputSignal, Dictionary<State, State>>()
                    {
                        { InputSignal.Quote,
                            new Dictionary<State, State>() {
                                { State.S0, State.S1 },
                                { State.S1, State.S2 },
                                { State.S2, State.Error }
                        } },
                        { InputSignal.Letter,
                            new Dictionary<State, State>() {
                                { State.S0, State.Error },
                                { State.S1, State.S1 },
                                { State.S2, State.Error }
                        } },
                        { InputSignal.Other,
                            new Dictionary<State, State>() {
                                { State.S0, State.Error },
                                { State.S1, State.Error },
                                { State.S2, State.Error }
                        } },
                    },
                    type: TokenType.String,
                    finishedStates: new State[] { State.S2 }
                )
        {

        }

        public override InputSignal DefineSignal(char symbol)
        {
            if (symbol == '"')
                return InputSignal.Quote;
            else if (symbol >= 'a' && symbol <= 'z' || symbol >= 'A' && symbol <= 'Z' || symbol >= '0' && symbol <= '9')
                return InputSignal.Letter;
            else if (symbol == ' ')
                return InputSignal.End;
            else return InputSignal.Other;
        }
    }
}
