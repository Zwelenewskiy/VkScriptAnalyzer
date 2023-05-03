using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class ExprNode : Node
    {
        public ExprNode(Token token)
        {
            this.token = token;
        }

        public Token token { get; set; }
        //public IVertex token { get; set; }

        public ExprNode Left { get; set; }
        public ExprNode Right { get; set; }
    }
}
