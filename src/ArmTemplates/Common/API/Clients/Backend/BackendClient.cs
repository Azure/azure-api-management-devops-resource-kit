// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Backend
{
    public class BackendClient : ApiClientBase, IBackendClient
    {
        const string GetAllBackendsRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/backends?api-version={4}";

        readonly ICommonTemplateResourceDataProcessor<BackendTemplateResource> commonTemplateResourceDataProcessor;

        public BackendClient(IHttpClientFactory httpClientFactory, ICommonTemplateResourceDataProcessor<BackendTemplateResource> commonTemplateResourceDataProcessor) : base(httpClientFactory)
        {
            this.commonTemplateResourceDataProcessor = commonTemplateResourceDataProcessor;
        }

        public async Task<List<BackendTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllBackendsRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var backendTemplateResources = await this.GetPagedResponseAsync<BackendTemplateResource>(azToken, requestUrl);
            this.commonTemplateResourceDataProcessor.ProcessData(backendTemplateResources);

            return backendTemplateResources;
        }
    }
}
