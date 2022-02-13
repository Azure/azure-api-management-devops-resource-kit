﻿using apimtemplate.Common.Constants;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Creator.Models;

namespace apimtemplate.Creator.TemplateCreators
{
    public class ReleaseTemplateCreator
    {
        public ReleaseTemplateResource CreateAPIReleaseTemplateResource(APIConfig api, string[] dependsOn)
        {
            string releaseName = $"release-revision-{api.apiRevision}";
            // create release resource with properties
            ReleaseTemplateResource releaseTemplateResource = new ReleaseTemplateResource()
            {
                name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}/{releaseName}')]",
                type = ResourceTypeConstants.APIRelease,
                apiVersion = GlobalConstants.APIVersion,
                properties = new ReleaseTemplateProperties()
                {
                    notes = $"Release created to make revision {api.apiRevision} current.",
                    apiId = $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{api.name}')]"
                },
                dependsOn = dependsOn
            };
            return releaseTemplateResource;
        }
    }
}
