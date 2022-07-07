// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Schemas
{
    public class SchemasClient : ApiClientBase, ISchemasClient
    {
        const string GetAllRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/schemas?api-version={4}";

        readonly ISchemaDataProcessor schemaProcessor;

        public SchemasClient(
            IHttpClientFactory httpClientFactory,
            ISchemaDataProcessor schemaDataProcessor): base(httpClientFactory)
        {
            this.schemaProcessor = schemaDataProcessor;
        }

        public async Task<List<SchemaTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllRequest,
                this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var schemaTemplates = await this.GetPagedResponseAsync<SchemaTemplateResource>(azToken, requestUrl);
            this.schemaProcessor.ProcessData(schemaTemplates, extractorParameters);

            return schemaTemplates;
        }
    }
}
