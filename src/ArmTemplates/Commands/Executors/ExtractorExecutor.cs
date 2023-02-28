// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Utils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiManagementService;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiReleases;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiVersionSet;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.AuthorizationServer;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Gateway;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.GatewayApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.IdentityProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Master;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.OpenIdConnectProviders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.PolicyFragments;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.TagApi;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.EntityExtractors.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;
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
        readonly IApiClientUtils apiClientUtils;

        readonly IApiExtractor apiExtractor;
        readonly IApiVersionSetExtractor apiVersionSetExtractor;
        readonly IAuthorizationServerExtractor authorizationServerExtractor;
        readonly IBackendExtractor backendExtractor;
        readonly ILoggerExtractor loggerExtractor;
        readonly IParametersExtractor parametersExtractor;
        readonly IMasterTemplateExtractor masterTemplateExtractor;
        readonly IPolicyExtractor policyExtractor;
        readonly IProductApisExtractor productApisExtractor;
        readonly IProductExtractor productExtractor;
        readonly INamedValuesExtractor namedValuesExtractor;
        readonly ITagApiExtractor tagApiExtractor;
        readonly ITagExtractor tagExtractor;
        readonly IGroupExtractor groupExtractor;
        readonly IApiRevisionExtractor apiRevisionExtractor;
        readonly IGatewayExtractor gatewayExtractor;
        readonly IGatewayApiExtractor gatewayApiExtractor;
        readonly IIdentityProviderExtractor identityProviderExtractor;
        readonly IApiManagementServiceExtractor apiManagementServiceExtractor;
        readonly ISchemaExtractor schemaExtractor;
        readonly IOpenIdConnectProviderExtractor openIdConnectProviderExtractor;
        readonly IPolicyFragmentsExtractor policyFragmentsExtractor;
        readonly IApiReleaseExtractor apiReleaseExtractor;
 
        public ExtractorExecutor(
            ILogger<ExtractorExecutor> logger,
            IApisClient apisClient,
            IApiExtractor apiExtractor,
            IApiVersionSetExtractor apiVersionSetExtractor,
            IAuthorizationServerExtractor authorizationServerExtractor,
            IBackendExtractor backendExtractor,
            ILoggerExtractor loggerExtractor,
            IParametersExtractor parametersExtractor,
            IMasterTemplateExtractor masterTemplateExtractor,
            IPolicyExtractor policyExtractor,
            IProductApisExtractor productApisExtractor,
            IProductExtractor productExtractor,
            INamedValuesExtractor namedValuesExtractor,
            ITagApiExtractor tagApiExtractor,
            ITagExtractor tagExtractor,
            IGroupExtractor groupExtractor,
            IApiRevisionExtractor apiRevisionExtractor,
            IGatewayExtractor gatewayExtractor,
            IGatewayApiExtractor gatewayApiExtractor,
            IIdentityProviderExtractor identityProviderExtractor,
            IApiManagementServiceExtractor apiManagementServiceExtractor,
            ISchemaExtractor schemaExtractor,
            IOpenIdConnectProviderExtractor openIdConnectProviderExtractor,
            IPolicyFragmentsExtractor policyFragmentsExtractor,
            IApiReleaseExtractor apiReleaseExtractor,
            IApiClientUtils apiClientUtils)
        {
            this.logger = logger;
            this.apisClient = apisClient;
            this.apiExtractor = apiExtractor;
            this.apiVersionSetExtractor = apiVersionSetExtractor;
            this.authorizationServerExtractor = authorizationServerExtractor;
            this.backendExtractor = backendExtractor;
            this.loggerExtractor = loggerExtractor;
            this.parametersExtractor = parametersExtractor;
            this.masterTemplateExtractor = masterTemplateExtractor;
            this.policyExtractor = policyExtractor;
            this.productApisExtractor = productApisExtractor;
            this.namedValuesExtractor = namedValuesExtractor;
            this.productExtractor = productExtractor;
            this.tagApiExtractor = tagApiExtractor;
            this.tagExtractor = tagExtractor;
            this.groupExtractor = groupExtractor;
            this.apiRevisionExtractor = apiRevisionExtractor;
            this.gatewayExtractor = gatewayExtractor;
            this.gatewayApiExtractor = gatewayApiExtractor;
            this.identityProviderExtractor = identityProviderExtractor;
            this.apiManagementServiceExtractor = apiManagementServiceExtractor;
            this.schemaExtractor = schemaExtractor;
            this.openIdConnectProviderExtractor = openIdConnectProviderExtractor;
            this.policyFragmentsExtractor = policyFragmentsExtractor;
            this.apiReleaseExtractor = apiReleaseExtractor;
            this.apiClientUtils = apiClientUtils;
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
            IParametersExtractor parametersExtractor = null,
            IMasterTemplateExtractor masterTemplateExtractor = null,
            IPolicyExtractor policyExtractor = null,
            IProductApisExtractor productApisExtractor = null,
            IProductExtractor productExtractor = null,
            INamedValuesExtractor namedValuesExtractor = null,
            ITagApiExtractor tagApiExtractor = null,
            ITagExtractor tagExtractor = null,
            IGroupExtractor groupExtractor = null,
            IApiRevisionExtractor apiRevisionExtractor = null,
            IGatewayExtractor gatewayExtractor = null,
            IGatewayApiExtractor gatewayApiExtractor = null,
            IIdentityProviderExtractor identityProviderExtractor = null,
            IApiManagementServiceExtractor apiManagementServiceExtractor = null,
            ISchemaExtractor schemaExtractor = null,
            IOpenIdConnectProviderExtractor openIdConnectProviderExtractor = null,
            IPolicyFragmentsExtractor policyFragmentsExtractor = null,
            IApiReleaseExtractor apiReleaseExtractor = null,
            IApiClientUtils apiClientUtils = null)
        => new ExtractorExecutor(
                logger,
                apisClient,
                apiExtractor,
                apiVersionSetExtractor,
                authorizationServerExtractor,
                backendExtractor,
                loggerExtractor,
                parametersExtractor,
                masterTemplateExtractor,
                policyExtractor,
                productApisExtractor,
                productExtractor,
                namedValuesExtractor,
                tagApiExtractor,
                tagExtractor,
                groupExtractor,
                apiRevisionExtractor,
                gatewayExtractor,
                gatewayApiExtractor,
                identityProviderExtractor,
                apiManagementServiceExtractor,
                schemaExtractor,
                openIdConnectProviderExtractor,
                policyFragmentsExtractor,
                apiReleaseExtractor,
                apiClientUtils);

        public void SetExtractorParameters(ExtractorParameters extractorParameters)
        {
            this.extractorParameters = extractorParameters;
        }

        public void SetExtractorParameters(ExtractorConsoleAppConfiguration extractorConfiguration)
        {
            this.extractorParameters = new ExtractorParameters(extractorConfiguration);

            if (!string.IsNullOrEmpty(extractorConfiguration.ServiceBaseUrl))
            {
                GlobalConstants.BaseManagementAzureUrl = extractorConfiguration.ServiceBaseUrl;
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
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of products template...");

            var productTemplate = await this.productExtractor.GenerateProductsTemplateAsync(
                singleApiName, baseFilesGenerationDirectory, this.extractorParameters);

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
                apiTemplate.TypedResources.ParametersFileName = FileNameGenerator.GenerateExtractorAPIParametersFileName(singleApiName);

                await FileWriter.SaveAsJsonAsync(
                    apiTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: apiTemplate.TypedResources.FileName);
            }

            this.logger.LogInformation("Finished generation of api template...");
            return apiTemplate;
        }

        /// <summary>
        /// Generates parameters template in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated parameters template</returns>
        public async Task<Template> GenerateParametersTemplateAsync(
            List<string> apisToExtract,
            LoggerTemplateResources loggerResources,
            BackendTemplateResources backendResources,
            NamedValuesResources namedValuesResources,
            IdentityProviderResources identityProviderResources,
            OpenIdConnectProviderResources openIdConnectProviderResources,
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of parameters template...");

            var templateParameters = await this.parametersExtractor.CreateMasterTemplateParameterValues(
                apisToExtract,
                this.loggerExtractor.Cache,
                loggerResources,
                backendResources,
                namedValuesResources,
                identityProviderResources,
                openIdConnectProviderResources,
                this.extractorParameters);

            if (!templateParameters.Parameters.IsNullOrEmpty())
            {
                await FileWriter.SaveAsJsonAsync(
                    templateParameters,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.Parameters);
            }

            this.logger.LogInformation("Finished generation of parameters template...");
            return templateParameters;
        }

        public async Task GenerateResourceParametersFiles(
            string baseFilesGenerationDirectory,
            Template mainParametersTemplate,
            Template<ApiTemplateResources> apiTemplate = null,
            Template<PolicyTemplateResources> policyTemplate = null,
            Template<ApiVersionSetTemplateResources> apiVersionSetTemplate = null,
            Template<ProductTemplateResources> productsTemplate = null,
            Template<ProductApiTemplateResources> productApisTemplate = null,
            Template<TagApiTemplateResources> apiTagsTemplate = null,
            Template<LoggerTemplateResources> loggersTemplate = null,
            Template<BackendTemplateResources> backendsTemplate = null,
            Template<AuthorizationServerTemplateResources> authorizationServersTemplate = null,
            Template<NamedValuesResources> namedValuesTemplate = null,
            Template<TagTemplateResources> tagTemplate = null,
            Template<GroupTemplateResources> groupTemplate = null,
            Template<IdentityProviderResources> identityProviderTemplate = null,
            Template<SchemaTemplateResources> schemaTemplate = null,
            Template<OpenIdConnectProviderResources> openIdConnectProviderTemplate = null,
            Template<PolicyFragmentsResources> policyFragmentsTemplate = null,
            Template<ApiReleaseTemplateResources> apiReleaseTemplate = null)
        {
            this.RenameExistingParametersDirectory(baseFilesGenerationDirectory);

            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, apiTemplate?.TypedResources.ParametersFileName, apiTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.GlobalServicePolicyParameters , policyTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.ApiVersionSetsParameters, apiVersionSetTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.ProductsParameters, productsTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.ProductAPIsParameters, productApisTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.TagApiParameters, apiTagsTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.LoggersParameters, loggersTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.BackendsParameters, backendsTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.AuthorizationServersParameters, authorizationServersTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.NamedValuesParameters, namedValuesTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.TagsParameters, tagTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.GroupsParameters, groupTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.IdentityProvidersParameters, identityProviderTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.SchemaParameters, schemaTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.OpenIdConnectProvidersParameters, openIdConnectProviderTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.PolicyFragmentsParameters, policyFragmentsTemplate, mainParametersTemplate);
            await this.GenerateResourceParametersFile(baseFilesGenerationDirectory, this.extractorParameters.FileNames.ApiReleaseParameters, apiReleaseTemplate, mainParametersTemplate);
        }

        public async Task GenerateResourceParametersFile<TTemplateResource>(string baseFilesGenerationDirectory, string fileName, Template<TTemplateResource> resourceTemplate, Template mainParametersTemplate) where TTemplateResource : ITemplateResources, new()
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(baseFilesGenerationDirectory))
            {
                this.logger.LogWarning("Filename or directory for parameters file generation is null or empty, skipping the generation");
                return;
            }

            if (resourceTemplate?.TypedResources?.HasContent() == true)
            {
                var parametersTemplate = this.parametersExtractor.CreateResourceTemplateParameterTemplate(resourceTemplate, mainParametersTemplate);

                if (!parametersTemplate.Parameters.IsNullOrEmpty())
                {
                    await FileWriter.SaveAsJsonAsync(
                    parametersTemplate,
                    directory: Path.Combine(baseFilesGenerationDirectory, this.extractorParameters.FileNames.ParametersDirectory),
                    fileName: fileName);
                }
            }
        }

        void RenameExistingParametersDirectory(string baseDirectoryLocation)
        {
            var parametersLocation = Path.Combine(baseDirectoryLocation, this.extractorParameters.FileNames.ParametersDirectory);
            if (Directory.Exists(parametersLocation))
            {
                var creationDateTime = Directory.GetCreationTime(parametersLocation).ToString("yyyyMMddHHmmss");
                var newParameterDirectory = $"{this.extractorParameters.FileNames.ParametersDirectory}{creationDateTime}";
                Directory.Move(parametersLocation, Path.Combine(baseDirectoryLocation, newParameterDirectory));
            }
        }

        public async Task<Template<MasterTemplateResources>> GenerateMasterTemplateAsync(
           string baseFilesGenerationDirectory,
           ApiTemplateResources apiTemplateResources = null,
           PolicyTemplateResources policyTemplateResources = null,
           ApiVersionSetTemplateResources apiVersionSetTemplateResources = null,
           ProductTemplateResources productsTemplateResources = null,
           ProductApiTemplateResources productApisTemplateResources = null,
           TagApiTemplateResources apiTagsTemplateResources = null,
           LoggerTemplateResources loggersTemplateResources = null,
           BackendTemplateResources backendsTemplateResources = null,
           AuthorizationServerTemplateResources authorizationServersTemplateResources = null,
           NamedValuesResources namedValuesTemplateResources = null,
           TagTemplateResources tagTemplateResources = null,
           GroupTemplateResources groupTemplateResources = null,
           IdentityProviderResources identityProviderTemplateResources = null,
           SchemaTemplateResources schemaTemplateResources = null,
           OpenIdConnectProviderResources openIdConnectProviderResources = null,
           PolicyFragmentsResources policyFragmentsResources = null)
        {
            if (string.IsNullOrEmpty(this.extractorParameters.LinkedTemplatesBaseUrl))
            {
                this.logger.LogInformation("'{0}' is not passed. Skipping master-template generation.", nameof(this.extractorParameters.LinkedTemplatesBaseUrl));
                return null;
            }

            this.logger.LogInformation("Started generation of master template...");

            var masterTemplate = this.masterTemplateExtractor.GenerateLinkedMasterTemplate(
                this.extractorParameters, apiTemplateResources, policyTemplateResources, apiVersionSetTemplateResources,
                productsTemplateResources, productApisTemplateResources, apiTagsTemplateResources, loggersTemplateResources,
                backendsTemplateResources, authorizationServersTemplateResources, namedValuesTemplateResources, tagTemplateResources, 
                groupTemplateResources, identityProviderTemplateResources, schemaTemplateResources, openIdConnectProviderResources, policyFragmentsResources);

            if (masterTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    masterTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.LinkedMaster);
            }

            this.logger.LogInformation("Finished generation of master template...");
            return masterTemplate;
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
        /// Generates tag-api templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load tag-api from</param>
        /// <param name="multipleApiNames">multiple API names to load tag-api from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated tag-api template</returns>
        public async Task<Template<TagApiTemplateResources>> GenerateTagApiTemplateAsync(
            string singleApiName,
            List<string> multipleApiNames,
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of tag-api template...");

            var apiTagTemplate = await this.tagApiExtractor.GenerateApiTagsTemplateAsync(singleApiName, multipleApiNames, this.extractorParameters);

            if (apiTagTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    apiTagTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.TagApi);
            }

            this.logger.LogInformation("Finished generation of tag-api template...");
            return apiTagTemplate;
        }

        /// <summary>
        /// Generates backend templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load backend from</param>
        /// <param name="multipleApiNames">multiple API names to load backend from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated backend template</returns>
        public async Task<Template<BackendTemplateResources>> GenerateBackendTemplateAsync(
            string singleApiName,
            List<PolicyTemplateResource> apiPolicies,
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of backend template...");

            var backendTemplate = await this.backendExtractor.GenerateBackendsTemplateAsync(
                singleApiName,
                apiPolicies,
                baseFilesGenerationDirectory,
                this.extractorParameters);

            if (backendTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    backendTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.Backends);
            }

            this.logger.LogInformation("Finished generation of backend template...");
            return backendTemplate;
        }

        /// <summary>
        /// Generates named-values templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load named-values from</param>
        /// <param name="multipleApiNames">multiple API names to load named-values from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated named-values template</returns>
        public async Task<Template<NamedValuesResources>> GenerateNamedValuesTemplateAsync(
            string singleApiName,
            List<PolicyTemplateResource> apiPolicies,
            List<LoggerTemplateResource> loggerResources,
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of named-values template...");

            var namedValuesTemplate = await this.namedValuesExtractor.GenerateNamedValuesTemplateAsync(
                singleApiName,
                apiPolicies,
                loggerResources,
                this.extractorParameters,
                baseFilesGenerationDirectory);

            if (namedValuesTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    namedValuesTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.NamedValues);
            }

            this.logger.LogInformation("Finished generation of named-values template...");
            return namedValuesTemplate;
        }

        /// <summary>
        /// Generates logger templates in the desired folder
        /// </summary>
        /// <param name="singleApiName">name of API to load logger from</param>
        /// <param name="multipleApiNames">multiple API names to load logger from</param>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated logger template</returns>
        public async Task<Template<LoggerTemplateResources>> GenerateLoggerTemplateAsync(
            List<string> apisToExtract,
            List<PolicyTemplateResource> apiPolicies,
            string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of logger template...");

            var loggerTemplate = await this.loggerExtractor.GenerateLoggerTemplateAsync(
                apisToExtract,
                apiPolicies,
                this.extractorParameters);

            if (this.extractorParameters.ParameterizeLogResourceId)
            {
                loggerTemplate.TypedResources.FormAllLoggerResourceIdsCache();
                loggerTemplate.TypedResources.SetLoggerResourceIdForEachLogger();
            }

            if (loggerTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    loggerTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.Loggers);
            }

            this.logger.LogInformation("Finished generation of logger template...");
            return loggerTemplate;
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
        /// Generates identity providers template in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated identity provider template</returns>
        public async Task<Template<IdentityProviderResources>> GenerateIdentityProviderTemplateAsync(string baseFilesGenerationDirectory)
        {
            if (!this.extractorParameters.ExtractIdentityProviders)
            {
                this.logger.LogInformation($"Skipping identityProvider extraction due to the setting parameter");
                return Template<IdentityProviderResources>.Empty;
            }

            this.logger.LogInformation("Started generation of identity provider template...");

            var identityProviderTemplate = await this.identityProviderExtractor.GenerateIdentityProvidersTemplateAsync(this.extractorParameters);

            if (identityProviderTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    identityProviderTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.IdentityProviders);
            }

            this.logger.LogInformation("Finished generation of identity providers template...");
            return identityProviderTemplate;
        }

        /// <summary>
        /// Generates openId connect providers template in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated openId connect provider template</returns>
        public async Task<Template<OpenIdConnectProviderResources>> GenerateOpenIdConnectProviderTemplateAsync(string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of openId connect provider template...");

            var openIdConnectProviderTemplate = await this.openIdConnectProviderExtractor.GenerateOpenIdConnectProvidersTemplateAsync(this.extractorParameters);

            if (openIdConnectProviderTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    openIdConnectProviderTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.OpenIdConnectProviders);
            }

            this.logger.LogInformation("Finished generation of openId connect providers template...");
            return openIdConnectProviderTemplate;
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

        public async Task<Template<ApiManagementServiceResources>> GenerateApiManagementServiceTemplate(string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of ApiManagement service template...");
            var apiManagementServiceTemplate = await this.apiManagementServiceExtractor.GenerateApiManagementServicesTemplateAsync(this.extractorParameters);

            if (apiManagementServiceTemplate?.HasResources() == true)
            {

                var typedService = apiManagementServiceTemplate.TypedResources.ApiManagementServices.FirstOrDefault();
                if (typedService.Sku is not null)
                {
                    this.extractorParameters.SetSkuType(typedService.Sku.Name);
                }
                
                await FileWriter.SaveAsJsonAsync(
                    apiManagementServiceTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.ApiManagementService);
            }
            
            this.logger.LogInformation("Finished generation of identity providers template...");
            return apiManagementServiceTemplate;
        }

        /// <summary>
        /// Generates schema templates in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated schemas template</returns>
        public async Task<Template<SchemaTemplateResources>> GenerateSchemasTemplateAsync(string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of schemas template...");

            var schemasTemplate = await this.schemaExtractor.GenerateSchemasTemplateAsync(this.extractorParameters);

            if (schemasTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    schemasTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.Schema);
            }

            this.logger.LogInformation("Finished generation of schemas template...");
            return schemasTemplate;
        }

        /// <summary>
        /// Generates api release template in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated api release template</returns>
        public async Task<Template<ApiReleaseTemplateResources>> GenerateApiReleaseTemplateAsync(string apiName, string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of api release template...");

            var apiReleaseTemplate = this.apiReleaseExtractor.GenerateSingleApiReleaseTemplate(apiName, this.extractorParameters);

            if (apiReleaseTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    apiReleaseTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.ApiRelease);
            }

            this.logger.LogInformation("Finished generation of apiRelease template...");
            return apiReleaseTemplate;
        }

        /// <summary>
        /// Generates api release template in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated api release template</returns>
        public async Task<Template<ApiReleaseTemplateResources>> GenerateApiReleasesTemplateAsync(string baseFilesGenerationDirectory)
        {
            if (!string.IsNullOrEmpty(this.extractorParameters.SingleApiName) || !this.extractorParameters.MultipleApiNames.IsNullOrEmpty())
            {
                this.logger.LogInformation("Skip generation of api releases template: not all apis are extracted");
                return null;
            }

            this.logger.LogInformation("Started generation of api releases template...");

            var apiReleaseTemplate = await this.apiReleaseExtractor.GenerateCurrentApiReleaseTemplate(this.extractorParameters);

            if (apiReleaseTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    apiReleaseTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.ApiRelease);
            }

            this.logger.LogInformation("Finished generation of apiReleases template...");
            return apiReleaseTemplate;
        }

        /// Generates policy fragments templates in the desired folder
        /// </summary>
        /// <param name="baseFilesGenerationDirectory">name of base folder where to save output files</param>
        /// <returns>generated policy fragments template</returns>
        public async Task<Template<PolicyFragmentsResources>> GeneratePolicyFragmentsTemplateAsync(List<PolicyTemplateResource> apiPolicies, string baseFilesGenerationDirectory)
        {
            this.logger.LogInformation("Started generation of policy fragments template...");

            var policyFragmentTemplate = await this.policyFragmentsExtractor.GeneratePolicyFragmentsTemplateAsync(apiPolicies, this.extractorParameters, baseFilesGenerationDirectory);

            if (policyFragmentTemplate?.HasResources() == true)
            {
                await FileWriter.SaveAsJsonAsync(
                    policyFragmentTemplate,
                    directory: baseFilesGenerationDirectory,
                    fileName: this.extractorParameters.FileNames.PolicyFragments);
            }

            this.logger.LogInformation("Finished generation of policy fragments template...");
            return policyFragmentTemplate;
        }

        /// <summary>
        /// Generates split api templates / folders for each api in this sourceApim 
        /// </summary>
        /// <returns></returns>
        async Task GenerateSplitAPITemplates()
        {
            // Generate folders based on all apiversionset
            var apiDictionary = await this.GetAllAPIsDictionary(this.extractorParameters);

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
        public async Task GenerateAPIVersionSetTemplates()
        {
            // get api dictionary and check api version set
            var apiDictionary = await this.apiClientUtils.GetAllAPIsDictionaryByVersionSetName(this.extractorParameters);
            if (!apiDictionary.ContainsKey(this.extractorParameters.ApiVersionSetName))
            {
                throw new NoApiVersionSetWithSuchNameFoundException(ErrorMessages.ApiVersionDoesNotExistErrorMessage);
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
                string versionSetFolder = Path.Combine(this.extractorParameters.FilesGenerationRootDirectory, this.extractorParameters.FileNames.VersionSetMasterFolder);
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
            bool generateSingleApiReleaseTemplate;
            List<string> revList = new List<string>();

            await foreach (var apiRevision in this.apiRevisionExtractor.GetApiRevisionsAsync(this.extractorParameters.SingleApiName, this.extractorParameters))
            {
                generateSingleApiReleaseTemplate = false;
                var apiRevisionName = apiRevision.ApiId.Split("/")[2];
                if (apiRevision.IsCurrent)
                {
                    currentRevision = apiRevisionName;
                    generateSingleApiReleaseTemplate = true;
                }

                // creating a folder for this api revision
                var revFileFolder = Path.Combine(this.extractorParameters.FilesGenerationRootDirectory, apiRevisionName);
                Directory.CreateDirectory(revFileFolder);
                revList.Add(apiRevisionName);

                await this.GenerateTemplates(revFileFolder, singleApiName: apiRevisionName);
                
                if (generateSingleApiReleaseTemplate)
                {
                    await this.GenerateApiReleaseTemplateAsync(apiRevisionName, revFileFolder);
                }
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
            
            // Fetch ApiManagement Service instance template 
            await this.GenerateApiManagementServiceTemplate(baseFilesGenerationDirectory);

            var apisToExtract = await this.GetApiNamesToExtract(singleApiName, multipleApiNames);
            // generate different templates using extractors and write to output
            apiTemplate = apiTemplate ?? await this.GenerateApiTemplateAsync(singleApiName, multipleApiNames, baseFilesGenerationDirectory);
            
            var globalServicePolicyTemplate = await this.GeneratePolicyTemplateAsync(baseFilesGenerationDirectory);
            var productApiTemplate = await this.GenerateProductApisTemplateAsync(singleApiName, multipleApiNames, baseFilesGenerationDirectory);
            var productTemplate = await this.GenerateProductsTemplateAsync(singleApiName, baseFilesGenerationDirectory);
            var apiVersionSetTemplate = await this.GenerateApiVersionSetTemplateAsync(singleApiName, baseFilesGenerationDirectory, apiTemplate.TypedResources.Apis);
            var authorizationServerTemplate = await this.GenerateAuthorizationServerTemplateAsync(singleApiName, baseFilesGenerationDirectory, apiTemplate.TypedResources.Apis);
            var tagTemplate = await this.GenerateTagTemplateAsync(singleApiName, apiTemplate.TypedResources, productTemplate.TypedResources, baseFilesGenerationDirectory);
            var apiTagTemplate = await this.GenerateTagApiTemplateAsync(singleApiName, multipleApiNames, baseFilesGenerationDirectory);
            var loggerTemplate = await this.GenerateLoggerTemplateAsync(apisToExtract, apiTemplate.TypedResources.GetAllPolicies(), baseFilesGenerationDirectory);
            var namedValueTemplate = await this.GenerateNamedValuesTemplateAsync(singleApiName, apiTemplate.TypedResources.GetAllPolicies(), loggerTemplate.TypedResources.Loggers, baseFilesGenerationDirectory);
            var backendTemplate = await this.GenerateBackendTemplateAsync(singleApiName, apiTemplate.TypedResources.GetAllPolicies(), baseFilesGenerationDirectory);
            var groupTemplate = await this.GenerateGroupsTemplateAsync(baseFilesGenerationDirectory);
            var identityProviderTemplate = await this.GenerateIdentityProviderTemplateAsync(baseFilesGenerationDirectory);
            var openIdConnectProviderTemplate = await this.GenerateOpenIdConnectProviderTemplateAsync(baseFilesGenerationDirectory);
            var schemasTempate = await this.GenerateSchemasTemplateAsync(baseFilesGenerationDirectory);
            var policyFragmentTemplate = await this.GeneratePolicyFragmentsTemplateAsync(apiTemplate.TypedResources.GetAllPolicies(), baseFilesGenerationDirectory);
            var apiReleasesTemplate = await this.GenerateApiReleasesTemplateAsync(baseFilesGenerationDirectory);
            await this.GenerateGatewayTemplateAsync(singleApiName, baseFilesGenerationDirectory);
            await this.GenerateGatewayApiTemplateAsync(singleApiName, multipleApiNames, baseFilesGenerationDirectory);
            
            var parametersTemplate = await this.GenerateParametersTemplateAsync(apisToExtract, loggerTemplate.TypedResources, backendTemplate.TypedResources, namedValueTemplate.TypedResources, identityProviderTemplate.TypedResources, openIdConnectProviderTemplate.TypedResources, baseFilesGenerationDirectory);
            
            await this.GenerateResourceParametersFiles(
                baseFilesGenerationDirectory, 
                parametersTemplate, 
                apiTemplate: apiTemplate,
                policyTemplate: globalServicePolicyTemplate,
                productApisTemplate: productApiTemplate,
                productsTemplate: productTemplate,
                apiVersionSetTemplate: apiVersionSetTemplate,
                authorizationServersTemplate: authorizationServerTemplate,
                tagTemplate: tagTemplate,
                apiTagsTemplate: apiTagTemplate,
                loggersTemplate: loggerTemplate,
                namedValuesTemplate: namedValueTemplate,
                backendsTemplate: backendTemplate,
                groupTemplate: groupTemplate,
                identityProviderTemplate: identityProviderTemplate,
                openIdConnectProviderTemplate: openIdConnectProviderTemplate,
                schemaTemplate: schemasTempate,
                policyFragmentsTemplate: policyFragmentTemplate,
                apiReleaseTemplate: apiReleasesTemplate);

            await this.GenerateMasterTemplateAsync(
                baseFilesGenerationDirectory,
                apiTemplateResources: apiTemplate.TypedResources,
                policyTemplateResources: globalServicePolicyTemplate.TypedResources,
                apiVersionSetTemplateResources: apiVersionSetTemplate.TypedResources,
                productsTemplateResources: productTemplate.TypedResources,
                productApisTemplateResources: productApiTemplate.TypedResources,
                apiTagsTemplateResources: apiTagTemplate.TypedResources,
                loggersTemplateResources: loggerTemplate.TypedResources,
                backendsTemplateResources: backendTemplate.TypedResources,
                authorizationServersTemplateResources: authorizationServerTemplate.TypedResources,
                namedValuesTemplateResources: namedValueTemplate.TypedResources,
                tagTemplateResources: tagTemplate.TypedResources,
                groupTemplateResources: groupTemplate.TypedResources,
                identityProviderTemplateResources: identityProviderTemplate.TypedResources,
                schemaTemplateResources: schemasTempate.TypedResources,
                openIdConnectProviderResources: openIdConnectProviderTemplate.TypedResources,
                policyFragmentsResources: policyFragmentTemplate.TypedResources);
        }


        async Task<List<string>> GetApiNamesToExtract(string singleApiName, List<string> multipleApiNames)
        {
            var apisToExtract = new List<string>();
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

            return apisToExtract;
        }

        /// <summary>
        /// Generates an api dictionary with apiName/versionsetName (if exist one) as key, list of apiNames as value
        /// </summary>
        /// <returns></returns>
        async Task<Dictionary<string, List<string>>> GetAllAPIsDictionary(ExtractorParameters extractorParameters)
        {
            // pull all apis from service
            var apis = await this.apisClient.GetAllAsync(extractorParameters);

            // Generate folders based on all apiversionset
            var apiDictionary = new Dictionary<string, List<string>>();
            
            foreach (var api in apis)
            {
                string apiDisplayName = api.Properties.DisplayName;
                
                if (!apiDictionary.ContainsKey(apiDisplayName))
                {
                    var apiVersionSet = new List<string>();
                    apiVersionSet.Add(api.Name);
                    apiDictionary[apiDisplayName] = apiVersionSet;
                }
                else
                {
                    apiDictionary[apiDisplayName].Add(api.Name);
                }
            }

            return apiDictionary;
        }
    }
}
