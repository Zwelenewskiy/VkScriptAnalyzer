using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class CallNode : ExprNode
    {
        public CallNode(Token token) : base(token)
        { }

        public System.Collections.Generic.IEnumerable<Token> parameters { get; set; }
    }
}
