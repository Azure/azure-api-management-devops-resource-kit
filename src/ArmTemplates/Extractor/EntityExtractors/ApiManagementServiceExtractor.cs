// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiManagementServiceExtractor: IApiManagementServiceExtractor
    {
        readonly ILogger<ApiManagementServiceExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        readonly IApiManagementServiceClient apiManagementServiceClient;

        public ApiManagementServiceExtractor(ILogger<ApiManagementServiceExtractor> logger, ITemplateBuilder templateBuilder, IApiManagementServiceClient apiManagementServiceClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.apiManagementServiceClient = apiManagementServiceClient;
        }

        public async Task<Template<ApiManagementServiceResources>> GenerateApiManagementServicesTemplateAsync(ExtractorParameters extractorParameters)
        {
            var apiManagementServiceTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<ApiManagementServiceResources>();

            var apiManagementService = await this.apiManagementServiceClient.GetApiManagementServiceAsync(extractorParameters);

            if (apiManagementService is null)
            {
                this.logger.LogWarning($"Api Management service '{extractorParameters.SourceApimName}' not found at '{extractorParameters.ResourceGroup}'");
                return apiManagementServiceTemplate;
            }

            apiManagementService.Type = ResourceTypeConstants.ApiManagementService;
            apiManagementService.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'))]";
            apiManagementService.ApiVersion = GlobalConstants.ApiVersion;

            apiManagementServiceTemplate.TypedResources.ApiManagementServices.Add(apiManagementService);

            return apiManagementServiceTemplate;
        }
    }
}
