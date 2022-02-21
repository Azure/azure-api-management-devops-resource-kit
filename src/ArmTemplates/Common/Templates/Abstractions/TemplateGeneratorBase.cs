using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public abstract class TemplateGeneratorBase
    {
        protected Template GenerateEmptyPropertyTemplateWithParameters()
        {
            var armTemplate = this.GenerateEmptyTemplate();
            armTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                {
                    ParameterNames.ApimServiceName, new TemplateParameterProperties() { type = "string" }
                }
            };

            return armTemplate;
        }

        protected Template GenerateEmptyApiTemplateWithParameters(ExtractorParameters extractorParameters)
        {
            Template armTemplate = this.GenerateEmptyTemplate();
            armTemplate.Parameters = new Dictionary<string, TemplateParameterProperties> { { ParameterNames.ApimServiceName, new TemplateParameterProperties() { type = "string" } } };
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
            if (extractorParameters.ParameterizeServiceUrl || extractorParameters.ServiceUrlParameters != null && extractorParameters.ServiceUrlParameters.Length > 0)
            {
                TemplateParameterProperties serviceUrlParamProperty = new TemplateParameterProperties()
                {
                    type = "object"
                };
                armTemplate.Parameters.Add(ParameterNames.ServiceUrl, serviceUrlParamProperty);
            }
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

        protected Template GenerateEmptyTemplate()
        {
            // creates empty template for use in all other template creators
            Template template = new Template
            {
                Schema = "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
                ContentVersion = "1.0.0.0",
                Parameters = { },
                Variables = { },
                Resources = new TemplateResource[] { },
                Outputs = { }
            };
            return template;
        }

        protected Template GenerateTemplateWithEmptyParameters()
        {
            // creates empty parameters file for use in all other template creators
            Template template = new Template
            {
                Schema = "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
                ContentVersion = "1.0.0.0",
                Parameters = { },
            };
            return template;
        }
    }
}
