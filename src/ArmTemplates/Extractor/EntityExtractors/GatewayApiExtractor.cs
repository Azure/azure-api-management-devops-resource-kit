// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.GatewayApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class GatewayApiExtractor : IGatewayApiExtractor
    {
        readonly ILogger<GatewayApiExtractor> logger;
        readonly ITemplateBuilder templateBuilder;
        readonly IGatewayClient gatewayClient;
        readonly IApisClient apisClient;
        readonly IApiClientUtils apiClientUtils;

        public GatewayApiExtractor(
            ILogger<GatewayApiExtractor> logger,
            ITemplateBuilder templateBuilder,
            IGatewayClient gatewayClient,
            IApisClient apisClient,
            IApiClientUtils apiClientUtils)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;
            this.gatewayClient = gatewayClient;
            this.apisClient = apisClient;
            this.apiClientUtils = apiClientUtils;
        }

        public async Task<Template<GatewayApiTemplateResources>> GenerateGatewayApiTemplateAsync(
            string singleApiName,
            List<string> multipleApiNames,
            ExtractorParameters extractorParameters)
        {
            var gatewayApiTemplate = this.templateBuilder
                                        .GenerateTemplateWithApimServiceNameProperty()
                                        .Build<GatewayApiTemplateResources>();

            var gateways = await this.gatewayClient.GetAllAsync(extractorParameters);
            if (gateways.IsNullOrEmpty())
            {
                this.logger.LogWarning("No Gateways data was found for {0}", extractorParameters.SourceApimName);
                return gatewayApiTemplate;
            }

            var multipleApiNamesSet = multipleApiNames?.ToHashSet();
            foreach (var gateway in gateways)
            {
                var gatewayApis = await this.apisClient.GetAllLinkedToGatewayAsync(gateway.Name, extractorParameters);
                if (gatewayApis.IsNullOrEmpty())
                {
                    this.logger.LogWarning("No gateway apis were found for gateway '{0}'", gateway.Name);
                    continue;
                }

                if (!string.IsNullOrEmpty(singleApiName))
                {
                    var serviceApi = await this.apiClientUtils.GetsingleApi(singleApiName, extractorParameters);

                    // inluding only api with singleApiName
                    var apis = gatewayApis.Where(x => x.Name == serviceApi.Name).ToList();
                    if (!apis.IsNullOrEmpty())
                    {
                        //apis contains only one api
                        apis[0].Name = singleApiName;
                        var gatewayApiResources = this.GenerateGatewayApiTemplateResources(gateway.Name, apis);
                        gatewayApiTemplate.TypedResources.GatewayApis.AddRange(gatewayApiResources);
                    }
                }
                else if (!multipleApiNames.IsNullOrEmpty())
                {
                    // including only apis existing in multipleApiNames
                    var apis = gatewayApis.Where(x => multipleApiNamesSet.Contains(x.Name));
                    if (!apis.IsNullOrEmpty())
                    {
                        var gatewayApiResources = this.GenerateGatewayApiTemplateResources(gateway.Name, apis);
                        gatewayApiTemplate.TypedResources.GatewayApis.AddRange(gatewayApiResources);
                    }
                }
                else
                {
                    // including every api in response
                    var gatewayApiResources = this.GenerateGatewayApiTemplateResources(gateway.Name, gatewayApis);
                    gatewayApiTemplate.TypedResources.GatewayApis.AddRange(gatewayApiResources);
                }
            }

            return gatewayApiTemplate;
        }

        List<GatewayApiTemplateResource> GenerateGatewayApiTemplateResources(string gatewayName, IEnumerable<ApiTemplateResource> apis)
        {
            var templateResources = new List<GatewayApiTemplateResource>();
            foreach (var api in apis)
            {
                var gatewayApiTemplateResource = new GatewayApiTemplateResource();

                gatewayApiTemplateResource.Type = ResourceTypeConstants.GatewayApi;
                gatewayApiTemplateResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{gatewayName}/{api.Name}')]";
                gatewayApiTemplateResource.ApiVersion = GlobalConstants.ApiVersion;
                gatewayApiTemplateResource.Scale = null;
                gatewayApiTemplateResource.Properties = new GatewayApiProperties 
                {
                    ProvisioningState = "created" 
                };

                templateResources.Add(gatewayApiTemplateResource);
            }

            return templateResources;
        }
    }
}
