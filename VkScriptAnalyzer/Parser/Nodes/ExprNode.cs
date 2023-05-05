using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class ExprNode : Node
    {
        public ExprNode(Token token)
        {
            Token = token;
        }

        public Token Token { get; set; }

        public ExprNode Left { get; set; }
        public ExprNode Right { get; set; }
    }

    public class KvalidentNode : ExprNode
    {
        public KvalidentNode(Token token) : base(token)
        {
        }
    }
}
