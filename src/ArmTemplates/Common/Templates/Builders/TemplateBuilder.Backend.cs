// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders
{
    public partial class TemplateBuilder : ITemplateBuilder
    {
        public TemplateBuilder AddParameterizeBackendSettings(ExtractorParameters extractorParameters)
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

        public TemplateBuilder AddParameterizeBackendProxySection(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeBackendProxySection)
            {
                var backendProxySectionParameterObject = new TemplateParameterProperties<BackendProxySectionParameter>
                {
                    Type = "object",
                    Metadata = new() { Description = "proxy settings for backend resource (view https://docs.microsoft.com/en-us/rest/api/apimanagement/current-ga/backend/create-or-update)" },
                    Value = new BackendProxySectionParameter
                    {
                        Url = "preset-proxy-url-value",
                        Username = "preset-proxy-username-value",
                        Password = "preset-proxy-password-value"
                    }
                };

                this.template.Parameters.Add(ParameterNames.BackendProxy, backendProxySectionParameterObject);
            }

            return this;
        }
    }
}
