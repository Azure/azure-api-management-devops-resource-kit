// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Policy
{
    public class PolicyClient : ApiClientBase, IPolicyClient
    {
        const string GetGlobalServicePolicyRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/policies/policy?api-version={4}&format=rawxml";
        const string GetPolicyLinkedToProductRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/products/{4}/policies?api-version={5}&format=rawxml";
        const string GetPolicyLinkedToApiRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/policies?api-version={5}&format=rawxml";
        const string GetPolicyLinkedToApiOperationRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/policies?api-version={6}&format=rawxml";

        public async Task<PolicyTemplateResource> GetGlobalServicePolicyAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetGlobalServicePolicyRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            return await this.GetResponseAsync<PolicyTemplateResource>(azToken, requestUrl);
        }

        public async Task<PolicyTemplateResource> GetPolicyLinkedToProductAsync(string productName, ExtractorParameters extractorParameters)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetPolicyLinkedToProductRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, productName, GlobalConstants.ApiVersion);

            var policies = await this.GetPagedResponseAsync<PolicyTemplateResource>(azToken, requestUrl);
            return policies.FirstOrDefault();
        }

        public async Task<PolicyTemplateResource> GetPolicyLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetPolicyLinkedToApiRequest,
                this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, GlobalConstants.ApiVersion);

            var policies = await this.GetPagedResponseAsync<PolicyTemplateResource>(azToken, requestUrl);
            return policies.FirstOrDefault();
        }

        public async Task<PolicyTemplateResource> GetPolicyLinkedToApiOperationAsync(string apiName, string operationName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetPolicyLinkedToApiOperationRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, operationName, GlobalConstants.ApiVersion);

            var policies = await this.GetPagedResponseAsync<PolicyTemplateResource>(azToken, requestUrl);
            return policies.FirstOrDefault();
        }
    }
}
