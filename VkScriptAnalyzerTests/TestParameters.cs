using System.Collections.Generic;
using VkScriptAnalyzer.Lexer;

namespace VkScriptAnalyzerTests;

internal class TestParameters
{
    public string InputText { get; set; }
    public List<Token> Sample { get; set; }
}