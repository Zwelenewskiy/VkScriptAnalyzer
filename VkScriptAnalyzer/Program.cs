using System;
using VkScriptAnalyzer.GlobalClasses;

namespace VkScriptAnalyzer
{
    class Program
    {
        static void Main()
        {
            var lexer = new Lexer("+ - / *");

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
