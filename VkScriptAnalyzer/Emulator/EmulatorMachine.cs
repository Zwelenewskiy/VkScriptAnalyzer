using System;
using VkScriptAnalyzer.Lexer;
using VkScriptAnalyzer.Parser;
using System.Linq;

namespace VkScriptAnalyzer.Emulator
{
    public class CalculateResult
    {
        private readonly object _value;
        public DataType DataType { get; set; }

        public CalculateResult(object val, DataType type)
        {
            _value = val;
            DataType = type;
        }

        public object GetResult()
        {
            return _value;
        }
    }

    public class EmulatorMachine
    {
        private readonly Node _ast;
        private Env _env;

        private readonly string[] _existingApiMethods = new string[]
        {
            "account_setOffline"
        };

        private int _apiCallsCount;

        public string ErrorMessage { get; private set; }

        public EmulatorMachine(Node ast)
        {
            _ast = ast;
            _apiCallsCount = 0;
        }

        public CalculateResult StartEmulate()
        {
            _env = new();
            _env.CreateScope();

            return Emulate(_ast);
        }

        /// <summary>
        /// Возвращает список вычисленных переменных из списка полей вершины AST
        /// </summary>
        private System.Collections.Generic.List<VariableSymbol> NodeFieldToObjectField(ObjectNode node)
        {
            var result = new System.Collections.Generic.List<VariableSymbol>();

            if (node == null)
            {
                return result;
            }

            foreach (var field in node.Fields)
            {
                var fieldValue = ExprInterpret(field.Expression);

                if (fieldValue == null)
                {
                    return null;
                }

                result.Add(new(
                    name: field.Name.Value,
                    value: fieldValue.GetResult(),
                    type: fieldValue.DataType,
                    scope: null
                ));
            }

            return result;
        }

        private CalculateResult Emulate(Node node)
        {
            switch (node)
            {
                case VarNode varNode when VarInterpret(varNode):
                    return Emulate(varNode.Next);
                case AssignNode assignNode when AssignInterpret(assignNode):
                    return Emulate(assignNode.Next);
                case IfNode ifNode:
                {
                    _env.CreateScope();

                    var ifResult = IfInterpret(ifNode); // результат может быть только при Return

                    _env.CloseScope();

                    return ifResult ?? Emulate(ifNode.Next);
                }
                case WhileNode whileNode:
                {
                    _env.CreateScope();

                    var whileResult = WhileInterpret(whileNode); // результат может быть только при Return

                    _env.CloseScope();

                    return whileResult ?? Emulate(whileNode.Next);
                }
                case ReturnNode returnNode:
                    return ReturnInterpret(returnNode);
                default:
                    return null;
            }
        }

        private bool VarInterpret(VarNode node)
        {
            if (node == null)
            {
                return false;
            }

            var symbol = _env.GetSymbolLocal(node.Id.Value);

            if (symbol == null)
            {
                var expressionValue = ExprInterpret(node.Expression);

                if (expressionValue == null)
                {
                    return false;
                }

                var result = expressionValue.GetResult();
                var scope = _env.GetCurrentScope();

                _env.AddSymbol(new VariableSymbol(
                    name: node.Id.Value,
                    value: result,
                    type: expressionValue.DataType,
                    scope: scope
                ));

                return node.NextVar == null || VarInterpret(node.NextVar);
            } else
            {
                var expressionValue = ExprInterpret(node.Expression);

                if (expressionValue == null)
                {
                    return false;
                }

                (symbol as VariableSymbol).Value = expressionValue.GetResult();
                _env.UpdateSymbolValue(symbol);

                return node.NextVar == null || VarInterpret(node.NextVar);
            }
        }

        // b.c.d
        //                   .
        //                  / \
        //        объект   b   .
        //                    / \
        //  идентификаторы   c   d
        private CalculateResult KvalidentInterpret(KvalidentNode node, ObjectSymbol obj)
        {
            if (obj.GetMember(node.Left.Token.Value) is not VariableSymbol { Value: ObjectSymbol } field)
            {
                return new(null, DataType.Object);
            }

            if (node.Right.Token.Type != TokenType.Identifier
                || node.Left.Token.Type != TokenType.Identifier) // базовый случай, когда потомки узла - идентификаторы. Ниже идти не нужно
            {
                return KvalidentInterpret(node.Right as KvalidentNode, field.Value as ObjectSymbol);
            }

            var result = (field.Value as ObjectSymbol).GetMember(node.Right.Token.Value) as VariableSymbol;

            return new(result.Value, result.DataType);
        }

        private CalculateResult ExprInterpret(ExprNode node)
        {
            if (node is KvalidentNode)
            {
                if (_env.GetSymbol(node.Left.Token.Value) is not VariableSymbol leftSymbol)
                {
                    ErrorMessage = $"Обнаружен необъявленный идентификатор: '{node.Left.Token.Value}' \nСтрока: {node.Left.Token.Pos}";

                    return null;
                }

                if (leftSymbol.Value is not ObjectSymbol objectSymbol)
                {
                    return new(null, DataType.Object);
                }

                if (node.Right is KvalidentNode right) // базовый случай, когда потомки узла - идентификаторы. Ниже идти не нужно
                {
                    return KvalidentInterpret(right, objectSymbol);
                }

                var result = objectSymbol.GetMember(node.Right.Token.Value) as VariableSymbol;

                return new(result.Value, result.DataType);
            }

            if (node is ObjectNode objectNode)
            {
                var fields = NodeFieldToObjectField(objectNode);

                if (fields == null)
                {
                    return null;
                }

                return new(new ObjectSymbol(
                        name: null,
                        scope: _env.GetCurrentScope(),
                        fields: fields
                    ),
                    type: DataType.Object
                );
            }

            if (node is CallNode callNode)
            {
                if (_existingApiMethods.Contains(callNode.SectionName.Value + "_" + callNode.Token.Value))
                {
                    var parameters = new System.Collections.Generic.List<VariableSymbol>();

                    if (callNode.Parameter != null)
                    {
                        foreach (var field in callNode.Parameter.Fields)
                        {
                            var fieldValue = ExprInterpret(field.Expression);

                            parameters.Add(new(
                                name: field.Name.Value,
                                value: fieldValue.GetResult(),
                                type: fieldValue.DataType,
                                scope: null
                            ));
                        }
                    }

                    try
                    {
                        if (_apiCallsCount == 25)
                        {
                            ErrorMessage = "Превышено количество вызовов методов API.";

                            return null;
                        }

                        var result = ApiMethodsExecutor.Instance.Execute(
                            sectionName: callNode.SectionName.Value,
                            methodName: callNode.Token.Value,
                            parameters: parameters
                        );

                        _apiCallsCount++;

                        return result;
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage =
                            $"Ошибка во время выполнения метода: '{callNode.SectionName.Value}.{callNode.Token.Value}' \nСтрока: {callNode.Token.Pos} \nОшибка: {ex.Message}";

                        return null;
                    }
                }

                ErrorMessage = $"Вызов несуществующего метода: '{callNode.SectionName.Value}.{callNode.Token.Value}' \nСтрока: {callNode.Token.Pos}";

                return null;
            }

            switch (node.Token.Type)
            {
                case TokenType.Number:
                    return new(double.Parse(node.Token.Value, System.Globalization.CultureInfo.InvariantCulture), DataType.Double);
                case TokenType.BoolDataType:
                    return new(bool.Parse(node.Token.Value), DataType.Bool);
                case TokenType.String:
                    return new(node.Token.Value, DataType.String);
            }

            var operatorToken = node.Token.Value;

            if (operatorToken is "+" or "-" or "*" or "/" or ">" or "<" or ">=" or "<=" or "==" or "!=")
            {
                var leftValue = ExprInterpret(node.Left);

                if (leftValue == null)
                {
                    return null;
                }

                var rightValue = ExprInterpret(node.Right);

                if (rightValue == null)
                {
                    return null;
                }

                if (leftValue.DataType == DataType.Double && rightValue.DataType == DataType.Double)
                {
                    try
                    {
                        return operatorToken switch
                        {
                            "+" => new((double) leftValue.GetResult() + (double) rightValue.GetResult(), DataType.Double),
                            "-" => new((double) leftValue.GetResult() - (double) rightValue.GetResult(), DataType.Double),
                            "*" => new((double) leftValue.GetResult() * (double) rightValue.GetResult(), DataType.Double),
                            "/" => new((double) leftValue.GetResult() / (double) rightValue.GetResult(), DataType.Double),
                            ">" => new((double) leftValue.GetResult() > (double) rightValue.GetResult(), DataType.Bool),
                            "<" => new((double) leftValue.GetResult() < (double) rightValue.GetResult(), DataType.Bool),
                            ">=" => new((double) leftValue.GetResult() >= (double) rightValue.GetResult(), DataType.Bool),
                            "<=" => new((double) leftValue.GetResult() <= (double) rightValue.GetResult(), DataType.Bool),
                            "==" => new((double) leftValue.GetResult() == (double) rightValue.GetResult(), DataType.Bool),
                            _ => new((double) leftValue.GetResult() != (double) rightValue.GetResult(), DataType.Bool)
                        };
                    }
                    catch (OverflowException)
                    {
                        ErrorMessage = $"Ошибка переполнения. Оператор '{node.Token.Value}'. Левый операнд: {(double) leftValue.GetResult()} "
                                       + $"Правый операнд: {(double) rightValue.GetResult()} \nСтрока: {node.Token.Pos}";

                        return null;
                    }
                }

                // несоответствие типов
                ErrorMessage =
                    $"Оператор '{node.Token.Value}' ожидает тип Double, но обнаружены {leftValue.DataType} и {rightValue.DataType} \nСтрока: {node.Token.Pos}";
            } else if (operatorToken is Keywords.And or Keywords.Or)
            {
                var leftValue = ExprInterpret(node.Left);

                if (leftValue == null)
                {
                    return null;
                }

                var rightValue = ExprInterpret(node.Right);

                if (rightValue == null)
                {
                    return null;
                }

                if (leftValue.DataType == DataType.Bool && rightValue.DataType == DataType.Bool)
                {
                    return operatorToken == Keywords.And
                        ? new((bool) leftValue.GetResult() && (bool) rightValue.GetResult(), DataType.Bool)
                        : new CalculateResult((bool) leftValue.GetResult() || (bool) rightValue.GetResult(), DataType.Bool);
                }

                ErrorMessage =
                    $"Оператор '{node.Token.Value}' ожидает тип Bool, но обнаружены {leftValue.DataType} и {rightValue.DataType} \nСтрока: {node.Token.Pos}";
            } else if (node.Token.Type == TokenType.Identifier)
            {
                var variable = _env.GetSymbol(node.Token.Value);

                if (variable == null)
                {
                    ErrorMessage = $"Обнаружен необъявленный идентификатор: '{node.Token.Value}' \nСтрока: {node.Token.Pos}";
                } else
                {
                    if (variable is VariableSymbol variableSymbol)
                    {
                        /*if (var_sym.DataType == DataType.Double)
                            return new CalculateResult(var_sym.Value, DataType.Double);
                        else if (var_sym.DataType == DataType.Bool)
                            return new CalculateResult(var_sym.Value, DataType.Bool);*/

                        return new(variableSymbol.Value, variableSymbol.DataType);
                    }

                    if (variable is FunctionSymbol)
                    {
                        // создание функций не поддерживается
                    }
                }
            }

            return null;
        }

        private bool AssignInterpret(AssignNode node)
        {
            var variableSymbol = (VariableSymbol) _env.GetSymbol(node.Id.Value);

            if (variableSymbol != null)
            {
                var expressionValue = ExprInterpret(node.Expression);

                if (expressionValue == null)
                {
                    return false;
                }

                variableSymbol.Value = expressionValue.GetResult();
                _env.UpdateSymbolValue(variableSymbol);

                return true;
            }

            ErrorMessage = $"Идентификатор '{node.Id.Value}' не объявлен \nСтрока: {node.Id.Pos}";

            return false;
        }

        private CalculateResult ReturnInterpret(ReturnNode node)
        {
            return ExprInterpret(node.Expression);
        }

        private bool ExprValueToBool(CalculateResult exprResult)
        {
            if (exprResult.DataType == DataType.Bool)
            {
                return (bool) exprResult.GetResult();
            }

            return (double) exprResult.GetResult() != 0;
        }

        private CalculateResult IfInterpret(IfNode node)
        {
            var conditionExpression = ExprInterpret(node.Condition);

            if (conditionExpression == null)
            {
                return null;
            }

            var conditionValue = ExprValueToBool(conditionExpression);

            if (conditionValue)
            {
                if (node.Body is not EmptyNode)
                {
                    return Emulate(node.Body);
                }
            } else
            {
                if (node.Else != null)
                {
                    return Emulate(node.Else);
                }
            }

            return null;
        }

        private CalculateResult WhileInterpret(WhileNode node)
        {
            var conditionExpression = ExprInterpret(node.Condition);

            if (conditionExpression == null || node.Body is EmptyNode)
            {
                return null;
            }

            var conditionValue = ExprValueToBool(conditionExpression);

            CalculateResult result = null;

            while (conditionValue)
            {
                result = Emulate(node.Body);

                conditionExpression = ExprInterpret(node.Condition);

                if (conditionExpression == null)
                {
                    break;
                }

                conditionValue = ExprValueToBool(conditionExpression);
            }

            return result;
        }
    }
}