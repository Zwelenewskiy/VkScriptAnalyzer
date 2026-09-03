using System.Linq;
using Entities.Emulator;
using Entities.Lexer;
using Entities.Parser;
using VkApi;

namespace Core.Emulator
{
    public class EmulatorMashine
    {
        private Node _ast;
        private Env _env;
        private string[] _existingApiMethods = new string[] { "account_setOffline" };
        private int _apiCallsCount;

        public string ErrorMessage { get; private set; }

        public EmulatorMashine(Node ast)
        {
            _ast = ast;
            _apiCallsCount = 0;
        }

        public CalculateResult StartEmulate()
        {
            _env = new Env();
            _env.CreateScope();

            return Emulate(_ast);
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
                    CalculateResult fieldValue = ExprInterpret(field.Expression);

                    if (fieldValue == null)
                        return null;

                    res.Add(new VariableSymbol(
                        name: field.Name.Value,
                        value: fieldValue.GetResult(),
                        type: fieldValue.DataType,
                        scope: null
                    ));
                }
            }

            return res;
        }

        private CalculateResult Emulate(Node node)
        {
            if(node is VarNode)
            {
                if (VarInterpret(node as VarNode))
                    return Emulate(node.Next);
            }
            if (node is AssignNode)
            {
                if (AssignInterpret(node as AssignNode))
                    return Emulate(node.Next);
            }
            if (node is IfNode)
            {
                _env.CreateScope();

                var ifResult = IfInterpret(node as IfNode);// результат может быть только при Return

                _env.CloseScope();   
                
                if (ifResult == null)
                    return Emulate(node.Next);
                else
                    return ifResult;
            }
            if (node is WhileNode)
            {
                _env.CreateScope();

                var whileResult = WhileInterpret(node as WhileNode);// результат может быть только при Return

                _env.CloseScope();

                if (whileResult == null)
                    return Emulate(node.Next);
                else
                    return whileResult;
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
                var symbol = _env.GetSymbolLocal(node.Id.Value);
                if (symbol == null)
                {
                    CalculateResult expr_val = ExprInterpret(node.Expression);                    

                    if (expr_val != null)
                    {
                        var result = expr_val.GetResult();
                        var scope = _env.GetCurrentScope();

                        _env.AddSymbol(new VariableSymbol(
                            name:  node.Id.Value, 
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
                        _env.UpdateSymbolValue(symbol);

                        if (node.NextVar == null)
                            return true;
                        else
                            return VarInterpret(node.NextVar);
                    }
                }
            }

            return false;
        }

        // b.c.d
        //                   .
        //                  / \
        //        объект   b   .
        //                    / \
        //  идентификаторы   c   d
        private CalculateResult KvalidentInterpret(KvalidentNode node, ObjectSymbol obj)
        {
            var field = obj.GetMember(node.Left.Token.Value) as VariableSymbol;

            if (field == null || field.Value is ObjectSymbol == false)
                return new CalculateResult(null, DataType.Object);

            if (node.Right.Token.Type == TokenType.Identifier
                   && node.Left.Token.Type == TokenType.Identifier)// базовый случай, когда потомки узла - идентификаторы. Ниже идти не нужно
            {
                var res = (field.Value as ObjectSymbol).GetMember(node.Right.Token.Value) as VariableSymbol;

                return new CalculateResult(res.Value, res.DataType);
            }
            else
            {
                return KvalidentInterpret(node.Right as KvalidentNode, field.Value as ObjectSymbol);
            }
        }

        private CalculateResult ExprInterpret(ExprNode node)
        {
            if (node is KvalidentNode)
            {
                var leftSymbol = _env.GetSymbol(node.Left.Token.Value) as VariableSymbol;
                if (leftSymbol == null)
                {
                    ErrorMessage = $"Обнаружен необъявленный идентификатор: '{node.Left.Token.Value}' \nСтрока: {node.Left.Token.PosNumber}";
                    return null;
                }

                if (leftSymbol.Value is ObjectSymbol)
                {
                    if(node.Right is KvalidentNode == false)// базовый случай, когда потомки узла - идентификаторы. Ниже идти не нужно
                    {
                        var res = (leftSymbol.Value as ObjectSymbol).GetMember(node.Right.Token.Value) as VariableSymbol;
                        return new CalculateResult(res.Value, res.DataType);
                    }

                    return KvalidentInterpret(node.Right as KvalidentNode, leftSymbol.Value as ObjectSymbol);
                }
                else
                {
                    return new CalculateResult(null, DataType.Object);
                }
            }

            if(node is ObjectNode)
            {
                var obj_node = node as ObjectNode;
                var fields = NodeFieldToObjectField(obj_node);

                if (fields == null)
                    return null;

                return new CalculateResult(new ObjectSymbol(
                        name:   null,
                        scope:  _env.GetCurrentScope(),
                        fields: fields
                        ),
                    type: DataType.Object
                );
            }

            if(node is CallNode)
            {
                var callNode = node as CallNode;
                if(_existingApiMethods.Contains(callNode.SectionName.Value + "_" + callNode.Token.Value))
                {
                    var parameters = new System.Collections.Generic.List<VariableSymbol>();

                    if(callNode.Parameter != null)
                    {
                        foreach (var field in callNode.Parameter.Fields)
                        {
                            CalculateResult fieldValue = ExprInterpret(field.Expression);

                            parameters.Add(new VariableSymbol(
                                name: field.Name.Value,
                                value: fieldValue.GetResult(),
                                type: fieldValue.DataType,
                                scope: null
                            ));
                        }
                    }

                    try
                    {
                        if(_apiCallsCount == 25)
                        {
                            ErrorMessage = $"Превышено количество вызовов методов API.";
                            return null;
                        }

                        var res = ApiMethodsExecutor.Instance.Execute(
                            sectionName: callNode.SectionName.Value,
                            methodname: callNode.Token.Value,
                            parameters: parameters
                        );

                        _apiCallsCount++;
                        return res;
                    }
                    catch (System.Exception ex)
                    {
                        ErrorMessage = $"Ошибка во время выполнения метода: '{callNode.SectionName.Value}.{callNode.Token.Value}' \nСтрока: {callNode.Token.PosNumber} \nОшибка: {ex.Message}";

                        return null;
                    }
                }
                else
                {
                    ErrorMessage = $"Вызов несуществующего метода: '{callNode.SectionName.Value}.{callNode.Token.Value}' \nСтрока: {callNode.Token.PosNumber}";
                    return null;
                }
            }

            if(node.Token.Type == TokenType.Number)
            {
                return new CalculateResult(double.Parse(node.Token.Value, System.Globalization.CultureInfo.InvariantCulture), DataType.Double);
            }

            if (node.Token.Type == TokenType.BoolDataType)
            {
                return new CalculateResult(bool.Parse(node.Token.Value), DataType.Bool);
            }

            if (node.Token.Type == TokenType.String)
            {
                return new CalculateResult(node.Token.Value, DataType.String);
            }

            string op = node.Token.Value;
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
                                ErrorMessage = $"Ошибка переполнения. Оператор '{node.Token.Value}'. Левый операнд: {(double)left_val.GetResult() } " +
                                    $"Правый операнд: {(double)right_val.GetResult()} \nСтрока: {node.Token.PosNumber}";

                                return null;
                            }
                            
                        }
                        else
                        {
                            // несоответствие типов
                            ErrorMessage = $"Оператор '{node.Token.Value}' ожидает тип Double, но обнаружены {left_val.DataType} и {right_val.DataType} \nСтрока: {node.Token.PosNumber}";
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
                            ErrorMessage = $"Оператор '{node.Token.Value}' ожидает тип Bool, но обнаружены {left_val.DataType} и {right_val.DataType} \nСтрока: {node.Token.PosNumber}";
                        }
                    }
                }
            }
            else if (node.Token.Type == TokenType.Identifier)
            {
                var var = _env.GetSymbol(node.Token.Value);
                if (var == null)
                {
                    ErrorMessage = $"Обнаружен необъявленный идентификатор: '{node.Token.Value}' \nСтрока: {node.Token.PosNumber}";
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
            var var_sym = (VariableSymbol)_env.GetSymbol(node.Id.Value);
            if(var_sym != null)
            {
                var expr_val = ExprInterpret(node.Expression);
                if(expr_val != null)
                {
                    var_sym.Value = expr_val.GetResult();
                    _env.UpdateSymbolValue(var_sym);
                    return true;
                }
            }
            else
            {
                ErrorMessage = $"Идентификатор '{node.Id.Value}' не объявлен \nСтрока: {node.Id.PosNumber}";
            }

            return false;
        }

        private CalculateResult ReturnInterpret(ReturnNode node)
        {
            return ExprInterpret(node.Expression);
        }

        private bool ExprValueToBool(CalculateResult exprResult)
        {
            if(exprResult.DataType == DataType.Bool)
            {
                return (bool)exprResult.GetResult();
            }
            else
            {
                if (((double)exprResult.GetResult()) == 0)
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
                        return Emulate(node.Body);
                }
                else
                {
                    if (node.Else != null)
                        return Emulate(node.Else);
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
                        res = Emulate(node.Body);

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
