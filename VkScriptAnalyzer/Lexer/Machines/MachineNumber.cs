using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MaсhineNumber : Machine
    {
        public MaсhineNumber() :
            base(
                stateTable: new()
                {
                    {
                        InputSignal.Digit, new()
                        {
                            {
                                State.S0, State.S2
                            },
                            {
                                State.S1, State.S2
                            },
                            {
                                State.S2, State.S2
                            },
                            {
                                State.S3, State.S4
                            },
                            {
                                State.S4, State.S4
                            }
                        }
                    },
                    {
                        InputSignal.Dot, new()
                        {
                            {
                                State.S0, State.SError
                            },
                            {
                                State.S1, State.SError
                            },
                            {
                                State.S2, State.S3
                            },
                            {
                                State.S3, State.SError
                            },
                            {
                                State.S4, State.SError
                            },
                        }
                    },
                    {
                        InputSignal.Comma, new()
                        {
                            {
                                State.S0, State.SError
                            },
                            {
                                State.S1, State.SError
                            },
                            {
                                State.S2, State.S3
                            },
                            {
                                State.S3, State.SError
                            },
                            {
                                State.S4, State.SError
                            },
                        }
                    },
                    {
                        InputSignal.Minus, new()
                        {
                            {
                                State.S0, State.S1
                            },
                            {
                                State.S1, State.SError
                            },
                            {
                                State.S2, State.SError
                            },
                            {
                                State.S3, State.SError
                            },
                            {
                                State.S4, State.SError
                            },
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
                            },
                            {
                                State.S3, State.SError
                            },
                            {
                                State.S4, State.SError
                            }
                        }
                    },
                },
                type: TokenType.Number,
                finishedStates: new State[]
                {
                    State.S2,
                    State.S4
                }
            )
        {
        }

        public override InputSignal DefineSignal(char symbol)
        {
            return symbol switch
            {
                '-' => InputSignal.Minus,
                '.' => InputSignal.Dot,
                ',' => InputSignal.Comma,
                _ => symbol switch
                {
                    >= '0' and <= '9' => InputSignal.Digit,
                    ' ' => InputSignal.End,
                    _ => InputSignal.Other
                }
            };
        }
    }
}