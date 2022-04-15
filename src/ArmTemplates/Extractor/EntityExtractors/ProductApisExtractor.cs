// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ProductApisExtractor : IProductApisExtractor
    {
        readonly ILogger<ProductApisExtractor> logger;
        
        readonly IProductsClient productsClient;
        readonly IApisClient apisClient;
        readonly ITemplateBuilder templateBuilder;

        public ProductApisExtractor(
            ILogger<ProductApisExtractor> logger,
            IProductsClient productClient,
            IApisClient apisClient,
            ITemplateBuilder templateBuilder)
        {
            this.logger = logger;
            
            this.productsClient = productClient;
            this.apisClient = apisClient;
            this.templateBuilder = templateBuilder;
        }

        public async Task<Template<ProductApiTemplateResources>> GenerateProductApisTemplateAsync(string singleApiName, List<string> multipleApiNames, ExtractorParameters extractorParameters)
        {
            var productApiTemplate = this.templateBuilder
                                            .GenerateTemplateWithApimServiceNameProperty()
                                            .Build<ProductApiTemplateResources>();

            if (!string.IsNullOrEmpty(singleApiName))
            {
                productApiTemplate.TypedResources.ProductApis = await this.GenerateSingleApiTemplateAsync(singleApiName, extractorParameters);
            }
            else if (!multipleApiNames.IsNullOrEmpty())
            {
                productApiTemplate.TypedResources.ProductApis = await this.GenerateMultipleApisTemplateAsync(multipleApiNames, extractorParameters);
            }
            else
            {
                var serviceApis = await this.apisClient.GetAllAsync(extractorParameters);
                this.logger.LogInformation("{0} APIs found ...", serviceApis.Count);

                var serviceApiNames = serviceApis.Select(api => api.Name).ToList();
                productApiTemplate.TypedResources.ProductApis = await this.GenerateMultipleApisTemplateAsync(serviceApiNames, extractorParameters);
            }

            return productApiTemplate;
        }

        public async Task<List<ProductApiTemplateResource>> GenerateSingleApiTemplateAsync(
            string singleApiName, 
            ExtractorParameters extractorParameters,
            bool addDependsOnParameter = false)
        {
            try
            {
                var serviceApi = await this.apisClient.GetSingleAsync(singleApiName, extractorParameters);

                if (serviceApi is null)
                {
                    throw new ServiceApiNotFoundException($"ServiceApi with name '{singleApiName}' not found");
                }

                this.logger.LogInformation("{0} Product API found ...", singleApiName);
                var dependsOnParameter = addDependsOnParameter
                    ? new string[] { $"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{ParameterNames.ApimServiceName}'), '{singleApiName}')]" }
                    : Array.Empty<string>();

                return await this.GenerateProductApiTemplateResourcesAsync(singleApiName, extractorParameters, dependsOnParameter);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Exception occured while generating service api products template for {singleApiName}.");
                throw;
            }
        }

        async Task<List<ProductApiTemplateResource>> GenerateMultipleApisTemplateAsync(List<string> multipleApiNames, ExtractorParameters extractorParameters)
        {
            this.logger.LogInformation("Processing {0} api-names...", multipleApiNames.Count);

            string[] dependsOn = Array.Empty<string>();
            var templateResources = new List<ProductApiTemplateResource>();
            foreach (string apiName in multipleApiNames)
            {
                var productApiTemplateResources = await this.GenerateProductApiTemplateResourcesAsync(apiName, extractorParameters, dependsOn);
                
                if (!productApiTemplateResources.IsNullOrEmpty())
                {
                    templateResources.AddRange(productApiTemplateResources);
                    string apiProductName = templateResources.Last().Name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                }
            }

            return templateResources;
        }

        async Task<List<ProductApiTemplateResource>> GenerateProductApiTemplateResourcesAsync(
            string apiName, 
            ExtractorParameters extractorParameters, 
            string[] dependsOn)
        {
            var productApiResources = new List<ProductApiTemplateResource>();
            this.logger.LogInformation("Extracting products from {0} API:", apiName);

            try
            {
                var productApis = await this.productsClient.GetAllLinkedToApiAsync(apiName, extractorParameters);

                string lastProductAPIName = null;
                foreach (var productApi in productApis)
                {
                    var originalProductApiName = productApi.Name;
                    this.logger.LogInformation("'{0}' Product association found", originalProductApiName);

                    // convert returned api product associations to template resource class
                    productApi.Type = ResourceTypeConstants.ProductApi;
                    productApi.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{originalProductApiName}/{apiName}')]";
                    productApi.ApiVersion = GlobalConstants.ApiVersion;
                    productApi.Scale = null;
                    productApi.DependsOn = lastProductAPIName != null ? new string[] { lastProductAPIName } : dependsOn;

                    productApiResources.Add(productApi);

                    lastProductAPIName = $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{originalProductApiName}', '{apiName}')]";
                }
            }
            catch (Exception ex) 
            {
                this.logger.LogError(ex, "Exception occured while generating Service Api's Products template.");
            }

            return productApiResources;
        }
    }
}