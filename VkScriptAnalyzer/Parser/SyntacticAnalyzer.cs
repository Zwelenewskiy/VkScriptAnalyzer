using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class SyntacticAnalyzer
    {
        private LexicalAnalyzer lexer;
        private Token token;
        private string token_val;

        private Token next_token;
        private Token pred_token;

        public SyntacticAnalyzer(string input)
        {
            lexer = new LexicalAnalyzer(input);
        }

        private void GetToken()
        {
            if(pred_token != null)
            {
                token = pred_token;
                token_val = pred_token.value.ToLower();
                pred_token = null;
            }
            else if(next_token != null)
            {
                token = next_token;
                token_val = next_token.value.ToLower();
                next_token = null;
            }
            else
            {
                token = lexer.GetToken();
                token_val = token.value.ToLower();
            }
        }

        private void GetNextToken()
        {
            pred_token = token;

            if(next_token == null)
                next_token = lexer.GetToken();

            token = next_token;
            token_val = next_token.value.ToLower();
        }

        public Node Parse()
        {

            return Instruction();
        }

        /*private Node InstructionList()
        {
            return Instruction();
        }*/

        private Node Instruction()
        {
            var node = new Node();
            GetToken();

            if(token_val == "if")
                return If(node);
            else if(token.type == TokenType.Identifier)
                return Assignment();

            return null;
        }

        private Node Assignment()
        {
            var res = new AssignNode(token);

            GetToken();
            if(token_val == "=")
            {
                res.Expression = Expr();

                if(token_val == ";")
                {
                    return res;
                }
            }

            return null;
        }

        private ExprNode Expr()
        {
            GetToken();
            var t1 = T1();
            if (token_val == "+" || token_val == "-")
            {
                var res = new ExprNode(token);
                res.Left = t1;
                res.Right = Expr();

                return res;
            }
            else if(t1 != null)
            {
                return t1;
            }
            else
            {
                // ошибка
            }

            return null;
        }

        private ExprNode T1()
        {
            var t2 = T2(); 
            GetToken();

            if (token_val == "*" || token_val == "/")
            {
                var res = new ExprNode(token);
                res.Left = t2;

                GetToken();
                res.Right = T1();

                return res;
            }
            else
            {
                return t2;
            }
        }

        private ExprNode T2()
        {
            if(token.type == TokenType.Identifier || token.type == TokenType.Number)
            {
                return new ExprNode(token);
            }
            else if(token_val == "(")
            {
                var e = Expr();

                if (token_val == ")")
                {
                    return e;
                }
            }
            else
            {
                // ошибка
            }

            return null;
        }

        /*private ExprNode Expr()
        {
            GetToken();

            var term = Term();
            if (term != null)
            {
                var expr1 = Expr1(term);
                if (expr1 == null)
                    return term;
                else
                    return expr1;
            }

            return null;
        }

        private ExprNode Call()
        {
            GetNextToken();
            if(token_val == "api")
            {
                GetToken();
                if(token_val == ".")
                {

                    if (token.type == TokenType.Identifier)
                    {
                        token.type = TokenType.Function;
                        return new ExprNode(token);
                    }
                }
            }

            return null;
        }

        private Node Kvalident()
        {
            GetNextToken();

            if (token_val == ".")
            {
                if (token_val == "(")
                {
                    //return Params();
                }
                else if (token_val == ".")
                {
                    GetToken();
                    if (token.type == TokenType.Identifier)
                    {
                        //return Kvalident1();
                    }
                }
            }            

            return null;
        }

        private ExprNode Term()
        {
            var factor = Factor();
            if (factor != null)
            {
                var term1 = Term1(factor);

                if (term1 != null)
                    return term1;
                else
                    return factor;
            }

            return null;
        }

        private ExprNode Factor()
        {
            if(token.type == TokenType.Number)
            {
                return new ExprNode(token);
            }
            else if(token.type == TokenType.Identifier)
            {
                var res = Call();

                if (res != null)
                {
                    return res;
                }
                else
                {
                    GetToken();
                    return new ExprNode(token);
                }
            }
            else if(token_val == "(")
            {
                var res = Expr();
                if(res != null)
                {
                    if (token_val == ")")
                        return res;
                }
            }

            return null;
        }

        private ExprNode Term1(ExprNode left_node)
        {
            GetToken();

            if(token_val == "*" || token_val == "/")
            {
                var res = new ExprNode(token);
                res.Left = left_node;
                res.Right = Expr();

                return res;
            }

            return null;
        }

        private ExprNode Expr1(ExprNode left_node)
        {
            if (token_val == "+" || token_val == "-")
            {
                var res = new ExprNode(token);
                res.Left = left_node;
                res.Right = Expr();

                return res;
            }

            return null;
        }*/

        private Node If(Node node)
        {
            /*GetToken();

            var node = new IfNode();
            if(token_val == "(")
            {
                node.Condition = Condition();

                GetToken();
                if (token_val == ")")
                {
                    GetToken();
                    if (token_val == "{")
                    {
                        node.Block = InstructionList();
                    }

                    GetToken();
                    if (token_val == "}")
                    {
                        return node;
                    }
                }
            }*/

            return null;
        }

        private ExprNode Condition()
        {
            return null;
        }
    }
}
