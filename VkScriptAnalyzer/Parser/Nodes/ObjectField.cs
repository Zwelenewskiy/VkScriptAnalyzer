namespace VkScriptAnalyzer.Parser
{
    public class ObjectField
    {
        public Lexer.Token Name { get; set; }
        public ExprNode Expression { get; set; }

        public ObjectField(Lexer.Token token)
        {
            Name = token;
        }

        // Для юнит-тестов
        public ObjectField()
        {

        }
    }
}
