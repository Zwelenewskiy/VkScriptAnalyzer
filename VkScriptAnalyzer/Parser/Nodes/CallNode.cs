using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class CallNode : ExprNode
    {
        public ObjectNode Parameter { get; set; }
        public Token SectionName { get; set; }

        public CallNode(Token token, Token sectionName) : base(token)
        {
            SectionName = sectionName;
        }
    }
}
