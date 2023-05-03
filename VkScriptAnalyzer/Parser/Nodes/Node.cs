namespace VkScriptAnalyzer.Parser
{
    public class Node
    {
        public Node Next { get; set; }
    }

    /*/// <summary>
    /// Вершина AST
    /// </summary>
    public interface IVertex
    {

    }

    /// <summary>
    /// Вершина AST - простой токен
    /// </summary>
    public class TokenVertex : IVertex
    {
        public TokenVertex(Lexer.Token token)
        {
            Token = token;
        }

        public Lexer.Token Token { get; set; }
    }

    /// <summary>
    /// Вершина AST - объект 
    /// </summary>
    public class ObjectVertex : IVertex
    {
        public ObjectVertex(Lexer.Token token)
        {
            //Token = token;
        }

        public ObjectNode Token { get; set; }
    }

    /// <summary>
    /// Вершина AST - поле объекта 
    /// </summary>
    public class FieldVertex : IVertex
    {
        /// <summary>
        /// Поле объекта
        /// </summary>
        public TokenVertex Token { get; set; }
        /// <summary>
        /// Объект, которому принадлежит поле
        /// </summary>
        public ObjectNode Owner { get; set; } 
    }*/
}
