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
    public class DiagnosticTemplateCreator : IDiagnosticTemplateCreator
    {
        public DiagnosticTemplateResource CreateAPIDiagnosticTemplateResource(ApiConfig api, string[] dependsOn)
        {
            // create diagnostic resource with properties
            DiagnosticTemplateResource diagnosticTemplateResource = new DiagnosticTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{api.Name}/{api.Diagnostic.Name}')]",
                Type = ResourceTypeConstants.APIDiagnostic,
                ApiVersion = GlobalConstants.ApiVersion,
                Properties = new DiagnosticTemplateProperties()
                {
                    AlwaysLog = api.Diagnostic.AlwaysLog,
                    Sampling = api.Diagnostic.Sampling,
                    Frontend = api.Diagnostic.Frontend,
                    Backend = api.Diagnostic.Backend,
                    EnableHttpCorrelationHeaders = api.Diagnostic.EnableHttpCorrelationHeaders
                },
                DependsOn = dependsOn
            };
            // reference the provided logger if loggerId is provided
            if (api.Diagnostic.LoggerId != null)
            {
                diagnosticTemplateResource.Properties.LoggerId = $"[resourceId('Microsoft.ApiManagement/service/loggers', parameters('{ParameterNames.ApimServiceName}'), '{api.Diagnostic.LoggerId}')]";
            }
            return diagnosticTemplateResource;
        }
    }
}
