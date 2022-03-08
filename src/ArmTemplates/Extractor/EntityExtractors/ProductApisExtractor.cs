using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ProductApisExtractor : TemplateGeneratorBase, IProductApisExtractor
    {
        readonly ILogger<ProductApisExtractor> logger;
        
        readonly IProductsClient productsClient;
        readonly IApisClient apisClient;

        public ProductApisExtractor(
            ILogger<ProductApisExtractor> logger,
            IProductsClient productClient,
            IApisClient apisClient)
        {
            this.logger = logger;
            
            this.productsClient = productClient;
            this.apisClient = apisClient;
        }

        public async Task<Template> GenerateProductApisTemplateAsync(string singleApiName, List<string> multipleApiNames, ExtractorParameters extractorParameters)
        {
            Template armTemplate = this.GenerateEmptyPropertyTemplateWithParameters();
            List<TemplateResource> templateResources = new List<TemplateResource>();

            if (!string.IsNullOrEmpty(singleApiName))
            {
                templateResources = await this.GenerateSingleApiTemplateAsync(singleApiName, extractorParameters);
            }
            else if (!multipleApiNames.IsNullOrEmpty())
            {
                templateResources = await this.GenerateMultipleApisTemplateAsync(multipleApiNames, extractorParameters);
            }
            else
            {
                var serviceApis = await this.apisClient.GetAllAsync(extractorParameters);
                this.logger.LogInformation("{0} APIs found ...", serviceApis.Count);

                var serviceApiNames = serviceApis.Select(api => api.Name).ToList();
                templateResources = await this.GenerateMultipleApisTemplateAsync(serviceApiNames, extractorParameters);
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        async Task<List<TemplateResource>> GenerateSingleApiTemplateAsync(string singleApiName, ExtractorParameters extractorParameters)
        {
            try
            {
                var serviceApi = await this.apisClient.GetSingleAsync(
                    extractorParameters.SourceApimName,
                    extractorParameters.ResourceGroup,
                    singleApiName);

                if (serviceApi is null)
                {
                    throw new ServiceApiNotFoundException($"ServiceApi with name '{singleApiName}' not found");
                }

                this.logger.LogInformation("{0} Product API found ...", singleApiName);
                return await this.GenerateProductApiTemplateResourcesAsync(singleApiName, extractorParameters, new string[] { });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Exception occured while generating service api products template for {singleApiName}.");
                throw;
            }
        }

        async Task<List<TemplateResource>> GenerateMultipleApisTemplateAsync(List<string> multipleApiNames, ExtractorParameters extractorParameters)
        {
            this.logger.LogInformation("Processing {0} api-names...", multipleApiNames.Count);

            string[] dependsOn = new string[] { };
            List<TemplateResource> templateResources = new List<TemplateResource>();
            foreach (string apiName in multipleApiNames)
            {
                var productApiTemplateResources = await this.GenerateProductApiTemplateResourcesAsync(apiName, extractorParameters, dependsOn);
                
                templateResources.AddRange(productApiTemplateResources);
                string apiProductName = templateResources.Last().Name.Split('/', 3)[1];
                dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
            }

            return templateResources;
        }

        async Task<List<TemplateResource>> GenerateProductApiTemplateResourcesAsync(
            string apiName, 
            ExtractorParameters extractorParameters, 
            string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            this.logger.LogInformation("Extracting products from {0} API:", apiName);

            try
            {
                var productApis = await this.productsClient.GetAllLinkedToApiAsync(extractorParameters, apiName);

                string lastProductAPIName = null;
                foreach (var productApi in productApis)
                {
                    this.logger.LogInformation("'{0}' Product association found", productApi.Name);

                    // convert returned api product associations to template resource class
                    productApi.Type = ResourceTypeConstants.ProductAPI;
                    productApi.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productApi.Name}/{apiName}')]";
                    productApi.ApiVersion = GlobalConstants.ApiVersion;
                    productApi.Scale = null;
                    productApi.DependsOn = lastProductAPIName != null ? new string[] { lastProductAPIName } : dependsOn;

                    templateResources.Add(productApi);

                    lastProductAPIName = $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{productApi.Name}', '{apiName}')]";
                }
            }
            catch (Exception ex) 
            {
                this.logger.LogError(ex, "Exception occured while generating Service Api's Products template.");
            }

            return templateResources;
        }
    }
}