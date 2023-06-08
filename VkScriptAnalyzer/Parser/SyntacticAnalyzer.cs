using System.Collections.Generic;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class SyntacticAnalyzer
    {
        private LexicalAnalyzer _lexer;
        private Token _currentToken;
        private Token _nextToken;
        private Token _predToken;

        private void GetToken()
        {
            if(_predToken != null)
            {
                _currentToken = _predToken;
                _predToken = null;
            }
            else if(_nextToken != null)
            {
                _currentToken = _nextToken;
                _nextToken = null;
            }
            else
            {
                _currentToken = _lexer.GetToken();
            }
        }

        private void GetNextToken()
        {
            _predToken = _currentToken;

            if(_nextToken == null)
                _nextToken = _lexer.GetToken();

            _currentToken = _nextToken;
        }

        /*private void GetToken()
        {
            current_token = lexer.GetToken();
        }*/

        private bool CheckToken(string tokenValue, bool showError = true)
        {
            if(_currentToken == null)
            {
                if (showError)
                    ErrorMessage = $"Обнаружен конец файла, но ожидалось'{tokenValue}' \nСтрока: {_lexer.PosNumber}";

                return false;
            }

            if (_currentToken.Value == tokenValue)
            {
                return true;
            }
            else
            {
                if(showError)
                    ErrorMessage = $"Обнаружен токен '{_currentToken.Value}', но ожидалось '{tokenValue}'\nСтрока: {_lexer.PosNumber}";

                return false;
            }
        }

        private bool CheckTokenType(TokenType type, bool showError = true)
        {
            if (_currentToken.Type == type)
            {
                return true;
            }
            else
            {
                if (showError)
                    ErrorMessage = $"Обнаружен токен '{_currentToken.Value}' с типом {_currentToken.Type}, но ожидался {type}\nСтрока: {_lexer.PosNumber}";

                return false;
            }
        }

        public string ErrorMessage { get; private set; }

        public SyntacticAnalyzer(string input)
        {
            _lexer = new LexicalAnalyzer(input);
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
            if(_currentToken != null)
            {
                if (CheckToken("if", showError: false))
                {
                    return If();
                }
                if (CheckToken("while", showError: false))
                {
                    return While();
                }
                if (CheckToken("var", showError: false))
                {
                    return Var();
                }
                if (CheckToken("return", showError: false))
                {
                    return Return();
                }
                else if (CheckTokenType(TokenType.Identifier, showError: false))
                {
                    return Assignment();
                }
            }

            return new EmptyNode();
        }

        #region Присвоение (выражение)
        private Node Assignment()
        {
            var res = new AssignNode(_currentToken);

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

            if (CheckToken("or", showError: false))
            {
                var res = new ExprNode(_currentToken);
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

            if (CheckToken("and", showError: false))
            {
                var res = new ExprNode(_currentToken);
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

            if (CheckToken("<", showError: false) || CheckToken(">", showError: false) || CheckToken("<=", showError: false) || CheckToken(">=", showError: false) || CheckToken("==", showError: false) || CheckToken("!=", showError: false))
            {
                var res = new ExprNode(_currentToken);
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

            if (CheckToken("+", showError: false) || CheckToken("-", showError: false))
            {
                var res = new ExprNode(_currentToken);
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

            if (CheckToken("*", showError: false) || CheckToken("/", showError: false))
            {
                var res = new ExprNode(_currentToken);
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

            if (CheckToken(".", showError: false))
            {
                var res = new KvalidentNode(_currentToken);
                res.Left = t6;

                GetToken();
                res.Right = T5();

                if(res.Right.Token.Type == TokenType.Identifier || res.Right.Token.Type == TokenType.Dot)
                {
                    return res;
                }

                ErrorMessage = $"Ожидался идентификатор поля, но обнаружено: '{res.Right.Token.Value}'\nСтрока: {_lexer.PosNumber}";
                return null;
            }
            else
            {
                return t6;
            }
        }

        private ExprNode T6()
        {
            if (CheckToken("API", showError: false))// TODO: ошибку с продолжением парсинга "api" убрать при анализе существования переменной
            {
                return Call();
            }
            if (CheckTokenType(TokenType.Identifier, showError: false) || CheckTokenType(TokenType.BoolDataType, showError: false) 
                || CheckTokenType(TokenType.Number, showError: false) || CheckTokenType(TokenType.String, showError: false))
            {
                if(_currentToken.Value == "null")
                    return new ObjectNode(new List<ObjectField>());

                return new ExprNode(_currentToken);
            }
            else if (CheckToken("(", showError: false))
            {
                var cnd = Expr();

                if (CheckToken(")"))
                {
                    return cnd;
                }
            }
            else if (CheckToken("{", showError: false))
            {
                return Object();
            }

            ErrorMessage = $"Обнаружен неразрешённый символ: '{_currentToken.Value}'\nСтрока: {_lexer.PosNumber}";

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
                    var section_name = _currentToken;

                    GetToken();
                    if (CheckToken("."))
                    {
                        GetToken();
                        if (CheckTokenType(TokenType.Identifier))
                        {
                            var call = new CallNode(_currentToken, section_name);
                            GetToken();

                            if (CheckToken("("))
                            {
                                GetToken();

                                if (CheckToken(")", showError: false))
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
                if (CheckTokenType(TokenType.String, showError: false))
                {
                    var field = new ObjectField(_currentToken);

                    GetToken();
                    if (CheckToken(":"))
                    {
                        var expr = Expr();

                        if (expr != null)
                        {
                            field.Expression = expr;

                            fields.Add(field);

                            if (CheckToken(",", showError: false))
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
                        if (CheckToken("else", showError: false))
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
            if (CheckToken("{", showError: false))
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
                    ErrorMessage = $"Ожидалась инструкция, но обнаружена пустота \nСтрока: {_lexer.PosNumber}";
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
                var res = new VarNode(_currentToken);

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
            if (CheckToken(",", showError: false))
            {
                GetToken();
                if (CheckTokenType(TokenType.Identifier))
                {
                    var res = new VarNode(_currentToken);

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