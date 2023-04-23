using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class AssignNode : Node
    {
        public AssignNode(Token token)
        {
            Id = token;
        }

        public Token Id { get; set; }
        public ExprNode Expression { get; set; }
    }
}
