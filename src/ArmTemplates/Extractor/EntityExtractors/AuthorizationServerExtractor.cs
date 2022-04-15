// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;

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

        public async Task<Template<AuthorizationServerTemplateResources>> GenerateAuthorizationServersTemplateAsync(
            string singleApiName, 
            List<ApiTemplateResource> apiTemplateResources, 
            ExtractorParameters extractorParameters)
        {
            var authorizationServerTemplate = this.templateBuilder
                                                    .GenerateTemplateWithApimServiceNameProperty()
                                                    .Build<AuthorizationServerTemplateResources>();

            var authorizationServers = await this.authorizationServerClient.GetAllAsync(extractorParameters);
            foreach (var authorizationServer in authorizationServers)
            {
                var originalAuthServerName = authorizationServer.Name;

                authorizationServer.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{authorizationServer.Name}')]";
                authorizationServer.Type = ResourceTypeConstants.AuthorizationServer;
                authorizationServer.ApiVersion = GlobalConstants.ApiVersion;

                // only extract the authorization server if this is a full extraction,
                // or in the case of a single api, if it is referenced by one of the api's authentication settings
                var isReferencedByApi = apiTemplateResources?.FirstOrDefault(apiResource =>
                    apiResource.Properties.AuthenticationSettings != null &&
                    apiResource.Properties.AuthenticationSettings.OAuth2 != null &&
                    apiResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId != null &&
                    apiResource.Properties.AuthenticationSettings.OAuth2.AuthorizationServerId.Contains(originalAuthServerName)
                ) is not null;
                
                if (string.IsNullOrEmpty(singleApiName) || isReferencedByApi)
                {
                    this.logger.LogDebug("'{0}' Authorization server found", originalAuthServerName);
                    authorizationServerTemplate.TypedResources.AuthorizationServers.Add(authorizationServer);
                }
            }

            return authorizationServerTemplate;
        }
    }
}
