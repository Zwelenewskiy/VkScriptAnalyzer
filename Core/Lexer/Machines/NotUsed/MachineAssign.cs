using System.Collections.Generic;
using Entities.Lexer;

namespace Core.Lexer.Mashines
{
    public class MashineAssign : Machine
    {
        public MashineAssign() :
            base(
                    stateTable: new Dictionary<InputSignal, Dictionary<State, State>>()
                                {
                                    { InputSignal.Colon,
                                        new Dictionary<State, State>() {
                                        {  State.S0, State.S1 },
                                        {  State.S1, State.Error },
                                        {  State.S2, State.Error }
                                    } },
                                    { InputSignal.Equal,
                                        new Dictionary<State, State>() {
                                        {  State.S0, State.Error },
                                        {  State.S1, State.S2 },
                                        {  State.S2, State.Error }
                                    } },
                                    { InputSignal.Other,
                                        new Dictionary<State, State>() {
                                        {  State.S0, State.Error },
                                        {  State.S1, State.Error },
                                        {  State.S2, State.Error }
                                    } },
                                }, 
                    type: TokenType.Assign,
                    finishedStates: new State[] { State.S2 }
                )
        {

        }

        public override InputSignal DefineSignal(char symbol)
        {
            if (symbol == ':')
                return InputSignal.Colon;
            else if (symbol == '=')
                return InputSignal.Equal;
            else if (symbol == ' ')
                return InputSignal.End;
            else return InputSignal.Other;
        }
    }
}
