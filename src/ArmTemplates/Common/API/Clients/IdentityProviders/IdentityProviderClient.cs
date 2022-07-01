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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.IdentityProviders
{
    public class IdentityProviderClient : ApiClientBase, IIdentityProviderClient
    {
        const string GetAllIdentityProvidersRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/identityProviders?api-version={4}";
        const string ListIdentyProviderSecret = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/identityProviders/{4}/listSecrets?api-version={5}";

        readonly IIdentityProviderProcessor identityProviderProcessor;

        public IdentityProviderClient(
            IHttpClientFactory httpClientFactory, 
            IIdentityProviderProcessor identityProviderProcessor): base(httpClientFactory)
        {
            this.identityProviderProcessor = identityProviderProcessor;
        }

        public async Task<List<IdentityProviderResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllIdentityProvidersRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var identityProviderTemplates = await this.GetPagedResponseAsync<IdentityProviderResource>(azToken, requestUrl);
            this.identityProviderProcessor.ProcessData(identityProviderTemplates, extractorParameters);

            return identityProviderTemplates;
        }

        public async Task<IdentityProviderSecret> ListIdentityProviderSecrets(string identityProviderName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(ListIdentyProviderSecret,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, identityProviderName, GlobalConstants.ApiVersion);

            return await this.GetResponseAsync<IdentityProviderSecret>(azToken, requestUrl, useCache: false, method: ClientHttpMethod.POST);
        }
    }
}
