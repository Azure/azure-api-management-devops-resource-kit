// --------------------------------------------------------------------------
//  <copyright file="GroupsClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Groups.Responses;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Groups
{
    public class GroupsClient : ApiClientBase, IGroupsClient
    {
        const string GetAllGroupsLinkedToProductRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/products/{4}/groups?api-version={5}";

        public async Task<List<GroupTemplateResource>> GetAllLinkedToProductAsync(ExtractorParameters extractorParameters, string productName)
        {
            (string azToken, string azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllGroupsLinkedToProductRequest,
                this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, productName, GlobalConstants.ApiVersion);

            var response = await this.CallApiManagementAsync<GetAllGroupsLinkedToProductResponse>(azToken, requestUrl);
            return response.Groups;
        }
    }
}
