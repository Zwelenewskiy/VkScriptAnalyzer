using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzerTests
{
    [TestClass]
    public class VkScriptAnalyzerTest
    {
        private void DoTest(TestParameters @params)
        {
            var lexer = new LexicalAnalyzer(@params.input_text);

            List<Token> result = new List<Token>();
            var token = lexer.GetToken();
            while (true)
            {
                if (token == null)
                    break;

                result.Add(token);

                token = lexer.GetToken();
            }

            CollectionAssert.AreEqual(@params.sample, result, new TokenListComparer());
        }

        [TestMethod]
        public void NumbersTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "123 1.23 1,23 1v23",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "123",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = "1.23",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = "1,23",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = "1v23",
                        type = TokenType.Unknown
                    }
                }
            });
        }

        [TestMethod]
        public void NumbersWithWhitepacesTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "123     1.23  1,23            1v23",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "123",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = "1.23",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = "1,23",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = "1v23",
                        type = TokenType.Unknown
                    }
                }
            });
        }

        [TestMethod]
        public void IdentifiersTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "abc ABC 1a a1 a1b",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "abc",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "ABC",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "1a",
                        type = TokenType.Unknown
                    },
                    new Token()
                    {
                        value = "a1",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "a1b",
                        type = TokenType.Identifier
                    }
                }
            });
        }

        [TestMethod]
        public void SeveralWhiteSpacesInStartAndEndTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "       abc 123       ",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "abc",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "123",
                        type = TokenType.Number
                    }
                }
            });
        }

        [TestMethod]
        public void OperatorsTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "+  - * /",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "+",
                        type = TokenType.Plus_Op
                    },
                    new Token()
                    {
                        value = "-",
                        type = TokenType.Minus_Op
                    },
                    new Token()
                    {
                        value = "*",
                        type = TokenType.Mul_Op
                    },
                    new Token()
                    {
                        value = "/",
                        type = TokenType.Div_Op
                    },
                }
            });
        }

        [TestMethod]
        public void MixedOperatorsTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "++  -- ** //",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "+",
                        type = TokenType.Plus_Op
                    },
                    new Token()
                    {
                        value = "+",
                        type = TokenType.Plus_Op
                    },
                    new Token()
                    {
                        value = "-",
                        type = TokenType.Minus_Op
                    },
                    new Token()
                    {
                        value = "-",
                        type = TokenType.Minus_Op
                    },
                    new Token()
                    {
                        value = "*",
                        type = TokenType.Mul_Op
                    },
                    new Token()
                    {
                        value = "*",
                        type = TokenType.Mul_Op
                    },
                    new Token()
                    {
                        value = "/",
                        type = TokenType.Div_Op
                    },
                    new Token()
                    {
                        value = "/",
                        type = TokenType.Div_Op
                    },
                }
            });
        }

        [TestMethod]
        public void LogicalOperatorsTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "&& || != ==",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "&&",
                        type = TokenType.And_Op
                    },
                    new Token()
                    {
                        value = "||",
                        type = TokenType.Or_Op
                    },
                    new Token()
                    {
                        value = "!=",
                        type = TokenType.NonEqual
                    },
                    new Token()
                    {
                        value = "==",
                        type = TokenType.Equal
                    },
                }
            });
        }
    }
}
