using System;
using Core.Emulator;
using Core.Parser;
using Entities.Emulator;
using Entities.Parser;

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
                var interpreter = new EmulatorMashine(ast);
                CalculateResult result = interpreter.StartEmulate();
                if (result == null)
                {
                    string errorMessage = interpreter.ErrorMessage;
                    if(errorMessage != null)
                    {
                        Console.WriteLine(errorMessage);
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

            foreach (string fieldName in node.Fields.Keys)
            {
                bool printComma = i < node.Fields.Keys.Count - 1;

                if ((node.Fields[fieldName] as VariableSymbol).Value is ObjectSymbol)
                {
                    PrintString(depth + 2, fieldName + ": {", printComma: false);

                    PrintObject(node: (node.Fields[fieldName] as VariableSymbol).Value as ObjectSymbol, depth: depth + 2, printBracket: false);

                    PrintString(depth + 2, "}", printComma);
                }
                else
                {
                    PrintString(depth + 2, $"{fieldName}: {(node.Fields[fieldName] as VariableSymbol).Value}", printComma);
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
