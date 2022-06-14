// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class IdentityProviderExtractor : IIdentityProviderExtractor
    {
        readonly ILogger<IdentityProviderExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        readonly IIdentityProviderClient identityProviderClient;

        public IdentityProviderExtractor(
            ILogger<IdentityProviderExtractor> logger,
            ITemplateBuilder templateBuilder,
            IIdentityProviderClient identityProviderClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;

            this.identityProviderClient = identityProviderClient;
        }

        public async Task<Template<IdentityProviderTemplateResources>> GenerateIdentityProvidersTemplateAsync(ExtractorParameters extractorParameters)
        {
            var identityProviderTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .AddParametrizedIdentityProvidersSecrets()
                                        .Build<IdentityProviderTemplateResources>();
            var identityProvideres = await this.identityProviderClient.GetAllAsync(extractorParameters);

            if (identityProvideres.IsNullOrEmpty())
            {
                this.logger.LogWarning($"No identityProviders were found for '{extractorParameters.SourceApimName}' at '{extractorParameters.ResourceGroup}'");
                return identityProviderTemplate;
            }

            foreach (var identityProvider in identityProvideres)
            {
                identityProvider.Type = ResourceTypeConstants.IdentityProviders;
                identityProvider.Properties.ClientSecret = $"[parameters('{ParameterNames.SecretValues}').identityProviders.{NamingHelper.GenerateValidParameterName(identityProvider.Name, ParameterPrefix.Property)}]";
                identityProvider.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{identityProvider.Name}')]";
                identityProvider.ApiVersion = GlobalConstants.ApiVersion;
                identityProviderTemplate.TypedResources.IdentityProviders.Add(identityProvider);
            }

            return identityProviderTemplate;
        }

        async Task PopulateClientSecretsValue(IdentityProviderTemplateResource identityProvider, ExtractorParameters extractorParameters)
        {
            if (extractorParameters.ExtractSecrets)
            {
                
            }
            else
            {

            }
        }
    }
}
