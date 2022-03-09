// --------------------------------------------------------------------------
//  <copyright file="TagClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Tags.Responses;
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

        readonly IApisClient apisClient;

        public TagClient(IApisClient apisClient)
        {
            this.apisClient = apisClient;
        }

        public async Task<string> GetAllAsync(ExtractorParameters extractorParameters, int skipNumOfRecords = 0)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllTagsRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, skipNumOfRecords, GlobalConstants.ApiVersion);

            return await this.CallApiManagementAsync(azToken, requestUrl);
        }

        public async Task<List<TagTemplateResource>> GetAllTagsLinkedToApiAsync(ExtractorParameters extractorParameters, string apiName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllTagsLinkedToApiRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, GlobalConstants.ApiVersion);

            var response = await this.CallApiManagementAsync<GetTagsResponse>(azToken, requestUrl);
            return response.Tags;
        }

        public async Task<List<TagTemplateResource>> GetAllTagsLinkedToProductAsync(ExtractorParameters extractorParameters, string productName)
        {
            var apisLinkedToProduct = await this.apisClient.GetAllLinkedToProductAsync(extractorParameters, productName);

            var tagTemplateResources = new List<TagTemplateResource>();
            foreach (var productApi in apisLinkedToProduct)
            {
                var apiTags = await this.GetAllTagsLinkedToApiAsync(extractorParameters, productApi.Name);

                if (!apiTags.IsNullOrEmpty())
                {
                    tagTemplateResources.AddRange(apiTags);
                }
            }

            return tagTemplateResources;
        }
    }
}
