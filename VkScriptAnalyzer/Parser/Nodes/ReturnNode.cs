namespace VkScriptAnalyzer.Parser
{
    public class ReturnNode : Node
    {
        public ReturnNode(ExprNode expr)
        {
            Expression = expr;
        }

        public ExprNode Expression { get; set; }
    }
}
