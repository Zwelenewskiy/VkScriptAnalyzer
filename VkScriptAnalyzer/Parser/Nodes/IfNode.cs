namespace VkScriptAnalyzer.Parser
{
    public class IfNode : Node
    {
        public ExprNode Condition { get; set; }
        public Node Body { get; set; }
        public Node Else { get; set; }
    }
}
