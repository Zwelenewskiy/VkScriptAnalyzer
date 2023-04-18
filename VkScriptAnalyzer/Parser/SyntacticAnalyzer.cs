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
        private Token current_token;

        private string error_message;

        //private Token next_token;
        //private Token pred_token;

        public SyntacticAnalyzer(string input)
        {
            lexer = new LexicalAnalyzer(input);
        }

        private bool CheckToken(string token_value)
        {
            if (current_token.value == token_value)
            {
                return true;
            }
            else
            {
                error_message = $"Обнаружен токен {current_token.value}, ожидался {token_value}";
                return false;
            }
        }

        private bool CheckTokenType(TokenType type)
        {
            if (current_token.type == type)
            {
                return true;
            }
            else
            {
                error_message = $"Обнаружен тип токен {current_token.type}, ожидался {type}";
                return false;
            }
        }

        private void GetToken()
        {
            current_token = lexer.GetToken();
        }

        /*private void GetToken()
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
        }*/

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

            if(CheckToken("if"))
                return If(node);
            else if(CheckTokenType(TokenType.Identifier))
                return Assignment();

            return null;
        }

        private Node Assignment()
        {
            var res = new AssignNode(current_token);

            GetToken();
            if(CheckToken("="))
            {
                res.Expression = Expr();

                if(CheckToken(";"))
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
            if (CheckToken("+") || CheckToken("-"))
            {
                var res = new ExprNode(current_token);
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

            if (CheckToken("*") || CheckToken("/"))
            {
                var res = new ExprNode(current_token);
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
            if(CheckToken("API"))
            {
                return Call();
            }
            else if(CheckTokenType(TokenType.Identifier) || CheckTokenType(TokenType.Number))
            {
                return new ExprNode(current_token);
            }
            else if(CheckToken("("))
            {
                var e = Expr();

                if (CheckToken(")"))
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

        private CallNode Call()
        {
            GetToken();
            if (CheckToken("."))
            {
                GetToken();

                if (CheckTokenType(TokenType.Identifier))
                {
                    var call = new CallNode(current_token);
                    GetToken();

                    if (CheckToken("("))
                    {
                        var parameters = new List<Token>();
                        Params(ref parameters);

                        if (CheckToken(")"))
                        {
                            call.parameters = parameters;
                            return call;
                        }
                    }
                }
            }

            return null;
        }

        private void Params(ref List<Token> parameters)
        {
            GetToken();

            if(CheckTokenType(TokenType.Identifier))
            {
                parameters.Add(current_token);
                GetToken();

                if(CheckToken(","))
                {
                    Params(ref parameters);
                }
                else
                {
                    return;
                }
            }
        }

        /*
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

        */

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
