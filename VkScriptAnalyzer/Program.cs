using System;
using VkScriptAnalyzer.Parser;
using VkScriptAnalyzer.Interpreter;

namespace VkScriptAnalyzer
{
    class Program
    {
        private const string INPUT_FILE_NAME = "input.vkscript";

        static void Main()
        {
            string input = System.IO.File.ReadAllText(INPUT_FILE_NAME);

            var parser = new SyntacticAnalyzer(input);
            Node ast = parser.Parse();

            if(ast == null)
            {
                Console.WriteLine(parser.ErrorMessage);
            }
            else
            {
                var interpreter = new InterpretMachine(ast);
                CalculateResult result = interpreter.Interpret();
                if (result == null)
                {
                    string error_message = interpreter.ErrorMessage;
                    if(error_message != null)
                    {
                        Console.WriteLine(error_message);
                    }
                    else
                    {
                        Console.WriteLine("Программа успешно завершена.");
                    }
                }
                else
                {
                    if(result.DataType == DataType.Object)
                    {
                        PrintObject(node: result.GetResult() as ObjectSymbol);
                    }
                    else
                    {
                        Console.WriteLine("Результат: " + result.GetResult());
                    }

                    Console.WriteLine();
                    Console.WriteLine("Программа успешно завершена.");
                }
            }

            /*var lexer = new Lexer.LexicalAnalyzer(input);

            var token = lexer.GetToken();
            while (true)
            {
                if (token == null)
                    break;

                Console.WriteLine($"Value: {token.value} \n Type: {token.type} \n Pos: {token.pos}");
                Console.WriteLine("------------------");

                token = lexer.GetToken();
            }*/

            Console.ReadKey();
        }

        static void PrintObject(ObjectSymbol node, int depth = 0, bool print_bracket = true)
        {
            if (print_bracket)
                PrintString(depth, "{", print_comma: false);

            int i = 0;
            foreach (string field_name in node.Fields.Keys)
            {
                bool print_comma = i < node.Fields.Keys.Count - 1;

                if (node.Fields[field_name] is ObjectSymbol)
                {
                    PrintString(depth + 2, field_name + ": {", print_comma: false);

                    PrintObject(node: node.Fields[field_name] as ObjectSymbol, depth: depth + 2, print_bracket: false);

                    PrintString(depth + 2, "}", print_comma);
                }
                else
                {
                    PrintString(depth + 2, $"{field_name}: {node.Fields[field_name]}", print_comma);
                }

                i++;
            }

            if (print_bracket)
                Console.WriteLine(new string(' ', depth) + "}");
        }

        static void PrintString(int indent, string value, bool print_comma)
        {
            Console.Write($"{new string(' ', indent)}{value}");

            if(print_comma)
                Console.Write(",");

            Console.WriteLine();
        }
    }
}
