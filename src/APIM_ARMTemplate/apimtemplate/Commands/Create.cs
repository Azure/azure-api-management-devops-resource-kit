using McMaster.Extensions.CommandLineUtils;
using Colors.Net;
using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
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

                // ensure required parameters have been passed in
                if (creatorConfig.outputLocation == null)
                {
                    throw new CommandParsingException(this, "Output location is required");
                }
                else if (creatorConfig.version == null)
                {
                    throw new CommandParsingException(this, "Version is required");
                }
                else if (creatorConfig.apimServiceName == null)
                {
                    throw new CommandParsingException(this, "APIM service name is required");
                }
                else if (creatorConfig.api == null)
                {
                    throw new CommandParsingException(this, "API configuration is required");
                }
                else if (creatorConfig.api.openApiSpec == null)
                {
                    throw new CommandParsingException(this, "Open API Spec is required");
                }
                else if (creatorConfig.api.suffix == null)
                {
                    throw new CommandParsingException(this, "API suffix is required");
                }
                else if (creatorConfig.api.name == null)
                {
                    throw new CommandParsingException(this, "API name is required");
                }
                else if (creatorConfig.linked == true && creatorConfig.linkedTemplatesBaseUrl == null)
                {
                    throw new CommandParsingException(this, "LinkTemplatesBaseUrl is required for linked templates");
                }
                else
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
                    } else
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
    }
}