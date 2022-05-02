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
        public TemplateBuilder AddParameterizeNamedValueParameters(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeNamedValue)
            {
                TemplateParameterProperties namedValueParameterProperties = new TemplateParameterProperties()
                {
                    Type = "object"
                };
                this.template.Parameters.Add(ParameterNames.NamedValues, namedValueParameterProperties);
            }

            return this;
        }

        public TemplateBuilder AddParameterizeNamedValuesKeyVaultSecretParameters(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParamNamedValuesKeyVaultSecrets)
            {
                TemplateParameterProperties keyVaultNamedValueParameterProperties = new TemplateParameterProperties()
                {
                    Type = "object"
                };
                this.template.Parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, keyVaultNamedValueParameterProperties);
            }

            return this;
        }
    }
}
