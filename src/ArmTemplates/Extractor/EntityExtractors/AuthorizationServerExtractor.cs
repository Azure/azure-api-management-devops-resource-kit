using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class AuthorizationServerExtractor : IAuthorizationServerExtractor
    {
        readonly ILogger<AuthorizationServerExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        readonly IAuthorizationServerClient authorizationServerClient;

        public AuthorizationServerExtractor(
            ILogger<AuthorizationServerExtractor> logger,
            ITemplateBuilder templateBuilder,
            IAuthorizationServerClient authorizationServerClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;

            this.authorizationServerClient = authorizationServerClient;
        }

        public async Task<Template> GenerateAuthorizationServersTemplateAsync(
            string singleApiName, 
            List<TemplateResource> apiTemplateResources, 
            ExtractorParameters extractorParameters)
        {
            var armTemplate = this.templateBuilder.GenerateTemplateWithApimServiceNameProperty().Build();

            // isolate api resources in the case of a single api extraction, as they may reference authorization servers
            var apiResources = apiTemplateResources
                ?.Where(resource => resource.Type == ResourceTypeConstants.API)
                ?.Select(resource => resource as APITemplateResource)
                ?.Where(resource => resource is not null);
            var templateResources = new List<TemplateResource>();

            var authorizationServers = await this.authorizationServerClient.GetAllAsync(extractorParameters);
            foreach (var authorizationServerTemplate in authorizationServers)
            {
                var originalAuthServerName = authorizationServerTemplate.Name;

                authorizationServerTemplate.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{authorizationServerTemplate.Name}')]";
                authorizationServerTemplate.Type = ResourceTypeConstants.AuthorizationServer;
                authorizationServerTemplate.ApiVersion = GlobalConstants.ApiVersion;

                // only extract the authorization server if this is a full extraction,
                // or in the case of a single api, if it is referenced by one of the api's authentication settings
                var isReferencedByApi = apiResources?.FirstOrDefault(apiResource =>
                    apiResource.Properties.AuthenticationSettings != null &&
                    apiResource.Properties.AuthenticationSettings.OAuth2 != null &&
                    apiResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId != null &&
                    apiResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId.Contains(originalAuthServerName)
                ) is not null;
                
                if (string.IsNullOrEmpty(singleApiName) || isReferencedByApi)
                {
                    this.logger.LogDebug("'{0}' Authorization server found", originalAuthServerName);
                    templateResources.Add(authorizationServerTemplate);
                }
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }
    }
}
