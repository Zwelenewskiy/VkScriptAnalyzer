using System.Collections.Generic;
using Entities.Lexer;

namespace Core.Lexer.Mashines
{
    public class MashineNumber : Machine
    {
        public MashineNumber() :
            base(
                    stateTable: new Dictionary<InputSignal, Dictionary<State, State>>()
                    {
                        { InputSignal.Digit,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S2 },
                            {  State.S1, State.S2 },
                            {  State.S2, State.S2 },
                            {  State.S3, State.S4 },
                            {  State.S4, State.S4 }
                        } },
                        { InputSignal.Dot,
                            new Dictionary<State, State>() {
                            {  State.S0, State.Error },
                            {  State.S1, State.Error },
                            {  State.S2, State.S3 },
                            {  State.S3, State.Error },
                            {  State.S4, State.Error },
                        } },
                        { InputSignal.Comma,
                            new Dictionary<State, State>() {
                            {  State.S0, State.Error },
                            {  State.S1, State.Error },
                            {  State.S2, State.S3 },
                            {  State.S3, State.Error },
                            {  State.S4, State.Error },
                        } },
                        { InputSignal.Minus,
                            new Dictionary<State, State>() {
                            {  State.S0, State.S1 },
                            {  State.S1, State.Error },
                            {  State.S2, State.Error },
                            {  State.S3, State.Error },
                            {  State.S4, State.Error },
                        } },
                        { InputSignal.Other,
                            new Dictionary<State, State>() {
                            {  State.S0, State.Error },
                            {  State.S1, State.Error },
                            {  State.S2, State.Error },
                            {  State.S3, State.Error },
                            {  State.S4, State.Error }
                            }
                        },
                    },
                    type: TokenType.Number,
                    finishedStates: new State[] { State.S2, State.S4 }
                )
        {

        }

        public override InputSignal DefineSignal(char symbol)
        {
            switch (symbol)
            {
                case '-': return InputSignal.Minus;
                case '.': return InputSignal.Dot;
                case ',': return InputSignal.Comma;
            }

            if (symbol >= '0' && symbol <= '9')
                return InputSignal.Digit;
            else if (symbol == ' ')
                return InputSignal.End;
            else return InputSignal.Other;
        }
    }
}
