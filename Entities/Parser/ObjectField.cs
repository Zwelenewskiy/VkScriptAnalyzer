using Entities.Lexer;

namespace Entities.Parser
{
    public class ObjectField
    {
        public Token Name { get; set; }
        public ExprNode Expression { get; set; }

        public ObjectField(Token token)
        {
            Name = token;
        }

        // Для юнит-тестов
        public ObjectField()
        {

        }
    }
}
