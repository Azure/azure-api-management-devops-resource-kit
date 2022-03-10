// --------------------------------------------------------------------------
//  <copyright file="TemplateBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders
{
    public class TemplateBuilder : ITemplateBuilder
    {
        public Template GenerateTemplateWithApimServiceNameProperty()
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

        public Template GenerateTemplateWithPresetProperties(ExtractorParameters extractorParameters)
            => this.GenerateTemplateWithApimServiceNameProperty()
                   .AddPolicyProperties(extractorParameters)
                   .AddParameterizeServiceUrlProperty(extractorParameters)
                   .AddParameterizeApiLoggerIdProperty(extractorParameters);

        public Template GenerateEmptyTemplate()
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
    }
}
