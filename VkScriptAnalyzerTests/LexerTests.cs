using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VkScriptAnalyzer;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzerTests
{
    internal class TokenListComparer : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            var t1 = (Token)x;
            var t2 = (Token)y;

            if (t1.Value == t2.Value
                && t1.Type == t2.Type)
            {
                return 0;
            }

            return -1;
        }
    }

    internal class TestParameters
    {
        public string InputText { get; set; }
        public List<Token> Sample { get; set; }
    }

    [TestClass]
    public class LexerTests
    {
        private void DoTest(TestParameters @params)
        {
            var lexer = new LexicalAnalyzer(@params.InputText);

            var result = new List<Token>();
            var token = lexer.GetToken();
            while (true)
            {
                if (token == null)
                {
                    break;
                }

                result.Add(token);

                token = lexer.GetToken();
            }

            CollectionAssert.AreEqual(@params.Sample, result, new TokenListComparer());
        }

        [TestMethod]
        public void NumbersTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "123 1.23 1,23 1v23",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = "123",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = "1.23",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = "1",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = ",",
                        Type = TokenType.Comma
                    },
                    new Token()
                    {
                        Value = "23",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = "1v23",
                        Type = TokenType.Unknown
                    }
                }
            });
        }

        [TestMethod]
        public void NumbersWithWhitespacesTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "123     1.23  1,23            1v23",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = "123",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = "1.23",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = "1",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = ",",
                        Type = TokenType.Comma
                    },
                    new Token()
                    {
                        Value = "23",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = "1v23",
                        Type = TokenType.Unknown
                    }
                }
            });
        }

        [TestMethod]
        public void IdentifiersTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "abc ABC 1a a1 a1b",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = "abc",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "ABC",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "1a",
                        Type = TokenType.Unknown
                    },
                    new Token()
                    {
                        Value = "a1",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "a1b",
                        Type = TokenType.Identifier
                    }
                }
            });
        }

        [TestMethod]
        public void SeveralWhiteSpacesInStartAndEndTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "       abc 123       ",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = "abc",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "123",
                        Type = TokenType.Number
                    }
                }
            });
        }

        [TestMethod]
        public void OperatorsTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "+  - * /",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = "+",
                        Type = TokenType.PlusOp
                    },
                    new Token()
                    {
                        Value = "-",
                        Type = TokenType.MinusOp
                    },
                    new Token()
                    {
                        Value = "*",
                        Type = TokenType.MulOp
                    },
                    new Token()
                    {
                        Value = "/",
                        Type = TokenType.DivOp
                    },
                }
            });
        }

        [TestMethod]
        public void MixedOperatorsTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "++  -- ** //",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = "+",
                        Type = TokenType.PlusOp
                    },
                    new Token()
                    {
                        Value = "+",
                        Type = TokenType.PlusOp
                    },
                    new Token()
                    {
                        Value = "-",
                        Type = TokenType.MinusOp
                    },
                    new Token()
                    {
                        Value = "-",
                        Type = TokenType.MinusOp
                    },
                    new Token()
                    {
                        Value = "*",
                        Type = TokenType.MulOp
                    },
                    new Token()
                    {
                        Value = "*",
                        Type = TokenType.MulOp
                    },
                    new Token()
                    {
                        Value = "/",
                        Type = TokenType.DivOp
                    },
                    new Token()
                    {
                        Value = "/",
                        Type = TokenType.DivOp
                    },
                }
            });
        }

        [TestMethod]
        public void LogicalOperatorsTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "!= ==",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = "!=",
                        Type = TokenType.NonEqual
                    },
                    new Token()
                    {
                        Value = "==",
                        Type = TokenType.Equal
                    },
                }
            });
        }

        [TestMethod]
        public void KeyWordsTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "var if else while and or return",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = Keywords.Var,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = Keywords.If,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = Keywords.Else,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = Keywords.While,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = Keywords.And,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = Keywords.Or,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = Keywords.Return,
                        Type = TokenType.KeyWord
                    },
                }
            });
        }

        [TestMethod]
        public void SimpleProgramTest()
        {
            DoTest(new TestParameters()
            {
                InputText = """
                            var a = 1.8;
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

                            return a;
                            """,
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = Keywords.Var,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = "a",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "=",
                        Type = TokenType.Assign
                    },
                    new Token()
                    {
                        Value = "1.8",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = ";",
                        Type = TokenType.Colon
                    },
                    new Token()
                    {
                        Value = Keywords.Var,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = "b",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "=",
                        Type = TokenType.Assign
                    },
                    new Token()
                    {
                        Value = "true",
                        Type = TokenType.BoolDataType
                    },
                    new Token()
                    {
                        Value = ";",
                        Type = TokenType.Colon
                    },
                    new Token()
                    {
                        Value = Keywords.If,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = "(",
                        Type = TokenType.LeftBracket
                    },
                    new Token()
                    {
                        Value = "b",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = ")",
                        Type = TokenType.RightBracket
                    },
                    new Token()
                    {
                        Value = "{",
                        Type = TokenType.CurlyLeftBracket
                    },
                    new Token()
                    {
                        Value = "a",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "=",
                        Type = TokenType.Assign
                    },
                    new Token()
                    {
                        Value = "4",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = ";",
                        Type = TokenType.Colon
                    },
                    new Token()
                    {
                        Value = "}",
                        Type = TokenType.CurlyRightBracket
                    },
                    new Token()
                    {
                        Value = Keywords.Else,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = "{",
                        Type = TokenType.CurlyLeftBracket
                    },
                    new Token()
                    {
                        Value = "a",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "=",
                        Type = TokenType.Assign
                    },
                    new Token()
                    {
                        Value = "5",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = ";",
                        Type = TokenType.Colon
                    },
                    new Token()
                    {
                        Value = "}",
                        Type = TokenType.CurlyRightBracket
                    },

                    new Token()
                    {
                        Value = Keywords.While,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = "(",
                        Type = TokenType.LeftBracket
                    },
                    new Token()
                    {
                        Value = "b",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = Keywords.And,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = "a",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = ">",
                        Type = TokenType.CloseQuotationMark
                    },
                    new Token()
                    {
                        Value = "123",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = ")",
                        Type = TokenType.RightBracket
                    },
                    new Token()
                    {
                        Value = "{",
                        Type = TokenType.CurlyLeftBracket
                    },
                    new Token()
                    {
                        Value = "a",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "=",
                        Type = TokenType.Assign
                    },
                    new Token()
                    {
                        Value = "a",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = "+",
                        Type = TokenType.PlusOp
                    },
                    new Token()
                    {
                        Value = "1",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = ";",
                        Type = TokenType.Colon
                    },
                    new Token()
                    {
                        Value = "}",
                        Type = TokenType.CurlyRightBracket
                    },
                    new Token()
                    {
                        Value = Keywords.Return,
                        Type = TokenType.KeyWord
                    },
                    new Token()
                    {
                        Value = "a",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = ";",
                        Type = TokenType.Colon
                    },
                }
            });
        }



        [TestMethod]
        public void DotSeparationTest()
        {
            DoTest(new TestParameters()
            {
                InputText = "1.2 a.b",
                Sample = new List<Token>()
                {
                    new Token()
                    {
                        Value = "1.2",
                        Type = TokenType.Number
                    },
                    new Token()
                    {
                        Value = "a",
                        Type = TokenType.Identifier
                    },
                    new Token()
                    {
                        Value = ".",
                        Type = TokenType.Dot
                    },
                    new Token()
                    {
                        Value = "b",
                        Type = TokenType.Identifier
                    },
                }
            });
        }
    }
}
