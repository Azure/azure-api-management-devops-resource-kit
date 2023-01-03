// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Gateway
{
    public class GatewayClient : ApiClientBase, IGatewayClient
    {
        const string GetAllGatewaysRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/gateways?api-version={4}";

        readonly ITemplateResourceDataProcessor<GatewayTemplateResource> templateResourceDataProcessor;

        public GatewayClient(
            IHttpClientFactory httpClientFactory,
            ITemplateResourceDataProcessor<GatewayTemplateResource> templateResourceDataProcessor
            ) : base(httpClientFactory)
        {
            this.templateResourceDataProcessor = templateResourceDataProcessor;
        }

        public async Task<List<GatewayTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllGatewaysRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var gatewatTemplateResources = await this.GetPagedResponseAsync<GatewayTemplateResource>(azToken, requestUrl);
            this.templateResourceDataProcessor.ProcessData(gatewatTemplateResources);
            return gatewatTemplateResources;
        }

    }
}
