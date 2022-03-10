// --------------------------------------------------------------------------
//  <copyright file="TemplateBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders
{
    public class TemplateBuilder : ITemplateBuilder
    {
        Template template;

        public Template Build() => this.template;

        public TemplateBuilder GenerateTemplateWithApimServiceNameProperty()
        {
            this.GenerateEmptyTemplate();
            this.template.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                {
                    ParameterNames.ApimServiceName, new TemplateParameterProperties() { type = "string" }
                }
            };

            return this;
        }

        public TemplateBuilder GenerateTemplateWithPresetProperties(ExtractorParameters extractorParameters)
            => this.GenerateTemplateWithApimServiceNameProperty()
                   .AddPolicyProperties(extractorParameters)
                   .AddParameterizeServiceUrlProperty(extractorParameters)
                   .AddParameterizeApiLoggerIdProperty(extractorParameters);

        public TemplateBuilder GenerateEmptyTemplate()
        {
            // creates empty template for use in all other template creators
            this.template = new Template
            {
                Schema = "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
                ContentVersion = "1.0.0.0",
                Parameters = { },
                Variables = { },
                Resources = new TemplateResource[] { },
                Outputs = { }
            };

            return this;
        }

        public TemplateBuilder AddPolicyProperties(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.PolicyXMLBaseUrl != null && extractorParameters.PolicyXMLSasToken != null)
            {
                TemplateParameterProperties policyTemplateSasTokenParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };

                this.template.Parameters.Add(ParameterNames.PolicyXMLSasToken, policyTemplateSasTokenParameterProperties);
            }

            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlParameterProperties = new TemplateParameterProperties()
                {
                    type = "string"
                };
                this.template.Parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlParameterProperties);
            }

            return this;
        }

        public TemplateBuilder AddParameterizeServiceUrlProperty(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeServiceUrl || extractorParameters.ServiceUrlParameters != null && extractorParameters.ServiceUrlParameters.Length > 0)
            {
                TemplateParameterProperties serviceUrlParamProperty = new TemplateParameterProperties()
                {
                    type = "object"
                };
                this.template.Parameters.Add(ParameterNames.ServiceUrl, serviceUrlParamProperty);
            }

            return this;
        }

        public TemplateBuilder AddParameterizeApiLoggerIdProperty(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                TemplateParameterProperties apiLoggerProperty = new TemplateParameterProperties()
                {
                    type = "object"
                };
                this.template.Parameters.Add(ParameterNames.ApiLoggerId, apiLoggerProperty);
            }

            return this;
        }
    }
}
