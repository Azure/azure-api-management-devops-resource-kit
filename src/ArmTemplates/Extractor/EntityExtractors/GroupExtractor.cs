// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
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

        public async Task<Template<GroupTemplateResources>> GenerateGroupsTemplateAsync(ExtractorParameters extractorParameters)
        {
            var groupsTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<GroupTemplateResources>();

            var allGroups = await this.groupsClient.GetAllAsync(extractorParameters);
            if (allGroups.IsNullOrEmpty())
            {
                this.logger.LogWarning($"No groups were found for '{extractorParameters.SourceApimName}' at '{extractorParameters.ResourceGroup}'");
                return groupsTemplate;
            }

            foreach (var extractedGroup in allGroups)
            {
                extractedGroup.Type = ResourceTypeConstants.Group;
                extractedGroup.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{extractedGroup.Name}')]";
                extractedGroup.ApiVersion = GlobalConstants.ApiVersion;

                groupsTemplate.TypedResources.Groups.Add(extractedGroup);
            }

            return groupsTemplate;
        }
    }
}
