// --------------------------------------------------------------------------
//  <copyright file="TemplateBuilderExtensions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders
{
    static class TemplateBuilderExtensions
    {
        public static Template AddPolicyProperties(this Template armTemplate, ExtractorParameters extractorParameters)
        {
            if (extractorParameters.PolicyXMLBaseUrl != null && extractorParameters.PolicyXMLSasToken != null)
            {
                TemplateParameterProperties policyTemplateSasTokenParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };

                armTemplate.Parameters.Add(ParameterNames.PolicyXMLSasToken, policyTemplateSasTokenParameterProperties);
            }

            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };
                armTemplate.Parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlParameterProperties);
            }

            return armTemplate;
        }

        public static Template AddParameterizeServiceUrlProperty(this Template armTemplate, ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeServiceUrl || extractorParameters.ServiceUrlParameters != null && extractorParameters.ServiceUrlParameters.Length > 0)
            {
                TemplateParameterProperties serviceUrlParamProperty = new TemplateParameterProperties()
                {
                    type = "object"
                };
                armTemplate.Parameters.Add(ParameterNames.ServiceUrl, serviceUrlParamProperty);
            }

            return armTemplate;
        }

        public static Template AddParameterizeApiLoggerIdProperty(this Template armTemplate, ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                TemplateParameterProperties apiLoggerProperty = new TemplateParameterProperties()
                {
                    type = "object"
                };
                armTemplate.Parameters.Add(ParameterNames.ApiLoggerId, apiLoggerProperty);
            }

            return armTemplate;
        }
    }
}
