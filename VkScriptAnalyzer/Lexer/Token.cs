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
        PlusOp = 43,
        Comma = 44,// ,
        MinusOp = 45,
        Dot = 46,
        MulOp = 42,
        DivOp = 47,
        Colon = 59,// :
        Assign = 61,// =
        OpenQuotationMark = 60, // <
        CloseQuotationMark = 62,// >
        CurlyLeftBracket = 123,// {
        CurlyRightBracket = 125,// }
    }

    public class Token
    {
        public string Value {get; set;}
        public int Line {get; set;}
        public int Pos {get; set;}
        public TokenType Type { get; set; }

        public Token()
        {
            Value = null;
        }
    }
}
