using System.Collections.Generic;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class SyntacticAnalyzer
    {
        private readonly LexicalAnalyzer _lexer;
        private Token _currentToken;
        private Token _nextToken;
        private Token _previousToken;

        private void GetToken()
        {
            if (_previousToken != null)
            {
                _currentToken = _previousToken;
                _previousToken = null;
            } else if (_nextToken != null)
            {
                _currentToken = _nextToken;
                _nextToken = null;
            } else
            {
                _currentToken = _lexer.GetToken();
            }
        }

        private void GetNextToken()
        {
            _previousToken = _currentToken;

            _nextToken ??= _lexer.GetToken();

            _currentToken = _nextToken;
        }

        /*private void GetToken()
        {
            current_token = lexer.GetToken();
        }*/

        private bool CheckToken(string tokenValue, bool showError = true)
        {
            if (_currentToken == null)
            {
                if (showError)
                {
                    ErrorMessage = $"Обнаружен конец файла, но ожидалось'{tokenValue}' \nСтрока: {_lexer.PosNumber}";
                }

                return false;
            }

            if (_currentToken.Value == tokenValue)
            {
                return true;
            }

            if (showError)
            {
                ErrorMessage = $"Обнаружен токен '{_currentToken.Value}', но ожидалось '{tokenValue}'\nСтрока: {_lexer.PosNumber}";
            }

            return false;
        }

        private bool CheckTokenType(TokenType type, bool showError = true)
        {
            if (_currentToken.Type == type)
            {
                return true;
            }

            if (showError)
            {
                ErrorMessage = $"Обнаружен токен '{_currentToken.Value}' с типом {_currentToken.Type}, но ожидался {type}\nСтрока: {_lexer.PosNumber}";
            }

            return false;
        }

        public string ErrorMessage { get; private set; }

        public SyntacticAnalyzer(string input)
        {
            _lexer = new(input);
        }

        public Node Parse()
        {
            return InstructionList();
        }

        private Node InstructionList()
        {
            GetToken();
            var res = Instruction();

            if (res is EmptyNode or null)
            {
                return res;
            }

            res.Next = InstructionList();

            return res.Next == null ? null : res;
        }

        private Node Instruction()
        {
            if (_currentToken == null)
            {
                return new EmptyNode();
            }

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

            return CheckTokenType(TokenType.Identifier, showError: false) ? Assignment() : new EmptyNode();
        }

        #region Присвоение (выражение)

        private Node Assignment()
        {
            var res = new AssignNode(_currentToken);

            GetToken();

            if (!CheckToken("="))
            {
                return null;
            }

            res.Expression = Expr();

            return CheckToken(";") ? res : null;
        }

        private ExprNode Expr()
        {
            GetToken();
            var t1 = T1();

            if (t1 == null)
            {
                return null;
            }

            if (!CheckToken("or", showError: false))
            {
                return t1;
            }

            var res = new ExprNode(_currentToken)
            {
                Left = t1,
                Right = Expr()
            };

            return res;
        }

        private ExprNode T1()
        {
            var t2 = T2();

            if (t2 == null)
            {
                return null;
            }

            if (!CheckToken("and", showError: false))
            {
                return t2;
            }

            var res = new ExprNode(_currentToken)
            {
                Left = t2
            };

            GetToken();
            res.Right = T1();

            return res;
        }

        private ExprNode T2()
        {
            var t3 = T3();

            if (t3 == null)
            {
                return null;
            }

            if (CheckToken("<", showError: false)
                || CheckToken(">", showError: false)
                || CheckToken("<=", showError: false)
                || CheckToken(">=", showError: false)
                || CheckToken("==", showError: false)
                || CheckToken("!=", showError: false))
            {
                var res = new ExprNode(_currentToken)
                {
                    Left = t3
                };

                GetToken();
                res.Right = T2();

                return res;
            }

            return t3;
        }

        private ExprNode T3()
        {
            var t4 = T4();

            if (t4 == null)
            {
                return null;
            }

            if (!CheckToken("+", showError: false) && !CheckToken("-", showError: false))
            {
                return t4;
            }

            var res = new ExprNode(_currentToken)
            {
                Left = t4
            };

            GetToken();
            res.Right = T3();

            return res;
        }

        private ExprNode T4()
        {
            var t5 = T5();

            if (t5 == null)
            {
                return null;
            }

            if (!CheckToken("*", showError: false) && !CheckToken("/", showError: false))
            {
                return t5;
            }

            var res = new ExprNode(_currentToken)
            {
                Left = t5
            };

            GetToken();
            res.Right = T4();

            return res;
        }

        private ExprNode T5()
        {
            var t6 = T6();

            if (t6 == null)
            {
                return null;
            }

            GetToken();

            if (!CheckToken(".", showError: false))
            {
                return t6;
            }

            var res = new KvalidentNode(_currentToken)
            {
                Left = t6
            };

            GetToken();
            res.Right = T5();

            if (res.Right.Token.Type is TokenType.Identifier or TokenType.Dot)
            {
                return res;
            }

            ErrorMessage = $"Ожидался идентификатор поля, но обнаружено: '{res.Right.Token.Value}'\nСтрока: {_lexer.PosNumber}";

            return null;
        }

        private ExprNode T6()
        {
            if (CheckToken("API", showError: false)) // TODO: ошибку с продолжением парсинга "api" убрать при анализе существования переменной
            {
                return Call();
            }

            if (CheckTokenType(TokenType.Identifier, showError: false)
                || CheckTokenType(TokenType.BoolDataType, showError: false)
                || CheckTokenType(TokenType.Number, showError: false)
                || CheckTokenType(TokenType.String, showError: false))
            {
                return _currentToken.Value == "null" ? new ObjectNode(new()) : new ExprNode(_currentToken);
            }

            if (CheckToken("(", showError: false))
            {
                var cnd = Expr();

                if (CheckToken(")"))
                {
                    return cnd;
                }
            } else if (CheckToken("{", showError: false))
            {
                return Object();
            }

            ErrorMessage = $"Обнаружен неразрешённый символ: '{_currentToken.Value}'\nСтрока: {_lexer.PosNumber}";

            return null;
        }

        private CallNode Call()
        {
            GetToken();

            if (!CheckToken("."))
            {
                return null;
            }

            GetToken();

            if (!CheckTokenType(TokenType.Identifier))
            {
                return null;
            }

            var section_name = _currentToken;

            GetToken();

            if (!CheckToken("."))
            {
                return null;
            }

            GetToken();

            if (!CheckTokenType(TokenType.Identifier))
            {
                return null;
            }

            var call = new CallNode(_currentToken, section_name);
            GetToken();

            if (!CheckToken("("))
            {
                return null;
            }

            GetToken();

            if (CheckToken(")", showError: false))
            {
                return call;
            }

            var parameter = Object();

            GetToken();

            if (!CheckToken(")"))
            {
                return null;
            }

            call.Parameter = parameter;

            return call;
        }

        #region Объект

        private ObjectNode Object()
        {
            var fields = new List<ObjectField>();
            Fields(ref fields);

            if (CheckToken("}"))
            {
                return new(fields);
            }

            return null;
        }

        private void Fields(ref List<ObjectField> fields)
        {
            GetToken();

            if (!CheckTokenType(TokenType.String, showError: false))
            {
                return;
            }

            var field = new ObjectField(_currentToken);

            GetToken();

            if (!CheckToken(":"))
            {
                return;
            }

            var expr = Expr();

            if (expr == null)
            {
                return;
            }

            field.Expression = expr;

            fields.Add(field);

            if (CheckToken(",", showError: false))
            {
                Fields(ref fields);
            }
        }

        #endregion

        #endregion

        private IfNode If()
        {
            GetToken();

            var res = new IfNode();

            if (!CheckToken("("))
            {
                return null;
            }

            res.Condition = Expr();

            if (res.Condition == null)
            {
                return null;
            }

            if (!CheckToken(")"))
            {
                return null;
            }

            var body = Body();

            if (body == null)
            {
                return null;
            }

            res.Body = body;

            GetNextToken();

            if (CheckToken("else", showError: false))
            {
                GetToken();
                GetToken();
                res.Else = Body();

                return res;
            }

            GetToken();

            return res;
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
            } else
            {
                var res = Instruction();

                if (res is not EmptyNode)
                {
                    return res;
                }

                ErrorMessage = $"Ожидалась инструкция, но обнаружена пустота \nСтрока: {_lexer.PosNumber}";

                return null;
            }

            //return null;
            return new EmptyNode();
        }

        private WhileNode While()
        {
            GetToken();

            var res = new WhileNode();

            if (!CheckToken("("))
            {
                return null;
            }

            res.Condition = Expr();

            if (!CheckToken(")"))
            {
                return null;
            }

            res.Body = Body();

            return res;
        }

        #region Объявление переменных

        private VarNode Var()
        {
            GetToken();

            if (!CheckTokenType(TokenType.Identifier))
            {
                return null;
            }

            var res = new VarNode(_currentToken);

            GetToken();

            if (!CheckToken("="))
            {
                return null;
            }

            res.Expression = Expr();

            if (res.Expression == null)
            {
                return null;
            }

            res.NextVar = Var1();

            return CheckToken(";") ? res : null;
        }

        private VarNode Var1()
        {
            if (!CheckToken(",", showError: false))
            {
                return null;
            }

            GetToken();

            if (!CheckTokenType(TokenType.Identifier))
            {
                return null;
            }

            var res = new VarNode(_currentToken);

            GetToken();

            if (!CheckToken("="))
            {
                return CheckToken(";") ? res : null;
            }

            res.Expression = Expr();

            if (res.Expression != null)
            {
                res.NextVar = Var1();
            }

            return res;
        }

        #endregion

        private ReturnNode Return()
        {
            var expr = Expr();

            if (expr != null && CheckToken(";"))
            {
                return new(expr);
            }

            return null;
        }
    }
}