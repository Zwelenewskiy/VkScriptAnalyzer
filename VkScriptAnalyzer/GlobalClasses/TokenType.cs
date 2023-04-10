namespace VkScriptAnalyzer.GlobalClasses
{
    public enum TokenType
    {
        Unknown,
        Identifier,
        Number,
        KeyWord,
        DataType,
        String,
        OneSymbol,
        NonEqual,
        Equal,
        LeftBracket = 40,// (
        RightBracket = 41,// )
        Plus_Op = 43,
        Minus_Op = 45,
        Mul_Op = 42,
        Div_Op = 47,
        Colon = 59,// :
        Assign = 61,// =
        OpenQuotationMark = 60, // <
        CloseQuotationMark = 62,// >
        CurlyLeftBracket = 123,// {
        CurlyRightBracket = 125,// }
    }
}
