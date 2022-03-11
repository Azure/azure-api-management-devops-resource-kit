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

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ApiVersionSetExtractor : EntityExtractorBase, IApiVersionSetExtractor
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

        public async Task<Template> GenerateApiVersionSetTemplateAsync(
            string singleApiName, 
            List<TemplateResource> apiTemplateResources, 
            ExtractorParameters extractorParameters)
        {
            var armTemplate = this.templateBuilder.GenerateTemplateWithApimServiceNameProperty().Build();

            // isolate api version set associations in the case of a single api extraction
            var apiResources = apiTemplateResources?.Where(resource => resource.Type == ResourceTypeConstants.API);
            var templateResources = new List<TemplateResource>();

            var apiVersionSets = await this.apiVersionSetClient.GetAllAsync(extractorParameters);
            foreach (var apiVersionSet in apiVersionSets)
            {
                // only extract the product if this is a full extraction, or in the case of a single api, if it is found in products associated with the api
                if (singleApiName == null || apiResources.SingleOrDefault(api => 
                        (api as APITemplateResource).Properties.ApiVersionSetId != null && 
                        (api as APITemplateResource).Properties.ApiVersionSetId.Contains(apiVersionSet.Name)) != null)
                {
                    this.logger.LogDebug("Found '{0}' api-version-set", apiVersionSet.Name);

                    apiVersionSet.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{apiVersionSet.Name}')]";
                    apiVersionSet.Type = ResourceTypeConstants.ApiVersionSet;
                    apiVersionSet.ApiVersion = GlobalConstants.ApiVersion;

                    templateResources.Add(apiVersionSet);
                }
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
