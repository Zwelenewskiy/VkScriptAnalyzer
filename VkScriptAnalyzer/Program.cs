using System;

namespace VkScriptAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer.input = "1564a";

            var token = Lexer.GetToken();
            while(true)
            {
                Console.WriteLine(token.Type + " : " + token.Value);

                if (token.Type == Lex_type.Unknown)
                    break;

                token = Lexer.GetToken();
            }

            Console.ReadKey();
        }
    }
}
