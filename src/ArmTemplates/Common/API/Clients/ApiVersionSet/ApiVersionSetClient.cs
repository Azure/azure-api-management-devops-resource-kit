// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiVersionSet
{
    public class ApiVersionSetClient : ApiClientBase, IApiVersionSetClient
    {
        const string GetAllVersionSetsRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apiVersionSets?api-version={4}";

        readonly ICommonTemplateResourceDataProcessor<ApiVersionSetTemplateResource> commonTemplateResourceDataProcessor;

        public ApiVersionSetClient(IHttpClientFactory httpClientFactory, ICommonTemplateResourceDataProcessor<ApiVersionSetTemplateResource> commonTemplateResourceDataProcessor) : base(httpClientFactory)
        {
            this.commonTemplateResourceDataProcessor = commonTemplateResourceDataProcessor;
        }

        public async Task<List<ApiVersionSetTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllVersionSetsRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var apiVersionSetTemplateresources = await this.GetPagedResponseAsync<ApiVersionSetTemplateResource>(azToken, requestUrl);
            this.commonTemplateResourceDataProcessor.ProcessData(apiVersionSetTemplateresources);
            return apiVersionSetTemplateresources;
        }
    }
}
