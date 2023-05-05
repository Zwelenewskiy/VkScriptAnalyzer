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
                        PrintObject(result.GetResult() as ObjectSymbol);
                    }
                    else
                    {
                        Console.WriteLine("Результат: " + result.GetResult());
                    }
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

        static void PrintObject(ObjectSymbol node)
        {

        }
    }
}
