// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders
{
    public partial class TemplateBuilder : ITemplateBuilder
    {
        public TemplateBuilder AddParameterizedBackendSettings(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeBackend)
            {
                TemplateParameterProperties extractBackendParametersProperties = new TemplateParameterProperties()
                {
                    Type = "object"
                };
                this.template.Parameters.Add(ParameterNames.BackendSettings, extractBackendParametersProperties);
            }

            return this;
        }
    }
}
