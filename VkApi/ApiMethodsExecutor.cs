using System.Collections.Generic;
using Core.ApiMethodsExecutor;
using Entities.Emulator;
using VkNet.Model;

namespace VkApi
{
    public class ApiMethodsExecutor : IApiMethodsExecutor
    {
        private readonly VkNet.VkApi _api;

        public ApiMethodsExecutor(string login, string password, ulong applicationId)
        {
            _api = new VkNet.VkApi();

            _api.Authorize(new ApiAuthParams
            {
                ApplicationId = applicationId,
                Login         = login,
                Password      = password,
                Settings      = VkNet.Enums.Filters.Settings.All
            });
        }

        public CalculateResult Execute(string sectionName, string methodname, List<VariableSymbol> parameters)
        {
            if(sectionName == "account")
            {
                if(methodname == "setOffline")
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
