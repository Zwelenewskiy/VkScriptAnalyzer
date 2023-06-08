using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MashineIdentifier : Machine
    {
        public MashineIdentifier() :
            base(
                    stateTable: new Dictionary<InputSignal, Dictionary<State, State>>()
                    {
                        { InputSignal.Digit,
                            new Dictionary<State, State>() {
                            {  State.S0, State.SError },
                            {  State.S1, State.S1 }
                        } },
                        { InputSignal.Letter,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S1 },
                            {  State.S1, State.S1 }
                        } },
                        { InputSignal.Other,
                            new Dictionary<State, State>() {
                            {  State.S0, State.SError },
                            {  State.S1, State.SError }
                        } },
                    },
                    type: TokenType.Identifier,
                    finishedStates: new State[] { State.S1 }
                )
        {

        }

        public override InputSignal DefineSignal(char symbol)
        {
            if (symbol >= 'a' && symbol <= 'z' || symbol >= 'A' && symbol <= 'Z')
            {
                return InputSignal.Letter;
            }

            if (symbol >= '0' && symbol <= '9')
            {
                return InputSignal.Digit;
            }
            if (symbol == ' ')
            {
                return InputSignal.End;
            }
            return InputSignal.Other;
        }
    }
}
