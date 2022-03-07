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
    public class ServiceApisProductsExtractor : TemplateGeneratorBase, IServiceApisProductsExtractor
    {
        readonly ILogger<ServiceApisProductsExtractor> logger;
        
        readonly IServiceApiProductsApiClient serviceApiProductsApiClient;
        readonly IServiceApisApiClient serviceApisApiClient;

        public ServiceApisProductsExtractor(
            ILogger<ServiceApisProductsExtractor> logger,
            IServiceApiProductsApiClient productApisApiClient,
            IServiceApisApiClient serviceApiClient)
        {
            this.logger = logger;
            
            this.serviceApiProductsApiClient = productApisApiClient;
            this.serviceApisApiClient = serviceApiClient;
        }

        public async Task<Template> GenerateServiceApisProductsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, ExtractorParameters extractorParameters)
        {
            Template armTemplate = this.GenerateEmptyPropertyTemplateWithParameters();
            List<TemplateResource> templateResources = new List<TemplateResource>();

            if (!string.IsNullOrEmpty(singleApiName))
            {
                try
                {
                    var serviceApi = await this.serviceApisApiClient.GetSingleServiceApiAsync(
                        extractorParameters.SourceApimName, 
                        extractorParameters.ResourceGroup, 
                        singleApiName);

                    if (serviceApi is null)
                    {
                        throw new ServiceApiNotFoundException($"ServiceApi with name '{singleApiName}' not found");
                    }

                    this.logger.LogInformation("{0} Product API found ...", singleApiName);
                    templateResources.AddRange(await this.GenerateServiceApiProductsTemplateResourcesAsync(singleApiName, extractorParameters, new string[] { }));
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Exception occured while generating service api products template for {singleApiName}.");
                    throw;
                }
            }
            else if (!multipleApiNames.IsNullOrEmpty())
            {
                this.logger.LogInformation("Processing {0} api-names...", multipleApiNames.Count);

                string[] dependsOn = new string[] { };
                foreach (string apiName in multipleApiNames)
                {
                    templateResources.AddRange(await this.GenerateServiceApiProductsTemplateResourcesAsync(apiName, extractorParameters, dependsOn));
                    string apiProductName = templateResources.Last().Name.Split('/', 3)[1];
                    dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{apiName}')]" };
                }
            }
            else
            {
                var serviceApis = await this.serviceApisApiClient.GetAllServiceApisAsync(extractorParameters);
                this.logger.LogInformation("{0} APIs found ...", serviceApis.Count);

                string[] dependsOn = new string[] { };
                foreach (var serviceApi in serviceApis)
                {
                    templateResources.AddRange(await this.GenerateServiceApiProductsTemplateResourcesAsync(serviceApi.Name, extractorParameters, dependsOn));

                    if (templateResources.Count > 0)
                    {
                        string apiProductName = templateResources.Last().Name.Split('/', 3)[1];
                        dependsOn = new string[] { $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{apiProductName}', '{serviceApi.Name}')]" };
                    }
                }
            }
            armTemplate.Resources = templateResources.ToArray();
            return armTemplate;
        }

        async Task<List<TemplateResource>> GenerateServiceApiProductsTemplateResourcesAsync(
            string apiName, 
            ExtractorParameters extractorParameters, 
            string[] dependsOn)
        {
            List<TemplateResource> templateResources = new List<TemplateResource>();
            this.logger.LogInformation("Extracting products from {0} API:", apiName);

            try
            {
                var serviceApiProducts = await this.serviceApiProductsApiClient.GetServiceApiProductsAsync(
                    extractorParameters.SourceApimName,
                    extractorParameters.ResourceGroup,
                    apiName);

                string lastProductAPIName = null;

                foreach (var serviceApiProduct in serviceApiProducts)
                {
                    this.logger.LogInformation("'{0}' Product association found", serviceApiProduct.Name);

                    // convert returned api product associations to template resource class
                    serviceApiProduct.Type = ResourceTypeConstants.ProductAPI;
                    serviceApiProduct.Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{serviceApiProduct.Name}/{apiName}')]";
                    serviceApiProduct.ApiVersion = GlobalConstants.ApiVersion;
                    serviceApiProduct.Scale = null;
                    serviceApiProduct.DependsOn = lastProductAPIName != null ? new string[] { lastProductAPIName } : dependsOn;

                    templateResources.Add(serviceApiProduct);

                    lastProductAPIName = $"[resourceId('Microsoft.ApiManagement/service/products/apis', parameters('{ParameterNames.ApimServiceName}'), '{serviceApiProduct.Name}', '{apiName}')]";
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