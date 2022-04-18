// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using System.IO;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications
{
    public class CreateApplicationCommand : IConsoleAppCommand<CreateConsoleAppConfiguration, CreatorConfig>
    {
        readonly ILogger<CreateApplicationCommand> logger;
        readonly CreatorExecutor creatorExecutor;
        readonly ITemplateBuilder templateBuilder;

        public CreateApplicationCommand(
            ILogger<CreateApplicationCommand> logger, 
            CreatorExecutor creatorExecutor,
            ITemplateBuilder templateBuilder)
        {
            this.logger = logger;
            this.creatorExecutor = creatorExecutor;
            this.templateBuilder = templateBuilder;
        }

        public async Task<CreatorConfig> ParseInputConfigurationAsync(CreateConsoleAppConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(CreateConsoleAppConfiguration));
            }

            if (string.IsNullOrEmpty(configuration.ConfigFile))
            {
                throw new ArgumentNullException(nameof(configuration.ConfigFile));
            }

            // convert config file to CreatorConfig class
            FileReader fileReader = new FileReader();
            GlobalConstants.CommandStartDateTime = DateTime.Now.ToString("MMyyyydd  hh mm ss");

            CreatorConfig creatorConfig = await fileReader.ConvertConfigYAMLToCreatorConfigAsync(configuration.ConfigFile);

            if (!string.IsNullOrEmpty(configuration.ApimNameValue))
            {
                creatorConfig.apimServiceName = configuration.ApimNameValue;
            }

            AppInsightsUpdater appInsightsUpdater = new AppInsightsUpdater();
            appInsightsUpdater.UpdateAppInsightNameAndInstrumentationKey(creatorConfig, configuration.AppInsightsInstrumentationKey, configuration.AppInsightsName);

            // Overwrite named values from build pipeline
            NamedValuesUpdater namedValuesUpdater = new NamedValuesUpdater();
            namedValuesUpdater.UpdateNamedValueInstances(creatorConfig, configuration.NamedValueKeys);

            // validate creator config
            CreatorConfigurationValidator creatorConfigurationValidator = new CreatorConfigurationValidator(creatorConfig);

            //if preferredAPIsForDeployment passed as parameter
            if (configuration.PreferredAPIsForDeployment != null && !string.IsNullOrEmpty(configuration.PreferredAPIsForDeployment))
            {
                creatorConfig.ConsiderAllApiForDeployments = false;
                creatorConfig.PreferredApis = configuration.PreferredAPIsForDeployment.Split(",");
            }

            //if parameterizeNamedValuesAndSecrets passed as parameter
            if (!string.IsNullOrEmpty(configuration.ParameterizeNamedValues))
            {
                creatorConfig.parameterizeNamedValues = true;
            }

            //if backendurlfile passed as parameter
            if (configuration.BackendUrlConfigFile != null && !string.IsNullOrEmpty(configuration.BackendUrlConfigFile))
            {
                CreatorApiBackendUrlUpdater creatorApiBackendUrlUpdater = new CreatorApiBackendUrlUpdater();
                creatorConfig = creatorApiBackendUrlUpdater.UpdateBackendServiceUrl(configuration.BackendUrlConfigFile, creatorConfig);
            }

            creatorConfigurationValidator.ValidateCreatorConfig();
            return creatorConfig;
        }

        public async Task ExecuteCommandAsync(CreatorConfig creatorConfig)
        {
            if (!Directory.Exists(creatorConfig.outputLocation))
            {
                Directory.CreateDirectory(creatorConfig.outputLocation);
            }

            FileReader fileReader = new FileReader();

            // initialize file helper classes
            FileNames fileNames = creatorConfig.baseFileName == null
                ? FileNameGenerator.GenerateFileNames(creatorConfig.apimServiceName)
                : FileNameGenerator.GenerateFileNames(creatorConfig.baseFileName);

            // initialize template creator classes
            APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(this.templateBuilder);
            LoggerTemplateCreator loggerTemplateCreator = new LoggerTemplateCreator(this.templateBuilder);
            BackendTemplateCreator backendTemplateCreator = new BackendTemplateCreator(this.templateBuilder);
            AuthorizationServerTemplateCreator authorizationServerTemplateCreator = new AuthorizationServerTemplateCreator(this.templateBuilder);
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator(this.templateBuilder);
            TagAPITemplateCreator tagAPITemplateCreator = new TagAPITemplateCreator();
            PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader, this.templateBuilder);
            ProductGroupTemplateCreator productGroupTemplateCreator = new ProductGroupTemplateCreator();
            SubscriptionTemplateCreator productSubscriptionsTemplateCreator = new SubscriptionTemplateCreator();
            DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
            ReleaseTemplateCreator releaseTemplateCreator = new ReleaseTemplateCreator();
            ProductTemplateCreator productTemplateCreator = new ProductTemplateCreator(policyTemplateCreator, productGroupTemplateCreator, productSubscriptionsTemplateCreator, this.templateBuilder);
            PropertyTemplateCreator propertyTemplateCreator = new PropertyTemplateCreator(this.templateBuilder);
            TagTemplateCreator tagTemplateCreator = new TagTemplateCreator(this.templateBuilder);
            APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, policyTemplateCreator, productAPITemplateCreator, tagAPITemplateCreator, diagnosticTemplateCreator, releaseTemplateCreator, this.templateBuilder);
            MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator(this.templateBuilder);

            // create templates from provided configuration
            Console.WriteLine("Creating global service policy template");
            Console.WriteLine("------------------------------------------");
            Template globalServicePolicyTemplate = creatorConfig.policy != null ? policyTemplateCreator.CreateGlobalServicePolicyTemplate(creatorConfig) : null;
            Console.WriteLine("Creating API version set template");
            Console.WriteLine("------------------------------------------");
            Template apiVersionSetsTemplate = creatorConfig.apiVersionSets != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
            Console.WriteLine("Creating product template");
            Console.WriteLine("------------------------------------------");
            Template productsTemplate = creatorConfig.products != null ? productTemplateCreator.CreateProductTemplate(creatorConfig) : null;
            Console.WriteLine("Creating product/APIs template");
            Console.WriteLine("------------------------------------------");
            Template productAPIsTemplate = creatorConfig.products != null && creatorConfig.apis != null ? productAPITemplateCreator.CreateProductAPITemplate(creatorConfig) : null;
            Console.WriteLine("Creating named values template");
            Console.WriteLine("------------------------------------------");
            Template propertyTemplate = creatorConfig.namedValues != null ? propertyTemplateCreator.CreatePropertyTemplate(creatorConfig) : null;
            Console.WriteLine("Creating logger template");
            Console.WriteLine("------------------------------------------");
            Template loggersTemplate = creatorConfig.loggers != null ? loggerTemplateCreator.CreateLoggerTemplate(creatorConfig) : null;
            Console.WriteLine("Creating backend template");
            Console.WriteLine("------------------------------------------");
            Template backendsTemplate = creatorConfig.backends != null ? backendTemplateCreator.CreateBackendTemplate(creatorConfig) : null;
            Console.WriteLine("Creating authorization server template");
            Console.WriteLine("------------------------------------------");
            Template authorizationServersTemplate = creatorConfig.authorizationServers != null ? authorizationServerTemplateCreator.CreateAuthorizationServerTemplate(creatorConfig) : null;

            // store name and whether the api will depend on the version set template each api necessary to build linked templates
            List<LinkedMasterTemplateAPIInformation> apiInformation = new List<LinkedMasterTemplateAPIInformation>();
            List<Template> apiTemplates = new List<Template>();
            Console.WriteLine("Creating API templates");
            Console.WriteLine("------------------------------------------");

            IDictionary<string, string[]> apiVersions = APITemplateCreator.GetApiVersionSets(creatorConfig);

            foreach (APIConfig api in creatorConfig.apis)
            {
                if (creatorConfig.ConsiderAllApiForDeployments || creatorConfig.PreferredApis.Contains(api.name))
                {
                    bool isServiceUrlParameterizeInYml = false;
                    if (creatorConfig.serviceUrlParameters != null && creatorConfig.serviceUrlParameters.Count > 0)
                    {
                        isServiceUrlParameterizeInYml = creatorConfig.serviceUrlParameters.Any(s => s.ApiName.Equals(api.name));
                        api.serviceUrl = isServiceUrlParameterizeInYml ?
                            creatorConfig.serviceUrlParameters.Where(s => s.ApiName.Equals(api.name)).FirstOrDefault().ServiceUrl : api.serviceUrl;
                    }
                    // create api templates from provided api config - if the api config contains a supplied apiVersion, split the templates into 2 for metadata and swagger content, otherwise create a unified template
                    List<Template> apiTemplateSet = await apiTemplateCreator.CreateAPITemplatesAsync(api);
                    apiTemplates.AddRange(apiTemplateSet);
                    // create the relevant info that will be needed to properly link to the api template(s) from the master template
                    apiInformation.Add(new LinkedMasterTemplateAPIInformation()
                    {
                        name = api.name,
                        isSplit = apiTemplateCreator.IsSplitAPI(api),
                        dependsOnGlobalServicePolicies = creatorConfig.policy != null,
                        dependsOnVersionSets = api.apiVersionSetId != null,
                        dependsOnVersion = masterTemplateCreator.GetDependsOnPreviousApiVersion(api, apiVersions),
                        dependsOnProducts = api.products != null,
                        dependsOnTags = api.tags != null,
                        dependsOnLoggers = await masterTemplateCreator.DetermineIfAPIDependsOnLoggerAsync(api, fileReader),
                        dependsOnAuthorizationServers = api.authenticationSettings != null && api.authenticationSettings.OAuth2 != null && api.authenticationSettings.OAuth2.AuthorizationServerId != null,
                        dependsOnBackends = await masterTemplateCreator.DetermineIfAPIDependsOnBackendAsync(api, fileReader),
                        isServiceUrlParameterize = isServiceUrlParameterizeInYml
                    });
                }
            }

            Console.WriteLine("Creating tag template");
            Console.WriteLine("------------------------------------------");
            Template tagTemplate = creatorConfig.tags != null ? tagTemplateCreator.CreateTagTemplate(creatorConfig) : null;

            // create parameters file
            Template templateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

            // write templates to outputLocation
            if (creatorConfig.linked == true)
            {
                // create linked master template
                Template masterTemplate = masterTemplateCreator.CreateLinkedMasterTemplate(creatorConfig, globalServicePolicyTemplate, apiVersionSetsTemplate, productsTemplate, productAPIsTemplate, propertyTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, tagTemplate, apiInformation, fileNames, creatorConfig.apimServiceName);
                FileWriter.WriteJSONToFile(masterTemplate, string.Concat(creatorConfig.outputLocation, fileNames.LinkedMaster));
            }
            foreach (Template apiTemplate in apiTemplates)
            {
                APITemplateResource apiResource = apiTemplate.Resources.FirstOrDefault(resource => resource.Type == ResourceTypeConstants.API) as APITemplateResource;
                APIConfig providedAPIConfiguration = creatorConfig.apis.FirstOrDefault(api => string.Compare(apiResource.Name, APITemplateCreator.MakeResourceName(api), true) == 0);
                // if the api version is not null the api is split into multiple templates. If the template is split and the content value has been set, then the template is for a subsequent api
                string apiFileName = FileNameGenerator.GenerateCreatorAPIFileName(providedAPIConfiguration.name, apiTemplateCreator.IsSplitAPI(providedAPIConfiguration), apiResource.Properties.Value != null);
                FileWriter.WriteJSONToFile(apiTemplate, string.Concat(creatorConfig.outputLocation, apiFileName));
            }
            if (globalServicePolicyTemplate != null)
            {
                FileWriter.WriteJSONToFile(globalServicePolicyTemplate, string.Concat(creatorConfig.outputLocation, fileNames.GlobalServicePolicy));
            }
            if (apiVersionSetsTemplate != null)
            {
                FileWriter.WriteJSONToFile(apiVersionSetsTemplate, string.Concat(creatorConfig.outputLocation, fileNames.ApiVersionSets));
            }
            if (productsTemplate != null)
            {
                FileWriter.WriteJSONToFile(productsTemplate, string.Concat(creatorConfig.outputLocation, fileNames.Products));
            }
            if (productAPIsTemplate != null)
            {
                FileWriter.WriteJSONToFile(productAPIsTemplate, string.Concat(creatorConfig.outputLocation, fileNames.ProductAPIs));
            }
            if (propertyTemplate != null)
            {
                FileWriter.WriteJSONToFile(propertyTemplate, string.Concat(creatorConfig.outputLocation, fileNames.NamedValues));
            }
            if (loggersTemplate != null)
            {
                FileWriter.WriteJSONToFile(loggersTemplate, string.Concat(creatorConfig.outputLocation, fileNames.Loggers));
            }
            if (backendsTemplate != null)
            {
                FileWriter.WriteJSONToFile(backendsTemplate, string.Concat(creatorConfig.outputLocation, fileNames.Backends));
            }
            if (authorizationServersTemplate != null)
            {
                FileWriter.WriteJSONToFile(authorizationServersTemplate, string.Concat(creatorConfig.outputLocation, fileNames.AuthorizationServers));
            }
            if (tagTemplate != null)
            {
                FileWriter.WriteJSONToFile(tagTemplate, string.Concat(creatorConfig.outputLocation, fileNames.Tags));
            }

            // write parameters to outputLocation
            FileWriter.WriteJSONToFile(templateParameters, string.Concat(creatorConfig.outputLocation, fileNames.Parameters));
            Console.WriteLine("Templates written to output location");

        }
    }
}
