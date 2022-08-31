// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.PolicyFragments;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.PolicyFragments
{
    public class PolicyFragmentsClient : ApiClientBase, IPolicyFragmentsClient
    {
        const string GetAllRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/policyFragments?api-version={4}&format=rawxml";

        readonly ITemplateResourceDataProcessor<PolicyFragmentsResource> templateResourceDataProcessor;

        public PolicyFragmentsClient(
            IHttpClientFactory httpClientFactory,
            ITemplateResourceDataProcessor<PolicyFragmentsResource> templateResourceDataProcessor) : base(httpClientFactory)
        {
            this.templateResourceDataProcessor = templateResourceDataProcessor;
        }

        public async Task<List<PolicyFragmentsResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllRequest,
                this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersionPreview);

            var policyFragments = await this.GetPagedResponseAsync<PolicyFragmentsResource>(azToken, requestUrl);
            this.templateResourceDataProcessor.ProcessData(policyFragments);

            return policyFragments;
        }
    }
}
