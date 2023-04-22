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
                        value = "1",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = ",",
                        type = TokenType.Comma
                    },
                    new Token()
                    {
                        value = "23",
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
                        value = "1",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = ",",
                        type = TokenType.Comma
                    },
                    new Token()
                    {
                        value = "23",
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
                input_text = "!= ==",
                sample = new List<Token>()
                {
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

        [TestMethod]
        public void KeyWordsTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "var if else while and or return",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "var",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "if",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "else",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "while",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "and",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "or",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "return",
                        type = TokenType.KeyWord
                    },
                }
            });
        }

        [TestMethod]
        public void SimpleProgramTest()
        {
            DoTest(new TestParameters()
            {
                input_text = @"var a = 1.8;
                                var b = true;

                                if(b){
	                                a = 4;
                                }
                                else{
	                                a = 5;
                                }

                                while(b and a > 123){
	                                a = a + 1;
                                }

                                return a;",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "var",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "a",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "=",
                        type = TokenType.Assign
                    },
                    new Token()
                    {
                        value = "1.8",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = ";",
                        type = TokenType.Colon
                    },
                    new Token()
                    {
                        value = "var",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "b",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "=",
                        type = TokenType.Assign
                    },
                    new Token()
                    {
                        value = "true",
                        type = TokenType.BoolDataType
                    },
                    new Token()
                    {
                        value = ";",
                        type = TokenType.Colon
                    },
                    new Token()
                    {
                        value = "if",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "(",
                        type = TokenType.LeftBracket
                    },
                    new Token()
                    {
                        value = "b",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = ")",
                        type = TokenType.RightBracket
                    },
                    new Token()
                    {
                        value = "{",
                        type = TokenType.CurlyLeftBracket
                    },
                    new Token()
                    {
                        value = "a",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "=",
                        type = TokenType.Assign
                    },
                    new Token()
                    {
                        value = "4",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = ";",
                        type = TokenType.Colon
                    },
                    new Token()
                    {
                        value = "}",
                        type = TokenType.CurlyRightBracket
                    },
                    new Token()
                    {
                        value = "else",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "{",
                        type = TokenType.CurlyLeftBracket
                    },
                    new Token()
                    {
                        value = "a",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "=",
                        type = TokenType.Assign
                    },
                    new Token()
                    {
                        value = "5",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = ";",
                        type = TokenType.Colon
                    },
                    new Token()
                    {
                        value = "}",
                        type = TokenType.CurlyRightBracket
                    },

                    new Token()
                    {
                        value = "while",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "(",
                        type = TokenType.LeftBracket
                    },
                    new Token()
                    {
                        value = "b",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "and",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "a",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = ">",
                        type = TokenType.CloseQuotationMark
                    },
                    new Token()
                    {
                        value = "123",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = ")",
                        type = TokenType.RightBracket
                    },
                    new Token()
                    {
                        value = "{",
                        type = TokenType.CurlyLeftBracket
                    },
                    new Token()
                    {
                        value = "a",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "=",
                        type = TokenType.Assign
                    },
                    new Token()
                    {
                        value = "a",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = "+",
                        type = TokenType.Plus_Op
                    },
                    new Token()
                    {
                        value = "1",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = ";",
                        type = TokenType.Colon
                    },
                    new Token()
                    {
                        value = "}",
                        type = TokenType.CurlyRightBracket
                    },
                    new Token()
                    {
                        value = "return",
                        type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        value = "a",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = ";",
                        type = TokenType.Colon
                    },
                }
            });
        }



        [TestMethod]
        public void DotSeparationTest()
        {
            DoTest(new TestParameters()
            {
                input_text = "1.2 a.b",
                sample = new List<Token>()
                {
                    new Token()
                    {
                        value = "1.2",
                        type = TokenType.Number
                    },
                    new Token()
                    {
                        value = "a",
                        type = TokenType.Identifier
                    },
                    new Token()
                    {
                        value = ".",
                        type = TokenType.Dot
                    },
                    new Token()
                    {
                        value = "b",
                        type = TokenType.Identifier
                    },
                }
            });
        }
    }
}
