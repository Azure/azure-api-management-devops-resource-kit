using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class ReleaseTemplateCreator
    {
        public ReleaseTemplateResource CreateAPIReleaseTemplateResource(APIConfig api, string[] dependsOn)
        {
            string releaseName = $"release-revision-{api.apiRevision}";
            // create release resource with properties
            ReleaseTemplateResource releaseTemplateResource = new ReleaseTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{api.name}/{releaseName}')]",
                type = ResourceTypeConstants.APIRelease,
                apiVersion = GlobalConstants.APIVersion,
                properties = new ReleaseTemplateProperties()
                {
                    notes = $"Release created to make revision {api.apiRevision} current.",
                    apiId = $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('ApimServiceName'), '{api.name}')]"
                },
                dependsOn = dependsOn
            };
            return releaseTemplateResource;
        }
    }
}
