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
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class GatewayExtractor : IGatewayExtractor
    {
        readonly ILogger<GatewayExtractor> logger;
        readonly ITemplateBuilder templateBuilder;
        readonly IGatewayClient gatewayClient;

        public GatewayExtractor(
            ILogger<GatewayExtractor> logger,
            ITemplateBuilder templateBuilder,
            IGatewayClient gatewayClient)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.gatewayClient = gatewayClient;
        }

        public async Task<Template<GatewayTemplateResources>> GenerateGatewayTemplateAsync(string singleApiName, ExtractorParameters extractorParameters)
        {
            var gatewayTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<GatewayTemplateResources>();

            var gateways = await this.gatewayClient.GetAllAsync(extractorParameters);
            if (gateways.IsNullOrEmpty())
            {
                this.logger.LogWarning("No Gateways data was found for {0}", extractorParameters.SourceApimName);
                return gatewayTemplate;
            }

            foreach (var gateway in gateways)
            {
                var gatewayOriginalName = gateway.Name;

                // convert returned backend to template resource class
                gateway.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{gatewayOriginalName}')]";
                gateway.ApiVersion = GlobalConstants.ApiVersion;
                gateway.Type = ResourceTypeConstants.Gateway;

                if (string.IsNullOrEmpty(singleApiName))
                {
                    // loading all gateways
                    gatewayTemplate.TypedResources.Gateways.Add(gateway);
                }
                else
                {
                    var doesApiReferenceGateway = await this.gatewayClient.DoesApiReferenceGatewayAsync(singleApiName, gatewayOriginalName, extractorParameters);
                    if (doesApiReferenceGateway)
                    {
                        gatewayTemplate.TypedResources.Gateways.Add(gateway);
                    }
                }
            }

            return gatewayTemplate;
        }       
    }
}
