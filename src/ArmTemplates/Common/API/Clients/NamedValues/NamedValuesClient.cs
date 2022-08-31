// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.NamedValues
{
    public class NamedValuesClient : ApiClientBase, INamedValuesClient
    {
        const string GetNamedValuesRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/namedValues?api-version={4}";
        readonly ITemplateResourceDataProcessor<NamedValueTemplateResource> templateResourceDataProcessor;

        public NamedValuesClient(IHttpClientFactory httpClientFactory, ITemplateResourceDataProcessor<NamedValueTemplateResource> templateResourceDataProcessor) : base(httpClientFactory)
        {
            this.templateResourceDataProcessor = templateResourceDataProcessor;
        }

        public async Task<List<NamedValueTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetNamedValuesRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var namedValuesTemplateResources = await this.GetPagedResponseAsync<NamedValueTemplateResource>(azToken, requestUrl);
            this.templateResourceDataProcessor.ProcessData(namedValuesTemplateResources);
            return namedValuesTemplateResources;
        }
    }
}
