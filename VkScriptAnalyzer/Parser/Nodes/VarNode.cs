using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class VarNode : Node
    {
        public VarNode(Token token)
        {
            Id = token;
        }

        public Token Id { get; set; }
        public Node Expression { get; set; }
        public Node NextVar { get; set; }
    }
}
