using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using apimtemplate.Creator.Utilities;
using apimtemplate.Commands.Abstractions;
using System.Threading.Tasks;
using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Creator.TemplateCreators;
using apimtemplate.Common.FileHandlers;
using apimtemplate.Creator.Models;
using apimtemplate.Common.Constants;

namespace apimtemplate.Commands.Applications
{
    public class CreateApplication : CommandLineApplicationBase
    {
        private CommandOption _appInsightsInstrumentationKey;
        private CommandOption _appInsightsName;
        private CommandOption _namedValueKeys;
        private CommandOption _apimNameValue;
        private CommandOption _configFile;
        private CommandOption _backendUrlConfigFile;
        private CommandOption _preferredAPIsForDeployment;

        public CreateApplication() : base()
        {
        }

        protected override void SetupApplicationAndCommands()
        {
            Name = GlobalConstants.CreateName;
            Description = GlobalConstants.CreateDescription;

            _appInsightsInstrumentationKey = Option("--appInsightsInstrumentationKey <appInsightsInstrumentationKey>", "AppInsights intrumentationkey", CommandOptionType.SingleValue);
            _appInsightsName = Option("--appInsightsName <appInsightsName>", "AppInsights Name", CommandOptionType.SingleValue);
            _namedValueKeys = Option("--namedValues <namedValues>", "Named Values", CommandOptionType.SingleValue);
            _apimNameValue = Option("--apimNameValue <apimNameValue>", "Apim Name Value", CommandOptionType.SingleValue);
            _configFile = Option("--configFile <configFile>", "Config YAML file location", CommandOptionType.SingleValue).IsRequired();
            _backendUrlConfigFile = Option("--backendurlconfigFile <backendurlconfigFile>", "backend url json file location", CommandOptionType.SingleValue);
            _preferredAPIsForDeployment = Option("--preferredAPIsForDeployment <preferredAPIsForDeployment>", "create ARM templates for the given APIs Name(comma separated) else leave this parameter blank then by default all api's will be considered", CommandOptionType.SingleValue);
        }

        protected override async Task<int> ExecuteCommand()
        {
            // convert config file to CreatorConfig class
            FileReader fileReader = new FileReader();
            bool considerAllApiForDeployments = true;
            string[] preferredApis = null;

            GlobalConstants.CommandStartDateTime = DateTime.Now.ToString("MMyyyydd  hh mm ss");

            CreatorConfig creatorConfig = await fileReader.ConvertConfigYAMLToCreatorConfigAsync(_configFile.Value());

            if (_apimNameValue != null && !string.IsNullOrEmpty(_apimNameValue.Value()))
            {
                creatorConfig.apimServiceName = _apimNameValue.Value();
            }

            AppInsightsUpdater appInsightsUpdater = new AppInsightsUpdater();
            appInsightsUpdater.UpdateAppInsightNameAndInstrumentationKey(creatorConfig, _appInsightsInstrumentationKey, _appInsightsName);

            // Overwrite named values from build pipeline
            NamedValuesUpdater namedValuesUpdater = new NamedValuesUpdater();
            namedValuesUpdater.UpdateNamedValueInstances(creatorConfig, _namedValueKeys);

            // validate creator config
            CreatorConfigurationValidator creatorConfigurationValidator = new CreatorConfigurationValidator(this);

            //if preferredAPIsForDeployment passed as parameter
            if (_preferredAPIsForDeployment != null && !string.IsNullOrEmpty(_preferredAPIsForDeployment.Value()))
            {
                considerAllApiForDeployments = false;
                preferredApis = _preferredAPIsForDeployment.Value().Split(",");
            }

            //if backendurlfile passed as parameter
            if (_backendUrlConfigFile != null && !string.IsNullOrEmpty(_backendUrlConfigFile.Value()))
            {
                CreatorApiBackendUrlUpdater creatorApiBackendUrlUpdater = new CreatorApiBackendUrlUpdater();
                creatorConfig = creatorApiBackendUrlUpdater.UpdateBackendServiceUrl(_backendUrlConfigFile.Value(), creatorConfig);
            }

            bool isValidCreatorConfig = creatorConfigurationValidator.ValidateCreatorConfig(creatorConfig);
            if (isValidCreatorConfig == true)
            {
                // required parameters have been supplied

                // initialize file helper classes
                FileNames fileNames = creatorConfig.baseFileName == null 
                    ? FileNameGenerator.GenerateFileNames(creatorConfig.apimServiceName) 
                    : FileNameGenerator.GenerateFileNames(creatorConfig.baseFileName);

                // initialize template creator classes
                APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator();
                LoggerTemplateCreator loggerTemplateCreator = new LoggerTemplateCreator();
                BackendTemplateCreator backendTemplateCreator = new BackendTemplateCreator();
                AuthorizationServerTemplateCreator authorizationServerTemplateCreator = new AuthorizationServerTemplateCreator();
                ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
                TagAPITemplateCreator tagAPITemplateCreator = new TagAPITemplateCreator();
                PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
                ProductGroupTemplateCreator productGroupTemplateCreator = new ProductGroupTemplateCreator();
                SubscriptionTemplateCreator productSubscriptionsTemplateCreator = new SubscriptionTemplateCreator();
                DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
                ReleaseTemplateCreator releaseTemplateCreator = new ReleaseTemplateCreator();
                ProductTemplateCreator productTemplateCreator = new ProductTemplateCreator(policyTemplateCreator, productGroupTemplateCreator, productSubscriptionsTemplateCreator);
                PropertyTemplateCreator propertyTemplateCreator = new PropertyTemplateCreator();
                TagTemplateCreator tagTemplateCreator = new TagTemplateCreator();
                APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, policyTemplateCreator, productAPITemplateCreator, tagAPITemplateCreator, diagnosticTemplateCreator, releaseTemplateCreator);
                MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();

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
                    if (considerAllApiForDeployments || preferredApis.Contains(api.name))
                    {
                        bool isServiceUrlParameterizeInYml = false;
                        if (creatorConfig.serviceUrlParameters != null && creatorConfig.serviceUrlParameters.Count > 0)
                        {
                            isServiceUrlParameterizeInYml = creatorConfig.serviceUrlParameters.Any(s => s.apiName.Equals(api.name));
                            api.serviceUrl = isServiceUrlParameterizeInYml ?
                                creatorConfig.serviceUrlParameters.Where(s => s.apiName.Equals(api.name)).FirstOrDefault().serviceUrl : api.serviceUrl;
                        }
                        // create api templates from provided api config - if the api config contains a supplied apiVersion, split the templates into 2 for metadata and swagger content, otherwise create a unified template
                        List<Template> apiTemplateSet = await apiTemplateCreator.CreateAPITemplatesAsync(api);
                        apiTemplates.AddRange(apiTemplateSet);
                        // create the relevant info that will be needed to properly link to the api template(s) from the master template
                        apiInformation.Add(new LinkedMasterTemplateAPIInformation()
                        {
                            name = api.name,
                            isSplit = apiTemplateCreator.isSplitAPI(api),
                            dependsOnGlobalServicePolicies = creatorConfig.policy != null,
                            dependsOnVersionSets = api.apiVersionSetId != null,
                            dependsOnVersion = masterTemplateCreator.GetDependsOnPreviousApiVersion(api, apiVersions),
                            dependsOnProducts = api.products != null,
                            dependsOnTags = api.tags != null,
                            dependsOnLoggers = await masterTemplateCreator.DetermineIfAPIDependsOnLoggerAsync(api, fileReader),
                            dependsOnAuthorizationServers = api.authenticationSettings != null && api.authenticationSettings.oAuth2 != null && api.authenticationSettings.oAuth2.authorizationServerId != null,
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
                    FileWriter.WriteJSONToFile(masterTemplate, string.Concat(creatorConfig.outputLocation, fileNames.linkedMaster));
                }
                foreach (Template apiTemplate in apiTemplates)
                {
                    APITemplateResource apiResource = apiTemplate.resources.FirstOrDefault(resource => resource.type == ResourceTypeConstants.API) as APITemplateResource;
                    APIConfig providedAPIConfiguration = creatorConfig.apis.FirstOrDefault(api => string.Compare(apiResource.name, APITemplateCreator.MakeResourceName(api), true) == 0);
                    // if the api version is not null the api is split into multiple templates. If the template is split and the content value has been set, then the template is for a subsequent api
                    string apiFileName = FileNameGenerator.GenerateCreatorAPIFileName(providedAPIConfiguration.name, apiTemplateCreator.isSplitAPI(providedAPIConfiguration), apiResource.properties.value != null);
                    FileWriter.WriteJSONToFile(apiTemplate, string.Concat(creatorConfig.outputLocation, apiFileName));
                }
                if (globalServicePolicyTemplate != null)
                {
                    FileWriter.WriteJSONToFile(globalServicePolicyTemplate, string.Concat(creatorConfig.outputLocation, fileNames.globalServicePolicy));
                }
                if (apiVersionSetsTemplate != null)
                {
                    FileWriter.WriteJSONToFile(apiVersionSetsTemplate, string.Concat(creatorConfig.outputLocation, fileNames.apiVersionSets));
                }
                if (productsTemplate != null)
                {
                    FileWriter.WriteJSONToFile(productsTemplate, string.Concat(creatorConfig.outputLocation, fileNames.products));
                }
                if (productAPIsTemplate != null)
                {
                    FileWriter.WriteJSONToFile(productAPIsTemplate, string.Concat(creatorConfig.outputLocation, fileNames.productAPIs));
                }
                if (propertyTemplate != null)
                {
                    FileWriter.WriteJSONToFile(propertyTemplate, string.Concat(creatorConfig.outputLocation, fileNames.namedValues));
                }
                if (loggersTemplate != null)
                {
                    FileWriter.WriteJSONToFile(loggersTemplate, string.Concat(creatorConfig.outputLocation, fileNames.loggers));
                }
                if (backendsTemplate != null)
                {
                    FileWriter.WriteJSONToFile(backendsTemplate, string.Concat(creatorConfig.outputLocation, fileNames.backends));
                }
                if (authorizationServersTemplate != null)
                {
                    FileWriter.WriteJSONToFile(authorizationServersTemplate, string.Concat(creatorConfig.outputLocation, fileNames.authorizationServers));
                }
                if (tagTemplate != null)
                {
                    FileWriter.WriteJSONToFile(tagTemplate, string.Concat(creatorConfig.outputLocation, fileNames.tags));
                }

                // write parameters to outputLocation
                FileWriter.WriteJSONToFile(templateParameters, string.Concat(creatorConfig.outputLocation, fileNames.parameters));
                Console.WriteLine("Templates written to output location");

            }
            return 0;
        }
    }
}
