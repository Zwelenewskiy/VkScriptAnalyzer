using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
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
                                        {  State.S1, State.SError },
                                        {  State.S2, State.SError }
                                    } },
                                    { InputSignal.Equal,
                                        new Dictionary<State, State>() {
                                        {  State.S0, State.SError },
                                        {  State.S1, State.S2 },
                                        {  State.S2, State.SError }
                                    } },
                                    { InputSignal.Other,
                                        new Dictionary<State, State>() {
                                        {  State.S0, State.SError },
                                        {  State.S1, State.SError },
                                        {  State.S2, State.SError }
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
