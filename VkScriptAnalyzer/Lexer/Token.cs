namespace VkScriptAnalyzer.Lexer
{
    public enum TokenType
    {
        Unknown,
        Identifier,
        Number,
        KeyWord,
        BoolDataType,
        String,
        OneSymbol,
        NonEqual,
        Equal,

        Function,

        LeftBracket = 40,// (
        RightBracket = 41,// )
        Plus_Op = 43,
        Comma = 44,// ,
        Minus_Op = 45,
        Dot = 46,
        Mul_Op = 42,
        Div_Op = 47,
        Colon = 59,// :
        Assign = 61,// =
        OpenQuotationMark = 60, // <
        CloseQuotationMark = 62,// >
        CurlyLeftBracket = 123,// {
        CurlyRightBracket = 125,// }
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
