using System.Collections.Generic;
using Entities.Lexer;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MashineAssign : Machine
    {
        public MashineAssign() :
            base(
                    stateTable: new Dictionary<Input_signal, Dictionary<State, State>>()
                                {
                                    { Input_signal.Colon,
                                        new Dictionary<State, State>() {
                                        {  State.S0, State.S1 },
                                        {  State.S1, State.S_error },
                                        {  State.S2, State.S_error }
                                    } },
                                    { Input_signal.Equal,
                                        new Dictionary<State, State>() {
                                        {  State.S0, State.S_error },
                                        {  State.S1, State.S2 },
                                        {  State.S2, State.S_error }
                                    } },
                                    { Input_signal.Other,
                                        new Dictionary<State, State>() {
                                        {  State.S0, State.S_error },
                                        {  State.S1, State.S_error },
                                        {  State.S2, State.S_error }
                                    } },
                                }, 
                    type: TokenType.Assign,
                    finishedStates: new State[] { State.S2 }
                )
        {

        }

        public override Input_signal DefineSignal(char symbol)
        {
            if (symbol == ':')
                return Input_signal.Colon;
            else if (symbol == '=')
                return Input_signal.Equal;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }
}
