using System.Collections.Generic;
using Entities.Emulator;

namespace Entities.ApiMethodsExecutor
{
    public interface IApiMethodsExecutor
    {
        CalculateResult Execute(string sectionName, string methodname, List<VariableSymbol> parameters);
    }
}
