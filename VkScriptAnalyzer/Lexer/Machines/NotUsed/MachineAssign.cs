using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MaсhineAssign : Machine
    {
        public MaсhineAssign() :
            base(
                stateTable: new()
                {
                    {
                        InputSignal.Colon, new()
                        {
                            {
                                State.S0, State.S1
                            },
                            {
                                State.S1, State.SError
                            },
                            {
                                State.S2, State.SError
                            }
                        }
                    },
                    {
                        InputSignal.Equal, new()
                        {
                            {
                                State.S0, State.SError
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
                type: TokenType.Assign,
                finishedStates: new State[]
                {
                    State.S2
                }
            )
        {
        }

        public override InputSignal DefineSignal(char symbol)
        {
            return symbol switch
            {
                ':' => InputSignal.Colon,
                '=' => InputSignal.Equal,
                ' ' => InputSignal.End,
                _ => InputSignal.Other
            };
        }
    }
}