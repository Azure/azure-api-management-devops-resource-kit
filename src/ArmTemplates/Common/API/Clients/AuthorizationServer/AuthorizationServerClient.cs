// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.AuthorizationServer
{
    public class AuthorizationServerClient : ApiClientBase, IAuthorizationServerClient
    {
        const string GetAllAuthorizationServersRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/authorizationServers?api-version={4}";

        readonly ICommonTemplateResourceDataProcessor<AuthorizationServerTemplateResource> commonTemplateResourceDataProcessor;

        public AuthorizationServerClient(IHttpClientFactory httpClientFactory, ICommonTemplateResourceDataProcessor<AuthorizationServerTemplateResource> commonTemplateResourceDataProcessor) : base(httpClientFactory)
        {
            this.commonTemplateResourceDataProcessor = commonTemplateResourceDataProcessor;
        }

        public async Task<List<AuthorizationServerTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllAuthorizationServersRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var authServerTemplateResources = await this.GetPagedResponseAsync<AuthorizationServerTemplateResource>(azToken, requestUrl);
            this.commonTemplateResourceDataProcessor.ProcessData(authServerTemplateResources);
            return authServerTemplateResources;
        }
    }
}
