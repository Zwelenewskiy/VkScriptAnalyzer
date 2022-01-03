using VkScriptAnalyzer.Mashines;

namespace VkScriptAnalyzer.GlobalClasses
{
    public enum TokenType
    {
        Unknown,
        Identifier,
        Number,
        KeyWord,
        DataType,
        Assign,
        String,
        OneSymbol,
        Plus_Op = 43,
        Minus_Op = 45,
        Mul_Op = 42,
        Div_Op = 47
    }

    public class Token
    {
        public string value {get; set;}
        public int line {get; set;}
        public int pos {get; set;}
        public TokenType type { get; set; }

        public Token()
        {
            value = null;
        }
    }
}
