using System;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzer.Parser
{
    public class Node
    {
        public Node Next { get; set; }
    }

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

    public class AssignNode : Node
    {
        public AssignNode(Token token)
        {
            Id = token;
        }

        public Token Id { get; set; }
        public ExprNode Expression { get; set; }
    }

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

    public class CallNode : ExprNode
    {
        public CallNode(Token token) : base(token) ///////////////////
        { }

        public System.Collections.Generic.IEnumerable<Token> parameters { get; set; }
    }

    public class IfNode : Node
    {
        public ExprNode Condition { get; set; }
        public Node Body { get; set; }
        public Node Else { get; set; }
    }

    public class WhileNode : Node
    {
        public ExprNode Condition { get; set; }
        public Node Body { get; set; }
    }

    public class ReturnNode : Node
    {
        public ReturnNode(ExprNode expr)
        {
            Expression = expr;
        }

        public ExprNode Expression { get; set; }
    }

    public class EmptyNode : Node
    {

    }
}
