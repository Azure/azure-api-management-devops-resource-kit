using apimtemplate.Common.Templates.Abstractions;

namespace apimtemplate.Creator.TemplateCreators
{
    public class TemplateCreator
    {
        public Template CreateEmptyTemplate()
        {
            // creates empty template for use in all other template creators
            Template template = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
                contentVersion = "1.0.0.0",
                parameters = { },
                variables = { },
                resources = new TemplateResource[] { },
                outputs = { }
            };
            return template;
        }

        public Template CreateEmptyParameters()
        {
            // creates empty parameters file for use in all other template creators
            Template template = new Template()
            {
                schema = "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
                contentVersion = "1.0.0.0",
                parameters = { },
            };
            return template;
        }
    }
}
