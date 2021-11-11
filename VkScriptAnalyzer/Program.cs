using System;

namespace VkScriptAnalyzer
{
    class Program
    {
        static void Main()
        {
            var lexer = new Lexer("1 -   -6");

            var token = lexer.GetToken();
            while(true)
            {
                if (token == null)
                    break;

                Console.WriteLine(token.value + " -> " + token.type);

                token = lexer.GetToken();
            }

            Console.ReadKey();
        }
    }
}
