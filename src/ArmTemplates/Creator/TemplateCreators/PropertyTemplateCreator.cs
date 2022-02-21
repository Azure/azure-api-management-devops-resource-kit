using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class PropertyTemplateCreator : TemplateGeneratorBase
    {
        public Template CreatePropertyTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template propertyTemplate = this.GenerateEmptyTemplate();

            // add parameters
            propertyTemplate.Parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (PropertyConfig namedValue in creatorConfig.namedValues)
            {
                // create property resource with properties
                PropertyTemplateResource propertyTemplateResource = new PropertyTemplateResource()
                {
                    Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{namedValue.displayName}')]",
                    Type = ResourceTypeConstants.Property,
                    ApiVersion = GlobalConstants.ApiVersion,
                    Properties = new PropertyResourceProperties()
                    {
                        displayName = namedValue.displayName,
                        value = namedValue.value,
                        secret = namedValue.secret,
                        tags = namedValue.tags,
                        keyVault = namedValue.keyVault

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
