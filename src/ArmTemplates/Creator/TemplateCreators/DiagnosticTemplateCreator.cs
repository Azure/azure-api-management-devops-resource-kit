// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

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
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.name}/{api.diagnostic.name}')]",
                Type = ResourceTypeConstants.APIDiagnostic,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new DiagnosticTemplateProperties()
                {
                    AlwaysLog = api.diagnostic.AlwaysLog,
                    Sampling = api.diagnostic.Sampling,
                    Frontend = api.diagnostic.Frontend,
                    Backend = api.diagnostic.Backend,
                    EnableHttpCorrelationHeaders = api.diagnostic.EnableHttpCorrelationHeaders
                },
                DependsOn = dependsOn
            };
            // reference the provided logger if loggerId is provided
            if (api.diagnostic.LoggerId != null)
            {
                diagnosticTemplateResource.Properties.LoggerId = $"[resourceId('Microsoft.ApiManagement/service/loggers', parameters('{ParameterNames.ApimServiceName}'), '{api.diagnostic.LoggerId}')]";
            }
            return diagnosticTemplateResource;
        }
    }
}
