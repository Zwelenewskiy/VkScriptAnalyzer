using VkScriptAnalyzer.Mashines;

namespace VkScriptAnalyzer.GlobalClasses
{
    public class Token
    {
        public string value {get; set;}
        public int line {get; set;}
        public int pos {get; set;}
        public TokenType type { get; set; }

        public Token()
        {
            value = null;
        }
    }
}
