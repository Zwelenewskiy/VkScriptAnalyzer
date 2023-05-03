using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class ExprNode : Node
    {
        public ExprNode(Token token)
        {
            this.Token = token;
        }

        public Token Token { get; set; }
        //public IVertex token { get; set; }

        public ExprNode Left { get; set; }
        public ExprNode Right { get; set; }
    }
}
