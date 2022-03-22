using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.GatewayApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors
{
    public class ExtractorExecutor
    {
        ExtractorParameters extractorParameters;

        readonly ILogger<ExtractorExecutor> logger;
        readonly IApisClient apisClient;

        readonly IApiExtractor apiExtractor;
        readonly IApiVersionSetExtractor apiVersionSetExtractor;
        readonly IAuthorizationServerExtractor authorizationServerExtractor;
        readonly IBackendExtractor backendExtractor;
        readonly ILoggerExtractor loggerExtractor;
        readonly IMasterTemplateExtractor masterTemplateExtractor;
        readonly IPolicyExtractor policyExtractor;
        readonly IProductApisExtractor productApisExtractor;
        readonly IProductExtractor productExtractor;
        readonly IPropertyExtractor propertyExtractor;
        readonly ITagApiExtractor apiTagExtractor;
        readonly ITagExtractor tagExtractor;
        readonly IGroupExtractor groupExtractor;
        readonly IApiRevisionExtractor apiRevisionExtractor;
        readonly IGatewayExtractor gatewayExtractor;
        readonly IGatewayApiExtractor gatewayApiExtractor;

        public ExtractorExecutor(
            ILogger<ExtractorExecutor> logger,
            IApisClient apisClient,
            IApiExtractor apiExtractor,
            IApiVersionSetExtractor apiVersionSetExtractor,
            IAuthorizationServerExtractor authorizationServerExtractor,
            IBackendExtractor backendExtractor,
            ILoggerExtractor loggerExtractor,
            IMasterTemplateExtractor masterTemplateExtractor,
            IPolicyExtractor policyExtractor,
            IProductApisExtractor productApisExtractor,
            IProductExtractor productExtractor,
            IPropertyExtractor propertyExtractor,
            ITagApiExtractor apiTagExtractor,
            ITagExtractor tagExtractor,
            IGroupExtractor groupExtractor,
            IApiRevisionExtractor apiRevisionExtractor,
            IGatewayExtractor gatewayExtractor,
            IGatewayApiExtractor gatewayApiExtractor)
        {
            this.logger = logger;
            this.apisClient = apisClient;
            this.apiExtractor = apiExtractor;
            this.apiVersionSetExtractor = apiVersionSetExtractor;
            this.authorizationServerExtractor = authorizationServerExtractor;
            this.backendExtractor = backendExtractor;
            this.loggerExtractor = loggerExtractor;
            this.masterTemplateExtractor = masterTemplateExtractor;
            this.policyExtractor = policyExtractor;
            this.productApisExtractor = productApisExtractor;
            this.propertyExtractor = propertyExtractor;
            this.productExtractor = productExtractor;
            this.apiTagExtractor = apiTagExtractor;
            this.tagExtractor = tagExtractor;
            this.groupExtractor = groupExtractor;
            this.apiRevisionExtractor = apiRevisionExtractor;
            this.gatewayExtractor = gatewayExtractor;
            this.gatewayApiExtractor = gatewayApiExtractor;
        }

        /// <summary>
        /// Allows to build ExtractorExecutor with only desired speficic extractors passed
        /// </summary>
        /// <returns>new ExtractorExecutor instance</returns>
        public static ExtractorExecutor BuildExtractorExecutor(
            ILogger<ExtractorExecutor> logger,
            IApisClient apisClient = null,
            IApiExtractor apiExtractor = null,
            IApiVersionSetExtractor apiVersionSetExtractor = null,
            IAuthorizationServerExtractor authorizationServerExtractor = null,
            IBackendExtractor backendExtractor = null,
            ILoggerExtractor loggerExtractor = null,
            IMasterTemplateExtractor masterTemplateExtractor = null,
            IPolicyExtractor policyExtractor = null,
            IProductApisExtractor productApisExtractor = null,
            IProductExtractor productExtractor = null,
            IPropertyExtractor propertyExtractor = null,
            ITagApiExtractor apiTagExtractor = null,
            ITagExtractor tagExtractor = null,
            IGroupExtractor groupExtractor = null,
            IApiRevisionExtractor apiRevisionExtractor = null,
            IGatewayExtractor gatewayExtractor = null,
            IGatewayApiExtractor gatewayApiExtractor = null)
        => new ExtractorExecutor(
                logger,
                apisClient,
                apiExtractor,
                apiVersionSetExtractor,
                authorizationServerExtractor,
                backendExtractor,
                loggerExtractor,
                masterTemplateExtractor,
                policyExtractor,
                productApisExtractor,
                productExtractor,
                propertyExtractor,
                apiTagExtractor,
                tagExtractor,
                groupExtractor,
                apiRevisionExtractor,
                gatewayExtractor,
                gatewayApiExtractor);

        public void SetExtractorParameters(ExtractorParameters extractorParameters)
        {
            this.extractorParameters = extractorParameters;
        }

        public void SetExtractorParameters(ExtractorConsoleAppConfiguration extractorConfiguration)
        {
            this.extractorParameters = new ExtractorParameters(extractorConfiguration);

            if (!string.IsNullOrEmpty(extractorConfiguration.ServiceBaseUrl))
            {
                EntityExtractorBase.BaseUrl = extractorConfiguration.ServiceBaseUrl;
            }
        }

        /// <summary>
        /// Retrieves parameters for extractor from the configuration and runs generation automatically.
        /// For specific template generation scenarios, please, use other exposed methods
        /// </summary>
        public async Task ExecuteGenerationBasedOnConfiguration()
        {
            this.logger.LogInformation("API Management Template");
            this.logger.LogInformation("Connecting to {0} API Management Service on {1} Resource Group ...", this.extractorParameters.SourceApimName, this.extractorParameters.ResourceGroup);

            if (this.extractorParameters.SplitApis)
            {
                this.logger.LogInformation("Starting templates with splitting for each API extraction...");
                await this.GenerateSplitAPITemplates();
                await this.GenerateTemplates(this.extractorParameters.FilesGenerationRootDirectory);
            }
            else if (!string.IsNullOrEmpty(this.extractorParameters.ApiVersionSetName))
            {
                this.logger.LogInformation("Starting API version set templates extraction...");
                await this.GenerateAPIVersionSetTemplates();
            }
            else if (!this.extractorParameters.MultipleApiNames.IsNullOrEmpty())
            {
                this.logger.LogInformation("Launching multiple APIs templates extraction...");
                await this.GenerateMultipleAPIsTemplates();
            }
            else if (!string.IsNullOrEmpty(this.extractorParameters.SingleApiName) && this.extractorParameters.IncludeAllRevisions)
            {
                this.logger.LogInformation("Launching single API with revisions templates extraction...");
                await this.GenerateSingleAPIWithRevisionsTemplates();
            }
            else
            {
                this.logger.LogInformation("No specific parameters are set for template generation...");
                await this.GenerateTemplates(this.extractorParameters.FilesGenerationRootDirectory, singleApiName: this.extractorParameters.SingleApiName);
            }
        }

        /// <summary>
        /// Generates policy templates in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save files</param>
        /// <returns>generated global service policy template</returns>
        public async Task<Template<PolicyTemplateResources>> GeneratePolicyTemplateAsync(string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of policy template...");

            var globalServicePolicyTemplate = await this.policyExtractor.GenerateGlobalServicePolicyTemplateAsync(
                this.extractorParameters,
                baseFilesGenerationDirectory);

            if (globalServicePolicyTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    globalServicePolicyTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.GlobalServicePolicy);
            }

            this.logger.LogInformation("Finished generation of policy template...");
            return globalServicePolicyTemplate;
        }

        /// <summary>
        /// Generates product-apis templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load product-apis from</param>
        /// <param name="multipleApiNames">multiple API names to load product-apis from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated product apis template</returns>
        public async Task<Template<ProductApiTemplateResources>> GenerateProductApisTemplateAsync(string singleApiName, List<string> multipleApiNames, string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of product-apis template...");

            var productApiTemplate = await this.productApisExtractor.GenerateProductApisTemplateAsync(singleApiName, multipleApiNames, this.extractorParameters);

            if (productApiTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    productApiTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.ProductAPIs);
            }

            this.logger.LogInformation("Finished generation of product-apis template...");
            return productApiTemplate;
        }

        /// <summary>
        /// Generates group templates in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated group template</returns>
        public async Task<Template<GroupTemplateResources>> GenerateGroupsTemplateAsync(string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of groups template...");

            var groupsTemplate = await this.groupExtractor.GenerateGroupsTemplateAsync(this.extractorParameters);

            if (groupsTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    groupsTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.Groups);
            }

            this.logger.LogInformation("Finished generation of groups template...");
            return groupsTemplate;
        }

        /// <summary>
        /// Generates product templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load products from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated products template</returns>
        public async Task<Template<ProductTemplateResources>> GenerateProductsTemplateAsync(
            string singleApiName, 
            string baseFilesGenerationDirectory,
            List<ProductApiTemplateResource> productApiResources)
        {
            this.logger.LogInformation("Started generation of products template...");

            var productTemplate = await this.productExtractor.GenerateProductsTemplateAsync(
                singleApiName, productApiResources, baseFilesGenerationDirectory, this.extractorParameters);

            if (productTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    productTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.Products);
            }

            this.logger.LogInformation("Finished generation of products template...");
            return productTemplate;
        }

        /// <summary>
        /// Generates api-version-set templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load api-version-set from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated api-version-set template</returns>
        public async Task<Template<ApiVersionSetTemplateResources>> GenerateApiVersionSetTemplateAsync(
            string singleApiName,
            string baseFilesGenerationDirectory,
            List<ApiTemplateResource> apiTemplateResources)
        {
            this.logger.LogInformation("Started generation of api-version-set template...");

            var apiVersionSetTemplate = await this.apiVersionSetExtractor.GenerateApiVersionSetTemplateAsync(
                singleApiName, apiTemplateResources, this.extractorParameters);

            if (apiVersionSetTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    apiVersionSetTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.ApiVersionSets);
            }

            this.logger.LogInformation("Finished generation of api-version-set template...");
            return apiVersionSetTemplate;
        }

        /// <summary>
        /// Generates authorization-server templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load authorization-server from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated authorization-server template</returns>
        public async Task<Template<AuthorizationServerTemplateResources>> GenerateAuthorizationServerTemplateAsync(
            string singleApiName,
            string baseFilesGenerationDirectory,
            List<ApiTemplateResource> apiTemplateResources)
        {
            this.logger.LogInformation("Started generation of authorization-server template...");

            var apiVersionSetTemplate = await this.authorizationServerExtractor.GenerateAuthorizationServersTemplateAsync(
                singleApiName, apiTemplateResources, this.extractorParameters);

            if (apiVersionSetTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    apiVersionSetTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.AuthorizationServers);
            }

            this.logger.LogInformation("Finished generation of authorization-server template...");
            return apiVersionSetTemplate;
        }

        /// <summary>
        /// Generates api templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load api from</param>
        /// <param name="multipleApiNames">multiple API names to load api from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated api template</returns>
        public async Task<Template<ApiTemplateResources>> GenerateApiTemplateAsync(
            string singleApiName,
            List<string> multipleApiNames,
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of api template...");

            var apiTemplate = await this.apiExtractor.GenerateApiTemplateAsync(
                singleApiName,
                multipleApiNames,
                baseFilesGenerationDirectory,
                this.extractorParameters);

            if (apiTemplate?.HasResources() == true)
            {
                apiTemplate.TypedResources.FileName = FileNameGenerator.GenerateExtractorAPIFileName(singleApiName, this.extractorParameters.FileNames.BaseFileName);

                await FileWriter.SaveAsJsonAsync(
                    apiTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: apiTemplate.TypedResources.FileName);
            }

            this.logger.LogInformation("Finished generation of api template...");
            return apiTemplate;
        }

        /// <summary>
        /// Generates tag templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load tag from</param>
        /// <param name="multipleApiNames">multiple API names to load tag from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated tag template</returns>
        public async Task<Template<TagTemplateResources>> GenerateTagTemplateAsync(
            string singleApiName,
            ApiTemplateResources apiTemplateResources,
            ProductTemplateResources productTemplateResources,
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of tag template...");

            var tagTemplate = await this.tagExtractor.GenerateTagsTemplateAsync(
                singleApiName,
                apiTemplateResources,
                productTemplateResources,
                this.extractorParameters);

            if (tagTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    tagTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.Tags);
            }

            this.logger.LogInformation("Finished generation of tag template...");
            return tagTemplate;
        }

        /// <summary>
        /// Generates api-tag templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load api-tag from</param>
        /// <param name="multipleApiNames">multiple API names to load api-tag from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated api-tag template</returns>
        public async Task<Template<TagApiTemplateResources>> GenerateApiTagTemplateAsync(
            string singleApiName,
            List<string> multipleApiNames,
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of api-tag template...");

            var apiTagTemplate = await this.apiTagExtractor.GenerateApiTagsTemplateAsync(singleApiName, multipleApiNames, this.extractorParameters);

            if (apiTagTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    apiTagTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.ApiTags);
            }

            this.logger.LogInformation("Finished generation of api-tag template...");
            return apiTagTemplate;
        }

        /// <summary>
        /// Generates gateway template in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load gateway from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated gateway template</returns>
        public async Task<Template<GatewayTemplateResources>> GenerateGatewayTemplateAsync(
            string singleApiName,
            string baseFilesGenerationDirectory)
        {
            if (!this.extractorParameters.ExtractGateways)
            {
                this.logger.LogInformation("Skipping GatewayTemplate generation because of configuration parameter");
                return null;
            }

            this.logger.LogInformation("Started generation of gateway template...");
            var gatewayTemplate = await this.gatewayExtractor.GenerateGatewayTemplateAsync(singleApiName, this.extractorParameters);

            if (gatewayTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    gatewayTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.Gateway);
            }

            this.logger.LogInformation("Finished generation of gateway template...");
            return gatewayTemplate;
        }

        /// <summary>
        /// Generates gateway-api template in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load gateway-api from</param>
        /// /// <param name="multipleApiNames">multiple API names to load gateway-api from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated gateway-api template</returns>
        public async Task<Template<GatewayApiTemplateResources>> GenerateGatewayApiTemplateAsync(
            string singleApiName,
            List<string> multipleApiNames,
            string baseFilesGenerationDirectory)
        {
            if (!this.extractorParameters.ExtractGateways)
            {
                this.logger.LogInformation("Skipping gateway-api template generation because of configuration parameter");
                return null;
            }

            this.logger.LogInformation("Started generation of gateway-api template...");
            var gatewayApiTemplate = await this.gatewayApiExtractor.GenerateGatewayApiTemplateAsync(singleApiName, multipleApiNames, this.extractorParameters);

            if (gatewayApiTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    gatewayApiTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.GatewayApi);
            }

            this.logger.LogInformation("Finished generation of gateway-api template...");
            return gatewayApiTemplate;
        }

        /// <summary>
        /// Generates split api templates / folders for each api in this sourceApim 
        /// </summary>
        /// <returns></returns>
        async Task GenerateSplitAPITemplates()
        {
            // Generate folders based on all apiversionset
            var apiDictionary = await this.GetAllAPIsDictionary(this.extractorParameters.SourceApimName, this.extractorParameters.ResourceGroup);

            // Generate templates based on each API/APIversionSet
            foreach (var versionSetEntry in apiDictionary)
            {
                string apiFileFolder = this.extractorParameters.FilesGenerationRootDirectory;

                // if it's APIVersionSet, generate the versionsetfolder for templates
                if (versionSetEntry.Value.Count > 1)
                {
                    // this API has VersionSet
                    string apiDisplayName = versionSetEntry.Key;

                    // create apiVersionSet folder
                    apiFileFolder = string.Concat(@apiFileFolder, $@"/{apiDisplayName}");
                    Directory.CreateDirectory(apiFileFolder);

                    // create master templates for each apiVersionSet
                    string versionSetFolder = string.Concat(@apiFileFolder, this.extractorParameters.FileNames.VersionSetMasterFolder);
                    Directory.CreateDirectory(versionSetFolder);
                    await this.GenerateTemplates(versionSetFolder, multipleApiNames: versionSetEntry.Value);

                    this.logger.LogInformation($@"Finish extracting APIVersionSet {versionSetEntry.Key}");
                }

                // Generate templates for each api 
                foreach (string apiName in versionSetEntry.Value)
                {
                    // create folder for each API
                    string tempFileFolder = string.Concat(@apiFileFolder, $@"/{apiName}");
                    Directory.CreateDirectory(tempFileFolder);
                    // generate templates for each API
                    await this.GenerateTemplates(tempFileFolder, singleApiName: apiName);

                    this.logger.LogInformation($@"Finish extracting API {apiName}");
                }
            }
        }

        /// <summary>
        /// Generates master template for each API within this version set and an extra master template to link these apis
        /// </summary>
        async Task GenerateAPIVersionSetTemplates()
        {
            // get api dictionary and check api version set
            var apiDictionary = await this.GetAllAPIsDictionary(this.extractorParameters.SourceApimName, this.extractorParameters.ResourceGroup);
            if (!apiDictionary.ContainsKey(this.extractorParameters.ApiVersionSetName))
            {
                throw new NoApiVersionSetWithSuchNameFoundException("API Version Set with this name doesn't exist");
            }
            else
            {
                this.logger.LogInformation("Started extracting the API version set {0}", this.extractorParameters.ApiVersionSetName);

                foreach (string apiName in apiDictionary[this.extractorParameters.ApiVersionSetName])
                {
                    // generate seperate folder for each API
                    string apiFileFolder = string.Concat(this.extractorParameters.FilesGenerationRootDirectory, $@"/{apiName}");
                    Directory.CreateDirectory(apiFileFolder);
                    await this.GenerateTemplates(apiFileFolder, singleApiName: apiName);
                }

                // create master templates for this apiVersionSet 
                string versionSetFolder = string.Concat(this.extractorParameters.FilesGenerationRootDirectory, this.extractorParameters.FileNames.VersionSetMasterFolder);
                Directory.CreateDirectory(versionSetFolder);
                await this.GenerateTemplates(versionSetFolder, multipleApiNames: apiDictionary[this.extractorParameters.ApiVersionSetName]);

                this.logger.LogInformation($@"Finished extracting APIVersionSet {this.extractorParameters.ApiVersionSetName}");
            }
        }

        /// <summary>
        /// Generates templates for multiple specified APIs
        /// </summary>
        async Task GenerateMultipleAPIsTemplates()
        {
            if (this.extractorParameters.MultipleApiNames.IsNullOrEmpty())
            {
                throw new Exception("MultipleAPIs parameter doesn't have any data");
            }

            this.logger.LogInformation("Started extracting multiple APIs (amount is {0} APIs)", this.extractorParameters.MultipleApiNames.Count);

            foreach (string apiName in this.extractorParameters.MultipleApiNames)
            {
                // generate seperate folder for each API
                string apiFileFolder = string.Concat(this.extractorParameters.FilesGenerationRootDirectory, $@"/{apiName}");
                Directory.CreateDirectory(apiFileFolder);
                await this.GenerateTemplates(apiFileFolder, singleApiName: apiName);
            }

            // create master templates for these apis 
            string groupApiFolder = string.Concat(this.extractorParameters.FilesGenerationRootDirectory, this.extractorParameters.FileNames.GroupAPIsMasterFolder);
            Directory.CreateDirectory(groupApiFolder);
            await this.GenerateTemplates(groupApiFolder, multipleApiNames: this.extractorParameters.MultipleApiNames);

            this.logger.LogInformation($@"Finished extracting mutiple APIs");
        }

        async Task GenerateSingleAPIWithRevisionsTemplates()
        {
            this.logger.LogInformation("Extracting singleAPI {0} with revisions", this.extractorParameters.SingleApiName);
            
            string currentRevision = null;
            List<string> revList = new List<string>();

            await foreach (var apiRevision in this.apiRevisionExtractor.GetApiRevisionsAsync(this.extractorParameters.SingleApiName, this.extractorParameters))
            {
                var apiRevisionName = apiRevision.ApiId.Split("/")[2];
                if (apiRevision.IsCurrent)
                {
                    currentRevision = apiRevisionName;
                }

                // creating a folder for this api revision
                var revFileFolder = Path.Combine(this.extractorParameters.FilesGenerationRootDirectory, apiRevisionName);
                Directory.CreateDirectory(revFileFolder);
                revList.Add(apiRevisionName);

                await this.GenerateTemplates(revFileFolder, singleApiName: apiRevisionName);
            }

            if (currentRevision is null)
            {
                throw new ApiRevisionNotFoundException($"Revision {this.extractorParameters.SingleApiName} doesn't exist, something went wrong!");
            }
            
            // generate revisions master folder
            var revisionMasterFolder = Path.Combine(this.extractorParameters.FilesGenerationRootDirectory, this.extractorParameters.FileNames.RevisionMasterFolder);
            Directory.CreateDirectory(revisionMasterFolder);

            var apiRevisionTemplate = await this.apiRevisionExtractor.GenerateApiRevisionTemplateAsync(
                currentRevision,
                revList,
                revisionMasterFolder,
                this.extractorParameters);

            await this.GenerateTemplates(revisionMasterFolder, apiTemplate: apiRevisionTemplate);
        }

        /// <summary>
        /// three condistions to use this function:
        /// 1. singleApiName is null, then generate one master template for the multipleAPIs in multipleApiNams
        /// 2. multipleApiNams is null, then generate separate folder and master template for each API 
        /// 3. when both singleApiName and multipleApiNams is null, then generate one master template to link all apis in the sourceapim
        /// </summary>
        async Task GenerateTemplates(
            string baseFilesGenerationDirectory,
            string singleApiName = null,
            List<string> multipleApiNames = null,
            Template<ApiTemplateResources> apiTemplate = null)
        {
            if (!string.IsNullOrEmpty(singleApiName) && !multipleApiNames.IsNullOrEmpty())
            {
                throw new SingleAndMultipleApisCanNotExistTogetherException("Can't specify single API and multiple APIs to extract at the same time");
            }

            // Get all Apis that will be extracted
            List<string> apisToExtract = new List<string>();
            if (!string.IsNullOrEmpty(singleApiName))
            {
                apisToExtract.Add(singleApiName);
            }
            else if (!multipleApiNames.IsNullOrEmpty())
            {
                apisToExtract.AddRange(multipleApiNames);
            }
            else
            {
                this.logger.LogInformation($"There were no `{nameof(singleApiName)}` or `{nameof(multipleApiNames)}` specified. Loading all API names from {nameof(ExtractorParameters.SourceApimName)} directly...");

                var apis = await this.apisClient.GetAllAsync(this.extractorParameters);
                if (apis.IsNullOrEmpty())
                {
                    this.logger.LogError("There were no apis found for api management {0}", this.extractorParameters.SourceApimName);
                    throw new ServiceApiNotFoundException("No apis found for api management");
                }

                var apiNames = apis.Select(api => api.Name);
                apisToExtract.AddRange(apiNames);
            }

            // generate different templates using extractors and write to output
            apiTemplate = apiTemplate ?? await this.GenerateApiTemplateAsync(singleApiName, multipleApiNames, baseFilesGenerationDirectory);

            // refactored
            var globalServicePolicyTemplate = await this.GeneratePolicyTemplateAsync(baseFilesGenerationDirectory);
            var productApiTemplate = await this.GenerateProductApisTemplateAsync(singleApiName, multipleApiNames, baseFilesGenerationDirectory);
            var productTemplate = await this.GenerateProductsTemplateAsync(singleApiName, baseFilesGenerationDirectory, apiTemplate.TypedResources.ApiProducts);
            var apiVersionSetTemplate = await this.GenerateApiVersionSetTemplateAsync(singleApiName, baseFilesGenerationDirectory, apiTemplate.TypedResources.Apis);
            var authorizationServerTemplate = await this.GenerateAuthorizationServerTemplateAsync(singleApiName, baseFilesGenerationDirectory, apiTemplate.TypedResources.Apis);
            var tagTemplate = await this.GenerateTagTemplateAsync(singleApiName, apiTemplate.TypedResources, productTemplate.TypedResources, baseFilesGenerationDirectory);
            var apiTagTemplate = await this.GenerateApiTagTemplateAsync(singleApiName, multipleApiNames, baseFilesGenerationDirectory);
            await this.GenerateGroupsTemplateAsync(baseFilesGenerationDirectory);
            await this.GenerateGatewayTemplateAsync(singleApiName, baseFilesGenerationDirectory);

            // not refactored
            Dictionary<string, object> apiLoggerId = null;
            if (this.extractorParameters.ParameterizeApiLoggerId)
            {
                apiLoggerId = await this.GetAllReferencedLoggers(apisToExtract, this.extractorParameters);
            }

            var apiTemplateResources = apiTemplate.Resources.ToList();
            Template loggerTemplate = await this.loggerExtractor.GenerateLoggerTemplateAsync(this.extractorParameters, singleApiName, apiTemplateResources, apiLoggerId);
            List<TemplateResource> loggerResources = loggerTemplate.Resources.ToList();
            Template namedValueTemplate = await this.propertyExtractor.GenerateNamedValuesTemplateAsync(singleApiName, apiTemplateResources, this.extractorParameters, this.backendExtractor, loggerResources);
            List<TemplateResource> namedValueResources = namedValueTemplate.Resources.ToList();
            var backendResult = await this.backendExtractor.GenerateBackendsARMTemplateAsync(this.extractorParameters.SourceApimName, this.extractorParameters.ResourceGroup, singleApiName, apiTemplateResources, namedValueResources, this.extractorParameters);

            Dictionary<string, string> loggerResourceIds = null;
            if (this.extractorParameters.ParameterizeLogResourceId)
            {
                loggerResourceIds = LoggerTemplateUtils.GetAllLoggerResourceIds(loggerResources);
                loggerTemplate = LoggerTemplateUtils.SetLoggerResourceId(loggerTemplate);
            }

            // create parameters file
            Template templateParameters = await this.masterTemplateExtractor.CreateMasterTemplateParameterValues(apisToExtract, this.extractorParameters, apiLoggerId, loggerResourceIds, backendResult.Item2, namedValueResources);

            
            // won't generate template when there is no resources
            if (!backendResult.Item1.Resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(backendResult.Item1, string.Concat(baseFilesGenerationDirectory, this.extractorParameters.FileNames.Backends));
            }
            if (!loggerTemplate.Resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(loggerTemplate, string.Concat(baseFilesGenerationDirectory, this.extractorParameters.FileNames.Loggers));
            }
            if (!namedValueTemplate.Resources.IsNullOrEmpty())
            {
                FileWriter.WriteJSONToFile(namedValueTemplate, string.Concat(baseFilesGenerationDirectory, this.extractorParameters.FileNames.NamedValues));
            }

            if (this.extractorParameters.LinkedTemplatesBaseUrl != null)
            {
                // create a master template that links to all other templates
                Template masterTemplate = this.masterTemplateExtractor.GenerateLinkedMasterTemplate(
                    apiTemplate, globalServicePolicyTemplate, apiVersionSetTemplate, productTemplate, productApiTemplate,
                    apiTagTemplate, loggerTemplate, backendResult.Item1, authorizationServerTemplate, namedValueTemplate,
                    tagTemplate, this.extractorParameters.FileNames, this.extractorParameters);

                FileWriter.WriteJSONToFile(masterTemplate, string.Concat(baseFilesGenerationDirectory, this.extractorParameters.FileNames.LinkedMaster));
            }

            // write parameters to outputLocation
            FileWriter.WriteJSONToFile(templateParameters, string.Concat(baseFilesGenerationDirectory, this.extractorParameters.FileNames.Parameters));
        }

        /// <summary>
        /// Generates an api dictionary with apiName/versionsetName (if exist one) as key, list of apiNames as value
        /// </summary>
        /// <returns></returns>
        async Task<Dictionary<string, List<string>>> GetAllAPIsDictionary(string sourceApim, string resourceGroup)
        {
            // pull all apis from service
            JToken[] apis = await this.apiExtractor.GetAllApiObjsAsync(sourceApim, resourceGroup);

            // Generate folders based on all apiversionset
            var apiDictionary = new Dictionary<string, List<string>>();
            foreach (JToken oApi in apis)
            {
                string apiDisplayName = ((JValue)oApi["properties"]["displayName"]).Value.ToString();
                if (!apiDictionary.ContainsKey(apiDisplayName))
                {
                    List<string> apiVersionSet = new List<string>();
                    apiVersionSet.Add(((JValue)oApi["name"]).Value.ToString());
                    apiDictionary[apiDisplayName] = apiVersionSet;
                }
                else
                {
                    apiDictionary[apiDisplayName].Add(((JValue)oApi["name"]).Value.ToString());
                }
            }
            return apiDictionary;
        }

        async Task<Dictionary<string, object>> GetAllReferencedLoggers(List<string> apisToExtract, ExtractorParameters extractorParameters)
        {
            var apiLoggerId = new Dictionary<string, object>();

            var serviceDiagnostics = await this.apiExtractor.GetServiceDiagnosticsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup);
            JObject oServiceDiagnostics = JObject.Parse(serviceDiagnostics);

            var serviceloggerIds = new Dictionary<string, string>();
            foreach (var serviceDiagnostic in oServiceDiagnostics["value"])
            {
                string diagnosticName = ((JValue)serviceDiagnostic["name"]).Value.ToString();
                string loggerId = ((JValue)serviceDiagnostic["properties"]["loggerId"]).Value.ToString();
                apiLoggerId.Add(ParameterNamingHelper.GenerateValidParameterName(diagnosticName, ParameterPrefix.Diagnostic), loggerId);
            }


            foreach (string curApiName in apisToExtract)
            {
                var loggerIds = new Dictionary<string, string>();
                var diagnostics = await this.apiExtractor.GetApiDiagnosticsAsync(extractorParameters.SourceApimName, extractorParameters.ResourceGroup, curApiName);
                JObject oDiagnostics = JObject.Parse(diagnostics);
                foreach (var diagnostic in oDiagnostics["value"])
                {
                    string diagnosticName = ((JValue)diagnostic["name"]).Value.ToString();
                    string loggerId = ((JValue)diagnostic["properties"]["loggerId"]).Value.ToString();
                    loggerIds.Add(ParameterNamingHelper.GenerateValidParameterName(diagnosticName, ParameterPrefix.Diagnostic), loggerId);
                }
                if (loggerIds.Count != 0)
                {
                    apiLoggerId.Add(ParameterNamingHelper.GenerateValidParameterName(curApiName, ParameterPrefix.Api), loggerIds);
                }
            }

            return apiLoggerId;
        }
    }
}
