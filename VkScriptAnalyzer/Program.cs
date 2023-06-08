using System;
using VkScriptAnalyzer.Parser;
using VkScriptAnalyzer.Emulator;

namespace VkScriptAnalyzer
{
    class Program
    {
        private const string InputFileName = "input.vkscript";

        static void Main()
        {
            string input = System.IO.File.ReadAllText(InputFileName);

            var parser = new SyntacticAnalyzer(input);
            Node ast = parser.Parse();

            if(ast == null)
            {
                Console.WriteLine(parser.ErrorMessage);
            }
            else
            {
                var interpreter = new EmulatorMashine(ast);
                CalculateResult result = interpreter.StartEmulate();
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
                        if(result.GetResult() == null)
                        {
                            Console.WriteLine("Результат: null");
                        }
                        else
                        {
                            Console.WriteLine("Результат:");
                            PrintObject(node: result.GetResult() as ObjectSymbol);
                        }
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

        static void PrintObject(ObjectSymbol node, int depth = 0, bool printBracket = true)
        {
            if (node.Fields.Keys.Count == 0)
            {
                Console.WriteLine(new string(' ', depth) + "null");
                return;
            }

            if (printBracket)
                PrintString(depth, "{", printComma: false);

            int i = 0;

            foreach (string field_name in node.Fields.Keys)
            {
                bool print_comma = i < node.Fields.Keys.Count - 1;

                if ((node.Fields[field_name] as VariableSymbol).Value is ObjectSymbol)
                {
                    PrintString(depth + 2, field_name + ": {", printComma: false);

                    PrintObject(node: (node.Fields[field_name] as VariableSymbol).Value as ObjectSymbol, depth: depth + 2, printBracket: false);

                    PrintString(depth + 2, "}", print_comma);
                }
                else
                {
                    PrintString(depth + 2, $"{field_name}: {(node.Fields[field_name] as VariableSymbol).Value}", print_comma);
                }

                i++;
            }

            if (printBracket)
                Console.WriteLine(new string(' ', depth) + "}");
        }

        static void PrintString(int indent, string value, bool printComma)
        {
            Console.Write($"{new string(' ', indent)}{value}");

            if(printComma)
                Console.Write(",");

            Console.WriteLine();
        }
    }
}
