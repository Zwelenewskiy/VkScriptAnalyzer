using VkScriptAnalyzer.Lexer;
using VkScriptAnalyzer.Parser;

namespace VkScriptAnalyzer.Interpreter
{
    public class CalculateResult
    {
        private object value;
        public DataType DataType { get; set; }

        public CalculateResult(object val, DataType type)
        {
            value = val;
            DataType = type;
        }

        public object GetResult()
        {
            return value;
        }
    }

    public class InterpretMachine
    {
        private Node ast;
        private Env env;

        public string ErrorMessage { get; private set; }

        public InterpretMachine(Node ast)
        {
            this.ast = ast;
        }

        public CalculateResult Interpret()
        {
            env = new Env();
            env.CreateScope();

            return Interpret(ast);
        }

        private CalculateResult Interpret(Node node)
        {
            if(node is VarNode)
            {
                if (VarInterpret(node as VarNode))
                    return Interpret(node.Next);
            }
            if (node is AssignNode)
            {
                if (AssignInterpret(node as AssignNode))
                    return Interpret(node.Next);
            }
            if (node is IfNode)
            {
                env.CreateScope();

                var if_result = IfInterpret(node as IfNode);// результат может быть только при Return

                env.CloseScope();   
                
                if (if_result == null)
                    return Interpret(node.Next);
                else
                    return if_result;
            }
            if (node is WhileNode)
            {
                env.CreateScope();

                var while_result = WhileInterpret(node as WhileNode);// результат может быть только при Return

                env.CloseScope();

                if (while_result == null)
                    return Interpret(node.Next);
                else
                    return while_result;
            }
            if (node is ReturnNode)
            {
                return ReturnInterpret(node as ReturnNode);
            }

            return null;
        }

        private bool VarInterpret(VarNode node)
        {
            if(node != null)
            {
                var symbol = env.GetSymbolLocal(node.Id.value);
                if (symbol == null)
                {
                    CalculateResult expr_res = ExprInterpret(node.Expression);                    

                    if (expr_res != null)
                    {
                        var result = expr_res.GetResult();
                        var scope = env.GetCurrentScope();

                        env.AddSymbol(new VariableSymbol(
                            name:  node.Id.value, 
                            value: result,
                            type:  expr_res.DataType,
                            scope: scope
                        ));

                        if (node.NextVar == null)
                            return true;
                        else
                            return VarInterpret(node.NextVar);
                    }
                }
            }

            return false;
        }

        private CalculateResult ExprInterpret(ExprNode node)
        {
            if(node.token.type == TokenType.Number)
            {
                return new CalculateResult(double.Parse(node.token.value, System.Globalization.CultureInfo.InvariantCulture), DataType.Double);
            }

            if (node.token.type == TokenType.BoolDataType)
            {
                return new CalculateResult(bool.Parse(node.token.value), DataType.Bool);
            }

            string op = node.token.value;
            if (op == "+" || op == "-" || op == "*" || op == "/"
                || op == ">" || op == "<" || op == ">=" || op == "<=" || op == "==" || op == "!="
            )
            {
                var left_val = ExprInterpret(node.Left);

                if (left_val != null)
                {
                    var right_val = ExprInterpret(node.Right);

                    if (right_val != null)
                    {
                        if (left_val.DataType == DataType.Double && right_val.DataType == DataType.Double)
                        {
                            if (op == "+")
                                return new CalculateResult((double)left_val.GetResult() + (double)right_val.GetResult(), DataType.Double);
                            else if (op == "-")
                                return new CalculateResult((double)left_val.GetResult() - (double)right_val.GetResult(), DataType.Double);
                            else if (op == "*")
                                return new CalculateResult((double)left_val.GetResult() * (double)right_val.GetResult(), DataType.Double);
                            else if (op == "/")
                                return new CalculateResult((double)left_val.GetResult() / (double)right_val.GetResult(), DataType.Double);
                            else if (op == ">")
                                return new CalculateResult((double)left_val.GetResult() > (double)right_val.GetResult(), DataType.Bool);
                            else if (op == "<")
                                return new CalculateResult((double)left_val.GetResult() < (double)right_val.GetResult(), DataType.Bool);
                            else if (op == ">=")
                                return new CalculateResult((double)left_val.GetResult() >= (double)right_val.GetResult(), DataType.Bool);
                            else if (op == "<=")
                                return new CalculateResult((double)left_val.GetResult() <= (double)right_val.GetResult(), DataType.Bool);
                            else if (op == "==")
                                return new CalculateResult((double)left_val.GetResult() == (double)right_val.GetResult(), DataType.Bool);
                            else
                                return new CalculateResult((double)left_val.GetResult() != (double)right_val.GetResult(), DataType.Bool);
                        }
                        else
                        {
                            // несоответствие типов
                            ErrorMessage = $"Оператор '{node.token.value}' ожидает тип Double, но обнаружен Bool";
                        }
                    }
                }
            }
            else if (op == "and" || op == "or")
            {
                var left_val = ExprInterpret(node.Left);

                if (left_val != null)
                {
                    var right_val = ExprInterpret(node.Right);

                    if (right_val != null)
                    {
                        if (left_val.DataType == DataType.Bool && right_val.DataType == DataType.Bool)
                        {
                            if(op == "and")
                                return new CalculateResult((bool)left_val.GetResult() && (bool)right_val.GetResult(), DataType.Bool);
                            else 
                                return new CalculateResult((bool)left_val.GetResult() || (bool)right_val.GetResult(), DataType.Bool);
                        }
                        else
                        {
                            ErrorMessage = $"Оператор '{node.token.value}' ожидает тип Bool, но обнаружен Double";
                        }
                    }
                }
            }
            else if (node.token.type == TokenType.Identifier)
            {
                var var = env.GetSymbol(node.token.value);
                if (var == null)
                {
                    ErrorMessage = $"Обнаружен необъявленный идентификатор: '{node.token.value}'";
                }
                else
                {
                    if (var is VariableSymbol)
                    {
                        var var_sym = var as VariableSymbol;

                        if (var_sym.DataType == DataType.Double)
                            return new CalculateResult(var_sym.Value, DataType.Double);
                        else if (var_sym.DataType == DataType.Bool)
                            return new CalculateResult(var_sym.Value, DataType.Bool);
                    }
                    else if (var is FunctionSymbol)
                    {

                    }
                }
            }

            return null;
        }

        private bool AssignInterpret(AssignNode node)
        {
            var var_sym = (VariableSymbol)env.GetSymbol(node.Id.value);
            if(var_sym != null)
            {
                var expr_val = ExprInterpret(node.Expression);
                if(expr_val != null)
                {
                    var_sym.Value = expr_val.GetResult();
                    env.UpdateSymbolValue(var_sym);
                    return true;
                }
            }
            else
            {
                ErrorMessage = $"Идентификатор '{node.Id.value}' не объявлен";
            }

            return false;
        }

        private CalculateResult ReturnInterpret(ReturnNode node)
        {
            return ExprInterpret(node.Expression);
        }

        private bool ExprValueToBool(CalculateResult expr_result)
        {
            if(expr_result.DataType == DataType.Bool)
            {
                return (bool)expr_result.GetResult();
            }
            else
            {
                if (((double)expr_result.GetResult()) == 0)
                    return false;
                else
                    return true;
            }
        }

        private CalculateResult IfInterpret(IfNode node)
        {
            var cond_expr = ExprInterpret(node.Condition);
            if(cond_expr != null)
            {
                bool cond_val = ExprValueToBool(cond_expr);
                if (cond_val)
                {
                    if (node.Body is EmptyNode == false)
                        return Interpret(node.Body);
                }
                else
                {
                    if (node.Else != null)
                        return Interpret(node.Else);
                }
            }

            return null;
        }

        private CalculateResult WhileInterpret(WhileNode node)
        {


            return null;
        }
    }
}
