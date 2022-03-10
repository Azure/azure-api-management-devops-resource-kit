using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors
{
    public class ProductExtractor : IProductExtractor
    {
        readonly ILogger<ProductExtractor> logger;
        readonly ITemplateBuilder templateBuilder;

        readonly IGroupsClient groupsClient;
        readonly IProductsClient productsClient;
        readonly ITagClient tagClient;

        readonly IPolicyExtractor policyExtractor;

        public ProductExtractor(
            ILogger<ProductExtractor> logger,
            IPolicyExtractor policyExtractor,
            IProductsClient productsClient,
            IGroupsClient groupsClient,
            ITagClient tagClient, 
            ITemplateBuilder templateBuilder)
        {
            this.logger = logger;
            this.templateBuilder = templateBuilder;

            this.productsClient = productsClient;
            this.groupsClient = groupsClient;
            this.tagClient = tagClient;

            this.policyExtractor = policyExtractor;
            this.templateBuilder = templateBuilder;
        }

        public async Task<Template> GenerateProductsARMTemplateAsync(
            string singleApiName,
            List<TemplateResource> armTemplateResources,
            string baseFilesGenerationDirectory,
            ExtractorParameters extractorParameters)
        {
            var armTemplate = this.templateBuilder.GenerateTemplateWithPresetProperties(extractorParameters);

            // isolate product api associations in the case of a single api extraction
            var productAPIResources = armTemplateResources?.Where(resource => resource.Type == ResourceTypeConstants.ProductApi);
            List<TemplateResource> templateResources = new List<TemplateResource>();

            var products = await this.productsClient.GetAllAsync(extractorParameters);

            foreach (var productTemplateResource in products)
            {
                var productOriginalName = productTemplateResource.Name;

                productTemplateResource.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productOriginalName}')]";
                productTemplateResource.ApiVersion = GlobalConstants.ApiVersion;

                // only extract the product if this is a full extraction, or in the case of a single api, if it is found in products associated with the api
                if (singleApiName == null || productAPIResources.SingleOrDefault(p => p.Name.Contains($"/{productOriginalName}/")) != null)
                {
                    this.logger.LogDebug("'{0}' product found", productOriginalName);
                    templateResources.Add(productTemplateResource);

                    await this.AddProductPolicyToTemplateResources(extractorParameters, productOriginalName, templateResources, baseFilesGenerationDirectory);
                    await this.AddProductTagsToTemplateResources(extractorParameters, productOriginalName, templateResources);
                }
            }

            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        async Task AddProductTagsToTemplateResources(
            ExtractorParameters extractorParameters,
            string productName,
            List<TemplateResource> templateResources)
        {
            try
            {
                var productTags = await this.tagClient.GetAllTagsLinkedToProductAsync(productName, extractorParameters);

                if (productTags.IsNullOrEmpty())
                {
                    this.logger.LogWarning($"No tags found for product {productName}");
                    return;
                }

                foreach (var productTag in productTags)
                {
                    string originalTagName = productTag.Name;
                    this.logger.LogDebug("'{0}' tag association found for {1} product", originalTagName, productName);

                    productTag.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productName}/{originalTagName}')]";
                    productTag.Type = ResourceTypeConstants.ProductTag;
                    productTag.ApiVersion = GlobalConstants.ApiVersion;
                    productTag.Scale = null;
                    productTag.DependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('{ParameterNames.ApimServiceName}'), '{productName}')]" };
                    
                    templateResources.Add(productTag);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occured while product tags template generation");
                throw;
            }
        }

        async Task AddProductPolicyToTemplateResources(
            ExtractorParameters extractorParameters,
            string productName,
            List<TemplateResource> templateResources,
            string filesGenerationDirectory)
        {
            var productResourceId = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products', parameters('{ParameterNames.ApimServiceName}'), '{productName}')]" };

            try
            {
                var groupsLinkedToProduct = await this.groupsClient.GetAllLinkedToProductAsync(productName, extractorParameters);

                foreach (var productGroup in groupsLinkedToProduct)
                {
                    productGroup.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productName}/{productGroup.Name}')]";
                    productGroup.Type = ResourceTypeConstants.ProductGroup;
                    productGroup.ApiVersion = GlobalConstants.ApiVersion;
                    productGroup.DependsOn = productResourceId;
                    templateResources.Add(productGroup);
                }

                var productPolicyResource = await this.policyExtractor.GenerateProductPolicyTemplateAsync(
                    extractorParameters,
                    productName,
                    productResourceId,
                    filesGenerationDirectory);

                if (productPolicyResource is not null)
                {
                    templateResources.Add(productPolicyResource);
                }
            }
            catch (Exception ex) 
            {
                this.logger.LogError(ex, "Exception occured while product policy template generation");
                throw;
            }
        }
    }
}
