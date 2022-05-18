// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders
{
    public partial class TemplateBuilder : ITemplateBuilder
    {
        Template template;

        /// <inheritdoc/>
        public Template Build() => this.template;

        /// <inheritdoc/>
        public Template<TTemplateResources> Build<TTemplateResources>()
            where TTemplateResources : ITemplateResources, new()
        {
            return this.Build<TTemplateResources>(new());
        }

        /// <inheritdoc/>
        public Template<TTemplateResources> Build<TTemplateResources>(TTemplateResources templateResources)
            where TTemplateResources : ITemplateResources, new()
        {
            // left Template.NotGeneric variant for backward compatibility
            // for refactored code please use Template.Generic and use explicit TTemplateResources type
            var genericTemplate = new Template<TTemplateResources>();

            genericTemplate.Schema = this.template.Schema;
            genericTemplate.ContentVersion = this.template.ContentVersion;
            genericTemplate.Parameters = this.template.Parameters;
            genericTemplate.Variables = this.template.Variables;
            genericTemplate.TypedResources = templateResources;
            genericTemplate.Outputs = this.template.Outputs;

            return genericTemplate;
        }

        public TemplateBuilder GenerateTemplateWithApimServiceNameProperty()
        {
            this.GenerateEmptyTemplate();
            this.template.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                {
                    ParameterNames.ApimServiceName, new TemplateParameterProperties() { Type = "string" }
                }
            };

            return this;
        }

        public TemplateBuilder GenerateTemplateWithPresetProperties(ExtractorParameters extractorParameters)
            => this.GenerateTemplateWithApimServiceNameProperty()
                   .AddPolicyProperties(extractorParameters)
                   .AddParameterizedServiceUrlProperty(extractorParameters)
                   .AddParameterizedApiLoggerIdProperty(extractorParameters)
                   .AddParameterizedApiScopeProperty(extractorParameters);

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
            if (extractorParameters.PolicyXMLBaseUrl != null)
            {
                TemplateParameterProperties policyTemplateBaseUrlParameterProperties = new TemplateParameterProperties()
                {
                    Type = "string"
                };
                this.template.Parameters.Add(ParameterNames.PolicyXMLBaseUrl, policyTemplateBaseUrlParameterProperties);


                if (extractorParameters.PolicyXMLSasToken != null)
                {
                    TemplateParameterProperties policyTemplateSasTokenParameterProperties = new TemplateParameterProperties()
                    {
                        Type = "string"
                    };

                    this.template.Parameters.Add(ParameterNames.PolicyXMLSasToken, policyTemplateSasTokenParameterProperties);
                }
            }
            return this;
        }

        public TemplateBuilder AddParameterizedServiceUrlProperty(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeServiceUrl || extractorParameters.ServiceUrlParameters != null && extractorParameters.ServiceUrlParameters.Length > 0)
            {
                TemplateParameterProperties serviceUrlParamProperty = new TemplateParameterProperties()
                {
                    Type = "object"
                };
                this.template.Parameters.Add(ParameterNames.ServiceUrl, serviceUrlParamProperty);
            }

            return this;
        }

        public TemplateBuilder AddParameterizedApiScopeProperty(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParametrizeApiOauth2Scope)
            {
                //check if there is existing api with such configured api scope or not if not let's not add it to the parameter later
                TemplateParameterProperties apiScopeParameterProperty = new TemplateParameterProperties()
                {
                    Type = "object"
                };
                this.template.Parameters.Add(ParameterNames.ApiOauth2ScopeSettings, apiScopeParameterProperty);
            }

            return this;
        }

        public TemplateBuilder AddParameterizedApiLoggerIdProperty(ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ParameterizeApiLoggerId)
            {
                TemplateParameterProperties apiLoggerProperty = new TemplateParameterProperties()
                {
                    Type = "object"
                };
                this.template.Parameters.Add(ParameterNames.ApiLoggerId, apiLoggerProperty);
            }

            return this;
        }
    }
}
