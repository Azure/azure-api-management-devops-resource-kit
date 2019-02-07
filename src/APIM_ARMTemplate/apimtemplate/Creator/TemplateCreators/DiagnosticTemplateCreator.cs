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
                name = $"[concat(parameters('ApimServiceName'), '/{creatorConfig.api.name}/diagnostic')]",
                type = "Microsoft.ApiManagement/service/apis/diagnostics",
                apiVersion = "2018-01-01",
                properties = creatorConfig.diagnostic,
                dependsOn = dependsOn
            };
            return diagnosticTemplateResource;
        }
    }
}
