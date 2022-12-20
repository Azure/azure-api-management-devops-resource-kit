// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils
{
    public class ApiClientUtils: IApiClientUtils
    {
        readonly IApisClient apisClient;
        
        public ApiClientUtils(IApisClient apisClient)
        {
            this.apisClient = apisClient;
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
    }
}
