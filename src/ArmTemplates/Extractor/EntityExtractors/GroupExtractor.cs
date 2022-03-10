using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.Authorization.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class GroupExtractor : IGroupExtractor
    {
        readonly ILogger<GroupExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        readonly IGroupsClient groupsClient;

        public GroupExtractor(
            ILogger<GroupExtractor> logger,
            ITemplateBuilder templateBuilder,
            IGroupsClient groupsClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;

            this.groupsClient = groupsClient;
        }

        public async Task<Template> GenerateGroupsTemplateAsync(ExtractorParameters extractorParameters)
        {
            Template armTemplate = this.templateBuilder.GenerateTemplateWithApimServiceNameProperty().Build();
            var templateResources = new List<GroupTemplateResource>();

            var allGroups = await this.groupsClient.GetAllAsync(extractorParameters);
            if (allGroups.IsNullOrEmpty())
            {
                this.logger.LogWarning($"No groups were found for '{extractorParameters.SourceApimName}' at '{extractorParameters.ResourceGroup}'");
                return armTemplate;
            }

            foreach (var extractedGroup in allGroups)
            {
                extractedGroup.Type = ResourceTypeConstants.Group;
                extractedGroup.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{extractedGroup.Name}')]";
                extractedGroup.ApiVersion = GlobalConstants.ApiVersion;
                extractedGroup.Scale = null;

                templateResources.Add(extractedGroup);
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
