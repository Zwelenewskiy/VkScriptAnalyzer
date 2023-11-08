using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MaсhineString : Machine
    {
        public MaсhineString() :
            base(
                stateTable: new()
                {
                    {
                        InputSignal.Quote, new()
                        {
                            {
                                State.S0, State.S1
                            },
                            {
                                State.S1, State.S2
                            },
                            {
                                State.S2, State.SError
                            }
                        }
                    },
                    {
                        InputSignal.Letter, new()
                        {
                            {
                                State.S0, State.SError
                            },
                            {
                                State.S1, State.S1
                            },
                            {
                                State.S2, State.SError
                            }
                        }
                    },
                    {
                        InputSignal.Other, new()
                        {
                            {
                                State.S0, State.SError
                            },
                            {
                                State.S1, State.SError
                            },
                            {
                                State.S2, State.SError
                            }
                        }
                    },
                },
                type: TokenType.String,
                finishedStates: new State[]
                {
                    State.S2
                }
            )
        {
        }

        public override InputSignal DefineSignal(char symbol)
        {
            switch (symbol)
            {
                case '"':
                    return InputSignal.Quote;
                case >= 'a' and <= 'z':
                case >= 'A' and <= 'Z':
                case >= '0' and <= '9':
                    return InputSignal.Letter;
                case ' ':
                    return InputSignal.End;
                default:
                    return InputSignal.Other;
            }
        }
    }
}