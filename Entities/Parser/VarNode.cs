using Entities.Lexer;

namespace Entities.Parser
{
    public class VarNode : Node
    {
        public VarNode(Token token)
        {
            Id = token;
        }

        public Token Id { get; set; }
        public ExprNode Expression { get; set; }
        public VarNode NextVar { get; set; }
    }
}
