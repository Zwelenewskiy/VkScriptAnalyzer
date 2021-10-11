using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkScriptAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer.input = "abc(";

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
