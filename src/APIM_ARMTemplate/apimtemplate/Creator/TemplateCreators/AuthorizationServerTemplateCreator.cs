using System.Collections.Generic;
using apimtemplate.Common.Constants;
using apimtemplate.Common.TemplateModels;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Creator.Models;

namespace apimtemplate.Creator.TemplateCreators
{
    public class AuthorizationServerTemplateCreator : TemplateCreator
    {
        public Template CreateAuthorizationServerTemplate(CreatorConfig creatorConfig)
        {
            // create empty template
            Template authorizationTemplate = CreateEmptyTemplate();

            // add parameters
            authorizationTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { ParameterNames.ApimServiceName, new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();
            foreach (AuthorizationServerTemplateProperties authorizationServerTemplateProperties in creatorConfig.authorizationServers)
            {
                // create authorization server resource with properties
                AuthorizationServerTemplateResource authorizationServerTemplateResource = new AuthorizationServerTemplateResource()
                {
                    name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{authorizationServerTemplateProperties.displayName}')]",
                    type = ResourceTypeConstants.AuthorizationServer,
                    apiVersion = GlobalConstants.APIVersion,
                    properties = authorizationServerTemplateProperties,
                    dependsOn = new string[] { }
                };
                resources.Add(authorizationServerTemplateResource);
            }

            authorizationTemplate.resources = resources.ToArray();
            return authorizationTemplate;
        }
    }
}
