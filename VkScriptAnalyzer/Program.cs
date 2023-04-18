using System;
using VkScriptAnalyzer.Parser;

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

            if(parser.error_message != null)
            {
                Console.WriteLine(parser.error_message);
            }

            /*var lexer = new Lexer.LexicalAnalyzer(input);

            var token = lexer.GetToken();
            while(true)
            {
                if (token == null)
                    break;

                Console.WriteLine("Value: " + token.value + Environment.NewLine + " Type: " +  token.type);
                Console.WriteLine("------------------");

                token = lexer.GetToken();
            }*/

            Console.ReadKey();
        }
    }
}
