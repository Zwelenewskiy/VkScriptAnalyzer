using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
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
                                { State.S2, State.SError }
                        } },
                        { InputSignal.Letter,
                            new Dictionary<State, State>() {
                                { State.S0, State.SError },
                                { State.S1, State.S1 },
                                { State.S2, State.SError }
                        } },
                        { InputSignal.Other,
                            new Dictionary<State, State>() {
                                { State.S0, State.SError },
                                { State.S1, State.SError },
                                { State.S2, State.SError }
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
            {
                return InputSignal.Quote;
            }

            if (symbol >= 'a' && symbol <= 'z' || symbol >= 'A' && symbol <= 'Z' || symbol >= '0' && symbol <= '9')
            {
                return InputSignal.Letter;
            }
            if (symbol == ' ')
            {
                return InputSignal.End;
            }
            return InputSignal.Other;
        }
    }
}
