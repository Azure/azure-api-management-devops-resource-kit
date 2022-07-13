// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.OpenIdConnectProviders
{
    public class OpenIdConnectProviderClient : ApiClientBase, IOpenIdConnectProvidersClient
    {
        const string GetAllOpenIdConnectProvidersProvidersRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/openidConnectProviders?api-version={4}";
        const string ListOpenIdConnectProviderSecret = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/openidConnectProviders/{4}/listSecrets?api-version={5}";

        readonly IOpenIdConnectProviderProcessor openIdConnectProviderProcessor;

        public OpenIdConnectProviderClient(
            IHttpClientFactory httpClientFactory,
            IOpenIdConnectProviderProcessor openIdConnectProviderProcessor) : base(httpClientFactory)
        {
            this.openIdConnectProviderProcessor = openIdConnectProviderProcessor;
        }

        public async Task<List<OpenIdConnectProviderResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllOpenIdConnectProvidersProvidersRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var openIdConnectProviderResources = await this.GetPagedResponseAsync<OpenIdConnectProviderResource>(azToken, requestUrl);
            this.openIdConnectProviderProcessor.ProcessData(openIdConnectProviderResources, extractorParameters);

            return openIdConnectProviderResources;
        }

        public async Task<OpenIdConnectProviderSecret> ListOpenIdConnectProviderSecretsAsync(string openIdConnectProviderName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(ListOpenIdConnectProviderSecret,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, openIdConnectProviderName, GlobalConstants.ApiVersion);

            return await this.GetResponseAsync<OpenIdConnectProviderSecret>(azToken, requestUrl, useCache: false, method: ClientHttpMethod.POST);
        }
    }
}
