// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Loggers
{
    public class LoggerClient : ApiClientBase, ILoggerClient
    {
        const string GetAllLoggersRequest = "{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ApiManagement/service/{3}/loggers?api-version={4}";

        readonly ITemplateResourceDataProcessor<LoggerTemplateResource> templateResourceDataProcessor;
        public LoggerClient(IHttpClientFactory httpClientFactory, ITemplateResourceDataProcessor<LoggerTemplateResource> templateResourceDataProcessor) : base(httpClientFactory)
        {
            this.templateResourceDataProcessor = templateResourceDataProcessor;
        }

        public async Task<List<LoggerTemplateResource>> GetAllAsync(ExtractorParameters extractorParameters)
        {
            var (azToken, azSubId) = await this.Auth.GetAccessToken();

            string requestUrl = string.Format(GetAllLoggersRequest,
               this.BaseUrl, azSubId, extractorParameters.ResourceGroup, extractorParameters.SourceApimName, GlobalConstants.ApiVersion);

            var loggerTemplateResources = await this.GetPagedResponseAsync<LoggerTemplateResource>(azToken, requestUrl);
            this.templateResourceDataProcessor.ProcessData(loggerTemplateResources);
            return loggerTemplateResources;
        }
    }
}
