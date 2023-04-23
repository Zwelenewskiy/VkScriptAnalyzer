
namespace VkScriptAnalyzer.Parser
{
    public class WhileNode : Node
    {
        public ExprNode Condition { get; set; }
        public Node Body { get; set; }
    }
}
