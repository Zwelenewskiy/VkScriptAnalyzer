using System.Collections.Generic;
using VkNet;
using VkNet.Model;

namespace VkScriptAnalyzer.Emulator
{
    public class ApiMethodsExecutor
    {
        private static ApiMethodsExecutor _instance;
        private VkApi _api;

        public static ApiMethodsExecutor Instance
        {
            get
            {
                return _instance ?? (_instance = new ApiMethodsExecutor());
            }
        }

        private ApiMethodsExecutor()
        {
            _api = new VkApi();

            _api.Authorize(new ApiAuthParams
            {
                ApplicationId = 7911433,
                Login         = "89534798532",
                Password      = "G9hvZxlynM{1~R3",
                Settings      = VkNet.Enums.Filters.Settings.All
            });
        }

        public CalculateResult Execute(string sectionName, string methodName, List<VariableSymbol> parameters)
        {
            if(sectionName == "account")
            {
                if(methodName == "setOffline")
                {
                    if (_api.Account.SetOffline())
                        return new CalculateResult(1, DataType.Double);
                    else
                        return new CalculateResult(0, DataType.Double);
                }
            }

            return null;
        }
    }
}
