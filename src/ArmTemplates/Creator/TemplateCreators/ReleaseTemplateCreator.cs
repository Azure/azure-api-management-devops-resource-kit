// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class ReleaseTemplateCreator
    {
        public ReleaseTemplateResource CreateAPIReleaseTemplateResource(APIConfig api, string[] dependsOn)
        {
            string releaseName = $"release-revision-{api.apiRevision}";
            // create release resource with properties
            ReleaseTemplateResource releaseTemplateResource = new ReleaseTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}/{releaseName}')]",
                Type = ResourceTypeConstants.APIRelease,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new ReleaseTemplateProperties()
                {
                    notes = $"Release created to make revision {api.apiRevision} current.",
                    apiId = $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{api.name}')]"
                },
                DependsOn = dependsOn
            };
            return releaseTemplateResource;
        }
    }
}
