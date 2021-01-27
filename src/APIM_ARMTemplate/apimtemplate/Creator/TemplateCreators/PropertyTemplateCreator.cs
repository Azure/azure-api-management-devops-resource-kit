using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class PropertyTemplateCreator : TemplateCreator
    {
        public Template CreatePropertyTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template propertyTemplate = CreateEmptyTemplate();

            // add parameters
            propertyTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            if (creatorConfig.paramNamedValue)
            {
                if (creatorConfig.namedValues.Any(x => x.value != null))
                {
                    propertyTemplate.parameters.Add(ParameterNames.NamedValues, new TemplateParameterProperties { type = "object" });
                }

                if (creatorConfig.namedValues.Any(x => x.keyVault != null))
                {
                    propertyTemplate.parameters.Add(ParameterNames.NamedValueKeyVaultSecrets, new TemplateParameterProperties { type = "object" });
                }
            }

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (PropertyConfig namedValue in creatorConfig.namedValues)
            {
                string value = namedValue.value == null ? null
                    : creatorConfig.paramNamedValue
                        ? $"[parameters('{ParameterNames.NamedValues}').{ExtractorUtils.GenValidParamName(namedValue.displayName, ParameterPrefix.Property)}]"
                        : namedValue.value;

                PropertyResourceKeyVaultProperties keyVault = namedValue.keyVault == null ? null
                    : creatorConfig.paramNamedValue
                        ? new PropertyResourceKeyVaultProperties
                        {
                            secretIdentifier = $"[parameters('{ParameterNames.NamedValueKeyVaultSecrets}').{ExtractorUtils.GenValidParamName(namedValue.displayName, ParameterPrefix.Property)}]"
                        }
                        : namedValue.keyVault;

                // create property resource with properties
                PropertyTemplateResource propertyTemplateResource = new PropertyTemplateResource()
                {
                    name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{namedValue.displayName}')]",
                    type = ResourceTypeConstants.Property,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = new PropertyResourceProperties()
                    {
                        displayName = namedValue.displayName,
                        value = value,
                        secret = namedValue.secret,
                        tags = namedValue.tags,
                        keyVault = keyVault
                    },
                    dependsOn = new string[] { }
                };
                resources.Add(propertyTemplateResource);
            }

            propertyTemplate.resources = resources.ToArray();
            return propertyTemplate;
        }
    }
}
