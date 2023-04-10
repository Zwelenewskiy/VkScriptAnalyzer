using System;

namespace VkScriptAnalyzer
{
    class Program
    {
        private const string INPUT_FILE_NAME = "input.vkscript";

        static void Main()
        {
            string input = System.IO.File.ReadAllText(INPUT_FILE_NAME);

            var lexer = new Lexer(input);

            var token = lexer.GetToken();
            while(true)
            {
                if (token == null)
                    break;

                Console.WriteLine("Value: " + token.value + Environment.NewLine + " Type: " +  token.type);
                Console.WriteLine("------------------");

                token = lexer.GetToken();
            }

            Console.ReadKey();
        }
    }
}
