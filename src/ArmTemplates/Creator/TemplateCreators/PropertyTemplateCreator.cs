// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class PropertyTemplateCreator : IPropertyTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public PropertyTemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template CreatePropertyTemplate(CreatorParameters creatorConfig)
        {
            // create empty template
            Template propertyTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            propertyTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ Type = "string" } }
            };

            if (creatorConfig.ParameterizeNamedValues)
            {
                if (creatorConfig.NamedValues.Any(x => x.Value != null))
                {
                    propertyTemplate.Parameters.Add(ParameterNames.NamedValues, new TemplateParameterProperties { Type = "object" });
                }

                if (creatorConfig.NamedValues.Any(x => x.KeyVault != null))
                {
                    propertyTemplate.Parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, new TemplateParameterProperties { Type = "object" });
                }
            }

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (PropertyConfig namedValue in creatorConfig.NamedValues)
            {
                string value = namedValue.Value == null ? null
                   : creatorConfig.ParameterizeNamedValues
                       ? $"[parameters('{ParameterNames.NamedValues}').{NamingHelper.GenerateValidParameterName(namedValue.DisplayName, ParameterPrefix.Property)}]"
                       : namedValue.Value;

                var keyVault = namedValue.KeyVault == null ? null
                    : creatorConfig.ParameterizeNamedValues
                        ? new NamedValueResourceKeyVaultProperties
                        {
                            SecretIdentifier = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}').{NamingHelper.GenerateValidParameterName(namedValue.DisplayName, ParameterPrefix.Property)}]"
                        }
                        : namedValue.KeyVault;

                // create property resource with properties
                NamedValueTemplateResource propertyTemplateResource = new NamedValueTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{namedValue.DisplayName}')]",
                    Type = ResourceTypeConstants.NamedValues,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new NamedValueProperties()
                    {
                        DisplayName = namedValue.DisplayName,
                        Value = value,
                        Secret = namedValue.Secret,
                        Tags = namedValue.Tags,
                        KeyVault = keyVault

                    },
                    DependsOn = new string[] { }
                };
                resources.Add(propertyTemplateResource);
            }

            propertyTemplate.Resources = resources.ToArray();
            return propertyTemplate;
        }
    }
}
