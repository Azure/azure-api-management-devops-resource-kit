using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class PropertyTemplateCreator
    {
        readonly ITemplateBuilder templateBuilder;

        public PropertyTemplateCreator(ITemplateBuilder templateBuilder)
        {
            this.templateBuilder = templateBuilder;
        }

        public Template CreatePropertyTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template propertyTemplate = this.templateBuilder.GenerateEmptyTemplate().Build();

            // add parameters
            propertyTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            if (creatorConfig.parameterizeNamedValues)
            {
                if (creatorConfig.namedValues.Any(x => x.value != null))
                {
                    propertyTemplate.Parameters.Add(ParameterNames.NamedValues, new TemplateParameterProperties { type = "object" });
                }

                if (creatorConfig.namedValues.Any(x => x.keyVault != null))
                {
                    propertyTemplate.Parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, new TemplateParameterProperties { type = "object" });
                }
            }

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (PropertyConfig namedValue in creatorConfig.namedValues)
            {
                string value = namedValue.value == null ? null
                   : creatorConfig.parameterizeNamedValues
                       ? $"[parameters('{ParameterNames.NamedValues}').{ParameterNamingHelper.GenerateValidParameterName(namedValue.displayName, ParameterPrefix.Property)}]"
                       : namedValue.value;

                PropertyResourceKeyVaultProperties keyVault = namedValue.keyVault == null ? null
                    : creatorConfig.parameterizeNamedValues
                        ? new PropertyResourceKeyVaultProperties
                        {
                            secretIdentifier = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}').{ParameterNamingHelper.GenerateValidParameterName(namedValue.displayName, ParameterPrefix.Property)}]"
                        }
                        : namedValue.keyVault;

                // create property resource with properties
                PropertyTemplateResource propertyTemplateResource = new PropertyTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{namedValue.displayName}')]",
                    Type = ResourceTypeConstants.Property,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new PropertyResourceProperties()
                    {
                        displayName = namedValue.displayName,
                        value = value,
                        secret = namedValue.secret,
                        tags = namedValue.tags,
                        keyVault = keyVault

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
