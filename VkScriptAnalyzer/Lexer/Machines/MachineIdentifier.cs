using System.Collections.Generic;

namespace VkScriptAnalyzer.Lexer.Mashines
{
    public class MaсhineIdentifier : Machine
    {
        public MaсhineIdentifier() :
            base(
                stateTable: new()
                {
                    {
                        InputSignal.Digit, new()
                        {
                            {
                                State.S0, State.SError
                            },
                            {
                                State.S1, State.S1
                            }
                        }
                    },
                    {
                        InputSignal.Letter, new()
                        {
                            {
                                State.S0, State.S1
                            },
                            {
                                State.S1, State.S1
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
                            }
                        }
                    },
                },
                type: TokenType.Identifier,
                finishedStates: new State[]
                {
                    State.S1
                }
            )
        {
        }

        public override InputSignal DefineSignal(char symbol)
        {
            return symbol switch
            {
                >= 'a' and <= 'z' or >= 'A' and <= 'Z' => InputSignal.Letter,
                >= '0' and <= '9' => InputSignal.Digit,
                ' ' => InputSignal.End,
                _ => InputSignal.Other
            };
        }
    }
}