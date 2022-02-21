// --------------------------------------------------------------------------
//  <copyright file="PolicyApiClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients
{
    public class PolicyApiClient : ApiClientBase, IPolicyApiClient
    {
        public PolicyApiClient(string baseUrl = null) : base(baseUrl)
        {
        }

        public async Task<PolicyTemplateResource> GetGlobalServicePolicyAsync(string apiManagementName, string resourceGroupName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/policies/policy?api-version={4}&format=rawxml",
               this.BaseUrl, azSubId, resourceGroupName, apiManagementName, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync<PolicyTemplateResource>(azToken, requestUrl);
        }
    }
}
