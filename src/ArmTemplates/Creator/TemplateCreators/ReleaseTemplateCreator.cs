// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class ReleaseTemplateCreator : IReleaseTemplateCreator
    {
        public ReleaseTemplateResource CreateAPIReleaseTemplateResource(ApiConfig api, string[] dependsOn)
        {
            string releaseName = $"release-revision-{api.ApiRevision}";
            // create release resource with properties
            ReleaseTemplateResource releaseTemplateResource = new ReleaseTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}/{releaseName}')]",
                Type = ResourceTypeConstants.APIRelease,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new ReleaseTemplateProperties()
                {
                    notes = $"Release created to make revision {api.ApiRevision} current.",
                    apiId = $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{api.Name}')]"
                },
                DependsOn = dependsOn
            };
            return releaseTemplateResource;
        }
    }
}
