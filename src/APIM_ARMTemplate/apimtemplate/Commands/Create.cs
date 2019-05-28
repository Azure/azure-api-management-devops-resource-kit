using McMaster.Extensions.CommandLineUtils;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class CreateCommand : CommandLineApplication
    {
        public CreateCommand()
        {
            this.Name = GlobalConstants.CreateName;
            this.Description = GlobalConstants.CreateDescription;

            // list command options
            CommandOption configFile = this.Option("--configFile <configFile>", "Config YAML file location", CommandOptionType.SingleValue).IsRequired();

            this.HelpOption();

            this.OnExecute(async () =>
            {
                // convert config file to CreatorConfig class
                FileReader fileReader = new FileReader();
                CreatorConfig creatorConfig = await fileReader.ConvertConfigYAMLToCreatorConfigAsync(configFile.Value());

                // validate creator config
                CreatorConfigurationValidator creatorConfigurationValidator = new CreatorConfigurationValidator(this);
                bool isValidCreatorConfig = creatorConfigurationValidator.ValidateCreatorConfig(creatorConfig);
                if (isValidCreatorConfig == true)
                {
                    // required parameters have been supplied

                    // initialize file helper classes
                    FileWriter fileWriter = new FileWriter();
                    FileNameGenerator fileNameGenerator = new FileNameGenerator();
                    FileNames fileNames = fileNameGenerator.GenerateFileNames(creatorConfig.apimServiceName);

                    // initialize template creator classes
                    APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator();
                    LoggerTemplateCreator loggerTemplateCreator = new LoggerTemplateCreator();
                    BackendTemplateCreator backendTemplateCreator = new BackendTemplateCreator();
                    AuthorizationServerTemplateCreator authorizationServerTemplateCreator = new AuthorizationServerTemplateCreator();
                    ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
                    PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
                    DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
                    ReleaseTemplateCreator releaseTemplateCreator = new ReleaseTemplateCreator();
                    ProductTemplateCreator productTemplateCreator = new ProductTemplateCreator(policyTemplateCreator);
                    APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, policyTemplateCreator, productAPITemplateCreator, diagnosticTemplateCreator, releaseTemplateCreator);
                    MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator();

                    // create templates from provided configuration
                    Console.WriteLine("Creating API version set template");
                    Console.WriteLine("------------------------------------------");
                    Template apiVersionSetsTemplate = creatorConfig.apiVersionSets != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
                    Console.WriteLine("Creating product template");
                    Console.WriteLine("------------------------------------------");
                    Template productsTemplate = creatorConfig.products != null ? productTemplateCreator.CreateProductTemplate(creatorConfig) : null;
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
                    foreach (APIConfig api in creatorConfig.apis)
                    {
                        // create api templates from provided api config - if the api config contains a supplied apiVersion, split the templates into 2 for metadata and swagger content, otherwise create a unified template
                        List<Template> apiTemplateSet = await apiTemplateCreator.CreateAPITemplatesAsync(api);
                        apiTemplates.AddRange(apiTemplateSet);
                        // create the relevant info that will be needed to properly link to the api template(s) from the master template
                        apiInformation.Add(new LinkedMasterTemplateAPIInformation()
                        {
                            name = api.name,
                            isSplit = apiTemplateCreator.isSplitAPI(api),
                            dependsOnVersionSets = api.apiVersionSetId != null,
                            dependsOnProducts = api.products != null,
                            dependsOnLoggers = await masterTemplateCreator.DetermineIfAPIDependsOnLoggerAsync(api, fileReader),
                            dependsOnAuthorizationServers = api.authenticationSettings != null && api.authenticationSettings.oAuth2 != null && api.authenticationSettings.oAuth2.authorizationServerId != null,
                            dependsOnBackends = await masterTemplateCreator.DetermineIfAPIDependsOnBackendAsync(api, fileReader)
                        });
                    }

                    // create parameters file
                    Template templateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

                    // write templates to outputLocation
                    if (creatorConfig.linked == true)
                    {
                        // create linked master template
                        Template masterTemplate = masterTemplateCreator.CreateLinkedMasterTemplate(apiVersionSetsTemplate, productsTemplate, loggersTemplate, backendsTemplate, authorizationServersTemplate, apiInformation, fileNames, creatorConfig.apimServiceName, fileNameGenerator);
                        fileWriter.WriteJSONToFile(masterTemplate, String.Concat(creatorConfig.outputLocation, fileNames.linkedMaster));
                    }
                    foreach (Template apiTemplate in apiTemplates)
                    {
                        APITemplateResource apiResource = apiTemplate.resources.FirstOrDefault(resource => resource.type == ResourceTypeConstants.API) as APITemplateResource;
                        APIConfig providedAPIConfiguration = creatorConfig.apis.FirstOrDefault(api => apiResource.name.Contains(api.name));
                        // if the api version is not null the api is split into multiple templates. If the template is split and the content value has been set, then the template is for a subsequent api
                        string apiFileName = fileNameGenerator.GenerateCreatorAPIFileName(providedAPIConfiguration.name, apiTemplateCreator.isSplitAPI(providedAPIConfiguration), apiResource.properties.value == null, creatorConfig.apimServiceName);
                        fileWriter.WriteJSONToFile(apiTemplate, String.Concat(creatorConfig.outputLocation, apiFileName));
                    }
                    if (apiVersionSetsTemplate != null)
                    {
                        fileWriter.WriteJSONToFile(apiVersionSetsTemplate, String.Concat(creatorConfig.outputLocation, fileNames.apiVersionSets));
                    }
                    if (productsTemplate != null)
                    {
                        fileWriter.WriteJSONToFile(productsTemplate, String.Concat(creatorConfig.outputLocation, fileNames.products));
                    }
                    if (loggersTemplate != null)
                    {
                        fileWriter.WriteJSONToFile(loggersTemplate, String.Concat(creatorConfig.outputLocation, fileNames.loggers));
                    }
                    if (backendsTemplate != null)
                    {
                        fileWriter.WriteJSONToFile(backendsTemplate, String.Concat(creatorConfig.outputLocation, fileNames.backends));
                    }
                    if (authorizationServersTemplate != null)
                    {
                        fileWriter.WriteJSONToFile(authorizationServersTemplate, String.Concat(creatorConfig.outputLocation, fileNames.authorizationServers));
                    }

                    // write parameters to outputLocation
                    fileWriter.WriteJSONToFile(templateParameters, String.Concat(creatorConfig.outputLocation, fileNames.parameters));
                    Console.WriteLine("Templates written to output location");
                    Console.WriteLine("Press any key to exit process:");
#if DEBUG
                    Console.ReadKey();
#endif
                }
                return 0;
            });
        }
    }
}