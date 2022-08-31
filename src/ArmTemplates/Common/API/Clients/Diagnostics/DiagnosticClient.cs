// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Diagnostics
{
    public class DiagnosticClient : ApiClientBase, IDiagnosticClient
    {
        const string GetAllDiagnosticsRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/diagnostics?api-version={4}";
        const string GetDiagnosticsLinkedToApiRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/apis/{4}/diagnostics?api-version={5}";

        readonly ITemplateResourceDataProcessor<DiagnosticTemplateResource> templateResourceDataProcessor;

        public DiagnosticClient(IHttpClientFactory httpClientFactory, ITemplateResourceDataProcessor<DiagnosticTemplateResource> templateResourceDataProcessor) : base(httpClientFactory)
        {
            this.templateResourceDataProcessor = templateResourceDataProcessor;
        }

        public async Task<List<DiagnosticTemplateResource>> GetApiDiagnosticsAsync(string apiName, ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetDiagnosticsLinkedToApiRequest,
                this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, apiName, GlobalConstants.ApiVersion);

            var diagnosticTemplateResources = await this.GetPagedResponseAsync<DiagnosticTemplateResource>(azToken, requestUrl);
            this.templateResourceDataProcessor.ProcessData(diagnosticTemplateResources);
            return diagnosticTemplateResources;
        }

        public async Task<List<DiagnosticTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            var requestUrl = string.Format(GetAllDiagnosticsRequest,
                this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var diagnosticTemplateResources = await this.GetPagedResponseAsync<DiagnosticTemplateResource>(azToken, requestUrl);
            this.templateResourceDataProcessor.ProcessData(diagnosticTemplateResources);
            return diagnosticTemplateResources;
        }
    }
}
