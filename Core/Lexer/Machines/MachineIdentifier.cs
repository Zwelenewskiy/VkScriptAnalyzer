using System.Collections.Generic;
using Entities.Lexer;

namespace Core.Lexer.Mashines
{
    public class MashineIdentifier : Machine
    {
        public MashineIdentifier() :
            base(
                    stateTable: new Dictionary<InputSignal, Dictionary<State, State>>()
                    {
                        { InputSignal.Digit,
                            new Dictionary<State, State>() {
                            {  State.S0, State.Error },
                            {  State.S1, State.S1 }
                        } },
                        { InputSignal.Letter,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S1 },
                            {  State.S1, State.S1 }
                        } },
                        { InputSignal.Other,
                            new Dictionary<State, State>() {
                            {  State.S0, State.Error },
                            {  State.S1, State.Error }
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
                return InputSignal.Letter;
            else if (symbol >= '0' && symbol <= '9')
                return InputSignal.Digit;
            else if (symbol == ' ')
                return InputSignal.End;
            else return InputSignal.Other;
        }
    }
}
