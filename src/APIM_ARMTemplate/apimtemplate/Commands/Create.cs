using McMaster.Extensions.CommandLineUtils;
using Colors.Net;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class CreateCommand : CommandLineApplication
    {
        public CreateCommand()
        {
            this.Name = Constants.CreateName;
            this.Description = Constants.CreateDescription;

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
                    TemplateCreator templateCreator = new TemplateCreator();
                    APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(templateCreator);
                    ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
                    PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader);
                    DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
                    APITemplateCreator apiTemplateCreator = new APITemplateCreator(fileReader, templateCreator, policyTemplateCreator, productAPITemplateCreator, diagnosticTemplateCreator);
                    MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator(templateCreator);

                    // create templates from provided configuration
                    Template apiVersionSetTemplate = creatorConfig.apiVersionSet != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
                    Template initialAPITemplate = await apiTemplateCreator.CreateInitialAPITemplateAsync(creatorConfig);
                    Template subsequentAPITemplate = apiTemplateCreator.CreateSubsequentAPITemplate(creatorConfig);
                    if (creatorConfig.linked == true)
                    {
                        CreatorFileNames creatorFileNames = fileWriter.GenerateCreatorLinkedFileNames();

                        // create linked master template
                        Template masterTemplate = masterTemplateCreator.CreateLinkedMasterTemplate(apiVersionSetTemplate, initialAPITemplate, subsequentAPITemplate, creatorFileNames);
                        Template masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

                        // write templates to outputLocation
                        if (apiVersionSetTemplate != null)
                        {
                            fileWriter.WriteJSONToFile(apiVersionSetTemplate, String.Concat(creatorConfig.outputLocation, creatorFileNames.apiVersionSet));
                        }
                        fileWriter.WriteJSONToFile(initialAPITemplate, String.Concat(creatorConfig.outputLocation, creatorFileNames.initialAPI));
                        fileWriter.WriteJSONToFile(subsequentAPITemplate, String.Concat(creatorConfig.outputLocation, creatorFileNames.subsequentAPI));
                        fileWriter.WriteJSONToFile(masterTemplate, String.Concat(creatorConfig.outputLocation, "/master.template.json"));
                        fileWriter.WriteJSONToFile(masterTemplateParameters, String.Concat(creatorConfig.outputLocation, "/master.parameters.json"));
                    }
                    else
                    {
                        // create unlinked master template
                        Template initialMasterTemplate = masterTemplateCreator.CreateInitialUnlinkedMasterTemplate(apiVersionSetTemplate, initialAPITemplate);
                        Template subsequentMasterTemplate = masterTemplateCreator.CreateSubsequentUnlinkedMasterTemplate(subsequentAPITemplate);
                        Template masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

                        // write templates to outputLocation
                        fileWriter.WriteJSONToFile(initialMasterTemplate, String.Concat(creatorConfig.outputLocation, "/master1.template.json"));
                        fileWriter.WriteJSONToFile(subsequentMasterTemplate, String.Concat(creatorConfig.outputLocation, "/master2.template.json"));
                        fileWriter.WriteJSONToFile(masterTemplateParameters, String.Concat(creatorConfig.outputLocation, "/master.parameters.json"));
                    }

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
            if (creatorConfig.api == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "API configuration is required");
            }
            if (creatorConfig.api.openApiSpec == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "Open API Spec is required");
            }
            if (creatorConfig.api.suffix == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "API suffix is required");
            }
            if (creatorConfig.api.name == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "API name is required");
            }
            if (creatorConfig.linked == true && creatorConfig.linkedTemplatesBaseUrl == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "LinkTemplatesBaseUrl is required for linked templates");
            }
            if (creatorConfig.apiVersionSet != null && creatorConfig.apiVersionSet.displayName == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "Display name is required if an API Version Set is provided");
            }
            if (creatorConfig.apiVersionSet != null && creatorConfig.apiVersionSet.versioningScheme == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "Versioning scheme is required if an API Version Set is provided");
            }
            if (creatorConfig.api.operations != null)
            {
                foreach (KeyValuePair<string, OperationsConfig> operation in creatorConfig.api.operations)
                {
                    if (operation.Value == null || operation.Value.policy == null)
                    {
                        isValid = false;
                        throw new CommandParsingException(this, "Policy XML is required if an API operation is provided");
                    }
                }
            }
            if (creatorConfig.api.diagnostic != null && creatorConfig.api.diagnostic.loggerId == null)
            {
                isValid = false;
                throw new CommandParsingException(this, "LoggerId is required if an API diagnostic is provided");
            }
            return isValid;
        }
    }
}