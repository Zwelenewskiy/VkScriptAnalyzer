using System.Linq;
using Entities.Emulator;
using Entities.Lexer;
using Entities.Parser;
using Core.ApiMethodsExecutor;

namespace Core.Emulator
{
    public class EmulatorMashine
    {
        private readonly Node _ast;
        private readonly IApiMethodsExecutor _api;
        private readonly string[] _existingApiMethods = [ "account_setOffline" ];
        private Env _env;
        private int _apiCallsCount;

        public EmulatorMashine(Node ast, IApiMethodsExecutor api)
        {
            _ast = ast;
            _apiCallsCount = 0;
            _api = api;
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
        private System.Collections.Generic.List<VariableSymbol> NodeFieldToObjectField(ObjectNode node, out CalculateResult error)
        {
            error = null;
            var res = new System.Collections.Generic.List<VariableSymbol>();

            if (node != null)
            {
                foreach (var field in node.Fields)
                {
                    CalculateResult fieldValue = ExprInterpret(field.Expression);

                    if (fieldValue == null || !fieldValue.IsSuccess)
                    {
                        error = fieldValue ?? new CalculateResult("Ошибка вычисления поля объекта");
                        return null;
                    }

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
                var varResult = VarInterpret(node as VarNode);
                if (varResult != null && !varResult.IsSuccess)
                    return varResult;

                return Emulate(node.Next);
            }
            if (node is AssignNode)
            {
                var assignResult = AssignInterpret(node as AssignNode);
                if (assignResult != null && !assignResult.IsSuccess)
                    return assignResult;

                return Emulate(node.Next);
            }
            if (node is IfNode)
            {
                _env.CreateScope();

                var ifResult = IfInterpret(node as IfNode);// результат может быть только при Return

                _env.CloseScope();   
                
                if (ifResult == null)
                    return Emulate(node.Next);
                if (!ifResult.IsSuccess)
                    return ifResult;

                return ifResult;
            }
            if (node is WhileNode)
            {
                _env.CreateScope();

                var whileResult = WhileInterpret(node as WhileNode);// результат может быть только при Return

                _env.CloseScope();

                if (whileResult == null)
                    return Emulate(node.Next);
                if (!whileResult.IsSuccess)
                    return whileResult;

                return whileResult;
            }
            if (node is ReturnNode)
            {
                return ReturnInterpret(node as ReturnNode);
            }

            return null;
        }

        private CalculateResult VarInterpret(VarNode node)
        {
            if(node != null)
            {
                var symbol = _env.GetSymbolLocal(node.Id.Value);
                if (symbol == null)
                {
                    CalculateResult exprVal = ExprInterpret(node.Expression);                    

                    if (exprVal == null || !exprVal.IsSuccess)
                        return exprVal;

                    var result = exprVal.GetResult();
                    var scope = _env.GetCurrentScope();

                    _env.AddSymbol(new VariableSymbol(
                        name:  node.Id.Value, 
                        value: result,
                        type:  exprVal.DataType,
                        scope: scope
                    ));

                    if (node.NextVar == null)
                        return null;
                    else
                        return VarInterpret(node.NextVar);
                }
                else
                {
                    CalculateResult exprVal = ExprInterpret(node.Expression);
                    if (exprVal == null || !exprVal.IsSuccess)
                        return exprVal;

                    (symbol as VariableSymbol).Value = exprVal.GetResult();
                    _env.UpdateSymbolValue(symbol);

                    if (node.NextVar == null)
                        return null;
                    else
                        return VarInterpret(node.NextVar);
                }
            }

            return new CalculateResult("Ошибка объявления переменной");
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
                    return new CalculateResult($"Обнаружен необъявленный идентификатор: '{node.Left.Token.Value}' \nСтрока: {node.Left.Token.PosNumber}");
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
                var objNode = node as ObjectNode;
                var fields = NodeFieldToObjectField(objNode, out CalculateResult fieldsError);

                if (fields == null)
                    return fieldsError;

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

                            if (fieldValue == null || !fieldValue.IsSuccess)
                                return fieldValue ?? new CalculateResult("Ошибка вычисления параметра метода");

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
                            return new CalculateResult("Превышено количество вызовов методов API.");
                        }

                        var res = _api.Execute(
                            sectionName: callNode.SectionName.Value,
                            methodname: callNode.Token.Value,
                            parameters: parameters
                        );

                        _apiCallsCount++;
                        return res;
                    }
                    catch (System.Exception ex)
                    {
                        return new CalculateResult($"Ошибка во время выполнения метода: '{callNode.SectionName.Value}.{callNode.Token.Value}' \nСтрока: {callNode.Token.PosNumber} \nОшибка: {ex.Message}");
                    }
                }
                else
                {
                    return new CalculateResult($"Вызов несуществующего метода: '{callNode.SectionName.Value}.{callNode.Token.Value}' \nСтрока: {callNode.Token.PosNumber}");
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
                var leftVal = ExprInterpret(node.Left);

                if (leftVal == null || !leftVal.IsSuccess)
                    return leftVal;

                var rightVal = ExprInterpret(node.Right);

                if (rightVal == null || !rightVal.IsSuccess)
                    return rightVal;

                if (leftVal.DataType == DataType.Double && rightVal.DataType == DataType.Double)
                {
                    try
                    {
                        if (op == "+")
                            return new CalculateResult((double)leftVal.GetResult() + (double)rightVal.GetResult(), DataType.Double);
                        else if (op == "-")
                            return new CalculateResult((double)leftVal.GetResult() - (double)rightVal.GetResult(), DataType.Double);
                        else if (op == "*")
                            return new CalculateResult((double)leftVal.GetResult() * (double)rightVal.GetResult(), DataType.Double);
                        else if (op == "/")
                            return new CalculateResult((double)leftVal.GetResult() / (double)rightVal.GetResult(), DataType.Double);
                        else if (op == ">")
                            return new CalculateResult((double)leftVal.GetResult() > (double)rightVal.GetResult(), DataType.Bool);
                        else if (op == "<")
                            return new CalculateResult((double)leftVal.GetResult() < (double)rightVal.GetResult(), DataType.Bool);
                        else if (op == ">=")
                            return new CalculateResult((double)leftVal.GetResult() >= (double)rightVal.GetResult(), DataType.Bool);
                        else if (op == "<=")
                            return new CalculateResult((double)leftVal.GetResult() <= (double)rightVal.GetResult(), DataType.Bool);
                        else if (op == "==")
                            return new CalculateResult((double)leftVal.GetResult() == (double)rightVal.GetResult(), DataType.Bool);
                        else
                            return new CalculateResult((double)leftVal.GetResult() != (double)rightVal.GetResult(), DataType.Bool);
                    }
                    catch (System.OverflowException)
                    {
                        return new CalculateResult($"Ошибка переполнения. Оператор '{node.Token.Value}'. Левый операнд: {(double)leftVal.GetResult() } " +
                            $"Правый операнд: {(double)rightVal.GetResult()} \nСтрока: {node.Token.PosNumber}");
                    }
                }
                else
                {
                    return new CalculateResult($"Оператор '{node.Token.Value}' ожидает тип Double, но обнаружены {leftVal.DataType} и {rightVal.DataType} \nСтрока: {node.Token.PosNumber}");
                }
            }
            else if (op == "and" || op == "or")
            {
                var leftVal = ExprInterpret(node.Left);

                if (leftVal == null || !leftVal.IsSuccess)
                    return leftVal;

                var rightVal = ExprInterpret(node.Right);

                if (rightVal == null || !rightVal.IsSuccess)
                    return rightVal;

                if (leftVal.DataType == DataType.Bool && rightVal.DataType == DataType.Bool)
                {
                    if(op == "and")
                        return new CalculateResult((bool)leftVal.GetResult() && (bool)rightVal.GetResult(), DataType.Bool);
                    else 
                        return new CalculateResult((bool)leftVal.GetResult() || (bool)rightVal.GetResult(), DataType.Bool);
                }
                else
                {
                    return new CalculateResult($"Оператор '{node.Token.Value}' ожидает тип Bool, но обнаружены {leftVal.DataType} и {rightVal.DataType} \nСтрока: {node.Token.PosNumber}");
                }
            }
            else if (node.Token.Type == TokenType.Identifier)
            {
                var var = _env.GetSymbol(node.Token.Value);
                if (var == null)
                {
                    return new CalculateResult($"Обнаружен необъявленный идентификатор: '{node.Token.Value}' \nСтрока: {node.Token.PosNumber}");
                }
                else
                {
                    if (var is VariableSymbol)
                    {
                        var varSym = var as VariableSymbol;

                        return new CalculateResult(varSym.Value, varSym.DataType);
                    }
                    else if (var is FunctionSymbol)
                    {
                        // создание функций не поддерживается
                    }
                }
            }

            return new CalculateResult("Ошибка вычисления выражения");
        }

        private CalculateResult AssignInterpret(AssignNode node)
        {
            var varSym = (VariableSymbol)_env.GetSymbol(node.Id.Value);
            if(varSym != null)
            {
                var exprVal = ExprInterpret(node.Expression);
                if (exprVal == null || !exprVal.IsSuccess)
                    return exprVal;

                varSym.Value = exprVal.GetResult();
                _env.UpdateSymbolValue(varSym);
                return null;
            }

            return new CalculateResult($"Идентификатор '{node.Id.Value}' не объявлен \nСтрока: {node.Id.PosNumber}");
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
            var condExpr = ExprInterpret(node.Condition);
            if (condExpr == null || !condExpr.IsSuccess)
                return condExpr;

            bool condVal = ExprValueToBool(condExpr);
            if (condVal)
            {
                if (node.Body is EmptyNode == false)
                    return Emulate(node.Body);
            }
            else
            {
                if (node.Else != null)
                    return Emulate(node.Else);
            }

            return null;
        }

        private CalculateResult WhileInterpret(WhileNode node)
        {
            CalculateResult condExpr = ExprInterpret(node.Condition);
            if (condExpr == null || !condExpr.IsSuccess)
                return condExpr;

            if (node.Body is EmptyNode == false)
            {
                bool condVal = ExprValueToBool(condExpr);

                CalculateResult res = null;
                while (condVal)
                {
                    res = Emulate(node.Body);
                    if (res != null && !res.IsSuccess)
                        return res;

                    condExpr = ExprInterpret(node.Condition);
                    if (condExpr == null || !condExpr.IsSuccess)
                        return condExpr;

                    condVal = ExprValueToBool(condExpr);
                }

                return res;
            }

            return null;
        }
    }
}
