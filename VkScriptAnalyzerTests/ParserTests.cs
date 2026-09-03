using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Entities.Lexer;
using Entities.Parser;
using Core.Parser;

namespace VkScriptAnalyzerTests
{
    [TestClass]
    public class ParserTests
    {
        private bool IsIdentical(Node sample, Node forCheck)
        {
            if (sample == null && forCheck == null)
                return true;

            else if (sample != null &&
                     forCheck == null)
                return false;
            else if (sample == null &&
                     forCheck != null)
                return false;
            else
            {
                bool result = false;
                if (sample is AssignNode)
                {
                    if(forCheck is AssignNode)
                    {
                        var sampleNode    = sample as AssignNode;
                        var forCheckNode = forCheck as AssignNode;

                        if(sampleNode.Id.Value == sampleNode.Id.Value)
                        {
                            result = IsIdentical(sampleNode.Expression, forCheckNode.Expression);

                            if(result)
                            {
                                if(sampleNode.Next is EmptyNode)
                                {
                                    if(forCheckNode.Next is EmptyNode)
                                    {
                                        result = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (sample is CallNode)
                {

                }
                else if (sample is EmptyNode)
                {

                }
                else if (sample is ObjectNode)
                {
                    if (forCheck is ObjectNode)
                    {
                        var sampleNode = sample as ObjectNode;
                        var forCheckNode = forCheck as ObjectNode;

                        if (sampleNode.Fields.Count == forCheckNode.Fields.Count)
                        {
                            for (int i = 0; i < sampleNode.Fields.Count; i++)
                            {
                                if (sampleNode.Fields[i].Name.Value == forCheckNode.Fields[i].Name.Value)
                                {
                                    return IsIdentical(sampleNode.Fields[i].Expression, forCheckNode.Fields[i].Expression);
                                }
                            }
                        }
                    }
                }
                else if (sample is ExprNode)
                {
                    if (forCheck is ExprNode)
                    {
                        var sampleNode = sample as ExprNode;
                        var forCheckNode = forCheck as ExprNode;

                        if (sampleNode.Token.Value == forCheckNode.Token.Value)
                        {
                            return IsIdentical(sampleNode.Left, forCheckNode.Left)
                                && IsIdentical(sampleNode.Right, forCheckNode.Right);
                        }
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
        }

        private void DoTest(Node sample, string input, string errorMessage = null)
        {
            var parser = new SyntacticAnalyzer(input);
            ParseResult parseResult = parser.Parse();

            if (errorMessage == null)
            {
                if (parseResult.IsSuccess)
                {
                    Assert.AreEqual(IsIdentical(sample, parseResult.Program), true);
                }
                else
                {
                    throw new AssertFailedException($"Ошибка построения AST: {parseResult.ErrorMessage}");
                }
            }
            else
            {
                Assert.AreEqual(errorMessage, parseResult.ErrorMessage);
            }
        }

        private Token Token(string val)
        {
            return new Token() { Value = val };
        }

        [TestMethod]
        public void Assign()
        {
            var sample = new AssignNode(new Token() { Value = "a"});
            sample.Expression  = new ExprNode(new Token() { Value = "1" });

            sample.Next = new EmptyNode();

            string input = "a = 1;";
            string errorMessage = null;

            DoTest(sample, input, errorMessage);
        }

        [TestMethod]
        public void AssignWithArithmeticExpression()
        {
            var sample = new AssignNode(new Token() { Value = "a" });
            sample.Expression                 = new ExprNode(Token("-"));
            sample.Expression.Right           = new ExprNode(Token("6"));
            sample.Expression.Left            = new ExprNode(Token("*"));
            sample.Expression.Left.Right      = new ExprNode(Token("3"));
            sample.Expression.Left.Left       = new ExprNode(Token("+"));
            sample.Expression.Left.Left.Right = new ExprNode(Token("2"));
            sample.Expression.Left.Left.Left  = new ExprNode(Token("1"));

            sample.Next = new EmptyNode();

            string input = "a = (1 + 2) * 3 - 6;";
            string errorMessage = null;

            DoTest(sample, input, errorMessage);
        }

        [TestMethod]
        public void AssignWithLogicalExpression()
        {
            var sample = new AssignNode(new Token() { Value = "a" });
            sample.Expression                 = new ExprNode(Token("or"));
            sample.Expression.Right           = new ExprNode(Token("c"));
            sample.Expression.Left            = new ExprNode(Token("and"));
            sample.Expression.Left.Right      = new ExprNode(Token("b"));
            sample.Expression.Left.Left       = new ExprNode(Token(">"));
            sample.Expression.Left.Left.Right = new ExprNode(Token("2"));
            sample.Expression.Left.Left.Left  = new ExprNode(Token("1"));

            sample.Next = new EmptyNode();

            string input = "a = (1 > 2) and b or c;";
            string errorMessage = null;

            DoTest(sample, input, errorMessage);
        }

        [TestMethod]
        public void AssignWithObjectWithoutNestedObjects()
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

            var sample = new AssignNode(new Token() { Value = "a" });
            sample.Expression = new ObjectNode(sampleFields);

            sample.Next = new EmptyNode();

            string input = @"a = {""f1"": 1, ""f2"": b};";
            string errorMessage = null;

            DoTest(sample, input, errorMessage);
        }
    }
}
