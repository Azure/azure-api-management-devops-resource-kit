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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.NamedValues
{
    public class NamedValuesClient : ApiClientBase, INamedValuesClient
    {
        const string GetNamedValuesRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/namedValues?api-version={4}";
        const string GetNamedValueSecretValueRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/namedValues/{4}/listValue?api-version={5}";
        readonly INamedValuesDataProcessor namedValuesDataProcessor;

        public NamedValuesClient(IHttpClientFactory httpClientFactory, INamedValuesDataProcessor namedValuesDataProcessor) : base(httpClientFactory)
        {
            this.namedValuesDataProcessor = namedValuesDataProcessor;
        }

        public async Task<List<NamedValueTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetNamedValuesRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var namedValuesTemplateResources = await this.GetPagedResponseAsync<NamedValueTemplateResource>(azToken, requestUrl);
            this.namedValuesDataProcessor.ProcessData(namedValuesTemplateResources);
            await this.FetchSecretsValue(namedValuesTemplateResources, extractorParameters);
            return namedValuesTemplateResources;
        }

        public async Task<NamedValuesSecretValue> ListNamedValueSecretValueAsync(string namedValueId, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetNamedValueSecretValueRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, namedValueId, GlobalConstants.ApiVersion);

            return await this.GetResponseAsync<NamedValuesSecretValue>(azToken, requestUrl, useCache: false, method: ClientHttpMethod.POST);
        }

        async Task FetchSecretsValue(List<NamedValueTemplateResource> namedValues, ExtractorParameters extractorParameters)
        {
            if (namedValues == null || namedValues.Count== 0)
            {
                return ;
            }

            if (!extractorParameters.ExtractSecrets)
            {
                return ;
            }

            foreach(var namedValue in namedValues)
            {
                if (namedValue.Properties.Secret && namedValue.Properties.KeyVault == null)
                {
                    var namaedValueSecretData = await this.ListNamedValueSecretValueAsync(namedValue.OriginalName, extractorParameters);
                    namedValue.Properties.OriginalValue = namaedValueSecretData?.Value;
                    namedValue.Properties.Value = namaedValueSecretData?.Value;
                }
            }
        }
    }
}
