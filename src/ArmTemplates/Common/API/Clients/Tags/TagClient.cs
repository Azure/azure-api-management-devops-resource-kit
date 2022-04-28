// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Tags
{
    public class TagClient : ApiClientBase, ITagClient
    {
        const string GetAllTagsLinkedToApiRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/tags?api-version={5}&format=rawxml";
        const string GetAllTagsRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/tags?$skip={4}&api-version={5}";
        const string GetTagsLinkedToApiOperationRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/operations/{5}/tags?api-version={6}&format=rawxml";

        readonly IApisClient apisClient;

        public TagClient(IApisClient apisClient)
        {
            this.apisClient = apisClient;
        }

        public async Task<List<TagTemplateResource>> GetTagsLinkedToApiOperationAsync(string apiName, string operationName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetTagsLinkedToApiOperationRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, operationName, GlobalConstants.ApiVersion);

            return await this.GetPagedResponseAsync<TagTemplateResource>(azToken, requestUrl);
        }

        public async Task<List<TagTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters, int skipAmountOfRecords = 0)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllTagsRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, skipAmountOfRecords, GlobalConstants.ApiVersion);

            return await this.GetPagedResponseAsync<TagTemplateResource>(azToken, requestUrl);
        }

        public async Task<List<TagTemplateResource>> GetAllTagsLinkedToApiAsync(string apiName, ExtractorParameters extractorParameters)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllTagsLinkedToApiRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, GlobalConstants.ApiVersion);

            return await this.GetPagedResponseAsync<TagTemplateResource>(azToken, requestUrl);
        }

        public async Task<List<TagTemplateResource>> GetAllTagsLinkedToProductAsync(string productName, ExtractorParameters extractorParameters)
        {
            var apisLinkedToProduct = await this.apisClient.GetAllLinkedToProductAsync(productName, extractorParameters);

            var tagTemplateResources = new List<TagTemplateResource>();
            foreach (var productApi in apisLinkedToProduct)
            {
                var apiTags = await this.GetAllTagsLinkedToApiAsync(productApi.Name, extractorParameters);

                if (!apiTags.IsNullOrEmpty())
                {
                    tagTemplateResources.AddRange(apiTags);
                }
            }

            return tagTemplateResources;
        }
    }
}
