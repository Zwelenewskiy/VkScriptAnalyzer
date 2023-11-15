using System;

namespace VkScriptAnalyzerTests;

internal class AssertFailedException : Exception
{
    public AssertFailedException(string s)
    {
    }
}