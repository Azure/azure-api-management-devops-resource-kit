using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class DiagnosticTemplateCreator
    {
        public DiagnosticTemplateResource CreateAPIDiagnosticTemplateResource(CreatorConfig creatorConfig, string[] dependsOn)
        {
            // create diagnostic resource with properties
            DiagnosticTemplateResource diagnosticTemplateResource = new DiagnosticTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}/{creatorConfig.api.diagnostic.name}')]",
                type = "Microsoft.ApiManagement/service/apis/diagnostics",
                apiVersion = "2018-06-01-preview",
                properties = new DiagnosticTemplateProperties()
                {
                    alwaysLog = creatorConfig.api.diagnostic.alwaysLog,
                    loggerId = creatorConfig.api.diagnostic.loggerId,
                    sampling = creatorConfig.api.diagnostic.sampling,
                    frontend = creatorConfig.api.diagnostic.frontend,
                    backend = creatorConfig.api.diagnostic.backend,
                    enableHttpCorrelationHeaders = creatorConfig.api.diagnostic.enableHttpCorrelationHeaders
                },
                dependsOn = dependsOn
            };
            return diagnosticTemplateResource;
        }
    }
}
