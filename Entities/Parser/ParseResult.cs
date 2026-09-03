namespace Entities.Parser
{
    public class ParseResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public Node Program { get; }

        public ParseResult(Node node)
        {
            Program = node;
            IsSuccess = true;
            ErrorMessage = null;
        }

        public ParseResult(string errorMessage)
        {
            Program = null;
            IsSuccess = false;
            ErrorMessage = errorMessage;
        }
    }
}
