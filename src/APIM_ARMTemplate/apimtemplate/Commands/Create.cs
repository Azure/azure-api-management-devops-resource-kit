using McMaster.Extensions.CommandLineUtils;
using Colors.Net;
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
                bool isValidCreatorConfig = ValidateCreatorConfig(creatorConfig);
                if (isValidCreatorConfig == true)
                {
                    // required parameters have been supplied

                    // initialize helper classes
                    FileWriter fileWriter = new FileWriter();
                    FileNameGenerator fileNameGenerator = new FileNameGenerator();
                    TemplateCreator templateCreator = new TemplateCreator();
                    APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(templateCreator);
                    LoggerTemplateCreator loggerTemplateCreator = new LoggerTemplateCreator(templateCreator);
                    ProductTemplateCreator productTemplateCreator = new ProductTemplateCreator(templateCreator);
                    ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
                    PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
                    DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
                    APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, templateCreator, policyTemplateCreator, productAPITemplateCreator, diagnosticTemplateCreator);
                    MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator(templateCreator);
                    CreatorFileNames creatorFileNames = fileNameGenerator.GenerateCreatorLinkedFileNames(creatorConfig);

                    // create templates from provided configuration
                    Template apiVersionSetsTemplate = creatorConfig.apiVersionSets != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
                    Template productsTemplate = creatorConfig.products != null ? productTemplateCreator.CreateProductTemplate(creatorConfig) : null;
                    Template loggersTemplate = creatorConfig.loggers != null ? loggerTemplateCreator.CreateLoggerTemplate(creatorConfig) : null;
                    // store name and full template on each api necessary to build unlinked templates
                    Dictionary<string, Template> initialAPITemplates = new Dictionary<string, Template>();
                    Dictionary<string, Template> subsequentAPITemplates = new Dictionary<string, Template>();
                    // store name and whether the api will depend on the version set template each api necessary to build linked templates
                    List<LinkedMasterTemplateAPIInformation> apiInformation = new List<LinkedMasterTemplateAPIInformation>();
                    // create parameters file
                    Template masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

                    foreach (APIConfig api in creatorConfig.apis)
                    {
                        Template initialAPITemplate = await apiTemplateCreator.CreateInitialAPITemplateAsync(creatorConfig, api);
                        Template subsequentAPITemplate = apiTemplateCreator.CreateSubsequentAPITemplate(api);
                        initialAPITemplates.Add(api.name, initialAPITemplate);
                        subsequentAPITemplates.Add(api.name, subsequentAPITemplate);
                        apiInformation.Add(new LinkedMasterTemplateAPIInformation()
                        {
                            name = api.name,
                            dependsOnVersionSets = api.apiVersionSetId != null,
                            dependsOnProducts = api.products != null,
                            dependsOnLoggers = masterTemplateCreator.DetermineIfAPIDependsOnLogger(api, fileReader)
                        });
                    }

                    // write templates to outputLocation
                    if (creatorConfig.linked == true)
                    {
                        // create linked master template
                        Template masterTemplate = masterTemplateCreator.CreateLinkedMasterTemplate(apiVersionSetsTemplate, productsTemplate, loggersTemplate, apiInformation, creatorFileNames, fileNameGenerator);
                        fileWriter.WriteJSONToFile(masterTemplate, String.Concat(creatorConfig.outputLocation, creatorFileNames.linkedMaster));
                    }
                    foreach (KeyValuePair<string, Template> initialAPITemplatePair in initialAPITemplates)
                    {
                        string initialAPIFileName = fileNameGenerator.GenerateAPIFileName(initialAPITemplatePair.Key, true);
                        fileWriter.WriteJSONToFile(initialAPITemplatePair.Value, String.Concat(creatorConfig.outputLocation, initialAPIFileName));
                    }
                    foreach (KeyValuePair<string, Template> subsequentAPITemplatePair in subsequentAPITemplates)
                    {
                        string subsequentAPIFileName = fileNameGenerator.GenerateAPIFileName(subsequentAPITemplatePair.Key, false);
                        fileWriter.WriteJSONToFile(subsequentAPITemplatePair.Value, String.Concat(creatorConfig.outputLocation, subsequentAPIFileName));
                    }
                    if (apiVersionSetsTemplate != null)
                    {
                        fileWriter.WriteJSONToFile(apiVersionSetsTemplate, String.Concat(creatorConfig.outputLocation, creatorFileNames.apiVersionSets));
                    }
                    if (productsTemplate != null)
                    {
                        fileWriter.WriteJSONToFile(productsTemplate, String.Concat(creatorConfig.outputLocation, creatorFileNames.products));
                    }
                    if (loggersTemplate != null)
                    {
                        fileWriter.WriteJSONToFile(loggersTemplate, String.Concat(creatorConfig.outputLocation, creatorFileNames.loggers));
                    }
                    // write parameters to outputLocation
                    fileWriter.WriteJSONToFile(masterTemplateParameters, String.Concat(creatorConfig.outputLocation, creatorConfig.linked == true ? creatorFileNames.linkedParameters : creatorFileNames.unlinkedParameters));
                    ColoredConsole.WriteLine("Templates written to output location");
                }
                return 0;
            });
        }

        public bool ValidateCreatorConfig(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            // ensure required parameters have been passed in
            if (creatorConfig.outputLocation == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "Output location is required");
            }
            if (creatorConfig.version == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "Version is required");
            }
            if (creatorConfig.apimServiceName == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "APIM service name is required");
            }
            if (creatorConfig.linked == true && creatorConfig.linkedTemplatesBaseUrl == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "LinkTemplatesBaseUrl is required for linked templates");
            }
            foreach (APIVersionSetConfig apiVersionSet in creatorConfig.apiVersionSets)
            {
                if (apiVersionSet != null && apiVersionSet.displayName == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this, "Display name is required if an API Version Set is provided");
                }
                if (apiVersionSet != null && apiVersionSet.versioningScheme == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this, "Versioning scheme is required if an API Version Set is provided");
                }
            }
            foreach (APIConfig api in creatorConfig.apis)
            {
                if (api == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this, "API configuration is required");
                }
                if (api.openApiSpec == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this, "Open API Spec is required");
                }
                if (api.suffix == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this, "API suffix is required");
                }
                if (api.name == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this, "API name is required");
                }
                if (api.operations != null)
                {
                    foreach (KeyValuePair<string, OperationsConfig> operation in api.operations)
                    {
                        if (operation.Value == null || operation.Value.policy == null)
                        {
                            isValid = false;
                            throw new CommandParsingException(this, "Policy XML is required if an API operation is provided");
                        }
                    }
                }
                if (api.diagnostic != null && api.diagnostic.loggerId == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this, "LoggerId is required if an API diagnostic is provided");
                }
            }
            return isValid;
        }
    }
}