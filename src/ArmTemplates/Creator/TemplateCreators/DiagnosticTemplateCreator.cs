using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
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
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}/{api.diagnostic.name}')]",
                Type = ResourceTypeConstants.APIDiagnostic,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new DiagnosticTemplateProperties()
                {
                    alwaysLog = api.diagnostic.alwaysLog,
                    sampling = api.diagnostic.sampling,
                    frontend = api.diagnostic.frontend,
                    backend = api.diagnostic.backend,
                    enableHttpCorrelationHeaders = api.diagnostic.enableHttpCorrelationHeaders
                },
                DependsOn = dependsOn
            };
            // reference the provided logger if loggerId is provided
            if (api.diagnostic.loggerId != null)
            {
                diagnosticTemplateResource.Properties.loggerId = $"[resourceId('Microsoft.ApiManagement/service/loggers', parameters('{ParameterNames.ApimServiceName}'), '{api.diagnostic.loggerId}')]";
            }
            return diagnosticTemplateResource;
        }
    }
}
