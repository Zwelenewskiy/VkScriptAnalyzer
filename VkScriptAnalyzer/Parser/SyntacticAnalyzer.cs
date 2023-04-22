using System.Collections.Generic;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class SyntacticAnalyzer
    {
        private LexicalAnalyzer lexer;
        private Token current_token;

        public string error_message { get; private set; }

        public SyntacticAnalyzer(string input)
        {
            lexer = new LexicalAnalyzer(input);
        }

        private Token next_token;
        private Token pred_token;

        private void GetToken()
        {
            if(pred_token != null)
            {
                current_token = pred_token;
                pred_token = null;
            }
            else if(next_token != null)
            {
                current_token = next_token;
                next_token = null;
            }
            else
            {
                current_token = lexer.GetToken();
            }
        }

        private void GetNextToken()
        {
            pred_token = current_token;

            if(next_token == null)
                next_token = lexer.GetToken();

            current_token = next_token;
        }

        /*private void GetToken()
        {
            current_token = lexer.GetToken();
        }*/

        private bool CheckToken(string token_value, bool show_error = true)
        {
            if(current_token == null)
            {
                if (show_error)
                    error_message = $"Обнаружен конец файла, но ожидалось'{token_value}'";

                return false;
            }

            if (current_token.value == token_value)
            {
                return true;
            }
            else
            {
                if(show_error)
                    error_message = $"Обнаружен токен '{current_token.value}', но ожидалось '{token_value}'";

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


        public Node Parse()
        {
            return InstructionList();
        }

        private Node InstructionList()
        {
            GetToken();
            var res = Instruction();

            if(res != null)
                res.Next = InstructionList();

            return res;
        }

        private Node Instruction()
        {
            if(current_token != null)
            {
                if (CheckToken("if", show_error: false))
                {
                    return If();
                }
                if (CheckToken("while", show_error: false))
                {
                    return While();
                }
                if (CheckToken("var", show_error: false))
                {
                    return Var();
                }
                else if (CheckTokenType(TokenType.Identifier))
                {
                    return Assignment();
                }
            }

            return null;
        }

        #region Присвоение (выражение)
        private Node Assignment()
        {
            var res = new AssignNode(current_token);

            GetToken();
            if (CheckToken("="))
            {
                res.Expression = Expr();

                if (CheckToken(";"))
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
            if (CheckToken("or", show_error: false))
            {
                var res = new ExprNode(current_token);
                res.Left = t1;
                res.Right = Expr();

                return res;
            }
            else if (t1 != null)
            {
                return t1;
            }

            return null;
        }

        private ExprNode T1()
        {
            var t2 = T2();

            if (CheckToken("and", show_error: false))
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
            var t3 = T3();

            if (CheckToken("<", show_error: false) || CheckToken(">", show_error: false) || CheckToken("<=", show_error: false) || CheckToken(">=", show_error: false) || CheckToken("==", show_error: false) || CheckToken("!=", show_error: false))
            {
                var res = new ExprNode(current_token);
                res.Left = t3;

                GetToken();
                res.Right = T2();

                return res;
            }
            else
            {
                return t3;
            }
        }

        private ExprNode T3()
        {
            var t4 = T4();

            if (CheckToken("+", show_error: false) || CheckToken("-", show_error: false))
            {
                var res = new ExprNode(current_token);
                res.Left = t4;

                GetToken();
                res.Right = T3();

                return res;
            }
            else
            {
                return t4;
            }
        }

        private ExprNode T4()
        {
            var t5 = T5();
            GetToken();

            if (CheckToken("*", show_error: false) || CheckToken("/", show_error: false))
            {
                var res = new ExprNode(current_token);
                res.Left = t5;

                GetToken();
                res.Right = T4();

                return res;
            }
            else
            {
                return t5;
            }
        } 

        private ExprNode T5()
        {
            if (CheckToken("API", show_error: false))// TODO: ошибку с продолжением парсинга "api" убрать при анализе существования переменной
            {
                return Call();
            }
            if (CheckTokenType(TokenType.Identifier, show_error: false) || CheckTokenType(TokenType.BoolDataType, show_error: false) || CheckTokenType(TokenType.Number, show_error: false))
            {
                return new ExprNode(current_token);
            }
            else if (CheckToken("("))
            {
                var cnd = Expr();

                if (CheckToken(")"))
                {
                    return cnd;
                }
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

            if (CheckTokenType(TokenType.Identifier, show_error: false) || CheckTokenType(TokenType.Number, show_error: false))
            {
                parameters.Add(current_token);
                GetToken();

                if (CheckToken(",", show_error: false))
                {
                    Params(ref parameters);
                }
                else
                {
                    return;
                }
            }
        }
        #endregion

        private IfNode If()
        {
            GetToken();

            var res = new IfNode();
            if(CheckToken("("))
            {
                res.Condition = Expr();

                if (CheckToken(")"))
                {
                    res.Body = Body();

                    GetNextToken();
                    if (CheckToken("else", show_error: false))
                    {
                        res.Else = Body();
                        return res;
                    }
                    else
                    {
                        GetToken();
                        return res;
                    }
                }
            }

            return null;
        }

        private Node Body()
        {
            GetToken();
            if (CheckToken("{", show_error: false))
            {
                var res = InstructionList(); 
                
                if (CheckToken("}"))
                {
                    return res;
                }
            }
            else
            {
                var res = Instruction();
                if (res == null)
                    error_message = "Ожидалась инструкция, но обнаружена пустота";
                else
                    return res;
            }

            return null;
        }

        private WhileNode While()
        {
            GetToken();

            var res = new WhileNode();
            if (CheckToken("("))
            {
                res.Condition = Expr();

                if (CheckToken(")"))
                {
                    res.Body = Body();
                    return res;
                }
            }

            return null;
        }

        private VarNode Var()
        {
            GetToken();
            if (CheckTokenType(TokenType.Identifier))
            {
                var res = new VarNode(current_token);

                GetToken();
                if (CheckToken("="))
                {
                    res.Expression = Expr();

                    if(res.Expression != null)
                    {
                        res.NextVar = Var1();
                    }

                    if (CheckToken(";"))
                    {
                        return res;
                    }
                }
            }

            return null;
        }

        private VarNode Var1()
        {
            if (CheckToken(",", show_error: false))
            {
                GetToken();
                if (CheckTokenType(TokenType.Identifier))
                {
                    var res = new VarNode(current_token);

                    GetToken();
                    if (CheckToken("="))
                    {
                        res.Expression = Expr();

                        if (res.Expression != null)
                        {
                            res.NextVar = Var1();
                        }

                        return res;
                    }
                    else if (CheckToken(";"))
                    {
                        return res;
                    }
                }
            }

            return null;            
        }
    }
}