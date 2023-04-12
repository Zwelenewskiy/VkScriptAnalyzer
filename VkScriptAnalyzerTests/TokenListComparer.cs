using System.Collections;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzerTests
{
    public class TokenListComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            Token t1 = (Token)x;
            Token t2 = (Token)y;

            if(t1.value == t2.value
                && t1.type == t2.type)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}
