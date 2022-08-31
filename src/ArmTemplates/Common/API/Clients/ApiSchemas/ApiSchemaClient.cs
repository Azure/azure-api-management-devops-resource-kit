// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.ApiSchemas
{
    public class ApiSchemaClient : ApiClientBase, IApiSchemaClient
    {
        const string GetAllApiSchemasRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/schemas?api-version={5}";
        
        readonly ITemplateResourceDataProcessor<ApiSchemaTemplateResource> templateResourceDataProcessor;

        public ApiSchemaClient(IHttpClientFactory httpClientFactory, ITemplateResourceDataProcessor<ApiSchemaTemplateResource> templateResourceDataProcessor) : base(httpClientFactory)
        {
            this.templateResourceDataProcessor = templateResourceDataProcessor;
        }

        public async Task<List<ApiSchemaTemplateResource>> GetApiSchemasAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllApiSchemasRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, GlobalConstants.ApiVersion);

            var apiSchemaTemplateResources = await this.GetPagedResponseAsync<ApiSchemaTemplateResource>(azToken, requestUrl);
            this.templateResourceDataProcessor.ProcessData(apiSchemaTemplateResources);
            return apiSchemaTemplateResources;
        }
    }
}
