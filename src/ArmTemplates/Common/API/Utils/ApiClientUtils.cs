// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Gateway;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils
{
    public class ApiClientUtils: IApiClientUtils
    {
        readonly IApisClient apisClient;
        readonly ILogger<ApiClientUtils> logger;

        public ApiClientUtils(IApisClient apisClient, ILogger<ApiClientUtils> logger)
        {
            this.apisClient = apisClient;
            this.logger = logger;
        }

        public async Task<Dictionary<string, List<string>>> GetAllAPIsDictionaryByVersionSetName(ExtractorParameters extractorParameters)
        {
            // pull all apis from service
            var apis = await this.apisClient.GetAllAsync(extractorParameters, expandVersionSet: true);

            // Generate apis dictionary based on all apiversionset
            var apiDictionary = new Dictionary<string, List<string>>();

            foreach (var api in apis)
            {
                string apiVersionSetName = api.Properties.ApiVersionSet?.Name;

                if (string.IsNullOrEmpty(apiVersionSetName))
                {
                    continue;
                }

                if (!apiDictionary.ContainsKey(apiVersionSetName))
                {
                    var apiVersionSet = new List<string>
                    {
                        api.Name
                    };
                    apiDictionary[apiVersionSetName] = apiVersionSet;
                }
                else
                {
                    apiDictionary[apiVersionSetName].Add(api.Name);
                }
            }

            return apiDictionary;
        }

        public async Task<ApiTemplateResource> GetSingleApi(string apiName, ExtractorParameters extractorParameters)
        {
            var serviceApi = await this.apisClient.GetSingleAsync(apiName, extractorParameters);

            if (serviceApi is null)
            {
                throw new ServiceApiNotFoundException($"ServiceApi with name '{apiName}' not found");
            }
            return serviceApi;
        }

        public async Task<bool> DoesApiReferenceGatewayAsync(string singleApiName, string gatewayName, ExtractorParameters extractorParameters)
        {
            var gatewayApis = await this.apisClient.GetAllLinkedToGatewayAsync(gatewayName, extractorParameters);

            if (gatewayApis.IsNullOrEmpty())
            {
                this.logger.LogDebug("Did not find any api linked to the gateway");
                return false;
            }

            var serviceApi = await this.GetSingleApi(singleApiName, extractorParameters);

            return gatewayApis.Any(gatewayApi => gatewayApi.Name == serviceApi.Name);
        }
    }
}
