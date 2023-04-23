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

        public Node Left { get; set; }
        public Node Right { get; set; }
    }
}
