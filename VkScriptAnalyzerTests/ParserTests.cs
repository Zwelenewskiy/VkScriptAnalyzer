using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
            if (sample == null &&
                forCheck != null)
            {
                return false;
            }
            bool result = false;
            if (sample is AssignNode)
            {
                if(forCheck is AssignNode)
                {
                    var sample_node    = sample as AssignNode;
                    var for_check_node = forCheck as AssignNode;

                    if(sample_node.Id.Value == sample_node.Id.Value)
                    {
                        result = IsIdentical(sample_node.Expression, for_check_node.Expression);

                        if(result)
                        {
                            if(sample_node.Next is EmptyNode)
                            {
                                if(for_check_node.Next is EmptyNode)
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
                    var sample_node = sample as ObjectNode;
                    var for_check_node = forCheck as ObjectNode;

                    if (sample_node.Fields.Count == for_check_node.Fields.Count)
                    {
                        for (int i = 0; i < sample_node.Fields.Count; i++)
                        {
                            if (sample_node.Fields[i].Name.Value == for_check_node.Fields[i].Name.Value)
                            {
                                return IsIdentical(sample_node.Fields[i].Expression, for_check_node.Fields[i].Expression);
                            }
                        }
                    }
                }
            }
            else if (sample is ExprNode)
            {
                if (forCheck is ExprNode)
                {
                    var sample_node = sample as ExprNode;
                    var for_check_node = forCheck as ExprNode;

                    if (sample_node.Token.Value == for_check_node.Token.Value)
                    {
                        return IsIdentical(sample_node.Left, for_check_node.Left)
                               && IsIdentical(sample_node.Right, for_check_node.Right);
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

        private void DoTest(Node sample, string input, string errorMessage = null)
        {
            var parser = new SyntacticAnalyzer(input);
            Node ast = parser.Parse();

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
            var sample = new AssignNode(new Token() { Value = "a"});
            sample.Expression  = new ExprNode(new Token() { Value = "1" });

            sample.Next = new EmptyNode();

            string input = "a = 1;";
            string error_message = null;

            DoTest(sample, input, error_message);
        }

        [TestMethod]
        public void Assign_With_Arithmetic_Expression()
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
            string error_message = null;

            DoTest(sample, input, error_message);
        }

        [TestMethod]
        public void Assign_With_Logical_Expression()
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
            string error_message = null;

            DoTest(sample, input, error_message);
        }

        [TestMethod]
        public void Assign_With_Object_Without_Nested_Objects()
        {
            var sample_fields = new List<ObjectField>(2) 
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
            sample.Expression = new ObjectNode(sample_fields);

            sample.Next = new EmptyNode();

            string input = @"a = {""f1"": 1, ""f2"": b};";
            string error_message = null;

            DoTest(sample, input, error_message);
        }
    }
}
