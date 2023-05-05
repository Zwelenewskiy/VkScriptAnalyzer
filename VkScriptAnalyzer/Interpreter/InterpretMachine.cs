using VkScriptAnalyzer.Lexer;
using VkScriptAnalyzer.Parser;
using System.Linq;

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
        private string[] existing_api_methods = new string[] { "account_setOffline" };

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

        /// <summary>
        /// Возвращает список вычисленных переменных из списка полей вершины AST
        /// </summary>
        private System.Collections.Generic.List<VariableSymbol> NodeFieldToObjectField(ObjectNode node)
        {
            var res = new System.Collections.Generic.List<VariableSymbol>();

            if (node != null)
            {
                foreach (var field in node.Fields)
                {
                    CalculateResult field_value = ExprInterpret(field.Expression);

                    if (field_value == null)
                        return null;

                    res.Add(new VariableSymbol(
                        name: field.Name.value,
                        value: field_value.GetResult(),
                        type: field_value.DataType,
                        scope: null
                    ));
                }
            }

            return res;
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
                    CalculateResult expr_val = ExprInterpret(node.Expression);                    

                    if (expr_val != null)
                    {
                        var result = expr_val.GetResult();
                        var scope = env.GetCurrentScope();

                        env.AddSymbol(new VariableSymbol(
                            name:  node.Id.value, 
                            value: result,
                            type:  expr_val.DataType,
                            scope: scope
                        ));

                        if (node.NextVar == null)
                            return true;
                        else
                            return VarInterpret(node.NextVar);
                    }
                }
                else
                {
                    CalculateResult expr_val = ExprInterpret(node.Expression);
                    if (expr_val != null)
                    {
                        (symbol as VariableSymbol).Value = expr_val.GetResult();
                        env.UpdateSymbolValue(symbol);

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
            if(node is ObjectNode)
            {
                var obj_node = node as ObjectNode;
                var fields = NodeFieldToObjectField(obj_node);

                if (fields == null)
                    return null;

                return new CalculateResult(new ObjectSymbol(
                        name:   null,
                        scope:  env.GetCurrentScope(),
                        fields: fields
                        ),
                    type: DataType.Object
                );
            }

            if(node is CallNode)
            {
                var call_node = node as CallNode;
                if(existing_api_methods.Contains(call_node.SectionName.value + "_" + call_node.Token.value))
                {
                    var parameters = new System.Collections.Generic.List<VariableSymbol>();

                    if(call_node.Parameter != null)
                    {
                        foreach (var field in call_node.Parameter.Fields)
                        {
                            CalculateResult field_value = ExprInterpret(field.Expression);

                            parameters.Add(new VariableSymbol(
                                name: field.Name.value,
                                value: field_value.GetResult(),
                                type: field_value.DataType,
                                scope: null
                            ));
                        }
                    }

                    try
                    {
                        var res = ApiMethodsExecutor.Instance.Execute(
                            section_name: call_node.SectionName.value,
                            method_name: call_node.Token.value,
                            parameters: parameters
                        );

                        return res;
                    }
                    catch (System.Exception ex)
                    {
                        ErrorMessage = $"Ошибка во время выполнения метода: '{call_node.SectionName.value}.{call_node.Token.value}' \n Ошибка: {ex.Message}";

                        return null;
                    }
                }
                else
                {
                    ErrorMessage = $"Вызов несуществующего метода: '{call_node.SectionName.value}.{call_node.Token.value}'";
                    return null;
                }
            }

            if(node.Token.type == TokenType.Number)
            {
                return new CalculateResult(double.Parse(node.Token.value, System.Globalization.CultureInfo.InvariantCulture), DataType.Double);
            }

            if (node.Token.type == TokenType.BoolDataType)
            {
                return new CalculateResult(bool.Parse(node.Token.value), DataType.Bool);
            }

            if (node.Token.type == TokenType.String)
            {
                return new CalculateResult(node.Token.value, DataType.String);
            }

            string op = node.Token.value;
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
                            try
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
                            catch (System.OverflowException)
                            {
                                ErrorMessage = $"Ошибка переполнения. Оператор '{node.Token.value}'. Левый операнд: {(double)left_val.GetResult() } Правый операнд: {(double)right_val.GetResult() }";

                                return null;
                            }
                            
                        }
                        else
                        {
                            // несоответствие типов
                            ErrorMessage = $"Оператор '{node.Token.value}' ожидает тип Double, но обнаружены {left_val.DataType} и {right_val.DataType}";
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
                            ErrorMessage = $"Оператор '{node.Token.value}' ожидает тип Bool, но обнаружен Double";
                        }
                    }
                }
            }
            else if (node.Token.type == TokenType.Identifier)
            {
                var var = env.GetSymbol(node.Token.value);
                if (var == null)
                {
                    ErrorMessage = $"Обнаружен необъявленный идентификатор: '{node.Token.value}'";
                }
                else
                {
                    if (var is VariableSymbol)
                    {
                        var var_sym = var as VariableSymbol;

                        /*if (var_sym.DataType == DataType.Double)
                            return new CalculateResult(var_sym.Value, DataType.Double);
                        else if (var_sym.DataType == DataType.Bool)
                            return new CalculateResult(var_sym.Value, DataType.Bool);*/

                        return new CalculateResult(var_sym.Value, var_sym.DataType);
                    }
                    else if (var is FunctionSymbol)
                    {
                        // создание функций не поддерживается
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
            CalculateResult cond_expr = ExprInterpret(node.Condition);
            if (cond_expr != null)
            {

                if (node.Body is EmptyNode == false)
                {
                    bool cond_val = ExprValueToBool(cond_expr);

                    CalculateResult res = null;
                    while (cond_val)
                    {
                        res = Interpret(node.Body);

                        cond_expr = ExprInterpret(node.Condition);
                        if (cond_expr == null)
                            break;
                        else
                            cond_val = ExprValueToBool(cond_expr);
                    }

                    return res;
                }
            }

            return null;
        }
    }
}
