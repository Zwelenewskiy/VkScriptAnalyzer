using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VkScriptAnalyzer;
using VkScriptAnalyzer.Lexer;
using VkScriptAnalyzer.Parser;

namespace VkScriptAnalyzerTests
{
    [TestClass]
    public class ParserTests
    {
        private bool IsIdentical(Node sample, Node forCheck)
        {
            if (sample == null && forCheck == null)
            {
                return true;
            }

            if (sample != null &&
                forCheck == null)
            {
                return false;
            }
            if (sample == null)
            {
                return false;
            }
            var result = false;
            if (sample is AssignNode assignNode)
            {
                if(forCheck is AssignNode forCheckNode && assignNode.Id.Value == assignNode.Id.Value)
                {
                    result = IsIdentical(assignNode.Expression, forCheckNode.Expression);

                    if (!result || assignNode.Next is not EmptyNode)
                    {
                        return result;
                    }
                    if(forCheckNode.Next is EmptyNode)
                    {
                        result = true;
                    }
                }
            }
            else if (sample is CallNode)
            {

            }
            else if (sample is EmptyNode)
            {

            }
            else if (sample is ObjectNode node)
            {
                if (forCheck is not ObjectNode forCheckNode || node.Fields.Count != forCheckNode.Fields.Count)
                {
                    return result;
                }
                for (var i = 0; i < node.Fields.Count; i++)
                {
                    if (node.Fields[i].Name.Value == forCheckNode.Fields[i].Name.Value)
                    {
                        return IsIdentical(node.Fields[i].Expression, forCheckNode.Fields[i].Expression);
                    }
                }
            }
            else if (sample is ExprNode sampleNode)
            {
                if (forCheck is not ExprNode forCheckNode)
                {
                    return result;
                }
                if (sampleNode.Token.Value == forCheckNode.Token.Value)
                {
                    return IsIdentical(sampleNode.Left, forCheckNode.Left)
                           && IsIdentical(sampleNode.Right, forCheckNode.Right);
                }
            }
                
            else if (sample is IfNode)
            {

            }
            else if (sample is ReturnNode)
            {

            }
            else if (sample is VarNode)
            {

            }
            else if (sample is WhileNode)
            {

            }

            return result;
        }

        private void DoTest(Node sample, string input, string errorMessage = null)
        {
            var parser = new SyntacticAnalyzer(input);
            var ast = parser.Parse();

            if (errorMessage == null)
            {
                if (parser.ErrorMessage == null)
                {
                    Assert.AreEqual(IsIdentical(sample, ast), true);
                }
                else
                {
                    throw new AssertFailedException($"Ошибка построения AST: {parser.ErrorMessage}");
                }
            }
            else
            {
                Assert.AreEqual(errorMessage, parser.ErrorMessage);
            }
        }

        private Token Token(string val)
        {
            return new Token() { Value = val };
        }

        [TestMethod]
        public void Assign()
        {
            var sample = new AssignNode(new Token() { Value = "a"})
            {
                Expression = new ExprNode(new Token() { Value = "1" }),
                Next = new EmptyNode()
            };

            const string input = "a = 1;";
            string errorMessage = null;

            DoTest(sample, input, errorMessage);
        }

        [TestMethod]
        public void Assign_With_Arithmetic_Expression()
        {
            var sample = new AssignNode(new Token() { Value = "a" })
            {
                Expression = new ExprNode(Token("-"))
                {
                    Right = new ExprNode(Token("6")),
                    Left = new ExprNode(Token("*"))
                    {
                        Right = new ExprNode(Token("3")),
                        Left = new ExprNode(Token("+"))
                        {
                            Right = new ExprNode(Token("2")),
                            Left = new ExprNode(Token("1"))
                        }
                    }
                },
                Next = new EmptyNode()
            };

            const string input = "a = (1 + 2) * 3 - 6;";
            string errorMessage = null;

            DoTest(sample, input, errorMessage);
        }

        [TestMethod]
        public void Assign_With_Logical_Expression()
        {
            var sample = new AssignNode(new Token() { Value = "a" })
            {
                Expression = new ExprNode(Token(Keywords.Or))
                {
                    Right = new ExprNode(Token("c")),
                    Left = new ExprNode(Token(Keywords.And))
                    {
                        Right = new ExprNode(Token("b")),
                        Left = new ExprNode(Token(">"))
                        {
                            Right = new ExprNode(Token("2")),
                            Left = new ExprNode(Token("1"))
                        }
                    }
                },
                Next = new EmptyNode()
            };

            const string input = "a = (1 > 2) and b or c;";
            string errorMessage = null;

            DoTest(sample, input, errorMessage);
        }

        [TestMethod]
        public void Assign_With_Object_Without_Nested_Objects()
        {
            var sampleFields = new List<ObjectField>(2) 
            {
                new ObjectField()
                {
                    Name       = Token("f1"),
                    Expression = new ExprNode(Token("1"))
                },
                new ObjectField()
                {
                    Name       = Token("f2"),
                    Expression = new ExprNode(Token("b"))
                }
            };

            var sample = new AssignNode(new Token() { Value = "a" })
            {
                Expression = new ObjectNode(sampleFields),
                Next = new EmptyNode()
            };

            const string input = """a = {"f1": 1, "f2": b};""";
            string errorMessage = null;

            DoTest(sample, input, errorMessage);
        }
    }
}
