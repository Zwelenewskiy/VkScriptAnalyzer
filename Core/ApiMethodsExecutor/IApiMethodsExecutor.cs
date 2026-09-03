using System.Collections.Generic;
using Entities.Emulator;

namespace Core.ApiMethodsExecutor
{
    public interface IApiMethodsExecutor
    {
        CalculateResult Execute(string sectionName, string methodname, List<VariableSymbol> parameters);
    }
}
