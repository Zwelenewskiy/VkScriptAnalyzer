using System.Collections.Generic;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class SyntacticAnalyzer
    {
        private LexicalAnalyzer lexer;
        private Token current_token;

        public string error_message;

        /*private Token next_token;
        private Token pred_token;

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
        }*/

        public SyntacticAnalyzer(string input)
        {
            lexer = new LexicalAnalyzer(input);
        }

        private bool CheckToken(string token_value, bool show_error = true)
        {
            if (current_token.value == token_value)
            {
                return true;
            }
            else
            {
                if(show_error)
                    error_message = $"Обнаружен токен '{current_token.value}', но ожидался '{token_value}'";

                return false;
            }
        }

        private bool CheckTokenType(TokenType type, bool show_error = true)
        {
            if (current_token.type == type)
            {
                return true;
            }
            else
            {
                if (show_error)
                    error_message = $"Обнаружен тип токен {current_token.type}, ожидался {type}";

                return false;
            }
        }

        private void GetToken()
        {
            current_token = lexer.GetToken();
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

            if(CheckToken("if", show_error: false))
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
            if (CheckToken("+", show_error: false) || CheckToken("-", show_error: false))
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

            if (CheckToken("*", show_error: false) || CheckToken("/", show_error: false))
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
            if(CheckToken("API", show_error: false))// TODO: ошибку с продолжением парсинга "api" убрать при анализе существования переменной
            {
                return Call();
            }
            else if(CheckTokenType(TokenType.Identifier, show_error: false) || CheckTokenType(TokenType.Number, show_error: false))
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
