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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class OpenIdConnectProviderExtractor : IOpenIdConnectProviderExtractor
    {
        readonly ILogger<OpenIdConnectProviderExtractor> logger;
        readonly ITemplateBuilder templateBuilder;
        readonly IOpenIdConnectProvidersClient openIdConnectProviderClient;

        public OpenIdConnectProviderExtractor(
            ILogger<OpenIdConnectProviderExtractor> logger,
            ITemplateBuilder templateBuilder,
            IOpenIdConnectProvidersClient openIdConnectProviderClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.openIdConnectProviderClient = openIdConnectProviderClient;
        }

        public async Task<Template<OpenIdConnectProviderResources>> GenerateOpenIdConnectProvidersTemplateAsync(ExtractorParameters extractorParameters)
        {
            var openIdConnectProviderTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .AddParametrizedSecrets()
                                        .Build<OpenIdConnectProviderResources>();
            var openIdConnectProviders = await this.openIdConnectProviderClient.GetAllAsync(extractorParameters);

            if (openIdConnectProviders.IsNullOrEmpty())
            {
                this.logger.LogWarning($"No openIdConnectProviders were found for '{extractorParameters.SourceApimName}' at '{extractorParameters.ResourceGroup}'");
                return openIdConnectProviderTemplate;
            }

            foreach (var openIdConnectProvider in openIdConnectProviders)
            {
                openIdConnectProvider.Type = ResourceTypeConstants.OpenIdConnectProvider;
                openIdConnectProvider.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{openIdConnectProvider.Name}')]";
                openIdConnectProvider.ApiVersion = GlobalConstants.ApiVersion;
                openIdConnectProvider.Properties.ClientSecret = $"[parameters('{ParameterNames.SecretValues}').{ParameterNames.OpenIdConnectProvidersSecretValues}.{NamingHelper.GenerateValidParameterName(openIdConnectProvider.OriginalName, ParameterPrefix.Property).ToLower()}]";
                
                openIdConnectProviderTemplate.TypedResources.OpenIdConnectProviders.Add(openIdConnectProvider);
            }

            return openIdConnectProviderTemplate;
        }
    }
}
