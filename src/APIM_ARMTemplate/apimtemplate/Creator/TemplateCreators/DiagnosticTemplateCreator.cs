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
                properties = creatorConfig.api.diagnostic,
                dependsOn = dependsOn
            };
            return diagnosticTemplateResource;
        }
    }
}
