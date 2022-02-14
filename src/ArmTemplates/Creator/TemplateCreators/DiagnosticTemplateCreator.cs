using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class DiagnosticTemplateCreator
    {
        public DiagnosticTemplateResource CreateAPIDiagnosticTemplateResource(APIConfig api, string[] dependsOn)
        {
            // create diagnostic resource with properties
            DiagnosticTemplateResource diagnosticTemplateResource = new DiagnosticTemplateResource()
            {
                name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}/{api.diagnostic.name}')]",
                type = ResourceTypeConstants.APIDiagnostic,
                apiVersion = GlobalConstants.APIVersion,
                properties = new DiagnosticTemplateProperties()
                {
                    alwaysLog = api.diagnostic.alwaysLog,
                    sampling = api.diagnostic.sampling,
                    frontend = api.diagnostic.frontend,
                    backend = api.diagnostic.backend,
                    enableHttpCorrelationHeaders = api.diagnostic.enableHttpCorrelationHeaders
                },
                dependsOn = dependsOn
            };
            // reference the provided logger if loggerId is provided
            if (api.diagnostic.loggerId != null)
            {
                diagnosticTemplateResource.properties.loggerId = $"[resourceId('Microsoft.ApiManagement/service/loggers', parameters('{ParameterNames.ApimServiceName}'), '{api.diagnostic.loggerId}')]";
            }
            return diagnosticTemplateResource;
        }
    }
}
