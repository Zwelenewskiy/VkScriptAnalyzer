using System.Collections.Generic;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class SyntacticAnalyzer
    {
        private LexicalAnalyzer lexer;
        private Token current_token;
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
                    ErrorMessage = $"Обнаружен конец файла, но ожидалось'{token_value}' \nСтрока: {lexer.PosNumber}";

                return false;
            }

            if (current_token.value == token_value)
            {
                return true;
            }
            else
            {
                if(show_error)
                    ErrorMessage = $"Обнаружен токен '{current_token.value}', но ожидалось '{token_value}'\nСтрока: {lexer.PosNumber}";

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
                    ErrorMessage = $"Обнаружен токен '{current_token.value}' с типом {current_token.type}, но ожидался {type}\nСтрока: {lexer.PosNumber}";

                return false;
            }
        }

        public string ErrorMessage { get; private set; }

        public SyntacticAnalyzer(string input)
        {
            lexer = new LexicalAnalyzer(input);
        }

        public Node Parse()
        {
            return InstructionList();
        }

        private Node InstructionList()
        {
            GetToken();
            var res = Instruction();

            if(res is EmptyNode == false && res != null)
            {
                res.Next = InstructionList();

                if (res.Next == null)
                    return null;
            }

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
                if (CheckToken("return", show_error: false))
                {
                    return Return();
                }
                else if (CheckTokenType(TokenType.Identifier, show_error: false))
                {
                    return Assignment();
                }
            }

            return new EmptyNode();
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

            if (t1 == null)
                return null;

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

            if (t2 == null)
                return null;

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

            if (t3 == null)
                return null;

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

            if (t4 == null)
                return null;

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

            if (t5 == null)
                return null;

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
            var t6 = T6();

            if (t6 == null)
                return null;

            GetToken();

            if (CheckToken(".", show_error: false))
            {
                var res = new KvalidentNode(current_token);
                res.Left = t6;

                GetToken();
                res.Right = T5();

                if(res.Right.Token.type == TokenType.Identifier || res.Right.Token.type == TokenType.Dot)
                {
                    return res;
                }

                ErrorMessage = $"Ожидался идентификатор поля, но обнаружено: '{res.Right.Token.value}'\nСтрока: {lexer.PosNumber}";
                return null;
            }
            else
            {
                return t6;
            }
        }

        private ExprNode T6()
        {
            if (CheckToken("API", show_error: false))// TODO: ошибку с продолжением парсинга "api" убрать при анализе существования переменной
            {
                return Call();
            }
            if (CheckTokenType(TokenType.Identifier, show_error: false) || CheckTokenType(TokenType.BoolDataType, show_error: false) 
                || CheckTokenType(TokenType.Number, show_error: false) || CheckTokenType(TokenType.String, show_error: false))
            {
                if(current_token.value == "null")
                    return new ObjectNode(new List<ObjectField>());

                return new ExprNode(current_token);
            }
            else if (CheckToken("(", show_error: false))
            {
                var cnd = Expr();

                if (CheckToken(")"))
                {
                    return cnd;
                }
            }
            else if (CheckToken("{", show_error: false))
            {
                return Object();
            }

            ErrorMessage = $"Обнаружен неразрешённый символ: '{current_token.value}'\nСтрока: {lexer.PosNumber}";

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
                    var section_name = current_token;

                    GetToken();
                    if (CheckToken("."))
                    {
                        GetToken();
                        if (CheckTokenType(TokenType.Identifier))
                        {
                            var call = new CallNode(current_token, section_name);
                            GetToken();

                            if (CheckToken("("))
                            {
                                GetToken();

                                if (CheckToken(")", show_error: false))
                                {
                                    return call;
                                }

                                var parameter = Object();

                                GetToken();
                                if (CheckToken(")"))
                                {
                                    call.Parameter = parameter;
                                    return call;
                                }
                            }
                        }
                    }
                                        
                }
            }

            return null;
        }

        #region Объект
        private ObjectNode Object()
        {
            var fields = new List<ObjectField>();
            Fields(ref fields);

            if (CheckToken("}"))
            {
                return new ObjectNode(fields);
            }

            return null;
        }

            private void Fields(ref List<ObjectField> fields)
            {
                GetToken();
                if (CheckTokenType(TokenType.String, show_error: false))
                {
                    var field = new ObjectField(current_token);

                    GetToken();
                    if (CheckToken(":"))
                    {
                        var expr = Expr();

                        if (expr != null)
                        {
                            field.Expression = expr;

                            fields.Add(field);

                            if (CheckToken(",", show_error: false))
                            {
                                Fields(ref fields);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
        #endregion
        #endregion

        private IfNode If()
        {
            GetToken();

            var res = new IfNode();
            if(CheckToken("("))
            {
                res.Condition = Expr();

                if(res.Condition != null)
                {
                    if (CheckToken(")"))
                    {
                        var body = Body();
                        if (body == null)
                            return null;
                        else
                            res.Body = body;

                        GetNextToken();
                        if (CheckToken("else", show_error: false))
                        {
                            GetToken();
                            GetToken();
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
                if (res is EmptyNode)
                {
                    ErrorMessage = $"Ожидалась инструкция, но обнаружена пустота \nСтрока: {lexer.PosNumber}";
                    return null;
                }
                else
                {
                    return res;
                }
            }

            //return null;
            return new EmptyNode();
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

        #region Объявление переменных
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

                    if(res.Expression == null)
                    {
                        return null;
                    }
                    else
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
        #endregion

        private ReturnNode Return()
        {
            var expr = Expr();
            if(expr != null)
            {
                if (CheckToken(";"))
                    return new ReturnNode(expr);
            }

            return null;
        }
    }
}