// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiVersionSetExtractor : IApiVersionSetExtractor
    {
        readonly ILogger<ApiVersionSetExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        readonly IApiVersionSetClient apiVersionSetClient;

        public ApiVersionSetExtractor(
            ILogger<ApiVersionSetExtractor> logger,
            ITemplateBuilder templateBuilder,
            IApiVersionSetClient apiVersionSetClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;

            this.apiVersionSetClient = apiVersionSetClient;
        }

        public async Task<Template<ApiVersionSetTemplateResources>> GenerateApiVersionSetTemplateAsync(
            string singleApiName,
            List<ApiTemplateResource> apiTemplateResources,
            ExtractorParameters extractorParameters)
        {
            var apiVersionSetTemplate = this.templateBuilder
                                            .GenerateTemplateWithApimServiceNameProperty()
                                            .Build<ApiVersionSetTemplateResources>();
            
            var apiVersionSets = await this.apiVersionSetClient.GetAllAsync(extractorParameters);
            foreach (var apiVersionSet in apiVersionSets)
            {
                // only extract the product if this is a full extraction, or in the case of a single api, if it is found in products associated with the api
                if (string.IsNullOrEmpty(singleApiName) || apiTemplateResources.Any(api => 
                        api.Properties.ApiVersionSetId != null && 
                        api.Properties.ApiVersionSetId.Contains(apiVersionSet.Name)))
                {
                    this.logger.LogDebug("Found '{0}' api-version-set", apiVersionSet.Name);

                    apiVersionSet.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiVersionSet.Name}')]";
                    apiVersionSet.Type = ResourceTypeConstants.ApiVersionSet;
                    apiVersionSet.ApiVersion = GlobalConstants.ApiVersion;

                    apiVersionSetTemplate.TypedResources.ApiVersionSets.Add(apiVersionSet);
                }
            }

            return apiVersionSetTemplate;
        }
    }
}
