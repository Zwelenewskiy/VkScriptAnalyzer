using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MashineNumber : Machine
    {
        public MashineNumber() :
            base(
                    state_table: new Dictionary<Input_signal, Dictionary<State, State>>()
                    {
                        { Input_signal.Digit,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S2 },
                            {  State.S1, State.S2 },
                            {  State.S2, State.S2 },
                            {  State.S3, State.S4 },
                            {  State.S4, State.S4 }
                        } },
                        { Input_signal.Dot,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S_error },
                            {  State.S1, State.S_error },
                            {  State.S2, State.S3 },
                            {  State.S3, State.S_error },
                            {  State.S4, State.S_error },
                        } },
                        { Input_signal.Comma,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S_error },
                            {  State.S1, State.S_error },
                            {  State.S2, State.S3 },
                            {  State.S3, State.S_error },
                            {  State.S4, State.S_error },
                        } },
                        { Input_signal.Minus,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S1 },
                            {  State.S1, State.S_error },
                            {  State.S2, State.S_error },
                            {  State.S3, State.S_error },
                            {  State.S4, State.S_error },
                        } },
                        { Input_signal.Other,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S_error },
                            {  State.S1, State.S_error },
                            {  State.S2, State.S_error },
                            {  State.S3, State.S_error },
                            {  State.S4, State.S_error }
                            }
                        },
                    },
                    type: TokenType.Number,
                    finished_states: new State[] { State.S2, State.S4 }
                )
        {

        }

        public override Input_signal DefineSignal(char symbol)
        {
            switch (symbol)
            {
                case '-': return Input_signal.Minus;
                case '.': return Input_signal.Dot;
                case ',': return Input_signal.Comma;
            }

            if (symbol >= '0' && symbol <= '9')
                return Input_signal.Digit;
            else if (symbol == ' ')
                return Input_signal.End;
            else return Input_signal.Other;
        }
    }
}
