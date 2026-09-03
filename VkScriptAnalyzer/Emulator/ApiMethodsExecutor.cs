using System.Collections.Generic;
using Entities.Emulator;
using VkNet;
using VkNet.Model;

namespace VkScriptAnalyzer.Emulator
{
    public class ApiMethodsExecutor
    {
        private static ApiMethodsExecutor _instance;
        private VkApi api;

        public static ApiMethodsExecutor Instance
        {
            get
            {
                return _instance ?? (_instance = new ApiMethodsExecutor());
            }
        }

        private ApiMethodsExecutor()
        {
            api = new VkApi();

            api.Authorize(new ApiAuthParams
            {
                ApplicationId = 7911433,
                Login         = "89534798532",
                Password      = "G9hvZxlynM{1~R3",
                Settings      = VkNet.Enums.Filters.Settings.All
            });
        }

        public CalculateResult Execute(string sectionName, string methodname, List<VariableSymbol> parameters)
        {
            if(sectionName == "account")
            {
                if(methodname == "setOffline")
                {
                    if (api.Account.SetOffline())
                        return new CalculateResult(1, DataType.Double);
                    else
                        return new CalculateResult(0, DataType.Double);
                }
            }

            return null;
        }
    }
}
