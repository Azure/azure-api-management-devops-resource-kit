using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public abstract class TemplateGeneratorBase
    {
        protected Template GenerateTemplateWithApimServiceNameProperty() => TemplateBuilder.GenerateTemplateWithApimServiceNameProperty();

        protected Template GenerateTemplateWithPresetProperties(ExtractorParameters extractorParameters) 
            => TemplateBuilder.GenerateTemplateWithApimServiceNameProperty()
                              .AddPolicyProperties(extractorParameters)
                              .AddParameterizeServiceUrlProperty(extractorParameters)
                              .AddParameterizeApiLoggerIdProperty(extractorParameters);

        // TODO use TemplateBuilder for preset templates generation
        // noted at https://github.com/Azure/azure-api-management-devops-resource-kit/issues/617
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

        // TODO use TemplateBuilder for preset templates generation
        // noted at https://github.com/Azure/azure-api-management-devops-resource-kit/issues/617
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
